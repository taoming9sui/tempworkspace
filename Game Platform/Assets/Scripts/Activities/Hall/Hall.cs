using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;


public class Hall : GameActivity
{
    public GameObject cameraObj;
    public GameObject panelObj;
    public GameObject sceneObj;
    public GameObject tipModelObj;
    public AudioPlayer audioPlayer;
    public AudioMixer audioMixer;
    public GameObject[] roomItemObjs;
    public LocalizationDictionary localDic;

    private IDictionary<string, float> m_updateTimerSet = new Dictionary<string, float>();
    private IList<RoomItemInfo> m_roomInfoList = new List<RoomItemInfo>();
    private IList<RoomItemInfo> m_pageRoomInfoList = new List<RoomItemInfo>();
    private int m_roomPageSize = 0;
    private int m_roomPageNo = 1;
    private int m_selectedRoomItemIdx = -1;
    private string m_playerId = "";
    private string m_playerName = "";
    private int m_playerHeadNo = 0;
    private int m_playerPoint = 0;

    #region unity触发器
    private void Awake()
    {
        //初始化信息列表
        m_roomPageSize = roomItemObjs.Length;
        //初始化计时器项目
        InitTimer();
    }
    private void Start()
    {
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
        //请求房间列表
        RequestHallInfo();
        //请求玩家信息
        RequestPlayerInfo();
        //播放bgm
        audioPlayer.PlayBGM("HallBGM1");
    }
    public override void OnDisconnect()
    {
        DisconnectModel();
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
                            string resultCode = data.GetValue("Content").ToString();
                            this.ReceiveResultTip(resultCode);
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
                            this.ReceivePlayerInfo(content.ToString());
                        }
                        break;
                    case "ChangePlayerInfoSuccess":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.ReceivePlayerInfo(content.ToString());
                        }
                        break;
                    case "InRoom":
                        {
                            JObject content = (JObject)data.GetValue("Content");
                            this.JoinRoomSuccess((string)content.GetValue("GameId"), (string)content.GetValue("RoomId"));
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
        InputField chat_input = panelObj.transform.Find("chatpanel/chat_input").GetComponent<InputField>();
        //获取输入框消息并清空
        string chat = chat_input.text;
        chat_input.text = "";
        SendChat(chat);
    }
    public void SendChatEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            InputField chat_input = panelObj.transform.Find("chatpanel/chat_input").GetComponent<InputField>();
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
        GameObject modelObj = panelObj.transform.Find("createroom_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        modelDialog.ModelShow((result) =>
        {
            switch (result)
            {
                case ModelDialog.ModelResult.Confirm:
                    {
                        string caption = modelObj.transform.Find("model/caption_input").GetComponent<InputField>().text;
                        string password = modelObj.transform.Find("model/password_input").GetComponent<InputField>().text;
                        SendCreateRoom(caption, password);
                    }
                    break;
            }
        });
    }
    public void DisconnectModel()
    {
        GameObject modelObj = panelObj.transform.Find("disconnect_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        modelDialog.ModelShow((result) =>
        {
            switch (result)
            {
                case ModelDialog.ModelResult.Confirm:
                    {
                        TryConnectAndLogin();
                    }
                    break;
                case ModelDialog.ModelResult.Cancel:
                    {
                        GameObject prefab = ResourceManager.Instance.Local.ActivityInfoSet["MainTheme"].ActivityPrefab;
                        GameManager.Instance.SetActivity(prefab);
                    }
                    break;
            }
        });
    }
    public void TipModel(string tipText)
    {
        //1对话框对象克隆
        GameObject modelObj = GameObject.Instantiate(tipModelObj, panelObj.transform);
        //2显示该克隆对象
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        Text tip_text = modelObj.transform.Find("model/tip_text").GetComponent<Text>();
        tip_text.text = tipText;
        //3确定后移除该克隆
        modelDialog.ModelShow((code) =>
        {
            Destroy(modelObj);
        });
    }
    public void ChangeRoomPage(int code)
    {
        //更改页数
        if (code > 0)
            SetRoomViewPage(m_roomPageNo + 1);
        else
            SetRoomViewPage(m_roomPageNo - 1);
        //取消选择其它房间项
        m_selectedRoomItemIdx = -1;
        for (int i = 0; i < roomItemObjs.Length; i++)
            roomItemObjs[i].GetComponent<Toggle>().isOn = false;
        //更新房间列表
        UpdateRoomItemUI();
    }
    public void RoomItemToggle(Toggle toggle)
    {
        if (toggle.isOn)
        {
            int no = toggle.GetComponent<CustomValue>().intValue;
            if (m_selectedRoomItemIdx != no)
            {
                m_selectedRoomItemIdx = no;
            }
            else
            {
                //二次被点击
                int roomItemNo = (m_roomPageNo - 1) * m_roomPageSize + no;
                TryJoinRoom(m_roomInfoList[roomItemNo]);
            }
        }
    }
    public void RoomFilterChanged()
    {
        //得到过滤后的列表
        FilterRoomList();
        //更新房间列表
        UpdateRoomItemUI();
    }
    public void ChangePlayerInfoButton()
    {
        GameObject modelObj = panelObj.transform.Find("changeplayerinfo_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        InputField playerid_text = modelObj.transform.Find("model/playerid_text").GetComponent<InputField>();
        InputField playername_input = modelObj.transform.Find("model/playername_input").GetComponent<InputField>();
        DropdownHandler head_dropdown = modelObj.transform.Find("model/head_dropdown").GetComponent<DropdownHandler>();
        playerid_text.text = m_playerId;
        playername_input.text = m_playerName;
        head_dropdown.SelectedIndex = m_playerHeadNo;
        modelDialog.ModelShow((result) =>
        {
            switch (result)
            {
                case ModelDialog.ModelResult.Confirm:
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
    public void SetMasterVol(Slider slider)
    {
        float decibel = 20 * Mathf.Log10(slider.value);
        if (decibel < -39)
            audioMixer.SetFloat("MasterVolume", -80);
        else
            audioMixer.SetFloat("MasterVolume", decibel);
    }
    public void SetBGMVol(Slider slider)
    {
        float decibel = 20 * Mathf.Log10(slider.value);
        if (decibel < -39)
            audioMixer.SetFloat("BGMVolume", -80);
        else
            audioMixer.SetFloat("BGMVolume", decibel);
    }
    public void SetSoundVol(Slider slider)
    {
        float decibel = 20 * Mathf.Log10(slider.value);
        if (decibel < -39)
            audioMixer.SetFloat("SoundVolume", -80);
        else
            audioMixer.SetFloat("SoundVolume", decibel);
    }
    public void ConfigButton()
    {
        GameObject modelObj = panelObj.transform.Find("config_model").gameObject;
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        Slider masterSlider = modelObj.transform.Find("model/master_slider").GetComponent<Slider>();
        Slider bgmSlider = modelObj.transform.Find("model/bgm_slider").GetComponent<Slider>();
        Slider soundSlider = modelObj.transform.Find("model/sound_slider").GetComponent<Slider>();
        // 获取当前音量
        audioMixer.GetFloat("MasterVolume", out float masterVol);
        audioMixer.GetFloat("BGMVolume", out float bgmVol);
        audioMixer.GetFloat("SoundVolume", out float soundVol);
        masterSlider.value = Mathf.Pow(10, masterVol / 20);
        bgmSlider.value = Mathf.Pow(10, bgmVol / 20);
        soundSlider.value = Mathf.Pow(10, soundVol / 20);
        modelDialog.ModelShow();
    }
    #endregion

    #region UI更新脚本
    private void ClearChat()
    {
        Text chat_text = panelObj.transform.Find("chatpanel/chat_textarea/Text").GetComponent<Text>();
        chat_text.text = "";
        chat_text.gameObject.GetComponent<ContentSizeFitter>().SetLayoutVertical();
    }
    private void AddChat(string sender, string chat)
    {
        //添加一行聊天信息
        Text chat_text = panelObj.transform.Find("chatpanel/chat_textarea/Text").GetComponent<Text>();
        chat_text.text += string.Format("<color=#A52A2AFF>[{0}] </color>{1}\n", sender, chat);
        //滚动条刷新到最底部
        chat_text.gameObject.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        Scrollbar scrollbar = panelObj.transform.Find("chatpanel/chat_scroll").GetComponent<Scrollbar>();
        StartCoroutine(DoAction_Delay(() =>
        {
            if (scrollbar.value < 0.2f || scrollbar.size > 0.5f)
                scrollbar.value = 0;
        }, 0.1f));
    }
    private void UpdateRoomItemUI()
    {
        //更新房间列表的显示

        for (int i = 0; i < roomItemObjs.Length; i++)
        {
            GameObject roomItemObj = roomItemObjs[i];
            roomItemObj.SetActive(false);
            if (i < m_pageRoomInfoList.Count)
            {
                RoomItemInfo roomItemInfo = m_pageRoomInfoList[i];
                if (roomItemInfo != null)
                {
                    roomItemObj.SetActive(true);

                    Text game_text = roomItemObj.transform.Find("game_image/Text").GetComponent<Text>();
                    Text status_text = roomItemObj.transform.Find("status_text").GetComponent<Text>();
                    Text count_text = roomItemObj.transform.Find("count_image/Text").GetComponent<Text>();
                    Text caption_text = roomItemObj.transform.Find("caption_image/Text").GetComponent<Text>();

                    game_text.text = roomItemInfo.GameName;
                    {
                        string p1 = "";
                        switch (roomItemInfo.Status)
                        {
                            case 1:
                                p1 = string.Format("<color=#7CFC00FF>{0}</color>",
                                    localDic.GetLocalText("text.hall.word_joinable"));
                                break;
                            case 2:
                                p1 = string.Format("<color=#B22222FF>{0}</color>",
                                    localDic.GetLocalText("text.hall.word_full"));
                                break;
                            case 3:
                                p1 = string.Format("<color=#B22222FF>游戏中</color>",
                                    localDic.GetLocalText("text.hall.word_playing"));
                                break;
                        }
                        string p2 = roomItemInfo.HasPassword ? localDic.GetLocalText("text.hall.word_password") : "";
                        status_text.text = string.Format("{0}  <color=#FFD700FF>{1}</color>", p1, p2);
                    }
                    count_text.text = roomItemInfo.Count + " / " + roomItemInfo.MaxCount;
                    caption_text.text = roomItemInfo.Caption;
                }
            }
        }
    }
    private void UpdateHallInfoUI(int playerCount)
    {
        Text playercount_text = panelObj.transform.Find("roompanel/right/statuspanel/playercount_text").GetComponent<Text>();
        playercount_text.text = playerCount.ToString();
    }
    private void UpdatePlayerInfoUI(string playerInfoStr)
    {
        //解析玩家信息JSON
        JObject jsonObj = JObject.Parse(playerInfoStr);
        m_playerId = jsonObj.GetValue("Id").ToString();
        m_playerName = jsonObj.GetValue("Name").ToString();
        m_playerPoint = (int)jsonObj.GetValue("Point");
        m_playerHeadNo = (int)jsonObj.GetValue("HeadNo");
        //更新UI界面
        Image player_photo = panelObj.transform.Find("infopanel/player_photo").GetComponent<Image>();
        Text playerid_text = panelObj.transform.Find("infopanel/playerid_text").GetComponent<Text>();
        Text playername_text = panelObj.transform.Find("infopanel/playername_text").GetComponent<Text>();
        Text playerpoint_text = panelObj.transform.Find("infopanel/playerpoint_text").GetComponent<Text>();
        Texture2D texture = ResourceManager.Instance.Local.PlayerHeadTextures[m_playerHeadNo];
        player_photo.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        playerid_text.text = m_playerId;
        playername_text.text = m_playerName;
        playerpoint_text.text = m_playerPoint.ToString();
    }
    private void UpdatePingUI(int ms)
    {
        //更新Ping显示
        Text playercount_text = panelObj.transform.Find("roompanel/right/statuspanel/ping_text").GetComponent<Text>();
        playercount_text.text = ms < 0 ? localDic.GetLocalText("text.hall.ping_fail") : string.Format("{0}ms", ms);
    }
    #endregion

    private void InitDropdown()
    {
        {
            DropdownHandler dropdown = panelObj.transform.Find("roompanel/right/filterpanel/filtergame_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            string allItemText = localDic.GetLocalText("text.hall.dropdown_all");
            dropdown.AddItem(null, allItemText);
            foreach (ResourceManager.GameInfo info in ResourceManager.Instance.Local.GameInfoSet.Values)
                dropdown.AddItem(info, info.GameName);
        }
        {
            DropdownHandler dropdown = panelObj.transform.Find("createroom_model/model/creategame_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            foreach (ResourceManager.GameInfo info in ResourceManager.Instance.Local.GameInfoSet.Values)
                dropdown.AddItem(info, info.GameName);
        }
        {
            DropdownHandler dropdown = panelObj.transform.Find("changeplayerinfo_model/model/head_dropdown").GetComponent<DropdownHandler>();
            dropdown.ClearItems();
            for (int i = 0; i < ResourceManager.Instance.Local.PlayerHeadTextures.Length; i++)
            {
                Texture2D texture = ResourceManager.Instance.Local.PlayerHeadTextures[i];
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                dropdown.AddItem(i, "", sprite);
            }
        }
    }
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
                ResourceManager.GameInfo gameInfo = null;
                ResourceManager.Instance.Local.GameInfoSet.TryGetValue(roomInfo.GameId, out gameInfo);
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
        UpdateRoomItemUI();
    }
    private void FilterRoomList()
    {
        m_pageRoomInfoList.Clear();
        {
            bool showPassword = panelObj.transform.Find("roompanel/right/filterpanel/check1").GetComponent<Toggle>().isOn;
            bool showFull = panelObj.transform.Find("roompanel/right/filterpanel/check2").GetComponent<Toggle>().isOn;
            bool showPlaying = panelObj.transform.Find("roompanel/right/filterpanel/check3").GetComponent<Toggle>().isOn;
            string gameId = "";
            {
                DropdownHandler filtergame_dropdown = panelObj.transform.Find("roompanel/right/filterpanel/filtergame_dropdown").GetComponent<DropdownHandler>();
                ResourceManager.GameInfo gameInfo = (ResourceManager.GameInfo)filtergame_dropdown.SelectedValue;
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
        if (info.HasPassword)
        {
            //房间需要密码 显示对话框
            GameObject modelObj = panelObj.transform.Find("joinroom_model").gameObject;
            ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
            InputField caption_text = modelObj.transform.Find("model/caption_text").GetComponent<InputField>();
            InputField game_text = modelObj.transform.Find("model/game_text").GetComponent<InputField>();
            InputField password_input = modelObj.transform.Find("model/password_input").GetComponent<InputField>();
            caption_text.text = info.Caption;
            game_text.text = info.GameName;
            password_input.text = "";
            modelDialog.ModelShow((result) =>
            {
                switch (result)
                {
                    case ModelDialog.ModelResult.Confirm:
                        {
                            string password = password_input.text;
                            SendJoinRoom(info, password);
                        }
                        break;
                }
            });
        }
        else
        {
            //房间不需要密码
            SendJoinRoom(info, "");
        }
    }
    private void TryConnectAndLogin()
    {
        GameManager.Instance.SocketConnect(()=> {
            GameManager.Instance.PlayerLogin();
        });      
    }

    #region 客户端接口和响应
    private void SendLogout()
    {
        GameManager.Instance.PlayerLogout();
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
    private void SendJoinRoom(RoomItemInfo info, string password)
    {
        JObject joinRoomJson = new JObject();
        joinRoomJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "JoinRoom");
            data.Add("RoomId", info.RoomId);
            data.Add("Password", password);
        }
        joinRoomJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(joinRoomJson);
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
    private void RequestHallInfo()
    {
        //Ping服务器
        GameManager.Instance.Ping((dt) =>
        {
            UpdatePingUI(dt);
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

    private void ReceiveChat(string sender, string chat)
    {
        AddChat(sender, chat);
    }
    private void ReceiveHallInfo(string roomListStr, int roomCount, int playerCount)
    {
        //更新大厅UI显示
        UpdateHallInfoUI(playerCount);
        //更新房间列表
        UpdateRoomList(roomListStr);
    }
    private void ReceivePlayerInfo(string playerInfoStr)
    {
        UpdatePlayerInfoUI(playerInfoStr);
    }
    private void ReceiveResultTip(string resultCode)
    {
        string tipText = localDic.GetLocalText(resultCode);
        TipModel(tipText);
    }
    private void JoinRoomSuccess(string gameId, string roomId)
    {
        //加入成功 跳转游戏活动
        ResourceManager.GameInfo gameinfo = ResourceManager.Instance.Local.GameInfoSet[gameId];
        GameObject prefab = ResourceManager.Instance.Local.ActivityInfoSet[gameinfo.ActivityId].ActivityPrefab;
        GameManager.Instance.SetActivity(prefab);
    }
    private void LogoutSuccess()
    {
        GameObject prefab = ResourceManager.Instance.Local.ActivityInfoSet["MainTheme"].ActivityPrefab;
        GameManager.Instance.SetActivity(prefab);
    }
    #endregion


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
