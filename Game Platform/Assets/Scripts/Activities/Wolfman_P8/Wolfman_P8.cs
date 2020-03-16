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
    public LocalizationDictionary localDic;
    public GameObject[] playerSeatObjs;
    public Sprite defaultHead;

    #region JSON视图模型
    private JsonBinder m_modelViewBinder = null;
    #endregion

    #region 本机私有变量
    private string m_identitySelect = "Random";
    private int m_voteTargetSeatNo = -1;
    private bool m_nowVoting = false;
    #endregion

    #region unity触发器
    private void Awake()
    {
        InitModelView();
        BindModelViewAction();
    }
    private void Update()
    {
        UpdateModelView();
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
                    case "GameLoopProcess":
                        ReceiveGameLoopProcess((JObject)data.GetValue("Content"));
                        break;
                    case "IdentityActionChoose":
                        ReceiveIdentityActionChoose((JObject)data.GetValue("Content"));
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
        //预定义视图结构
        JObject modelViewObj = new JObject();
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
        modelViewObj.Add("PlayerSeatArray", playerSeatArray);
        JObject gameProperty = new JObject();
        {
            gameProperty.Add("IsPlaying", false);
            gameProperty.Add("PublicProcess", "Default");
            gameProperty.Add("GameloopProcess", "Default");
            gameProperty.Add("DayTime", 0);
            gameProperty.Add("DayNumber", 0);
        }
        modelViewObj.Add("GameProperty", gameProperty);
        JObject playerProperty = new JObject();
        {
            playerProperty.Add("PlayerId", "");
            playerProperty.Add("PlayerName", "");
            playerProperty.Add("PlayerHeadNo", 0);
            playerProperty.Add("SeatNo", 0);
            playerProperty.Add("IsSpeaking", false);
            playerProperty.Add("IsReady", false);
            playerProperty.Add("Identity", null);
            playerProperty.Add("WaitTimestamp", 0);
        }
        modelViewObj.Add("PlayerProperty", playerProperty);
        //构建模型视图绑定器
        m_modelViewBinder = new JsonBinder(modelViewObj);
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
    public void SelectVoteTargetButton(int selectNo)
    {
        if (m_voteTargetSeatNo != selectNo)
            m_voteTargetSeatNo = selectNo;
        else
            m_voteTargetSeatNo = -1;

        for (int i = 0; i < playerSeatObjs.Length; i++)
        {
            int seatNo = i;
            GameObject seatObj = playerSeatObjs[seatNo];
            SetPlayerSeatStatusMark(seatObj, "Selected", seatNo == m_voteTargetSeatNo);
        }
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
    private void SetPlayerSeatStatusMark(GameObject seatObj, string markType, bool show)
    {
        GameObject selected_mark = seatObj.transform.Find("status_panel/selected_mark").gameObject;
        GameObject ready_mark = seatObj.transform.Find("status_panel/ready_mark").gameObject;
        GameObject speaking_mark = seatObj.transform.Find("status_panel/speaking_mark").gameObject;
        switch (markType)
        {
            case "Selected":
                selected_mark.SetActive(show);
                break;
            case "Ready":
                ready_mark.SetActive(show);
                break;
            case "Speaking":
                speaking_mark.SetActive(show);
                break;
        }
    }
    private void SetPlayerSeatPlayerInfo(GameObject seatObj, int seatNo, Sprite headSprite, string name)
    {
        Text number_text = seatObj.transform.Find("number_text/Text").GetComponent<Text>();
        Image head_image = seatObj.transform.Find("head_image/Image").GetComponent<Image>();
        Text name_text = seatObj.transform.Find("name_text/Text").GetComponent<Text>();
        //设置编号
        number_text.text = (seatNo + 1).ToString();
        //设置头像
        if (headSprite == null)
            headSprite = defaultHead;
        head_image.sprite = headSprite;
        //设置名称
        if (name == null)
            name = "空座位";
        name_text.text = name;
    }
    private void BindModelViewAction()
    {
        //玩家列表
        m_modelViewBinder.AddBind("PlayerSeatArray", (jToken, arrIdxs) =>
        {
            JArray seatArray = (JArray)jToken;
            foreach (JToken t in seatArray)
            {
                JObject seatObj = (JObject)t;
                UpdatePlayerSeatUI(seatObj);
            }
        });
        m_modelViewBinder.AddBind("PlayerSeatArray[d]", (jToken, arrIdxs) =>
        {
            JObject seatObj = (JObject)jToken;
            UpdatePlayerSeatUI(seatObj);
        });
        //游戏全局状态
        m_modelViewBinder.AddBind("GameProperty", (jToken, arrIdxs) =>
        {
            JObject gamePropertyJObj = (JObject)m_modelViewBinder.GetValue("GameProperty");
            JObject playerPropertyJObj = (JObject)m_modelViewBinder.GetValue("PlayerProperty");
            UpdateBottomUI(gamePropertyJObj, playerPropertyJObj);
        });
        //玩家状态
        m_modelViewBinder.AddBind("PlayerProperty", (jToken, arrIdxs) =>
        {
            JObject gamePropertyJObj = (JObject)m_modelViewBinder.GetValue("GameProperty");
            JObject playerPropertyJObj = (JObject)m_modelViewBinder.GetValue("PlayerProperty");
            UpdateBottomUI(gamePropertyJObj, playerPropertyJObj);
        });
        //
    }
    private void UpdateModelView()
    {
        this.m_modelViewBinder.ApplyUpdate();
    }
    private void UpdatePlayerSeatUI(JObject playerSeatJObj)
    {
        //读JSON
        int seatNo = (int)playerSeatJObj.GetValue("SeatNo");
        bool hasPlayer = (bool)playerSeatJObj.GetValue("HasPlayer");
        string name = (string)playerSeatJObj.GetValue("Name");
        int headNo = (int)playerSeatJObj.GetValue("HeadNo");
        bool connected = (bool)playerSeatJObj.GetValue("Connected");
        bool isSpeaking = (bool)playerSeatJObj.GetValue("IsSpeaking");
        bool isReady = (bool)playerSeatJObj.GetValue("IsReady");
        bool isDead = (bool)playerSeatJObj.GetValue("IsDead");
        //更新界面
        GameObject seatObj = playerSeatObjs[seatNo];
        if (hasPlayer)
        {
            Texture2D texture = ResourceManager.Instance.Local.PlayerHeadTextures[headNo];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            SetPlayerSeatPlayerInfo(seatObj, seatNo, sprite, name);
        }
        else
        {
            SetPlayerSeatPlayerInfo(seatObj, seatNo, null, null);
        }
        SetPlayerSeatStatusMark(seatObj, "Ready", isReady);
    }
    private void UpdateBottomUI(JObject gamePropertyJObj, JObject playerPropertyJObj)
    {
        //读JSON
        bool isPlaying = (bool)gamePropertyJObj.GetValue("IsPlaying");
        string publicProcess = (string)gamePropertyJObj.GetValue("PublicProcess");
        string gameloopProcess = (string)gamePropertyJObj.GetValue("GameloopProcess");
        bool isReady = (bool)playerPropertyJObj.GetValue("IsReady");
        JToken identityJToken = playerPropertyJObj.GetValue("Identity");
        //准备界面切换
        {
            bool flag = publicProcess == "PlayerReady";
            GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
            getready_panel.SetActive(flag);
            if (flag)
            {
                GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
                GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
                getready_button.SetActive(!isReady);
                cancelready_button.SetActive(isReady);
            }
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

                CountdownBar countdown_bar = startprepare_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                countdown_bar.StartCountdown(3f);
            }
        }
        //游戏控制界面切换
        {
            bool flag = publicProcess == "GameLoop";
            GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
            gamecontrol_panel.SetActive(flag);
            //身份界面切换
            if (identityJToken.Type == JTokenType.Object)
            {
                UpdateCaptionUI(gamePropertyJObj, playerPropertyJObj);
                UpdateIdentityUI(gamePropertyJObj, playerPropertyJObj);
                UpdateProcessUI(gamePropertyJObj, playerPropertyJObj);
            }
        }

    }
    private void UpdateCaptionUI(JObject gamePropertyJObj, JObject playerPropertyJObj)
    {

    }
    private void UpdateIdentityUI(JObject gamePropertyJObj, JObject playerPropertyJObj)
    {
        //读JSON
        JObject identityJObj = (JObject)playerPropertyJObj.GetValue("Identity");
        bool isDead = (bool)identityJObj.GetValue("IsDead");
        string idType = (string)identityJObj.GetValue("IdentityType");
        int gameCamp = (int)identityJObj.GetValue("GameCamp");

        GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
        GameObject identity_panel = gamecontrol_panel.transform.Find("identity_panel").gameObject;

        GameObject villager_panel = identity_panel.transform.Find("villager_panel").gameObject;
        GameObject wolfman_panel = identity_panel.transform.Find("wolfman_panel").gameObject;
        GameObject prophet_panel = identity_panel.transform.Find("prophet_panel").gameObject;
        GameObject hunter_panel = identity_panel.transform.Find("hunter_panel").gameObject;
        GameObject defender_panel = identity_panel.transform.Find("defender_panel").gameObject;
        GameObject witch_panel = identity_panel.transform.Find("witch_panel").gameObject;
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
            //身份面板座位编号
            int seatNo = (int)m_modelViewBinder.GetValue("PlayerProperty.SeatNo");
            Text seatno_text = currentPanel.transform.Find("seatno_text").GetComponent<Text>();
            seatno_text.text = (seatNo + 1).ToString();
        }
    }
    private void UpdateProcessUI(JObject gamePropertyJObj, JObject playerPropertyJObj)
    {
        //读JSON
        JObject identityJObj = (JObject)playerPropertyJObj.GetValue("Identity");
        string gameloopProcess = (string)gamePropertyJObj.GetValue("GameloopProcess");
        string currentAction = (string)identityJObj.GetValue("CurrentAction");
        string identityType = (string)identityJObj.GetValue("IdentityType");
        long waitTimeStamp = (long)playerPropertyJObj.GetValue("WaitTimestamp");

        GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
        GameObject process_panel = gamecontrol_panel.transform.Find("process_panel").gameObject;
        {
            GameObject defender_defend_panel = process_panel.transform.Find("defender_defend_panel").gameObject;
            {
                bool flag = currentAction == "Defender_Defend";
                defender_defend_panel.SetActive(flag);
            }
            GameObject checkidentity_panel = process_panel.transform.Find("checkidentity_panel").gameObject;
            {
                bool flag = gameloopProcess == "CheckIdentity" && currentAction == "Default";
                checkidentity_panel.SetActive(flag);
                if (flag)
                {
                    Text info_text = checkidentity_panel.transform.Find("info_text").GetComponent<Text>();
                    string name = GetIdentityName(identityType);
                    if (string.IsNullOrEmpty(name))
                        name = "随机";
                    info_text.text = string.Format("你的身份牌是<color=#FFFF66>【{0}】</color>\n<color=#FFCC33> 冷静一下 开始你的表演</color>", name);

                    float seconds = GetWaitSeconds(waitTimeStamp);
                    CountdownBar countdown_bar = checkidentity_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                    countdown_bar.StartCountdown(seconds);
                }
            }
            GameObject nightcloseeye_panel = process_panel.transform.Find("nightcloseeye_panel").gameObject;
            {
                bool flag = gameloopProcess == "NightCloseEye" && currentAction == "Default";
                nightcloseeye_panel.SetActive(flag);
            }
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
            m_modelViewBinder.SetValue("PlayerSeatArray", playerSeatArray);
        }
        //2游戏全局状态
        {
            //解析JSON
            JObject gameProperty = (JObject)content.GetValue("GameProperty");
            m_modelViewBinder.SetValue("GameProperty", gameProperty);
            //系统提示信息
            bool isPlaying = (bool)m_modelViewBinder.GetValue("GameProperty.IsPlaying");
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
            m_modelViewBinder.SetValue("PlayerProperty", playerProperty);
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
        GameObject prefab = ResourceManager.Instance.Local.ActivityInfoSet["Hall"].ActivityPrefab;
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
            m_modelViewBinder.SetValue(jPath, value);
        }
    }
    private void SetIdentityExpectionCommand()
    {
        JObject cmdJson = new JObject();
        cmdJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "SetIdentityExpectionCommand");
            data.Add("IdentityExpection", m_identitySelect);
        }
        cmdJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(cmdJson);
    }
    private void IdentityOperationCommand(string operation, bool YesOrNot, int targetSeatNo, string text)
    {
        JObject cmdJson = new JObject();
        cmdJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "IdentityOperationCommand");
            JObject detailJObj = new JObject();
            {
                detailJObj.Add("Operation", operation);
                detailJObj.Add("YesOrNot", YesOrNot);
                detailJObj.Add("TargetSeatNo", targetSeatNo);
                detailJObj.Add("Text", text);
            }
            data.Add("Detail", detailJObj);
        }
        cmdJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(cmdJson);
    }

    private void ReceiveSeatChange(JArray changeArray)
    {
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
    }
    private void ReceiveGameReset(JArray changeArray)
    {
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
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
            m_modelViewBinder.SetValue(jPath, value);
        }
        //2发送身份选择指令
        SetIdentityExpectionCommand();
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
            m_modelViewBinder.SetValue(jPath, value);
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
    private void ReceiveGameLoopProcess(JObject content)
    {
        //1同步视图变量
        JArray changeArray = (JArray)content.GetValue("ModelViewChange");
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
        //2
        string process = (string)content.GetValue("Process");
        switch (process)
        {
            //分配身份
            case "DistributeIdentity":
                {
                    string identityType = (string)m_modelViewBinder.GetValue("PlayerProperty.Identity.IdentityType");
                    JObject logContent = new JObject();
                    logContent.Add("LogType", "Judge");
                    string text = string.Format("你的身份是<color=#FF0033>【{0}】</color>", GetIdentityName(identityType));
                    logContent.Add("Text", text);
                    AddGameLog(logContent);
                }
                break;
            //天黑闭眼
            case "NightCloseEye":
                {
                    int dayNumber = (int)m_modelViewBinder.GetValue("GameProperty.DayNumber");
                    if (dayNumber == 1)
                    {
                        JObject logContent = new JObject();
                        logContent.Add("LogType", "Judge");
                        string text = "游戏正式开始！第一个夜晚来临！";
                        logContent.Add("Text", text);
                        AddGameLog(logContent);
                    }
                }
                break;
        }
    }
    private void ReceiveIdentityActionChoose(JObject content)
    {
        //1同步视图变量
        JArray changeArray = (JArray)content.GetValue("ModelViewChange");
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
        //2
        string identityAction = (string)content.GetValue("IdentityAction");
        switch (identityAction)
        {

        }
    }
    private void ReceiveIdentityActionResult(JObject content)
    {

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
    private float GetWaitSeconds(long waitTimestamp)
    {
        System.TimeSpan span = new System.DateTime(waitTimestamp) - System.DateTime.UtcNow;
        float seconds = (float)span.TotalSeconds;
        return seconds;
    }
    #endregion
}
