using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;
    public IDictionary<string, GameInfo> GameInfoSet { get; private set; }
    public Texture2D[] PlayerHeads { get; private set; }
    public bool HasConnection { get { return m_websocket == null ? false : m_websocket.IsAlive; } }

    public string serverIP = "localhost";
    public int serverPort = 8888;
    public string serverPath = "Fuck";
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
        //初始化 游戏记录信息
        {
            GameInfoSet = new Dictionary<string, GameInfo>();
            GameInfoSet["Wolfman"] = new GameInfo("Wolfman", "狼人杀", "Wolfman");
        }
        //初始化 头像记录信息
        {
            PlayerHeads = new Texture2D[0];
            Texture2D[] textures = Resources.LoadAll<Texture2D>("Profile Pictrues");
            PlayerHeads = textures;
        }
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
        while (m_invokeQueue.TryDequeue(out action))
        {
            action();
        }
    }
    private void M_websocket_OnOpen(object sender, System.EventArgs e)
    {
        PushInvoke(() =>
        {
            if (currentActivity != null)
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
    private IEnumerator WaitPing(Ping ping, float delayTime, System.Action<int> callback)
    {
        float t = 0f;
        bool success = true;
        while (!ping.isDone)
        {
            t += Time.deltaTime;
            if (t > delayTime)
            {
                success = false;
                break;
            }            
            yield return 0;
        }
        if (success)
            callback(ping.time);
        else
            callback(-1);
    }

    #region 对外调用
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void SetActivity(string activity, Object param = null)
    {
        if (this.currentActivity != null)
        {
            this.currentActivity.OnActivityDisabled();
            this.currentActivity.gameObject.SetActive(false);
        }

        Transform activityTran = this.activitiesObj.transform.Find(activity);
        if (activityTran != null)
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
    public bool SocketConnect()
    {
        if (m_websocket != null)
            if (m_websocket.IsAlive)
                return true;

        m_websocket = new WebSocket(string.Format("ws://{0}:{1}/{2}", serverIP, serverPort, serverPath));
        m_websocket.OnOpen += M_websocket_OnOpen;
        m_websocket.OnClose += M_websocket_OnClose;
        m_websocket.OnMessage += M_websocket_OnMessage;
        m_websocket.OnError += M_websocket_OnError;
        m_websocket.Connect();

        return m_websocket.IsAlive;
    }
    public void Ping(System.Action<int> callback)
    {
        Ping ping = new Ping(serverIP);
        //HARDCODE 最高3s延迟
        StartCoroutine(WaitPing(ping, 3f, callback));
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
    public void PlayerLogout()
    {
        JObject logoutJson = new JObject();
        logoutJson.Add("Type", "Client_Center");
        JObject data = new JObject();
        {
            data.Add("Action", "Logout");
        }
        logoutJson.Add("Data", data); ;
        GameManager.Instance.SendMessage(logoutJson);
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
