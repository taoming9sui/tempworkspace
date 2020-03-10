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

    #region JSON视图模型
    private JObject m_modelViewObj = null;
    private JsonBinder m_jsonBinder = null;
    #endregion

    #region 本机私有变量
    private string m_identitySelect = "Random";
    #endregion

    #region unity触发器
    private void Awake()
    {
        InitModelView();
        BindModelViewEvent();
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
                        SynchronizeGameResponse((JObject)data.GetValue("ModelView"));
                        break;
                    case "SeatChange":
                        ReceiveSeatChange((JArray)data.GetValue("ModelViewChange"));
                        break;
                    case "ReadyResponse":
                        ReadyResponse((JArray)data.GetValue("ModelViewChange"));
                        break;
                    case "StartPrepare":
                        ReceiveStartPrepare((JArray)data.GetValue("ModelViewChange"));
                        break;
                    case "GameStart":
                        ReceiveGameStart((JArray)data.GetValue("ModelViewChange"));
                        break;
                    case "DistributeIdentity":
                        ReceiveDistributeIdentity((JArray)data.GetValue("ModelViewChange"));
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
    private void InitModelView()
    {
        m_modelViewObj = new JObject();
        //1玩家列表名片状态
        JArray playerSeatArray = new JArray();
        for (int i = 0; i < 8; i++)
        {
            JObject jobj = new JObject();
            jobj.Add("SeatNo", i);
            jobj.Add("HasPlayer", false);
            jobj.Add("Name", "");
            jobj.Add("HeadNo", 0);
            jobj.Add("Connected", false);
            jobj.Add("IsSpeaking", false);
            jobj.Add("IsReady", false);
            jobj.Add("IsDead", false);
            playerSeatArray.Add(jobj);
        }
        m_modelViewObj.Add("PlayerSeatArray", playerSeatArray);
        //2游戏全局属性
        JObject gameProperty = new JObject();
        {
            gameProperty.Add("IsPlaying", false);
            gameProperty.Add("PublicProcess", "Default");
            gameProperty.Add("GameloopProcess", "Default");
            gameProperty.Add("DayTime", 0);
            gameProperty.Add("DayNumber", 0);
        }
        m_modelViewObj.Add("GameProperty", gameProperty);
        //3该玩家当前属性
        JObject playerProperty = new JObject();
        {
            playerProperty.Add("PlayerId", "");
            playerProperty.Add("PlayerName", "");
            playerProperty.Add("PlayerHeadNo", 0);
            playerProperty.Add("SeatNo", 0);
            playerProperty.Add("IsSpeaking", false);
            playerProperty.Add("IsReady", false);
            playerProperty.Add("Identity", null);
        }
        m_modelViewObj.Add("PlayerProperty", playerProperty);
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
    private void BindModelViewEvent()
    {
        m_jsonBinder = new JsonBinder(m_modelViewObj);
        //玩家列表
        m_jsonBinder.AddBind("PlayerSeatArray", (jToken, arrIdxs) =>
        {
            JArray seatArray = (JArray)jToken;
            foreach (JToken t in seatArray)
            {
                //读JSON
                JObject seatObj = (JObject)t;
                int seatNo = (int)seatObj.GetValue("SeatNo");
                bool hasPlayer = (bool)seatObj.GetValue("HasPlayer");
                string name = (string)seatObj.GetValue("Name");
                int headNo = (int)seatObj.GetValue("HeadNo");
                bool connected = (bool)seatObj.GetValue("Connected");
                bool isSpeaking = (bool)seatObj.GetValue("IsSpeaking");
                bool isReady = (bool)seatObj.GetValue("IsReady");
                bool isDead = (bool)seatObj.GetValue("IsDead");
                //更新界面
                PlayerSeat seat = playerSeats[seatNo];
                if (hasPlayer)
                {
                    Texture2D texture = ResourceManager.Instance.PlayerHeadTextures[headNo];
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    seat.SetPlayer(seatNo, sprite, name);
                    seat.SetStasusMark(PlayerSeat.StasusMark.Ready, isReady);
                }
                else
                {
                    seat.SetPlayer(seatNo);
                    seat.SetStasusMark(PlayerSeat.StasusMark.Ready, isReady);
                }
            }
        });
        m_jsonBinder.AddBind("PlayerSeatArray[d]", (jToken, arrIdxs) =>
        {
            //读JSON
            JObject seatObj = (JObject)jToken;
            int seatNo = (int)seatObj.GetValue("SeatNo");
            bool hasPlayer = (bool)seatObj.GetValue("HasPlayer");
            string name = (string)seatObj.GetValue("Name");
            int headNo = (int)seatObj.GetValue("HeadNo");
            bool connected = (bool)seatObj.GetValue("Connected");
            bool isSpeaking = (bool)seatObj.GetValue("IsSpeaking");
            bool isReady = (bool)seatObj.GetValue("IsReady");
            bool isDead = (bool)seatObj.GetValue("IsDead");
            //更新界面
            PlayerSeat seat = playerSeats[seatNo];
            if (hasPlayer)
            {
                Texture2D texture = ResourceManager.Instance.PlayerHeadTextures[headNo];
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                seat.SetPlayer(seatNo, sprite, name);
                seat.SetStasusMark(PlayerSeat.StasusMark.Ready, isReady);
            }
            else
            {
                seat.SetPlayer(seatNo);
                seat.SetStasusMark(PlayerSeat.StasusMark.Ready, isReady);
            }
        });
        //游戏全局状态
        m_jsonBinder.AddBind("GameProperty", (jToken, arrIdxs) =>
        {
            //读JSON
            JObject gameProperty = (JObject)jToken;
            bool isPlaying = (bool)gameProperty.GetValue("IsPlaying");
            string publicProcess = (string)gameProperty.GetValue("PublicProcess");
            string gameloopProcess = (string)gameProperty.GetValue("GameloopProcess");
            int dayTime = (int)gameProperty.GetValue("DayTime");
            int dayNumber = (int)gameProperty.GetValue("DayNumber");
            //准备界面切换
            {
                bool flag = publicProcess == "PlayerReady";
                GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
                getready_panel.SetActive(flag);
            }
            //开始预备界面切换
            {
                bool flag = publicProcess == "StartPrepare";
                GameObject startprepare_panel = panelObj.transform.Find("bottom/startprepare_panel").gameObject;
                startprepare_panel.SetActive(flag);
                if (flag)
                {
                    Text info_text = startprepare_panel.transform.Find("info_text").GetComponent<Text>();
                    string name = GetIdentityName(m_identitySelect);
                    if (string.IsNullOrEmpty(name))
                        name = "随机";
                    info_text.text = string.Format("游戏即将开始！你的期望身份是<color=#FFFF66>【{0}】</color>", name);
                }
            }
            //游戏控制界面切换
            {
                bool flag = publicProcess == "GameLoop";
                GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
                gamecontrol_panel.SetActive(flag);
            }
        });
        //玩家状态
        m_jsonBinder.AddBind("PlayerProperty", (jToken, arrIdxs) =>
        {
            //读JSON
            JObject playerProperty = (JObject)jToken;
            string playerId = (string)playerProperty.GetValue("PlayerId");
            string playerName = (string)playerProperty.GetValue("PlayerName");
            int playerHeadNo = (int)playerProperty.GetValue("PlayerHeadNo");
            int seatNo = (int)playerProperty.GetValue("SeatNo");
            bool isSpeaking = (bool)playerProperty.GetValue("IsSpeaking");
            bool isReady = (bool)playerProperty.GetValue("IsReady");
            JToken identityJToken = playerProperty.GetValue("Identity");
            //准备按钮切换
            {
                GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
                GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
                GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
                getready_button.SetActive(!isReady);
                cancelready_button.SetActive(isReady);
            }
            //根据身份信息更新身份面板显示
            if (identityJToken.Type == JTokenType.Object)
            {
                //读JSON
                JObject identityJObj = (JObject)identityJToken;
                bool isDead = (bool)identityJObj.GetValue("IsDead");
                string idType = (string)identityJObj.GetValue("IdentityType");
                int gameCamp = (int)identityJObj.GetValue("GameCamp");
                string currentAction = (string)identityJObj.GetValue("CurrentAction");
                //左边身份面板更新
                GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
                GameObject villager_panel = gamecontrol_panel.transform.Find("identity_panel/villager_panel").gameObject;
                GameObject wolfman_panel = gamecontrol_panel.transform.Find("identity_panel/wolfman_panel").gameObject;
                GameObject prophet_panel = gamecontrol_panel.transform.Find("identity_panel/prophet_panel").gameObject;
                GameObject hunter_panel = gamecontrol_panel.transform.Find("identity_panel/hunter_panel").gameObject;
                GameObject defender_panel = gamecontrol_panel.transform.Find("identity_panel/defender_panel").gameObject;
                GameObject witch_panel = gamecontrol_panel.transform.Find("identity_panel/witch_panel").gameObject;
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
                if (currentPanel != null)
                {
                    Text seatno_text = currentPanel.transform.Find("seatno_text").GetComponent<Text>();
                    seatno_text.text = (seatNo + 1).ToString();
                }
                //右边控制面板更新

            }
        });
        m_jsonBinder.AddBind("PlayerProperty.IsReady", (jToken, arrIdxs) =>
        {
            //读JSON
            bool isReady = (bool)jToken;
            //准备按钮切换
            {
                GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
                GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
                GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
                getready_button.SetActive(!isReady);
                cancelready_button.SetActive(isReady);
            }
        });
        //
        //
        //
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
        //1座位列表
        {
            JArray playerSeatArray = (JArray)content.GetValue("PlayerSeatArray");
            m_jsonBinder.SetValue("PlayerSeatArray", playerSeatArray);
        }
        //2游戏全局状态
        {
            //解析JSON
            JObject gameProperty = (JObject)content.GetValue("GameProperty");
            m_jsonBinder.SetValue("GameProperty", gameProperty);
            //系统提示信息
            bool isPlaying = (bool)m_jsonBinder.GetValue("GameProperty.IsPlaying");
            if (!isPlaying)
            {
                JObject logContent = new JObject();
                logContent.Add("LogType", "Tip");
                logContent.Add("Text", "当房间满员，且所有玩家已准备时，游戏将自动开始");
                AddGameLog(logContent);
            }
        }
        //3玩家状态
        {
            //解析JSON
            JObject playerProperty = (JObject)content.GetValue("PlayerProperty");
            m_jsonBinder.SetValue("PlayerProperty", playerProperty);
        }
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
        GameObject prefab = ResourceManager.Instance.ActivityInfoSet["Hall"].ActivityPrefab;
        GameManager.Instance.SetActivity(prefab);
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
    private void ReadyResponse(JArray changeArray)
    {
        foreach(JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_jsonBinder.SetValue(jPath, value);
        }
    }

    private void ReceiveSeatChange(JArray changeArray)
    {
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_jsonBinder.SetValue(jPath, value);
        }
    }
    private void ReceiveGameReset(JArray changeArray)
    {
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_jsonBinder.SetValue(jPath, value);
        }
    }
    private void ReceiveStartPrepare(JArray changeArray)
    {
        //1同步变量
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_jsonBinder.SetValue(jPath, value);
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
    }
    private void ReceiveGameStart(JArray changeArray)
    {
        //1同步变量
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_jsonBinder.SetValue(jPath, value);
        }
        //2法官通告 开始游戏
        {
            ClearGameLog();
            JObject logContent = new JObject();
            logContent.Add("LogType", "Judge");
            logContent.Add("Text", "游戏开始！");
            AddGameLog(logContent);
        }
    }
    private void ReceiveDistributeIdentity(JArray changeArray)
    {
        //1同步变量
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_jsonBinder.SetValue(jPath, value);
        }
        //2法官通告身份信息
        {
            JObject logContent = new JObject();
            logContent.Add("LogType", "Judge");
            string identityType = (string)m_jsonBinder.GetValue("PlayerProperty.Identity.IdentityType");
            string text = string.Format("你的身份是<color=#FF0033>【{0}】</color>", GetIdentityName(identityType));
            logContent.Add("Text", text);
            AddGameLog(logContent);
        }
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
    #endregion
}
