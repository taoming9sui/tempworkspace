using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;
    public IDictionary<string, GameInfo> GameInfos { get; private set; }

    public string serverAddress = "118.113.201.76:8848/Fuck";
    public GameObject activitiesObj;
    public GameActivity currentActivity;
    public Camera defaultCamera;

    private WebSocket m_websocket = null;
    private string m_playerId = "";
    private string m_password = "";
    private ConcurrentQueue<System.Action> m_invokeQueue = new ConcurrentQueue<System.Action>();


    private void Awake()
    {
        Instance = this;

        GameInfos = new Dictionary<string, GameInfo>();
        GameInfos["Wolfman"] = new GameInfo("Wolfman", "狼人杀", "Wolfman");
    }

    private void Start()
    {
        SetActivity("MainTheme");
    }

    private void Update()
    {
        ExcuteInvoke();
    }

    private void OnApplicationQuit()
    {
        if (m_websocket != null)
        {
            if (m_websocket.IsAlive)
            {
                m_websocket.Close();
            }
        }
    }

    private void PushInvoke(System.Action action)
    {
        m_invokeQueue.Enqueue(action);
    }
    private void ExcuteInvoke()
    {
        System.Action action;
        while(m_invokeQueue.TryDequeue(out action))
        {
            action();
        }
    }
    private void M_websocket_OnOpen(object sender, System.EventArgs e)
    {
        PushInvoke(() =>
        {
            if(currentActivity != null)
                currentActivity.OnConnect();
        });

    }
    private void M_websocket_OnMessage(object sender, MessageEventArgs e)
    {
        PushInvoke(() =>
        {
            try
            {
                JObject jsonData = JObject.Parse(e.Data);
                if (currentActivity != null)
                    currentActivity.OnMessage(jsonData);
            }
            catch (System.Exception ex) { Debug.Log(ex.Message); }
        });
    }
    private void M_websocket_OnClose(object sender, CloseEventArgs e)
    {
        PushInvoke(() =>
        {
            if (currentActivity != null)
                currentActivity.OnDisconnect();
        });
    }
    private void M_websocket_OnError(object sender, ErrorEventArgs e)
    {
    }


    #region 对外调用
    public bool SocketConnect()
    {
        if (m_websocket != null)
            if (m_websocket.IsAlive)
                return true;

        m_websocket = new WebSocket("ws://" + serverAddress);
        m_websocket.OnOpen += M_websocket_OnOpen;
        m_websocket.OnClose += M_websocket_OnClose;
        m_websocket.OnMessage += M_websocket_OnMessage;
        m_websocket.OnError += M_websocket_OnError;
        m_websocket.Connect();

        return m_websocket.IsAlive;
    }
    public void PlayerLogin(string playerId, string password)
    {
        JObject loginJson = new JObject();
        loginJson.Add("Type", "Client_Center");
        JObject data = new JObject();
        {
            data.Add("Action", "Login");
            data.Add("PlayerId", playerId);
            data.Add("Password", password);
        }
        loginJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(loginJson);
    }
    public void PlayerLogin()
    {
        JObject loginJson = new JObject();
        loginJson.Add("Type", "Client_Center");
        JObject data = new JObject();
        {
            data.Add("Action", "Login");
            data.Add("PlayerId", m_playerId);
            data.Add("Password", m_password);
        }
        loginJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(loginJson);
    }
    public void SendMessage(JObject jsonData)
    {
        if (m_websocket != null)
        {
            if (m_websocket.IsAlive)
            {
                m_websocket.Send(jsonData.ToString());
            }
        }    
    }
    public void SetActivity(string activity, Object param = null)
    {
        if(this.currentActivity != null)
        {
            this.currentActivity.OnActivityDisabled();
            this.currentActivity.gameObject.SetActive(false);
        }

        Transform activityTran = this.activitiesObj.transform.Find(activity);
        if(activityTran != null)
        {
            GameActivity gameActivity = activityTran.GetComponent<GameActivity>();
            this.currentActivity = gameActivity;
            gameActivity.gameObject.SetActive(true);
            gameActivity.OnActivityEnabled(param);
        }
        else
        {
            Debug.Log("未定义的Activity");
        }
    }
    #endregion

    #region 数据类
    public class GameInfo
    {
        public GameInfo(string id, string name, string activity)
        {
            GameId = id;
            GameName = name;
            GameActivity = activity;
        }

        public string GameId { get; private set; }
        public string GameName { get; private set; }
        public string GameActivity { get; private set; }
    }
    #endregion

}
