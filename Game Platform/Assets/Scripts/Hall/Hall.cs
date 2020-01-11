using Newtonsoft.Json.Linq;
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
    private RoomItemInfo[] m_pageRoomInfos = new RoomItemInfo[6];  //HARDCODE 一页显示六个房间
    private int m_roomPageSize = 6;  //HARDCODE 一页显示六个房间
    private int m_roomPageNo = 1;

    #region unity触发器
    private void Awake()
    {
        //添加计时器项目
        m_updateTimerSet["RoomListRequest"] = 0f;
        //添加下拉框选项
        InitDrapdown();
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        IList<string> keys = new List<string>(m_updateTimerSet.Keys);
        foreach (string key in keys)
            m_updateTimerSet[key] += dt;


        if (m_updateTimerSet["RoomListRequest"] > 5f)
        {
            m_updateTimerSet["RoomListRequest"] = 0f;
            this.RequestRoomList();
        }

    }
    #endregion

    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
        ClearChat();
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
                    case "HallChat":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.ReceiveChat(content.GetValue("Sender").ToString(), content.GetValue("Chat").ToString());
                        }
                        break;
                    case "Tip":
                        {
                            string content = data.GetValue("Content").ToString();
                            this.TipModel("show", content);
                        }
                        break;
                    case "ResponseRoomList":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.ReceiveRoomList((string)content.GetValue("RoomList"));
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
    public void CreateRoomModel(string code)
    {
        GameObject modelObj = canvasObj.transform.Find("createroom_model").gameObject;
        switch (code)
        {
            case "show":
                modelObj.SetActive(true);
                break;
            case "confirm":
                {
                    string caption = modelObj.transform.Find("model/caption_input").GetComponent<InputField>().text;
                    string password = modelObj.transform.Find("model/password_input").GetComponent<InputField>().text;
                    SendCreateRoom(caption, password);
                    modelObj.SetActive(false);
                }
                break;
            case "cancel":
                modelObj.SetActive(false);
                break;
        }
    }
    public void TipModel(string code)
    {
        GameObject tip_model = canvasObj.transform.Find("tip_model").gameObject;
        Text tip_text = tip_model.transform.Find("model/tip_text").GetComponent<Text>();
        switch (code)
        {
            case "confirm":
                {
                    tip_model.SetActive(false);
                }
                break;
        }
    }
    public void TipModel(string code, string tip)
    {
        GameObject tip_model = canvasObj.transform.Find("tip_model").gameObject;
        Text tip_text = tip_model.transform.Find("model/tip_text").GetComponent<Text>();
        switch (code)
        {
            case "show":
                {
                    tip_text.text = tip;
                    tip_model.SetActive(true);
                }
                break;
        }
    }
    public void ChangeRoomPage(int code)
    {
        if (code > 0)
            SetRoomViewPage(m_roomPageNo + 1);
        else
            SetRoomViewPage(m_roomPageNo - 1);
    }
    #endregion

    private void InitDrapdown()
    {
        {
            DropdownHandler dropdown = canvasObj.transform.Find("roompanel/right/filterpanel/filtergame_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            dropdown.AddItem<object>("所有", null);
            foreach (GameManager.GameInfo info in GameManager.Instance.GameInfos.Values)
                dropdown.AddItem<GameManager.GameInfo>(info.GameName, info);
        }
        {
            DropdownHandler dropdown = canvasObj.transform.Find("createroom_model/model/creategame_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            foreach (GameManager.GameInfo info in GameManager.Instance.GameInfos.Values)
                dropdown.AddItem<GameManager.GameInfo>(info.GameName, info);
        }
    }
    private void ClearChat()
    {
        Text chat_text = canvasObj.transform.Find("chatpanel/chat_textarea/Text").GetComponent<Text>();
        chat_text.text = "";
        chat_text.gameObject.GetComponent<ContentSizeFitter>().SetLayoutVertical();
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
    private void RequestRoomList()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "RequestRoomList");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void ReceiveRoomList(string roomListStr)
    {
        JArray jsonArr = JArray.Parse(roomListStr);
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
                GameManager.Instance.GameInfos.TryGetValue(roomInfo.GameId, out gameInfo);
                if (gameInfo != null)
                    gameName = gameInfo.GameName;
            }
            roomInfo.GameName = gameName;
            roomInfo.Caption = (string)jobj.GetValue("RoomCaption");
            roomInfo.HasPassword = (bool)jobj.GetValue("HasPassword");
            roomInfo.Status = (int)jobj.GetValue("RoomStatus");
            roomInfo.Count = (int)jobj.GetValue("PlayerCount");
            roomInfo.MaxCount = (int)jobj.GetValue("MaxPlayerCount");
            //应用过滤条件

            //添加项
            m_roomInfoList.Add(roomInfo);
        }
        UpdateRoomView();
    }
    private void SetRoomViewPage(int pageNo)
    {
        //检查页数是否超出范围
        bool check = (pageNo - 1) * m_roomPageSize < m_roomInfoList.Count && pageNo > 0;
        //设置当前房间页数
        if (check)
        {
            m_roomPageNo = pageNo;
            UpdateRoomView();
        }      
    }
    private void UpdateRoomView()
    {
        //刷新m_pageRooms
        int startNo = (m_roomPageNo - 1) * m_roomPageSize;
        m_pageRoomInfos = new RoomItemInfo[m_roomPageSize];
        for (int i = startNo; i < startNo + m_roomPageSize; i++)
        {
            if (i < 0 || i >= m_roomInfoList.Count)
                continue;
            m_pageRoomInfos[i] = m_roomInfoList[i];
        }
        //更新房间列表的显示
        for (int i = 0; i < roomItems.Length; i++)
        {
            RoomItem roomItem = roomItems[i];
            roomItem.SetVisiblity(false);

            if (i < m_roomInfoList.Count)
            {
                RoomItemInfo roomItemInfo = m_roomInfoList[i];
                if (roomItemInfo != null)
                {
                    roomItem.SetVisiblity(true);
                    roomItem.SetRoomInfo(roomItemInfo.GameName, roomItemInfo.Caption, roomItemInfo.HasPassword, roomItemInfo.Status, roomItemInfo.Count, roomItemInfo.MaxCount);
                }
            }
        }
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
