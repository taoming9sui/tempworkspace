using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolfman_P8 : GameActivity
{
    public GameObject cameraObj;
    public GameObject panelObj;
    public GameObject sceneObj;
    public PlayerHead[] playerHeads;

    #region unity触发器
    private void Awake()
    {
        //初始化头像
        InitPlayerHeads();
    }
    private void Update()
    {
    }
    #endregion

    #region 活动触发器
    public override void OnActivityEnabled(Object param)
    {
        //初始化游戏 从服务器状态同步
        RequestGameStatus();
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
                        SynchronizeGameState((JObject)data.GetValue("Content"));
                        break;
                    case "PlayerChange":
                        PlayerHeadChange((JObject)data.GetValue("Content"));
                        break;
                    case "GetReadyResponse":
                        GetReadySuccess();
                        break;
                    case "CancelReadyResponse":
                        CancelReadySuccess();
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
                        ExitGameSuccess();
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
    public void ExitButton()
    {
        ExitGameRequest();
    }
    public void GetReadyButton()
    {
        GetReadyRequest();
    }
    public void CancelReadyButton()
    {
        CancelReadyRequest();
    }
    #endregion

    private void InitPlayerHeads()
    {
        for(int i=0; i<playerHeads.Length; i++)
            playerHeads[i].SetPlayer(i, null, null);
    }
    private void PlayerHeadChange(JObject content)
    {
        string change = (string)content.GetValue("Change");
        switch (change)
        {
            case "Join":
                {
                    int seatNo = (int)content.GetValue("SeatNo");
                    int headNo = (int)content.GetValue("HeadNo");
                    string name = (string)content.GetValue("Name");
                    Texture2D texture = GameManager.Instance.PlayerHeads[headNo];
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    playerHeads[seatNo].SetPlayer(seatNo, sprite, name);
                }
                break;
            case "Leave":
                {
                    int seatNo = (int)content.GetValue("SeatNo");
                    playerHeads[seatNo].SetPlayer(seatNo, null, null);
                }
                break;
            case "Ready":
                {
                    int seatNo = (int)content.GetValue("SeatNo");
                    bool isReady = (bool)content.GetValue("IsReady");
                    playerHeads[seatNo].SetStasusMark(PlayerHead.StasusMark.Ready, isReady);
                }
                break;
        }
    }
    private void RequestGameStatus()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "RequestGameStatus");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void SynchronizeGameState(JObject content)
    {
        //1同步玩家头像状态
        {
            JArray playerHeadArray = (JArray)content.GetValue("PlayerHeadArray");
            for (int i = 0; i < playerHeadArray.Count; i++)
            {
                JToken jToken = playerHeadArray[i];
                if (jToken.Type == JTokenType.Object)
                {
                    JObject jobj = (JObject)jToken;
                    int seatNo = (int)jobj.GetValue("SeatNo");
                    int headNo = (int)jobj.GetValue("HeadNo");
                    string name = (string)jobj.GetValue("Name");
                    Texture2D texture = GameManager.Instance.PlayerHeads[headNo];
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    playerHeads[seatNo].SetPlayer(seatNo, sprite, name);
                }
                else
                {
                    playerHeads[i].SetPlayer(i, null, null);
                }
            }
        }
        //2同步游戏进程
    }
    private void ExitGameRequest()
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
    private void ExitGameSuccess()
    {
        //返回大厅
        GameManager.Instance.SetActivity("Hall");
    }
    private void GetReadyRequest()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "GetReady");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void GetReadySuccess()
    {
        GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
        GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
        GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
        getready_button.SetActive(false);
        cancelready_button.SetActive(true);
    }
    private void CancelReadyRequest()
    {
        JObject requestJson = new JObject();
        requestJson.Add("Type", "Client_Room");
        JObject data = new JObject();
        {
            data.Add("Action", "CancelReady");
        }
        requestJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(requestJson);
    }
    private void CancelReadySuccess()
    {
        GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
        GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
        GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
        getready_button.SetActive(true);
        cancelready_button.SetActive(false);
    }

}
