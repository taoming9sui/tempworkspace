using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Wolfman_P8 : GameActivity
{
    public GameObject cameraObj;
    public GameObject panelObj;
    public GameObject sceneObj;
    public AudioPlayer audioPlayer;
    public LocalizationDictionary localDic;
    public GameObject[] playerSeatObjs;
    public Sprite defaultHead;

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
        SynchronizeStateCommand();
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
                    case "SynchronizeState":
                        ReceiveSynchronizeState(
                            (JArray)data.SelectToken("Content.ModelViewChange"));
                        break;
                    case "SeatChange":
                        ReceiveSeatChange(
                            (SeatChangeType)(int)data.SelectToken("Content.ChangeType"), 
                            (JArray)data.SelectToken("Content.ModelViewChange"));
                        break;
                    case "GameTip":
                        ReceiveGameTip(
                            (GameTipType)(int)data.SelectToken("Content.TipType"),
                            (string)data.SelectToken("Content.TipText"));
                        break;
                    case "JudgeAnnounce":

                        break;
                    case "BaseFunctionResult":
                        ReceiveBaseFunctionResult(
                            (BaseFunctionType)(int)data.SelectToken("Content.FunctionType"),
                            (JObject)data.SelectToken("Content.ResultDetail"),
                            (JArray)data.SelectToken("Content.ModelViewChange"));
                        break;
                    case "IdentityFunctionResult":

                        break;
                    case "PublicProcess":
                        ReceivePublicProcess(
                            (PublicProcessState)(int)data.SelectToken("Content.ProcessState"),
                            (JArray)data.SelectToken("Content.ModelViewChange"));
                        break;
                    case "GameloopProcess":
                        ReceiveGameloopProcess(
                            (GameloopProcessState)(int)data.SelectToken("Content.ProcessState"),
                            (JArray)data.SelectToken("Content.ModelViewChange"));
                        break;
                    case "IdentityTranslate":
                        ReceiveIdentityTranslate(
                            (IdentityTranslateType)(int)data.SelectToken("Content.TranslateType"),
                            (JArray)data.SelectToken("Content.ModelViewChange"));
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

}
