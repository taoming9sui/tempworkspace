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

    #region 需要状态同步的数据
    private class GameIdentity
    {
        
    }
    private GameIdentity m_gameIdentity = null;
    private string m_playerProgress = "Ready";
    private bool m_isReady = false;
    private class PlayerInfo
    {
        public int SeatNo = 0;
        public bool HasPlayer = false;
        public int HeadNo = 0;
        public string PlayerName = "空座位";
        public bool isSpeaking = false;
        public bool isReady = false;
    }
    private PlayerInfo[] m_playerInfos = new PlayerInfo[8];
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
                        ChangePlayerInfo((JObject)data.GetValue("Content"));
                        break;
                    case "ReadyResponse":
                        ReadyResponse((bool)data.GetValue("Ready"));
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
        ReadyRequest(true);
    }
    public void CancelReadyButton()
    {
        ReadyRequest(false);
    }
    #endregion

    #region 初始化
    private void InitPlayerInfos()
    {
        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            PlayerInfo info = new PlayerInfo();
            info.SeatNo = i;
            m_playerInfos[i] = info;
        }
    }
    #endregion

    #region 界面
    private void UpdatePlayerHead()
    {
        foreach (PlayerInfo info in m_playerInfos)
        {
            PlayerHead head = playerHeads[info.SeatNo];
            if (info.HasPlayer)
            {
                Texture2D texture = GameManager.Instance.PlayerHeads[info.HeadNo];
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                head.SetPlayer(info.SeatNo, sprite, info.PlayerName);
                head.SetStasusMark(PlayerHead.StasusMark.Ready, info.isReady);
            }
            else
            {
                playerHeads[info.SeatNo].SetPlayer(info.SeatNo);
                head.SetStasusMark(PlayerHead.StasusMark.Ready, info.isReady);
            }
        }
    }
    private void UpdateProgressGUI()
    {
        switch (m_playerProgress)
        {
            case "Ready":
                {
                    GameObject getready_panel = panelObj.transform.Find("bottom/getready_panel").gameObject;
                    GameObject getready_button = getready_panel.transform.Find("getready_button").gameObject;
                    GameObject cancelready_button = getready_panel.transform.Find("cancelready_button").gameObject;
                    getready_button.SetActive(!m_isReady);
                    cancelready_button.SetActive(m_isReady);
                }
                break;
        }
    }
    #endregion

    #region 操作指令
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
        //1同步座位状态
        {
            JArray playerSeatArray = (JArray)content.GetValue("PlayerSeatArray");
            for (int i = 0; i < playerSeatArray.Count; i++)
            {
                //解析JSON
                JObject jobj = (JObject)playerSeatArray[i];
                int seatNo = (int)jobj.GetValue("SeatNo");
                bool hasPlayer = (bool)jobj.GetValue("HasPlayer");
                bool isReady = (bool)jobj.GetValue("IsReady");
                int headNo = (int)jobj.GetValue("HeadNo");
                string name = (string)jobj.GetValue("Name");
                //更新数据
                PlayerInfo info = m_playerInfos[seatNo];
                info.HasPlayer = hasPlayer;
                info.isReady = isReady;
                info.PlayerName = name;
                info.HeadNo = headNo;
                //更新界面
                UpdatePlayerHead();
            }
        }
        //2同步游戏状态
        {
            //解析JSON
            string progress = (string)content.GetValue("PlayerProgress");
            //更新数据
            m_playerProgress = progress;
            //更新界面
            UpdateProgressGUI();
        }
    }
    private void ChangePlayerInfo(JObject content)
    {
        //更新数据
        string change = (string)content.GetValue("Change");
        switch (change)
        {
            case "Join":
                {
                    int seatNo = (int)content.GetValue("SeatNo");
                    int headNo = (int)content.GetValue("HeadNo");
                    string name = (string)content.GetValue("Name");
                    PlayerInfo info = m_playerInfos[seatNo];
                    info.HasPlayer = true;
                    info.PlayerName = name;
                    info.HeadNo = headNo;
                }
                break;
            case "Leave":
                {
                    int seatNo = (int)content.GetValue("SeatNo");
                    PlayerInfo info = new PlayerInfo();
                    info.SeatNo = seatNo;
                    m_playerInfos[seatNo] = info;
                }
                break;
            case "Ready":
                {
                    int seatNo = (int)content.GetValue("SeatNo");
                    bool isReady = (bool)content.GetValue("IsReady");
                    PlayerInfo info = m_playerInfos[seatNo];
                    info.isReady = isReady;
                }
                break;
        }
        //更新界面
        UpdatePlayerHead();
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
    private void ReadyRequest(bool ready)
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
        m_isReady = ready;
        UpdateProgressGUI();
    }
    #endregion

}
