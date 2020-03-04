using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wolfman_P8 : GameActivity
{
    public GameObject cameraObj;
    public GameObject panelObj;
    public GameObject sceneObj;
    public AudioPlayer audioPlayer;
    public PlayerSeat[] playerSeats;

    #region 服务端同步数据
    //游戏全局状态
    private class PlayerSeatInfo
    {
        public int SeatNo = 0;
        public bool HasPlayer = false;
        public string PlayerName = "";
        public int HeadNo = 0;
        public bool isDead = false;
        public bool Connected = false;
        public bool isSpeaking = false;
        public bool isReady = false;
    }
    private PlayerSeatInfo[] m_playerSeatInfos = new PlayerSeatInfo[8];
    private bool m_isPlaying = false;
    private string m_publicProcess = "Default";
    private string m_gameloopProcess = "Default";
    private int m_dayTime = 0;
    private int m_dayNumber = 0;
    //玩家状态
    private string m_playerId = "";
    private string m_playerName = "";
    private int m_playerHeadNo = 0;
    private int m_seatNo = 0;
    private bool m_isReady = false;
    private bool m_isSpeaking = false;
    //狼人杀角色状态
    private GameIdentity m_identity = null;
    private class GameIdentity
    {
        public string IdentityType = "Default";
        public string CurrentAction = "";
        public bool isDead = false;
        public int GameCamp = 0;
    }
    private class Villager : GameIdentity
    {
        public Villager()
        {
            IdentityType = "Villager";
        }
    }
    private class Wolfman : GameIdentity
    {
        public Wolfman()
        {
            IdentityType = "Wolfman";
        }
    }
    private class Prophet : GameIdentity
    {
        public Prophet()
        {
            IdentityType = "Prophet";
        }
    }
    private class Hunter : GameIdentity
    {
        public Hunter()
        {
            IdentityType = "Hunter";
        }
    }
    private class Defender : GameIdentity
    {
        public int LastDefendNo = -1;
        public Defender()
        {
            IdentityType = "Defender";
        }
    }
    private class Witch : GameIdentity
    {
        public bool Poison = true;
        public bool Antidote = true;
        public Witch()
        {
            IdentityType = "Witch";
        }
    }
    #endregion

    #region 客户端本机数据
    private string m_identitySelect = "Random";
    #endregion

    #region unity触发器
    private void Awake()
    {
        InitPlayerInfos();
    }
    private void Update()
    {
    }
    #endregion

    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
        //从服务器获取状态同步
        SynchronizeGameCommand();
        //清空记录框
        {
            ClearGameLog();
        }
    }
    public override void OnDisconnect()
    {
    }
    public override void OnConnect()
    {
    }
    public override void OnMessage(JObject jsonData)
    {
        try
        {
            string type = jsonData.GetValue("Type").ToString();
            if (type == "Server_Room")
            {
                JObject data = (JObject)jsonData.GetValue("Data");
                string action = data.GetValue("Action").ToString();
                switch (action)
                {
                    case "ResponseGameStatus":
                        SynchronizeGameResponse((JObject)data.GetValue("Content"));
                        break;
                    case "SeatChange":
                        ReceiveSeatChange((JArray)data.GetValue("Content"));
                        break;
                    case "GameTip":
                        ReceiveGameTip((string)data.GetValue("Tip"));
                        break;
                    case "ReadyResponse":
                        ReadyResponse((bool)data.GetValue("Ready"));
                        break;
                    case "StartPrepare":
                        ReceiveStartPrepare();
                        break;
                    case "GameStart":
                        ReceiveGameStart();
                        break;
                    case "DistributeIdentity":
                        ReceiveDistributeIdentity((JObject)data.GetValue("Identity"));
                        break;
                }
            }
            else if (type == "Server_Hall")
            {
                JObject data = (JObject)jsonData.GetValue("Data");
                string action = data.GetValue("Action").ToString();
                switch (action)
                {
                    case "OutRoom":
                        ExitGameResponse();
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

    #region 初始化
    private void InitPlayerInfos()
    {
        for (int i = 0; i < m_playerSeatInfos.Length; i++)
        {
            PlayerSeatInfo info = new PlayerSeatInfo();
            info.SeatNo = i;
            m_playerSeatInfos[i] = info;
        }
    }
    #endregion

    #region UI交互脚本
    public void ExitButton()
    {
        ExitGameCommand();
    }
    public void GetReadyButton()
    {
        ReadyCommand(true);
    }
    public void CancelReadyButton()
    {
        ReadyCommand(false);
    }
    public void IdentitySelectToggle(Toggle toggle)
    {
        if (toggle.isOn)
        {          
            CustomValue valueObj = toggle.gameObject.GetComponent<CustomValue>();
            string idExpection = valueObj.stringValue;
            m_identitySelect = idExpection;
        }
    }
    #endregion

    #region UI界面更新
    private void UpdatePlayerSeat()
    {
        foreach (PlayerSeatInfo info in m_playerSeatInfos)
        {
            PlayerSeat seat = playerSeats[info.SeatNo];
            if (info.HasPlayer)
            {
                Texture2D texture = ResourceManager.Instance.PlayerHeadTextures[info.HeadNo];
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                seat.SetPlayer(info.SeatNo, sprite, info.PlayerName);
                seat.SetStasusMark(PlayerSeat.StasusMark.Ready, info.isReady);
            }
            else
            {
                playerSeats[info.SeatNo].SetPlayer(info.SeatNo);
                seat.SetStasusMark(PlayerSeat.StasusMark.Ready, info.isReady);
            }
        }
    }
    private void UpdateBottomGUI()
    {
        //1准备界面
        {
            bool flag = m_publicProcess == "PlayerReady";
            GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
            getready_panel.SetActive(flag);
            if (flag)
            {
                GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
                GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
                getready_button.SetActive(!m_isReady);
                cancelready_button.SetActive(m_isReady);
            }
        }
        //2开始预备界面
        {
            bool flag = m_publicProcess == "StartPrepare";
            GameObject startprepare_panel = panelObj.transform.Find("bottom/startprepare_panel").gameObject;
            startprepare_panel.SetActive(flag);
            if (flag)
            {
                Text text_text = startprepare_panel.transform.Find("info_text").GetComponent<Text>();
                string name = GetIdentityName(m_identitySelect);
                if (string.IsNullOrEmpty(name))
                    name = "随机";
                text_text.text = string.Format("游戏即将开始！你的期望身份是<color=#FFFF66>【{0}】</color>", name);
            }
        }
        //3游戏控制界面
        {
            bool flag = m_publicProcess == "GameLoop";
            GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
            gamecontrol_panel.SetActive(flag);
            if (flag)
            {
                //3.1根据身份信息更新身份面板显示
                GameObject villager_panel = gamecontrol_panel.transform.Find("identity_panel/villager_panel").gameObject;
                GameObject wolfman_panel = gamecontrol_panel.transform.Find("identity_panel/wolfman_panel").gameObject;
                GameObject prophet_panel = gamecontrol_panel.transform.Find("identity_panel/prophet_panel").gameObject;
                GameObject hunter_panel = gamecontrol_panel.transform.Find("identity_panel/hunter_panel").gameObject;
                GameObject defender_panel = gamecontrol_panel.transform.Find("identity_panel/defender_panel").gameObject;
                GameObject witch_panel = gamecontrol_panel.transform.Find("identity_panel/witch_panel").gameObject;
                if (m_identity != null)
                {
                    string idType = m_identity.IdentityType;
                    villager_panel.SetActive(idType == "Villager");
                    wolfman_panel.SetActive(idType == "Wolfman");
                    prophet_panel.SetActive(idType == "Prophet");
                    hunter_panel.SetActive(idType == "Hunter");
                    defender_panel.SetActive(idType == "Defender");
                    witch_panel.SetActive(idType == "Witch");
                    GameObject currentPanel = null;
                    switch (idType)
                    {
                        case "Villager":
                            currentPanel = villager_panel;                          
                            break;
                        case "Wolfman":
                            currentPanel = wolfman_panel;                         
                            break;
                        case "Prophet":
                            currentPanel = prophet_panel;                      
                            break;
                        case "Hunter":
                            currentPanel = hunter_panel;                           
                            break;
                        case "Defender":
                            currentPanel = defender_panel;
                            break;
                        case "Witch":
                            currentPanel = witch_panel;
                            break;
                    }
                    if(currentPanel != null)
                    {
                        Text seatno_text = currentPanel.transform.Find("seatno_text").GetComponent<Text>();
                        seatno_text.text = (m_seatNo + 1).ToString();
                    }
                }
            }
        }
    }
    private void ClearGameLog()
    {
        MessageContainer message_container = panelObj.transform.Find("middle/message_container").GetComponent<MessageContainer>();
        message_container.ClearMessage();
    }
    private void AddGameLog(JObject logContent)
    {
        MessageContainer message_container = panelObj.transform.Find("middle/message_container").GetComponent<MessageContainer>();
        string logType = (string)logContent.GetValue("LogType");
        switch (logType)
        {
            case "Tip":
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string txt = (string)logContent.GetValue("Text");
                    builder.AppendLine("<color=#FF6633>【提示】</color>");
                    builder.Append(txt);
                    message_container.AddMessage(builder.ToString());
                }
                break;
            case "Judge":
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string txt = (string)logContent.GetValue("Text");
                    builder.AppendLine("<color=#660000>【法官】</color>");
                    builder.Append(txt);
                    message_container.AddMessage(builder.ToString());
                }
                break;
        }
    }
    private void UpdateVoiceState()
    {

    }
    #endregion

    #region 命令和响应
    private void SynchronizeGameCommand()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "SynchronizeGameCommand");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void SynchronizeGameResponse(JObject content)
    {
        //1同步座位列表
        {
            JArray playerSeatArray = (JArray)content.GetValue("PlayerSeatArray");
            for (int i = 0; i < playerSeatArray.Count; i++)
            {
                //解析JSON
                JObject jobj = (JObject)playerSeatArray[i];
                int seatNo = (int)jobj.GetValue("SeatNo");
                bool hasPlayer = (bool)jobj.GetValue("HasPlayer");
                string name = (string)jobj.GetValue("Name");
                int headNo = (int)jobj.GetValue("HeadNo");
                bool connected = (bool)jobj.GetValue("Connected");
                bool isReady = (bool)jobj.GetValue("IsReady");
                bool isDead = (bool)jobj.GetValue("IsDead");
                bool isSpeaking = (bool)jobj.GetValue("IsSpeaking");
                //更新数据
                PlayerSeatInfo info = m_playerSeatInfos[seatNo];
                info.HasPlayer = hasPlayer;
                info.PlayerName = name;
                info.HeadNo = headNo;
                info.Connected = connected;
                info.isReady = isReady;
                info.isDead = isDead;
                info.isSpeaking = isSpeaking;
            }
        }
        //2同步游戏变量
        {
            //解析JSON
            JObject gameProperty = (JObject)content.GetValue("GameProperty");
            m_isPlaying = (bool)gameProperty.GetValue("IsPlaying");
            m_publicProcess = (string)gameProperty.GetValue("PublicProcess");
            m_gameloopProcess = (string)gameProperty.GetValue("GameloopProcess");
            m_dayTime = (int)gameProperty.GetValue("DayTime");
            m_dayNumber = (int)gameProperty.GetValue("DayNumber");
            //系统提示信息
            if (!m_isPlaying)
            {
                JObject logContent = new JObject();
                logContent.Add("LogType", "Tip");
                logContent.Add("Text", "当房间满员，且所有玩家已准备时，游戏将自动开始");
                AddGameLog(logContent);
            }
        }
        //3同步玩家变量
        {
            //解析JSON
            JObject playerProperty = (JObject)content.GetValue("PlayerProperty");
            m_playerId = (string)playerProperty.GetValue("PlayerId");
            m_playerName = (string)playerProperty.GetValue("PlayerName");
            m_playerHeadNo = (int)playerProperty.GetValue("PlayerHeadNo");
            m_seatNo = (int)playerProperty.GetValue("SeatNo");
            m_isReady = (bool)playerProperty.GetValue("IsReady");
            m_isSpeaking = (bool)playerProperty.GetValue("IsSpeaking");
            JToken identityJObj = playerProperty.GetValue("Identity");
            if (identityJObj.Type == JTokenType.Object)
                m_identity = GetIdentityObj((JObject)identityJObj);
            else
                m_identity = null;
        }
        //4更新界面
        UpdateBottomGUI();
        UpdatePlayerSeat();
        UpdateVoiceState();
    }
    private void ExitGameCommand()
    {
        //发送登出消息
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Hall");
        JObject data = new JObject();
        {
            data.Add("Action", "LeaveRoom");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void ExitGameResponse()
    {
        GameManager.Instance.SetActivity("Hall");
    }
    private void ReadyCommand(bool ready)
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "ReadyCommand");
            data.Add("Ready", ready);
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void ReadyResponse(bool ready)
    {
        //1同步变量
        m_isReady = ready;
        //2更新界面
        UpdateBottomGUI();
    }

    private void ReceiveSeatChange(JArray content)
    {
        //1同步变量
        foreach(JToken token in content)
        {
            if(token.Type == JTokenType.Object)
            {
                JObject changeObj = (JObject)token;
                //更新数据
                string change = (string)changeObj.GetValue("Change");
                switch (change)
                {
                    case "Join":
                        {
                            int seatNo = (int)changeObj.GetValue("SeatNo");
                            int headNo = (int)changeObj.GetValue("HeadNo");
                            string name = (string)changeObj.GetValue("Name");
                            PlayerSeatInfo info = m_playerSeatInfos[seatNo];
                            info.HasPlayer = true;
                            info.PlayerName = name;
                            info.HeadNo = headNo;
                            info.Connected = true;
                            info.isReady = false;
                            info.isDead = false;
                            info.isSpeaking = false;
                        }
                        break;
                    case "Leave":
                        {
                            int seatNo = (int)changeObj.GetValue("SeatNo");
                            PlayerSeatInfo info = m_playerSeatInfos[seatNo];
                            info.HasPlayer = false;
                            info.PlayerName = "";
                            info.HeadNo = 0;
                            info.Connected = false;
                            info.isReady = false;
                            info.isDead = false;
                            info.isSpeaking = false;
                        }
                        break;
                    case "Ready":
                        {
                            int seatNo = (int)changeObj.GetValue("SeatNo");
                            bool isReady = (bool)changeObj.GetValue("IsReady");
                            PlayerSeatInfo info = m_playerSeatInfos[seatNo];
                            info.isReady = isReady;
                        }
                        break;
                }
            }
        }
        //2更新界面
        UpdatePlayerSeat();
    }
    private void ReceiveGameTip(string tip)
    {
        JObject logContent = new JObject();
        logContent.Add("LogType", "Tip");
        logContent.Add("Text", tip);
        AddGameLog(logContent);
    }
    private void ReceiveGameReset()
    {
        //重置变量
        m_isPlaying = false;
        m_publicProcess = "PlayerReady";
        m_isReady = false;
        m_identity = null;
        //切换界面
        UpdateBottomGUI();
    }
    private void ReceiveStartPrepare()
    {
        //1同步变量
        m_isReady = false;
        m_isSpeaking = false;
        m_isPlaying = true;
        m_publicProcess = "StartPrepare";
        foreach (PlayerSeatInfo seatInfo in m_playerSeatInfos)
        {
            seatInfo.isReady = false;
        }
        //2发送身份选择指令
        {
            JObject cmdJson = new JObject();
            cmdJson.Add("Type", "Client_Room");
            JObject data = new JObject();
            {
                data.Add("Action", "IdentityExpectionCommand");
                data.Add("IdentityExpection", m_identitySelect);
            }
            cmdJson.Add("Data", data); ;
            GameManager.Instance.SendMessage(cmdJson);
        }
        //3准备开始游戏的消息
        {
            JObject logContent = new JObject();
            logContent.Add("LogType", "Judge");
            logContent.Add("Text", "游戏将要开始！");
            AddGameLog(logContent);
        }
        //4更新界面
        UpdatePlayerSeat();
        UpdateBottomGUI();
        UpdateVoiceState();
    }
    private void ReceiveGameStart()
    {
        //1同步变量
        m_publicProcess = "GameLoop";
        //2法官通告 开始游戏
        {
            ClearGameLog();
            JObject logContent = new JObject();
            logContent.Add("LogType", "Judge");
            logContent.Add("Text", "游戏开始！");
            AddGameLog(logContent);
        }
        //3更新界面
        UpdateBottomGUI();
    }
    private void ReceiveDistributeIdentity(JObject identityJObj)
    {
        m_gameloopProcess = "CheckIdentity";
        m_identity = GetIdentityObj(identityJObj);
        //1法官通告身份信息
        {
            JObject logContent = new JObject();
            logContent.Add("LogType", "Judge");
            string text = string.Format("你的身份是<color=#FF0033>【{0}】</color>", GetIdentityName(m_identity.IdentityType));
            logContent.Add("Text", text);
            AddGameLog(logContent);
        }
        //2刷新界面
        UpdateBottomGUI();
    }
    #endregion

    #region 工具函数
    private string GetIdentityName(string identityType)
    {
        string name = "";
        switch (identityType)
        {
            case "Villager":
                name = "村民";
                break;
            case "Wolfman":
                name = "狼人";
                break;
            case "Prophet":
                name = "先知";
                break;
            case "Hunter":
                name = "猎人";
                break;
            case "Defender":
                name = "守卫";
                break;
            case "Witch":
                name = "女巫";
                break;
        }
        return name;
    }
    private GameIdentity GetIdentityObj(JObject identityJObj)
    {
        GameIdentity identityObj = null;
        string identityType = (string)identityJObj.GetValue("IdentityType");
        bool isDead = (bool)identityJObj.GetValue("IsDead");
        int gameCamp = (int)identityJObj.GetValue("GameCamp");
        string currentAction = (string)identityJObj.GetValue("CurrentAction");
        switch (identityType)
        {
            case "Villager":
                identityObj = new Villager();
                break;
            case "Wolfman":
                identityObj = new Wolfman();
                break;
            case "Prophet":
                identityObj = new Prophet();
                break;
            case "Hunter":
                identityObj = new Hunter();
                break;
            case "Defender":
                identityObj = new Defender();
                {
                    Defender defender = (Defender)identityObj;
                    defender.LastDefendNo = (int)identityJObj.GetValue("LastDefendNo");
                }
                break;
            case "Witch":
                identityObj = new Witch();
                {
                    Witch witch = (Witch)identityObj;
                    witch.Antidote = (bool)identityJObj.GetValue("Antidote");
                    witch.Poison = (bool)identityJObj.GetValue("Poison");
                }
                break;
        }
        identityObj.isDead = isDead;
        identityObj.GameCamp = gameCamp;
        identityObj.CurrentAction = currentAction;

        return identityObj;
    }
    #endregion
}
