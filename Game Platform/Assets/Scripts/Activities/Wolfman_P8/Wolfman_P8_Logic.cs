﻿using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Wolfman_P8 : GameActivity
{

    #region 枚举类
    private enum PublicProcessState
    {
        Default, PlayerReady, StartPrepare, GameLoop
    }
    private enum GameloopProcessState
    {
        Default, CheckIdentity, NightCloseEye, DayOpenEye, SquareSpeak, SquareVote, End
    }
    private enum CurrentActionType
    {
        Default, Defender_Defend, Wolfman_Kill, Hunter_Revenge, Witch_Magic, Prophet_Foresee, Square_Speak, Square_Vote
    }
    private enum ActionDecisionType
    {
        Default,
        Defender_Defend_Excute, Defender_Defend_Abandon,
        Wolfman_Kill_Excute, Wolfman_Kill_Abandon,
        Prophet_Foresee_Excute, Prophet_Foresee_Abandon,
        Witch_Magic_Poison, Witch_Magic_Save, Witch_Magic_Abandon,
        Hunter_Revenge_Excute, Hunter_Revenge_Abandon,
        Square_Speak_Begin, Square_Speak_End,
        Square_Vote_Excute, Square_Vote_Abandon
    }
    private enum GameIdentityType
    {
        Default, Villager, Wolfman, Prophet, Hunter, Defender, Witch
    }
    private enum BaseFunctionType
    {
        Default, GameReady, SetIdentityExpection, TestVoice
    }
    private enum SeatChangeType
    {
        Default, Join, Leave, Disconnect, Reconnect, Ready, Speak, Dead
    }
    private enum IdentityTranslateType
    {
        Default, GoDead,
        Defender_Defend_Begin, Defender_Defend_End
    }
    private enum IdentityFunctionType
    {
        Default
    }
    private enum GameTipType
    {
        Default, CommonLog
    }
    private enum JudgeAnnounceType
    {
        Default
    }
    private enum DayTime {
        Default, Night, Day
    };
    #endregion

    #region JSON视图模型
    private JsonBinder m_modelViewBinder = null;
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
            gameProperty.Add("PublicProcess", (int)PublicProcessState.Default);
            gameProperty.Add("GameloopProcess", (int)GameloopProcessState.Default);
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

    #region 客户端私有变量
    private string m_identitySelect = "Random";
    private int m_voteTargetSeatNo = -1;
    private bool m_nowVoting = false;
    #endregion

    #region UI交互脚本
    public void ExitButton()
    {
        ExitGameCommand();
    }
    public void GetReadyButton()
    {
        JObject parms = new JObject();
        parms.Add("IsReady", true);
        BaseFunctionCommand(BaseFunctionType.GameReady, parms);
    }
    public void CancelReadyButton()
    {
        JObject parms = new JObject();
        parms.Add("IsReady", false);
        BaseFunctionCommand(BaseFunctionType.GameReady, parms);
    }
    public void SelectVoteTargetButton(int selectNo)
    {
        if (m_voteTargetSeatNo != selectNo)
            m_voteTargetSeatNo = selectNo;
        else
            m_voteTargetSeatNo = -1;

        UpdateVoteTargetMark();
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
    public void IdentityActionDecideButton(string decision)
    {
        switch (decision)
        {
            case "Defender_Defend_Excute":
                {
                    int targetSeatNo = m_voteTargetSeatNo;
                    if (targetSeatNo == -1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_needtarget_logtip");
                        AddGameLog(GameLogType.Tip, text);
                    }
                    else
                    {
                        IdentityActionDecideCommand(ActionDecisionType.Defender_Defend_Excute, targetSeatNo);
                    }
                }
                break;
            case "Defender_Defend_Abandon":
                {
                    IdentityActionDecideCommand(ActionDecisionType.Defender_Defend_Abandon, -1);
                }
                break;
        }
    }
    #endregion

    #region UI界面更新
    private enum GameLogType { None, Tip, Judge };
    private void ClearGameLog()
    {
        MessageContainer message_container = panelObj.transform.Find("middle/message_container").GetComponent<MessageContainer>();
        message_container.ClearMessage();
    }
    private void AddGameLog(GameLogType logType, string logText)
    {
        MessageContainer message_container = panelObj.transform.Find("middle/message_container").GetComponent<MessageContainer>();
        switch (logType)
        {
            case GameLogType.Tip:
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string title = localDic.GetLocalText("text.wolfman_p8.logtitle_tip");
                    builder.AppendLine(title);
                    builder.Append(logText);
                    message_container.AddMessage(builder.ToString());
                }
                break;
            case GameLogType.Judge:
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string title = localDic.GetLocalText("text.wolfman_p8.logtitle_judge");
                    builder.AppendLine(title);
                    builder.Append(logText);
                    message_container.AddMessage(builder.ToString());
                }
                break;
        }
    }
    private void UpdateVoteTargetMark()
    {
        int no = m_voteTargetSeatNo;
        if (!m_nowVoting)
        {
            no = -1;
        }
        for (int i = 0; i < playerSeatObjs.Length; i++)
        {
            int seatNo = i;
            GameObject seatObj = playerSeatObjs[seatNo];
            SetPlayerSeatStatusMark(seatObj, "Selected", seatNo == no);
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
        //更新玩家头像列表
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
        PublicProcessState publicProcess = (PublicProcessState)(int)gamePropertyJObj.GetValue("PublicProcess");
        bool isReady = (bool)playerPropertyJObj.GetValue("IsReady");
        JToken identityJToken = playerPropertyJObj.GetValue("Identity");
        //准备界面切换
        {
            bool flag = publicProcess == PublicProcessState.PlayerReady;
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
            bool flag = publicProcess == PublicProcessState.StartPrepare;
            GameObject startprepare_panel = panelObj.transform.Find("bottom/startprepare_panel").gameObject;
            startprepare_panel.SetActive(flag);
            if (flag)
            {
                Text info_text = startprepare_panel.transform.Find("info_text").GetComponent<Text>();
                string uname = GetIdentityExpectionName(m_identitySelect);
                string templateText = localDic.GetLocalText("template.wolfman_p8.gamepreparestart_info");
                info_text.text = string.Format(templateText, localDic.GetLocalText(uname));

                CountdownBar countdown_bar = startprepare_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                countdown_bar.StartCountdown(3f);
            }
        }
        //游戏控制界面切换
        {
            bool flag = publicProcess == PublicProcessState.GameLoop;
            GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
            gamecontrol_panel.SetActive(flag);
            //身份界面切换
            if (identityJToken.Type == JTokenType.Object)
            {
                UpdateCaptionUI(gamePropertyJObj);
                UpdateIdentityUI(playerPropertyJObj);
                UpdateProcessUI(gamePropertyJObj, playerPropertyJObj);
            }
        }

    }
    private void UpdateIdentityUI(JObject playerPropertyJObj)
    {
        //读JSON
        JObject identityJObj = (JObject)playerPropertyJObj.GetValue("Identity");
        bool isDead = (bool)identityJObj.GetValue("IsDead");
        GameIdentityType identityType = (GameIdentityType)(int)identityJObj.GetValue("IdentityType");
        int gameCamp = (int)identityJObj.GetValue("GameCamp");

        GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
        GameObject identity_panel = gamecontrol_panel.transform.Find("identity_panel").gameObject;

        GameObject villager_panel = identity_panel.transform.Find("villager_panel").gameObject;
        GameObject wolfman_panel = identity_panel.transform.Find("wolfman_panel").gameObject;
        GameObject prophet_panel = identity_panel.transform.Find("prophet_panel").gameObject;
        GameObject hunter_panel = identity_panel.transform.Find("hunter_panel").gameObject;
        GameObject defender_panel = identity_panel.transform.Find("defender_panel").gameObject;
        GameObject witch_panel = identity_panel.transform.Find("witch_panel").gameObject;
        villager_panel.SetActive(identityType == GameIdentityType.Villager);
        wolfman_panel.SetActive(identityType == GameIdentityType.Wolfman);
        prophet_panel.SetActive(identityType == GameIdentityType.Prophet);
        hunter_panel.SetActive(identityType ==  GameIdentityType.Hunter);
        defender_panel.SetActive(identityType == GameIdentityType.Defender);
        witch_panel.SetActive(identityType == GameIdentityType.Witch);
        GameObject currentPanel = null;
        switch (identityType)
        {
            case GameIdentityType.Villager:
                currentPanel = villager_panel;
                break;
            case GameIdentityType.Wolfman:
                currentPanel = wolfman_panel;
                break;
            case GameIdentityType.Prophet:
                currentPanel = prophet_panel;
                break;
            case GameIdentityType.Hunter:
                currentPanel = hunter_panel;
                break;
            case GameIdentityType.Defender:
                currentPanel = defender_panel;
                break;
            case GameIdentityType.Witch:
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
        GameloopProcessState gameloopProcess = (GameloopProcessState)(int)gamePropertyJObj.GetValue("GameloopProcess");
        CurrentActionType currentAction = (CurrentActionType)(int)identityJObj.GetValue("CurrentAction");
        GameIdentityType identityType = (GameIdentityType)(int)identityJObj.GetValue("IdentityType");
        long waitTimeStamp = (long)playerPropertyJObj.GetValue("WaitTimestamp");

        GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
        GameObject process_panel = gamecontrol_panel.transform.Find("process_panel").gameObject;
        {
            GameObject defender_defend_panel = process_panel.transform.Find("defender_defend_panel").gameObject;
            {
                bool flag = currentAction == CurrentActionType.Defender_Defend;
                defender_defend_panel.SetActive(flag); 
                if (flag)
                {
                    float seconds = GetWaitSeconds(waitTimeStamp);
                    CountdownBar countdown_bar = defender_defend_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                    countdown_bar.StartCountdown(seconds);
                }
            }
            GameObject checkidentity_panel = process_panel.transform.Find("checkidentity_panel").gameObject;
            {
                bool flag = gameloopProcess == GameloopProcessState.CheckIdentity && currentAction == CurrentActionType.Default;
                checkidentity_panel.SetActive(flag);
                if (flag)
                {
                    Text info_text = checkidentity_panel.transform.Find("info_text").GetComponent<Text>();
                    string uname = GetIdentityName(identityType);
                    string templateText = localDic.GetLocalText("template.wolfman_p8.checkidentity_info");
                    info_text.text = string.Format(templateText, localDic.GetLocalText(uname));

                    float seconds = GetWaitSeconds(waitTimeStamp);
                    CountdownBar countdown_bar = checkidentity_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                    countdown_bar.StartCountdown(seconds);
                }
            }
            GameObject nightcloseeye_panel = process_panel.transform.Find("nightcloseeye_panel").gameObject;
            {
                bool flag = gameloopProcess == GameloopProcessState.NightCloseEye && currentAction == CurrentActionType.Default;
                nightcloseeye_panel.SetActive(flag);
            }
        }
    }
    private void UpdateCaptionUI(JObject gamePropertyJObj)
    {
        //读JSON
        GameloopProcessState gameloopProcess = (GameloopProcessState)(int)gamePropertyJObj.GetValue("GameloopProcess");
        DayTime dayTime = (DayTime)(int)gamePropertyJObj.GetValue("DayTime");
        int dayNumber = (int)gamePropertyJObj.GetValue("DayNumber");

        GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
        GameObject caption_panel = gamecontrol_panel.transform.Find("caption_panel").gameObject;

        GameObject checkidentity_caption = caption_panel.transform.Find("checkidentity_caption").gameObject;
        {
            bool flag = gameloopProcess == GameloopProcessState.CheckIdentity;
            checkidentity_caption.SetActive(flag);
        }
        GameObject nightcloseeye_caption = caption_panel.transform.Find("nightcloseeye_caption").gameObject;
        {
            bool flag = gameloopProcess == GameloopProcessState.NightCloseEye;
            nightcloseeye_caption.SetActive(flag);
            if (flag)
            {
                string templateText = localDic.GetLocalText("template.wolfman_p8.nightcloseeye_caption");
                string text = string.Format(templateText, dayNumber);
                nightcloseeye_caption.GetComponent<Text>().text = text;
            }
        }

    }
    #endregion

    #region 客户端接口和响应
    private void SynchronizeStateCommand()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "SynchronizeState");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
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
    private void BaseFunctionCommand(BaseFunctionType functionType, JObject parms)
    {
        JObject cmdJson = new JObject();
        cmdJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "BaseFunction");
            JObject content = new JObject();
            {
                content.Add("FunctionType", (int)functionType);
                content.Add("Params", parms);
            }
            data.Add("Content", content);      
        }
        cmdJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(cmdJson);
    }
    private void IdentityActionDecideCommand(ActionDecisionType decisionType, int targetSeatNo)
    {
        JObject cmdJson = new JObject();
        cmdJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "IdentityActionDecide");
            JObject content = new JObject();
            {
                content.Add("DecisionType", (int)decisionType);
                content.Add("TargetSeatNo", targetSeatNo);
            }
            data.Add("Content", content);
        }
        cmdJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(cmdJson);
    }

    private void ReceiveSynchronizeState(JArray changeArray)
    {
        //更新模型视图
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
        //输出系统提示信息
        bool isPlaying = (bool)m_modelViewBinder.GetValue("GameProperty.IsPlaying");
        if (!isPlaying)
        {
            string text = localDic.GetLocalText("text.wolfman_p8.newjoin_logtip");
            AddGameLog(GameLogType.Tip, text);
        }
    }
    private void ReceiveGameTip(GameTipType tipType, string tipText)
    {
        switch (tipType)
        {
            case GameTipType.CommonLog:
                AddGameLog(GameLogType.Tip, tipText);
                break;
        }
    }
    private void ReceiveSeatChange(SeatChangeType changeType, JArray changeArray)
    {
        //更新模型视图
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
    }
    private void ReceiveBaseFunctionResult(BaseFunctionType functionType, JObject resultDetail, JArray changeArray)
    {
        switch (functionType)
        {
            default:
                break;
        }
        //更新模型视图
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
    }
    private void ReceivePublicProcess(PublicProcessState processState, JArray changeArray)
    {
        switch (processState)
        {
            case PublicProcessState.StartPrepare:
                {
                    //1发送身份选择指令
                    JObject parms = new JObject();
                    GameIdentityType expectionType = GameIdentityType.Default;
                    switch (m_identitySelect)
                    {
                        case "Wolfman":
                            expectionType = GameIdentityType.Wolfman;
                            break;
                        case "Villager":
                            expectionType = GameIdentityType.Villager;
                            break;
                        case "Defender":
                            expectionType = GameIdentityType.Defender;
                            break;
                        case "Prophet":
                            expectionType = GameIdentityType.Prophet;
                            break;
                        case "Witch":
                            expectionType = GameIdentityType.Witch;
                            break;
                        case "Hunter":
                            expectionType = GameIdentityType.Hunter;
                            break;
                    }
                    parms.Add("IdentityExpection", (int)expectionType);
                    BaseFunctionCommand(BaseFunctionType.SetIdentityExpection, parms);
                    //2准备开始游戏的消息
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.gamepreparestart_logtip");
                        AddGameLog(GameLogType.Judge, text);
                    }
                }
                break;
            case PublicProcessState.GameLoop:
                {
                    //法官通告 开始游戏
                    {
                        ClearGameLog();
                        string text = localDic.GetLocalText("text.wolfman_p8.gamestart_logtip");
                        AddGameLog(GameLogType.Judge, text);
                    }
                }
                break;
        }
        //更新模型视图
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }
    }
    private void ReceiveGameloopProcess(GameloopProcessState processState, JArray changeArray)
    {
        //更新模型视图
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }

        switch (processState)
        {
            //分配身份
            case GameloopProcessState.CheckIdentity:
                {
                    GameIdentityType identityType = (GameIdentityType)(int)m_modelViewBinder.GetValue("PlayerProperty.Identity.IdentityType");
                    string uname = GetIdentityName(identityType);
                    string templateText = localDic.GetLocalText("template.wolfman_p8.checkidentity_logjudge");
                    string text = string.Format(templateText, localDic.GetLocalText(uname));
                    AddGameLog(GameLogType.Judge, text);
                }
                break;
            //天黑闭眼
            case GameloopProcessState.NightCloseEye:
                {
                    int dayNumber = (int)m_modelViewBinder.GetValue("GameProperty.DayNumber");
                    if (dayNumber == 1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.firstnight_logjudge");
                        AddGameLog(GameLogType.Judge, text);
                    }
                }
                break;
        }
    }
    private void ReceiveIdentityTranslate(IdentityTranslateType translateType, JArray changeArray)
    {
        //更新模型视图
        foreach (JToken jToken in changeArray)
        {
            JObject change = (JObject)jToken;
            string jPath = (string)change.GetValue("JPath");
            JToken value = change.GetValue("Value");
            m_modelViewBinder.SetValue(jPath, value);
        }

        switch (translateType)
        {
            case IdentityTranslateType.Defender_Defend_Begin:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            default:
                m_nowVoting = false;
                UpdateVoteTargetMark();
                break;
        }
    }
    #endregion

    #region 工具函数
    private string GetIdentityExpectionName(string identityExpection)
    {
        string name = "text.wolfman_p8.identityexpection_random";
        switch (identityExpection)
        {
            case "Villager":
                name = "text.wolfman_p8.identityexpection_villager";
                break;
            case "Wolfman":
                name = "text.wolfman_p8.identityexpection_wolfman";
                break;
            case "Prophet":
                name = "text.wolfman_p8.identityexpection_prophet";
                break;
            case "Hunter":
                name = "text.wolfman_p8.identityexpection_hunter";
                break;
            case "Defender":
                name = "text.wolfman_p8.identityexpection_defender";
                break;
            case "Witch":
                name = "text.wolfman_p8.identityexpection_witch";
                break;
        }
        return name;
    }
    private string GetIdentityName(GameIdentityType identityType)
    {
        string name = "";
        switch (identityType)
        {
            case GameIdentityType.Villager:
                name = "text.wolfman_p8.identityname_villager";
                break;
            case GameIdentityType.Wolfman:
                name = "text.wolfman_p8.identityname_wolfman";
                break;
            case GameIdentityType.Prophet:
                name = "text.wolfman_p8.identityname_prophet";
                break;
            case GameIdentityType.Hunter:
                name = "text.wolfman_p8.identityname_hunter";
                break;
            case GameIdentityType.Defender:
                name = "text.wolfman_p8.identityname_defender";
                break;
            case GameIdentityType.Witch:
                name = "text.wolfman_p8.identityname_witch";
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