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
        Default, Defender_Defend, Wolfman_Kill, Hunter_Revenge, Witch_Magic, Prophet_Foresee, Square_Speak, Square_Vote, LastWord
    }
    private enum ActionDecisionType
    {
        Default,
        Defender_Defend_Excute, Defender_Defend_Abandon,
        Wolfman_Kill_Excute, Wolfman_Kill_Abandon,
        Prophet_Foresee_Excute, Prophet_Foresee_Abandon,
        Witch_Magic_Poison, Witch_Magic_Save, Witch_Magic_Abandon,
        Hunter_Revenge_Excute, Hunter_Revenge_Abandon,
        Square_Speak_Begin, Square_Speak_End, Square_Speak_Abandon,
        Square_Vote_Excute, Square_Vote_Abandon,
        LastWord_Begin, LastWord_End, LastWord_Abandon,
    }
    private enum ActionIntentionType
    {
        Default, Abandon, Defend, Foresee, Kill, Poison, Save, Vote, Revenge
    }
    private enum GameIdentityType
    {
        Default, Villager, Wolfman, Prophet, Hunter, Defender, Witch
    }
    private enum OperationFlagType
    {
        Default, Poisoned, Saved, Defended, Killed, BanishTicket
    }
    private enum OperationResultType
    {
        Default, Saved, Defended, Poisoned, Killed, Banned, Revenged
    }
    private enum DeathCauseType
    {
        Default, Killed, Poison, Overdose, Shot, Banish
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
        Defender_Defend_Begin, Defender_Defend_End,
        Prophet_Foresee_Begin, Prophet_Foresee_End,
        Wolfman_Kill_Begin, Wolfman_Kill_End,
        Witch_Magic_Begin, Witch_Magic_End,
    }
    private enum IdentityFunctionType
    {
        Default,
        Prophet_ForeseeLog,
        Wolfman_BodyLanguage,
        Wolfman_Whisper
    }
    private enum GameTipType
    {
        Default, CommonLog, CommonModel, PlayerChangeLog
    }
    private enum JudgeAnnounceType
    {
        Default,
        DistributeIdentity_Result,
        Defender_Defend_Excute, Defender_Defend_Abandon,
        Wolfman_Kill_Excute, Wolfman_Kill_Abandon,
        Prophet_Foresee_Excute, Prophet_Foresee_Abandon,
        Witch_Magic_Poison, Witch_Magic_Save, Witch_Magic_Abandon,
        Hunter_Revenge_Excute, Hunter_Revenge_Abandon,
        Foresee_Result,
        Revenge_Report,
        Speak_Begin, Speak_Abandon, Speak_End,
        LastWord_Begin, LastWord_Abandon, LastWord_End,
        Square_Vote_Report, LastNight_Report, GameOverLog_Report
    }
    private enum BodyLanguageType
    {
        Default,
        Nod, LOL
    }
    private enum GameOverLogType
    {
        Default,
        Start, DistributeIdentity, NightCloseEye, DayOpenEye, End,
        Defender_Defend,
        Wolfman_Kill,
        Prophet_Foresee,
        Witch_Magic,
        Hunter_Revenge,
        Square_Vote,
        LastNight
    }
    private enum DayTime { Default, Night, Day };
    private enum IdentityCamp { Default, Villager, Wolfman }
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
            JObject identityObj = new JObject();
            {
                identityObj.Add("IsDead", false);
                identityObj.Add("IdentityType", 0);
                identityObj.Add("GameCamp", 0);
                identityObj.Add("CurrentAction", 0);
            }
            playerProperty.Add("Identity", identityObj);
            playerProperty.Add("WaitTimestamp", 0);
        }
        modelViewObj.Add("PlayerProperty", playerProperty);
        //构建模型视图绑定器
        m_modelViewBinder = new JsonBinder(modelViewObj, (ex) =>
        {
            Debug.LogError(ex.StackTrace);
        });
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
    public void SelectVoteTargetButton(Toggle toggle)
    {
        if (toggle.isOn)
        {
            if (m_nowVoting)
            {
                CustomValue valueObj = toggle.transform.parent.GetComponent<CustomValue>();
                int seatNo = valueObj.intValue;
                m_voteTargetSeatNo = seatNo;
            }
            else
            {
                toggle.isOn = false;
            }
        }
        else
        {
            m_voteTargetSeatNo = -1;
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
    public void IdentityActionDecideButton(string decision)
    {
        switch (decision)
        {
            case "Defender_Defend_Excute":
                {
                    int targetSeatNo = m_voteTargetSeatNo;
                    if (targetSeatNo == -1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_needtarget_modeltip");
                        TipModel(text);
                    }
                    else
                    {
                        IdentityActionDecideCommand(ActionDecisionType.Defender_Defend_Excute, targetSeatNo);
                    }
                }
                break;
            case "Defender_Defend_Abandon":
                {
                    IdentityActionDecideCommand(ActionDecisionType.Defender_Defend_Abandon);
                }
                break;
            case "Prophet_Foresee_Excute":
                {
                    int targetSeatNo = m_voteTargetSeatNo;
                    if (targetSeatNo == -1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_needtarget_modeltip");
                        TipModel(text);
                    }
                    else
                    {
                        IdentityActionDecideCommand(ActionDecisionType.Prophet_Foresee_Excute, targetSeatNo);
                    }
                }
                break;
            case "Prophet_Foresee_Abandon":
                {
                    IdentityActionDecideCommand(ActionDecisionType.Prophet_Foresee_Abandon);
                }
                break;
            case "Wolfman_Kill_Excute":
                {
                    int targetSeatNo = m_voteTargetSeatNo;
                    if (targetSeatNo == -1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_needtarget_modeltip");
                        TipModel(text);
                    }
                    else
                    {
                        IdentityActionDecideCommand(ActionDecisionType.Wolfman_Kill_Excute, targetSeatNo);
                    }
                }
                break;
            case "Witch_Magic_Save":
                {
                    IdentityActionDecideCommand(ActionDecisionType.Witch_Magic_Save);
                }
                break;
            case "Witch_Magic_Poison":
                {
                    int targetSeatNo = m_voteTargetSeatNo;
                    if (targetSeatNo == -1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_needtarget_modeltip");
                        TipModel(text);
                    }
                    else
                    {
                        IdentityActionDecideCommand(ActionDecisionType.Witch_Magic_Poison, targetSeatNo);
                    }
                }
                break;
            case "Witch_Magic_Abandon":
                {
                    IdentityActionDecideCommand(ActionDecisionType.Witch_Magic_Abandon);
                }
                break;
        }
    }
    public void IdentityFunctionButton(string paramStr)
    {
        string[] paramArrs = paramStr.Split(';');
        string function = paramArrs[0];
        switch (function)
        {
            case "Prophet_ForeseeLog":
                {
                    IdentityFunctionCommand(IdentityFunctionType.Prophet_ForeseeLog, new JObject());
                }
                break;
            case "Wolfman_BodyLanguage":
                {
                    JObject parms = new JObject();
                    {
                        string type = paramArrs[1];
                        switch (type)
                        {
                            case "Nod":
                                parms.Add("BodyLanguageType", (int)BodyLanguageType.Nod);
                                break;
                            case "LOL":
                                parms.Add("BodyLanguageType", (int)BodyLanguageType.LOL);
                                break;
                        }
                    }
                    IdentityFunctionCommand(IdentityFunctionType.Wolfman_BodyLanguage, parms);
                }
                break;
        }
    }
    public void IdentityFunctionInput(InputField inputField)
    {
        //1
        if (!Input.GetKeyDown(KeyCode.Return))
            return;
        string inputText = inputField.text;
        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();

        //2
        CustomValue valueObj = inputField.gameObject.GetComponent<CustomValue>();
        string function = valueObj.stringValue;
        switch (function)
        {
            case "Wolfman_Whisper":
                {

                    string whisper = inputText.Length > 2 ?
                        inputText.Substring(0, 2) :
                        inputText;
                    JObject parms = new JObject();
                    {
                        parms.Add("Whisper", whisper);
                    }
                    IdentityFunctionCommand(IdentityFunctionType.Wolfman_Whisper, parms);
                }
                break;
        }
    }
    #endregion

    #region UI界面更新
    private enum UILogType { None, Tip, Judge, Wolfman };
    private enum SeatUIMarkType { Default, Ready, Speaking, DisConnected };
    private void ClearGameLog()
    {
        VerticalContainer message_container = panelObj.transform.Find("middle/message_container").GetComponent<VerticalContainer>();
        message_container.ClearItems();
    }
    private void AddGameLog(UILogType logType, string logText)
    {
        VerticalContainer message_container = panelObj.transform.Find("middle/message_container").GetComponent<VerticalContainer>();
        switch (logType)
        {
            case UILogType.Tip:
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string title = localDic.GetLocalText("text.wolfman_p8.logtitle_tip");
                    builder.AppendLine(title);
                    builder.Append(logText);
                    GameObject messageObj = message_container.NewItem(commonLogItemPrefab);
                    Text textObj = messageObj.transform.Find("Text").GetComponent<Text>();
                    textObj.text = builder.ToString();
                }
                break;
            case UILogType.Judge:
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string title = localDic.GetLocalText("text.wolfman_p8.logtitle_judge");
                    builder.AppendLine(title);
                    builder.Append(logText);
                    GameObject messageObj = message_container.NewItem(commonLogItemPrefab);
                    Text textObj = messageObj.transform.Find("Text").GetComponent<Text>();
                    textObj.text = builder.ToString();
                }
                break;
            case UILogType.Wolfman:
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    string title = localDic.GetLocalText("text.wolfman_p8.logtitle_wolfman");
                    builder.AppendLine(title);
                    builder.Append(logText);
                    GameObject messageObj = message_container.NewItem(commonLogItemPrefab);
                    Text textObj = messageObj.transform.Find("Text").GetComponent<Text>();
                    textObj.text = builder.ToString();
                }
                break;
        }
    }
    private void TipModel(string tipText)
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
    private void InfoModel(string titleText, string infoText)
    {
        //1对话框对象克隆
        GameObject modelObj = GameObject.Instantiate(infoModelObj, panelObj.transform);
        //2显示该克隆对象
        ModelDialog modelDialog = modelObj.GetComponent<ModelDialog>();
        Text title_text = modelObj.transform.Find("model/title_text").GetComponent<Text>();
        title_text.text = titleText + "\n";
        Text info_text = modelObj.transform.Find("model/info_text").GetComponent<Text>();
        info_text.text = infoText;
        //3确定后移除该克隆
        modelDialog.ModelShow((code) =>
        {
            Destroy(modelObj);
        });
    }
    private void UpdateVoteTargetMark()
    {
        for (int i = 0; i < playerSeatObjs.Length; i++)
        {
            GameObject seatObj = playerSeatObjs[i];
            Toggle selected_toggle = seatObj.transform.Find("selected_toggle").GetComponent<Toggle>();
            selected_toggle.isOn = false;
        }
    }
    private void SetPlayerSeatStateMark(GameObject seatObj, SeatUIMarkType markType, bool show)
    {
        GameObject ready_mark = seatObj.transform.Find("status_panel/ready_mark").gameObject;
        GameObject speaking_mark = seatObj.transform.Find("status_panel/speaking_mark").gameObject;
        GameObject disconnected_mark = seatObj.transform.Find("status_panel/disconnected_mark").gameObject;
        switch (markType)
        {
            case SeatUIMarkType.Ready:
                ready_mark.SetActive(show);
                break;
            case SeatUIMarkType.Speaking:
                speaking_mark.SetActive(show);
                break;
            case SeatUIMarkType.DisConnected:
                disconnected_mark.SetActive(show);
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
            UpdateBackgroundUI(gamePropertyJObj);
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
        SetPlayerSeatStateMark(seatObj, SeatUIMarkType.Ready, isReady);
        SetPlayerSeatStateMark(seatObj, SeatUIMarkType.Speaking, isSpeaking);
        SetPlayerSeatStateMark(seatObj, SeatUIMarkType.DisConnected, !connected && hasPlayer);
    }
    private void UpdateBottomUI(JObject gamePropertyJObj, JObject playerPropertyJObj)
    {
        //读JSON
        bool isPlaying = (bool)gamePropertyJObj.GetValue("IsPlaying");
        PublicProcessState publicProcess = (PublicProcessState)(int)gamePropertyJObj.GetValue("PublicProcess");
        bool isReady = (bool)playerPropertyJObj.GetValue("IsReady");
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
                Text expection_text = startprepare_panel.transform.Find("expection_text").GetComponent<Text>();
                string u_name = GetIdentityExpectionName(m_identitySelect);
                expection_text.text = string.Format("<color=#FFFF66>【{0}】</color>", localDic.GetLocalText(u_name));

                CountdownBar countdown_bar = startprepare_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                countdown_bar.StartCountdown(3f);
            }
        }
        //游戏控制界面切换
        {
            bool flag = publicProcess == PublicProcessState.GameLoop;
            GameObject gamecontrol_panel = panelObj.transform.Find("bottom/gamecontrol_panel").gameObject;
            gamecontrol_panel.SetActive(flag);
            //切换 标题/身份牌/当前操作流程 的显示
            UpdateCaptionUI(gamePropertyJObj);
            UpdateIdentityUI(playerPropertyJObj);
            UpdateProcessUI(gamePropertyJObj, playerPropertyJObj);
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
        hunter_panel.SetActive(identityType == GameIdentityType.Hunter);
        defender_panel.SetActive(identityType == GameIdentityType.Defender);
        witch_panel.SetActive(identityType == GameIdentityType.Witch);

        GameObject currentPanel = null;
        switch (identityType)
        {
            case GameIdentityType.Villager:
                {
                    currentPanel = villager_panel;
                }
                break;
            case GameIdentityType.Wolfman:
                {
                    currentPanel = wolfman_panel;
                }
                break;
            case GameIdentityType.Prophet:
                {
                    currentPanel = prophet_panel;
                }
                break;
            case GameIdentityType.Hunter:
                {
                    currentPanel = hunter_panel;
                }
                break;
            case GameIdentityType.Defender:
                {
                    currentPanel = defender_panel;
                }
                break;
            case GameIdentityType.Witch:
                {
                    bool antidote = (bool)identityJObj.GetValue("Antidote");
                    bool poison = (bool)identityJObj.GetValue("Poison");
                    witch_panel.transform.Find("antidote_flag").gameObject.SetActive(antidote);
                    witch_panel.transform.Find("poison_flag").gameObject.SetActive(poison);
                    currentPanel = witch_panel;
                }
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
            GameObject prophet_foresee_panel = process_panel.transform.Find("prophet_foresee_panel").gameObject;
            {
                bool flag = currentAction == CurrentActionType.Prophet_Foresee;
                prophet_foresee_panel.SetActive(flag);
                if (flag)
                {
                    float seconds = GetWaitSeconds(waitTimeStamp);
                    CountdownBar countdown_bar = prophet_foresee_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                    countdown_bar.StartCountdown(seconds);
                }
            }
            GameObject wolfman_kill_panel = process_panel.transform.Find("wolfman_kill_panel").gameObject;
            {
                bool flag = currentAction == CurrentActionType.Wolfman_Kill;
                wolfman_kill_panel.SetActive(flag);
                if (flag)
                {
                    float seconds = GetWaitSeconds(waitTimeStamp);
                    CountdownBar countdown_bar = wolfman_kill_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                    countdown_bar.StartCountdown(seconds);
                }
            }
            GameObject witch_magic_panel = process_panel.transform.Find("witch_magic_panel").gameObject;
            {
                bool flag = currentAction == CurrentActionType.Witch_Magic;
                witch_magic_panel.SetActive(flag);
                if (flag)
                {
                    float seconds = GetWaitSeconds(waitTimeStamp);
                    CountdownBar countdown_bar = witch_magic_panel.transform.Find("countdown_bar").GetComponent<CountdownBar>();
                    countdown_bar.StartCountdown(seconds);

                    bool antidote = (bool)identityJObj.GetValue("Antidote");
                    bool poison = (bool)identityJObj.GetValue("Poison");
                    int premonitionNo = (int)identityJObj.GetValue("PremonitionNo");

                    GameObject save_panel = witch_magic_panel.transform.Find("save_panel").gameObject;
                    bool save_panel_flag = antidote && premonitionNo != -1;
                    save_panel.SetActive(save_panel_flag);
                    if (save_panel_flag)
                    {
                        Text premonition_text = save_panel.transform.Find("premonition_text").GetComponent<Text>();
                        string playerno_template = localDic.GetLocalText("template.wolfman_p8.word_playerno");
                        string text = string.Format(playerno_template, premonitionNo + 1);
                        premonition_text.text = text;
                    }

                    GameObject poison_panel = witch_magic_panel.transform.Find("poison_panel").gameObject;
                    bool poison_panel_flag = poison;
                    poison_panel.SetActive(poison_panel_flag);
                }
            }
            GameObject checkidentity_panel = process_panel.transform.Find("checkidentity_panel").gameObject;
            {
                bool flag = gameloopProcess == GameloopProcessState.CheckIdentity && currentAction == CurrentActionType.Default;
                checkidentity_panel.SetActive(flag);
                if (flag)
                {
                    Text expection_text = checkidentity_panel.transform.Find("expection_text").GetComponent<Text>();
                    string u_name = GetIdentityUName(identityType);
                    expection_text.text = string.Format("<color=#FFFF66>【{0}】</color>", localDic.GetLocalText(u_name));

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
        GameObject dayopeneye_caption = caption_panel.transform.Find("dayopeneye_caption").gameObject;
        {
            bool flag = gameloopProcess == GameloopProcessState.DayOpenEye;
            dayopeneye_caption.SetActive(flag);
            if (flag)
            {
                string templateText = localDic.GetLocalText("template.wolfman_p8.dayopeneye_caption");
                string text = string.Format(templateText, dayNumber);
                dayopeneye_caption.GetComponent<Text>().text = text;
            }
        }
    }
    private void UpdateBackgroundUI(JObject gamePropertyJObj)
    {
        DayTime dayTime = (DayTime)(int)gamePropertyJObj.GetValue("DayTime");
        GameObject background = panelObj.transform.Find("background").gameObject;

        GameObject day_image = background.transform.Find("day_image").gameObject;
        {
            bool flag = dayTime == DayTime.Day;
            day_image.SetActive(flag);
        }
        GameObject night_image = background.transform.Find("night_image").gameObject;
        {
            bool flag = dayTime == DayTime.Night;
            night_image.SetActive(flag);
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
    private void IdentityFunctionCommand(IdentityFunctionType functionType, JObject parms)
    {
        JObject cmdJson = new JObject();
        cmdJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "IdentityFunction");
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
    private void IdentityActionDecideCommand(ActionDecisionType decisionType, int targetSeatNo = -1)
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
            AddGameLog(UILogType.Tip, text);
        }
        //设置投票状态
        CurrentActionType currentAction = 
            (CurrentActionType)(int)m_modelViewBinder.GetValue("PlayerProperty.Identity.CurrentAction");
        switch (currentAction)
        {
            case CurrentActionType.Defender_Defend:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            case CurrentActionType.Prophet_Foresee:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            case CurrentActionType.Wolfman_Kill:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            case CurrentActionType.Witch_Magic:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            default:
                m_nowVoting = false;
                UpdateVoteTargetMark();
                break;
        }
    }
    private void ReceiveGameTip(GameTipType tipType, JObject parms)
    {
        switch (tipType)
        {
            case GameTipType.CommonLog:
                {
                    string u_text = (string)parms.GetValue("Text");
                    string text = localDic.GetLocalText(u_text);
                    AddGameLog(UILogType.Tip, text);
                }
                break;
            case GameTipType.PlayerChangeLog:
                {
                    string u_template = (string)parms.GetValue("Template");
                    string templateText = localDic.GetLocalText(u_template);
                    int seatNo = (int)parms.GetValue("SeatNo");
                    AddGameLog(UILogType.Tip, string.Format(templateText, seatNo + 1));
                }
                break;
            case GameTipType.CommonModel:
                {
                    string u_text = (string)parms.GetValue("Text");
                    string text = localDic.GetLocalText(u_text);
                    TipModel(text);
                }
                break;
        }
    }
    private void ReceiveJudgeAnnounce(JudgeAnnounceType announceType, JObject parms)
    {
        switch (announceType)
        {
            case JudgeAnnounceType.Foresee_Result:
                {
                    IdentityCamp campType = (IdentityCamp)(int)parms.GetValue("Camp");
                    string u_campName = GetCampUName(campType);
                    string campName = localDic.GetLocalText(u_campName);
                    int seatNo = (int)parms.GetValue("SeatNo");
                    string template = localDic.GetLocalText("template.wolfman_p8.foresee_result_announce");
                    string text = string.Format(template, seatNo + 1, campName);
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Defender_Defend_Excute:
                {
                    int targetSeatNo = (int)parms.GetValue("SeatNo");
                    string template = localDic.GetLocalText("template.wolfman_p8.actiondecide_defend_excute_response");
                    string text = string.Format(template, targetSeatNo + 1);
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Defender_Defend_Abandon:
                {
                    string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_defend_abandon_response");
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Prophet_Foresee_Excute:
                {
                    int targetSeatNo = (int)parms.GetValue("SeatNo");
                    string template = localDic.GetLocalText("template.wolfman_p8.actiondecide_foresee_excute_response");
                    string text = string.Format(template, targetSeatNo + 1);
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Prophet_Foresee_Abandon:
                {
                    string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_foresee_abandon_response");
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Wolfman_Kill_Excute:
                {
                    int targetSeatNo = (int)parms.GetValue("SeatNo");
                    string template = localDic.GetLocalText("template.wolfman_p8.actiondecide_kill_excute_response");
                    string text = string.Format(template, targetSeatNo + 1);
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Wolfman_Kill_Abandon:
                {
                    string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_kill_abandon_response");
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Witch_Magic_Save:
                {
                    int targetSeatNo = (int)parms.GetValue("SeatNo");
                    string template = localDic.GetLocalText("template.wolfman_p8.actiondecide_magic_save_response");
                    string text = string.Format(template, targetSeatNo + 1);
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Witch_Magic_Poison:
                {
                    int targetSeatNo = (int)parms.GetValue("SeatNo");
                    string template = localDic.GetLocalText("template.wolfman_p8.actiondecide_magic_poison_response");
                    string text = string.Format(template, targetSeatNo + 1);
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.Witch_Magic_Abandon:
                {
                    string text = localDic.GetLocalText("text.wolfman_p8.actiondecide_magic_abandon_response");
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            case JudgeAnnounceType.LastNight_Report:
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    JArray deadReports = (JArray)parms.GetValue("DeadReports");
                    if(deadReports.Count > 0)
                    {
                        string text1 = localDic.GetLocalText("text.wolfman_p8.someonedead_logtip");
                        sb.AppendLine(text1);
                        string template = "----- <color=#ff0000>{0}</color> {1}";
                        string deathWord = localDic.GetLocalText("text.wolfman_p8.word_death");
                        string seatNoTemplate = localDic.GetLocalText("template.wolfman_p8.word_seatno");
                        foreach (JToken token in deadReports)
                        {
                            JObject item = (JObject)token;
                            int seatNo = (int)item.GetValue("SeatNo");
                            string noText = string.Format(seatNoTemplate, seatNo + 1);
                            string text2 = string.Format(template, noText, deathWord);
                            sb.AppendLine(text2);
                        }
                    }
                    else
                    {
                        string text1 = localDic.GetLocalText("text.wolfman_p8.peacefulnight_logtip");
                        sb.Append(text1);
                    }
                    AddGameLog(UILogType.Judge, sb.ToString());
                }
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
    private void ReceiveIdentityFunctionResult(IdentityFunctionType functionType, JObject resultDetail, JArray changeArray)
    {
        switch (functionType)
        {
            case IdentityFunctionType.Prophet_ForeseeLog:
                {
                    string title = localDic.GetLocalText("text.wolfman_p8.foreseelog_title_modelinfo");
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    {
                        string noTemplate = localDic.GetLocalText("template.wolfman_p8.word_seatno");
                        JArray foreseeLogs = (JArray)resultDetail.GetValue("ForeseeLogs");
                        foreach (JToken jToken in foreseeLogs)
                        {
                            JObject logItem = (JObject)jToken;
                            int seatNo = (int)logItem.GetValue("SeatNo");
                            IdentityCamp campType = (IdentityCamp)(int)logItem.GetValue("Camp");
                            string noText = string.Format(noTemplate, seatNo + 1);
                            string u_campName = GetCampUName(campType);
                            string campName = localDic.GetLocalText(u_campName);
                            string lineText = string.Format(
                                "<color=#FF0033>{0}</color> =====> <color=#FF3300>【{1}】</color>", noText, campName);
                            sb.AppendLine(lineText);
                        }
                        if (foreseeLogs.Count == 0)
                        {
                            string nologText = localDic.GetLocalText("text.wolfman_p8.foreseelog_nolog_modelinfo");
                            sb.Append(nologText);
                        }
                    }
                    InfoModel(title, sb.ToString());
                }
                break;
            case IdentityFunctionType.Wolfman_Whisper:
                {
                    string whisper = (string)resultDetail.GetValue("Whisper");
                    int seatNo = (int)resultDetail.GetValue("SeatNo");
                    string noTemplate = localDic.GetLocalText("template.wolfman_p8.word_seatno");
                    string noText = string.Format(noTemplate, seatNo + 1);
                    string text = string.Format(
                        "<color=#FF0033>{0}</color> ----- <color=#FF3300>{1}</color>", noText, whisper);
                    AddGameLog(UILogType.Wolfman, text);
                }
                break;
            case IdentityFunctionType.Wolfman_BodyLanguage:
                {
                    BodyLanguageType bodyLanguageType = (BodyLanguageType)(int)resultDetail.GetValue("BodyLanguageType");
                    int seatNo = (int)resultDetail.GetValue("SeatNo");
                    string noTemplate = localDic.GetLocalText("template.wolfman_p8.word_seatno");
                    string noText = string.Format(noTemplate, seatNo + 1);
                    string bodyLanguageText = "";
                    switch (bodyLanguageType)
                    {
                        case BodyLanguageType.LOL:
                            bodyLanguageText = localDic.GetLocalText("text.wolfman_p8.word_lol");
                            break;
                        case BodyLanguageType.Nod:
                            bodyLanguageText = localDic.GetLocalText("text.wolfman_p8.word_nod");
                            break;
                    }
                    string text = string.Format(
                        "<color=#FF0033>{0}</color> ----- <color=#FF3300>{1}</color>", noText, bodyLanguageText);
                    AddGameLog(UILogType.Wolfman, text);
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
                        AddGameLog(UILogType.Judge, text);
                    }
                }
                break;
            case PublicProcessState.GameLoop:
                {
                    //法官通告 开始游戏
                    {
                        ClearGameLog();
                        string text = localDic.GetLocalText("text.wolfman_p8.gamestart_logtip");
                        AddGameLog(UILogType.Judge, text);
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
                    string u_name = GetIdentityUName(identityType);
                    string templateText = localDic.GetLocalText("template.wolfman_p8.checkidentity_logjudge");
                    string text = string.Format(templateText, localDic.GetLocalText(u_name));
                    AddGameLog(UILogType.Judge, text);
                }
                break;
            //天黑闭眼
            case GameloopProcessState.NightCloseEye:
                {
                    int dayNumber = (int)m_modelViewBinder.GetValue("GameProperty.DayNumber");
                    if (dayNumber == 1)
                    {
                        string text = localDic.GetLocalText("text.wolfman_p8.firstnight_logjudge");
                        AddGameLog(UILogType.Judge, text);
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

        //设置投票状态
        CurrentActionType currentAction =
            (CurrentActionType)(int)m_modelViewBinder.GetValue("PlayerProperty.Identity.CurrentAction");
        switch (currentAction)
        {
            case CurrentActionType.Defender_Defend:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            case CurrentActionType.Prophet_Foresee:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            case CurrentActionType.Wolfman_Kill:
                m_nowVoting = true;
                UpdateVoteTargetMark();
                break;
            case CurrentActionType.Witch_Magic:
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
        string u_name = "text.wolfman_p8.word_random";
        switch (identityExpection)
        {
            case "Villager":
                u_name = "text.wolfman_p8.word_villager";
                break;
            case "Wolfman":
                u_name = "text.wolfman_p8.word_wolfman";
                break;
            case "Prophet":
                u_name = "text.wolfman_p8.word_prophet";
                break;
            case "Hunter":
                u_name = "text.wolfman_p8.word_hunter";
                break;
            case "Defender":
                u_name = "text.wolfman_p8.word_defender";
                break;
            case "Witch":
                u_name = "text.wolfman_p8.word_witch";
                break;
        }
        return u_name;
    }
    private string GetIdentityUName(GameIdentityType identityType)
    {
        string u_name = "";
        switch (identityType)
        {
            case GameIdentityType.Villager:
                u_name = "text.wolfman_p8.word_villager";
                break;
            case GameIdentityType.Wolfman:
                u_name = "text.wolfman_p8.word_wolfman";
                break;
            case GameIdentityType.Prophet:
                u_name = "text.wolfman_p8.word_prophet";
                break;
            case GameIdentityType.Hunter:
                u_name = "text.wolfman_p8.word_hunter";
                break;
            case GameIdentityType.Defender:
                u_name = "text.wolfman_p8.word_defender";
                break;
            case GameIdentityType.Witch:
                u_name = "text.wolfman_p8.word_witch";
                break;
        }
        return u_name;
    }
    private string GetCampUName(IdentityCamp campType)
    {
        string u_name = "";
        switch (campType)
        {
            case IdentityCamp.Villager:
                u_name = "text.wolfman_p8.word_wolfmancamp";
                break;
            case IdentityCamp.Wolfman:
                u_name = "text.wolfman_p8.word_villagercamp";
                break;
        }
        return u_name;
    }
    private float GetWaitSeconds(long waitTimestamp)
    {
        System.TimeSpan span = new System.DateTime(waitTimestamp) - System.DateTime.UtcNow;
        float seconds = (float)span.TotalSeconds;
        return seconds;
    }
    #endregion
}
