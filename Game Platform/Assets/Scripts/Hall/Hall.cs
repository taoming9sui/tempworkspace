﻿using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hall : GameActivity
{
    public GameObject cameraObj;
    public GameObject canvasObj;
    public GameObject sceneObj;
    public RoomItem[] roomItems;

    private IDictionary<string, float> m_updateTimerSet = new Dictionary<string, float>();
    private IList<RoomItemInfo> m_roomInfoList = new List<RoomItemInfo>();
    private IList<RoomItemInfo> m_pageRoomInfoList = new List<RoomItemInfo>();
    private int m_roomPageSize = 0;
    private int m_roomPageNo = 1;
    private string m_playerId = "";
    private string m_playerName = "";
    private int m_playerHeadNo = 0;
    private int m_playerPoint = 0;

    #region unity触发器
    private void Awake()
    {
        //初始化信息列表
        m_roomPageSize = roomItems.Length;
        //初始化计时器项目
        InitTimer();
        //初始化下拉框选项
        InitDropdown();
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        TimerUpdate(dt);
    }
    #endregion

    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
        //清空聊天框
        ClearChat();
        //请求房间列表
        RequestHallInfo();
        //请求玩家信息
        RequestPlayerInfo();
    }
    public override void OnDisconnect()
    {
        GameManager.Instance.SetActivity("MainTheme");
    }
    public override void OnConnect()
    {
    }
    public override void OnMessage(JObject jsonData)
    {
        try
        {
            string type = jsonData.GetValue("Type").ToString();
            if (type == "Server_Hall")
            {
                JObject data = (JObject)jsonData.GetValue("Data");
                string action = data.GetValue("Action").ToString();
                switch (action)
                {
                    case "LogoutSuccess":
                        {
                            this.LogoutSuccess();
                        }
                        break;
                    case "HallChat":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.ReceiveChat(content.GetValue("Sender").ToString(), content.GetValue("Chat").ToString());
                        }
                        break;
                    case "Tip":
                        {
                            string content = data.GetValue("Content").ToString();
                            this.TipModel(content);
                        }
                        break;
                    case "ResponseHallInfo":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.ReceiveHallInfo((string)content.GetValue("RoomList"), (int)content.GetValue("RoomCount"), (int)content.GetValue("PlayerCount"));
                        }
                        break;
                    case "ResponsePlayerInfo":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.UpdatePlayerInfo(content.ToString());
                        }
                        break;
                    case "ChangePlayerInfoSuccess":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.UpdatePlayerInfo(content.ToString());
                        }
                        break;           
                }
            }
            else if (type == "Server_Center")
            {
                JObject data = (JObject)jsonData.GetValue("Data");
                string action = data.GetValue("Action").ToString();
                switch (action)
                {
                    case "LogoutSuccess":
                        {
                            this.LogoutSuccess();
                        }
                        break;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    #endregion

    #region UI交互脚本
    public void SendChatButton()
    {
        InputField chat_input = canvasObj.transform.Find("chatpanel/chat_input").GetComponent<InputField>();
        //获取输入框消息并清空
        string chat = chat_input.text;
        chat_input.text = "";
        SendChat(chat);
    }
    public void SendChatEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            InputField chat_input = canvasObj.transform.Find("chatpanel/chat_input").GetComponent<InputField>();
            //获取输入框消息并清空
            string chat = chat_input.text;
            chat_input.text = "";
            SendChat(chat);
            //重新获取焦点
            chat_input.Select();
            chat_input.ActivateInputField();
        }
    }
    public void CreateRoomModel()
    {
        GameObject modelObj = canvasObj.transform.Find("createroom_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        modelDialog.ModelShow((code) =>
        {
            switch (code)
            {
                case "confirm":
                    {
                        string caption = modelObj.transform.Find("model/caption_input").GetComponent<InputField>().text;
                        string password = modelObj.transform.Find("model/password_input").GetComponent<InputField>().text;
                        SendCreateRoom(caption, password);
                    }
                    break;
            }
        });
    }
    public void TipModel(string tip)
    {
        GameObject modelObj = canvasObj.transform.Find("tip_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        Text tip_text = modelObj.transform.Find("model/tip_text").GetComponent<Text>();
        tip_text.text = tip;
        modelDialog.ModelShow();
    }
    public void ChangeRoomPage(int code)
    {
        //更改页数
        if (code > 0)
            SetRoomViewPage(m_roomPageNo + 1);
        else
            SetRoomViewPage(m_roomPageNo - 1);
        //取消选择其它房间项
        for (int i = 0; i < roomItems.Length; i++)
            roomItems[i].Selected = false;
        //更新房间列表
        UpdateRoomView();
    }
    public void RoomItemClicked(int number)
    {
        //取消选择其它房间项
        for (int i = 0; i < roomItems.Length; i++)
        {
            if (i != number)
                roomItems[i].Selected = false;
        }
        if (roomItems[number].Selected)
        {
            //二次被点击
            int roomItemNo = (m_roomPageNo - 1) * m_roomPageSize + number;
            TryJoinRoom(m_roomInfoList[roomItemNo]);
        }
        else
        {
            //首次被点击
            roomItems[number].Selected = true;
        }
    }
    public void RoomFilterChanged()
    {
        //得到过滤后的列表
        FilterRoomList();
        //更新房间列表
        UpdateRoomView();
    }
    public void ChangePlayerInfoButton()
    {
        GameObject modelObj = canvasObj.transform.Find("changeplayerinfo_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        InputField playerid_text = modelObj.transform.Find("model/playerid_text").GetComponent<InputField>();
        InputField playername_input = modelObj.transform.Find("model/playername_input").GetComponent<InputField>();
        DropdownHandler head_dropdown = modelObj.transform.Find("model/head_dropdown").GetComponent<DropdownHandler>();
        playerid_text.text = m_playerId;
        playername_input.text = m_playerName;
        head_dropdown.SelectedIndex = m_playerHeadNo;
        modelDialog.ModelShow((code) =>
        {
            switch (code)
            {
                case "confirm":
                    {
                        string name = playername_input.text;
                        int headNo = (int)head_dropdown.SelectedValue;
                        SendChangePlayerInfo(name, headNo);
                    }
                    break;
            }
        });
    }
    public void LogoutButton()
    {
        SendLogout();
    }
    #endregion

    private void InitTimer()
    {
        m_updateTimerSet["RoomListRequest"] = 0f;
    }
    private void TimerUpdate(float dt)
    {
        IList<string> keys = new List<string>(m_updateTimerSet.Keys);
        foreach (string key in keys)
            m_updateTimerSet[key] += dt;


        if (m_updateTimerSet["RoomListRequest"] > 5f)
        {
            m_updateTimerSet["RoomListRequest"] = 0f;
            this.RequestHallInfo();
        }
    }
    private void InitDropdown()
    {
        {
            DropdownHandler dropdown = canvasObj.transform.Find("roompanel/right/filterpanel/filtergame_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            dropdown.AddItem(null, "所有");
            foreach (GameManager.GameInfo info in GameManager.Instance.GameInfoSet.Values)
                dropdown.AddItem(info, info.GameName);
        }
        {
            DropdownHandler dropdown = canvasObj.transform.Find("createroom_model/model/creategame_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            foreach (GameManager.GameInfo info in GameManager.Instance.GameInfoSet.Values)
                dropdown.AddItem(info, info.GameName);
        }
        {
            DropdownHandler dropdown = canvasObj.transform.Find("changeplayerinfo_model/model/head_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            for (int i = 0; i < GameManager.Instance.PlayerHeads.Length; i++)
            {
                Texture2D texture = GameManager.Instance.PlayerHeads[i];
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                dropdown.AddItem(i, "", sprite);
            }
        }
    }
    private void ClearChat()
    {
        Text chat_text = canvasObj.transform.Find("chatpanel/chat_textarea/Text").GetComponent<Text>();
        chat_text.text = "";
        chat_text.gameObject.GetComponent<ContentSizeFitter>().SetLayoutVertical();
    }
    private void SendLogout()
    {
        GameManager.Instance.PlayerLogout();
    }
    private void LogoutSuccess()
    {
        GameManager.Instance.SetActivity("MainTheme");
    }
    private void SendChat(string chat)
    {
        if (chat != string.Empty)
        {
            //发送消息
            JObject dataJson = new JObject();
            dataJson.Add("Type", "Client_Hall");
            JObject data = new JObject();
            {
                data.Add("Action", "HallChat");
                data.Add("Chat", chat);
            }
            dataJson.Add("Data", data); ;
            GameManager.Instance.SendMessage(dataJson);
        }
    }
    private void SendCreateRoom(string caption, string password)
    {
        JObject createRoomJson = new JObject();
        createRoomJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "CreateRoom");
            data.Add("GameId", "");
            data.Add("Caption", caption);
            data.Add("Password", password);
        }
        createRoomJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(createRoomJson);
    }
    private void ReceiveChat(string sender, string chat)
    {
        //添加一行聊天信息
        Text chat_text = canvasObj.transform.Find("chatpanel/chat_textarea/Text").GetComponent<Text>();
        chat_text.text += string.Format("<color=#A52A2AFF>[{0}] </color>{1}\n", sender, chat);
        chat_text.gameObject.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        //滚动条刷新到最底部
        Scrollbar scrollbar = canvasObj.transform.Find("chatpanel/chat_scroll").GetComponent<Scrollbar>();
        if (scrollbar.value < 0.2f || scrollbar.size > 0.8f)
            scrollbar.value = 0;
    }
    private void RequestPlayerInfo()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "RequestPlayerInfo");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void UpdatePlayerInfo(string playerInfoStr)
    {
        //解析玩家信息JSON
        JObject jsonObj = JObject.Parse(playerInfoStr);
        m_playerId = jsonObj.GetValue("Id").ToString();
        m_playerName = jsonObj.GetValue("Name").ToString();
        m_playerPoint = (int)jsonObj.GetValue("Point");
        m_playerHeadNo = (int)jsonObj.GetValue("HeadNo");
        //更新UI界面
        Image player_photo = canvasObj.transform.Find("infopanel/player_photo").GetComponent<Image>();
        Text playerid_text = canvasObj.transform.Find("infopanel/playerid_text").GetComponent<Text>();
        Text playername_text = canvasObj.transform.Find("infopanel/playername_text").GetComponent<Text>();
        Text playerpoint_text = canvasObj.transform.Find("infopanel/playerpoint_text").GetComponent<Text>();
        Texture2D texture = GameManager.Instance.PlayerHeads[m_playerHeadNo];
        player_photo.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        playerid_text.text = string.Format("ID：{0}", m_playerId);
        playername_text.text = m_playerName;
        playerpoint_text.text = string.Format("点数：{0}", m_playerPoint);
    }
    private void SendChangePlayerInfo(string name, int headNo)
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "ChangePlayerInfo");
            data.Add("Name", name);
            data.Add("HeadNo", headNo);
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void RequestHallInfo()
    {
        //Ping服务器
        GameManager.Instance.Ping((dt) =>
        {
            //更新Ping显示
            Text playercount_text = canvasObj.transform.Find("roompanel/right/statuspanel/ping_text").GetComponent<Text>();
            playercount_text.text = dt < 0 ? "失败" : string.Format("{0}ms", dt);
        });
        //获取大厅信息
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "RequestHallInfo");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void ReceiveHallInfo(string roomListStr, int roomCount, int playerCount)
    {
        //更新玩家数显示
        Text playercount_text = canvasObj.transform.Find("roompanel/right/statuspanel/playercount_text").GetComponent<Text>();
        playercount_text.text = playerCount.ToString();
        //更新房间列表
        UpdateRoomList(roomListStr);
    }
    private void UpdateRoomList(string roomListStr)
    {
        JArray jsonArr = JArray.Parse(roomListStr);
        //得到全部
        m_roomInfoList.Clear();
        foreach (JToken token in jsonArr)
        {
            //转换JSON得到RoomInfo
            JObject jobj = (JObject)token;
            RoomItemInfo roomInfo = new RoomItemInfo();
            roomInfo.RoomId = (string)jobj.GetValue("RoomId");
            roomInfo.GameId = (string)jobj.GetValue("GameId");
            string gameName = "";
            {
                GameManager.GameInfo gameInfo = null;
                GameManager.Instance.GameInfoSet.TryGetValue(roomInfo.GameId, out gameInfo);
                if (gameInfo != null)
                    gameName = gameInfo.GameName;
            }
            roomInfo.GameName = gameName;
            roomInfo.Caption = (string)jobj.GetValue("RoomCaption");
            roomInfo.HasPassword = (bool)jobj.GetValue("HasPassword");
            roomInfo.Status = (int)jobj.GetValue("RoomStatus");
            roomInfo.Count = (int)jobj.GetValue("PlayerCount");
            roomInfo.MaxCount = (int)jobj.GetValue("MaxPlayerCount");
            m_roomInfoList.Add(roomInfo);
        }
        //得到过滤后的列表
        FilterRoomList();
        //更新房间列表
        UpdateRoomView();
    }
    private void FilterRoomList()
    {
        m_pageRoomInfoList.Clear();
        {
            bool showPassword = canvasObj.transform.Find("roompanel/right/filterpanel/check1").GetComponent<Toggle>().isOn;
            bool showFull = canvasObj.transform.Find("roompanel/right/filterpanel/check2").GetComponent<Toggle>().isOn;
            bool showPlaying = canvasObj.transform.Find("roompanel/right/filterpanel/check3").GetComponent<Toggle>().isOn;
            string gameId = "";
            {
                DropdownHandler filtergame_dropdown = canvasObj.transform.Find("roompanel/right/filterpanel/filtergame_dropdown").GetComponent<DropdownHandler>();
                GameManager.GameInfo gameInfo = (GameManager.GameInfo)filtergame_dropdown.SelectedValue;
                if (gameInfo != null)
                    gameId = gameInfo.GameId;
            }

            foreach (RoomItemInfo roomInfo in m_roomInfoList)
            {
                if (!showPassword)
                {
                    if (roomInfo.HasPassword)
                        continue;
                }
                if (!showFull)
                {
                    if (roomInfo.Status == 2)
                        continue;
                }
                if (!showPlaying)
                {
                    if (roomInfo.Status == 3)
                        continue;
                }
                if (!string.IsNullOrEmpty(gameId))
                {
                    if (!roomInfo.GameId.Equals(gameId))
                        continue;
                }
                m_pageRoomInfoList.Add(roomInfo);
            }
        }
    }
    private void UpdateRoomView()
    {
        //更新房间列表的显示
        for (int i = 0; i < roomItems.Length; i++)
        {
            RoomItem roomItem = roomItems[i];
            roomItem.SetVisiblity(false);

            if (i < m_pageRoomInfoList.Count)
            {
                RoomItemInfo roomItemInfo = m_pageRoomInfoList[i];
                if (roomItemInfo != null)
                {
                    roomItem.SetVisiblity(true);
                    roomItem.SetRoomInfo(roomItemInfo.GameName, roomItemInfo.Caption, roomItemInfo.HasPassword, roomItemInfo.Status, roomItemInfo.Count, roomItemInfo.MaxCount);
                }
            }
        }
    }
    private void SetRoomViewPage(int pageNo)
    {
        //检查页数是否超出范围
        bool check = (pageNo - 1) * m_roomPageSize < m_pageRoomInfoList.Count && pageNo > 0;
        //设置当前房间页数
        if (check)
            m_roomPageNo = pageNo;
    }
    private void TryJoinRoom(RoomItemInfo info)
    {
    }
    private IEnumerator DoAction_Delay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    #region 工具类
    private class RoomItemInfo
    {
        public string RoomId { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string Caption { get; set; }
        public bool HasPassword { get; set; }
        public int Status { get; set; }
        public int Count { get; set; }
        public int MaxCount { get; set; }
    }
    #endregion
}
