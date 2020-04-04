using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;
    public bool HasConnection { get { return m_playersocket == null ? false : m_playersocket.IsAlive; } }

    public string serverIP = "localhost";
    public int serverPort = 8888;
    public string serverPath = "Fuck";
    public GameObject activitiesObj;
    public GameActivity currentActivity;
    public Camera defaultCamera;

    private PlayerSocket m_playersocket = null;
    private string m_playerId = "";
    private string m_password = "";
    private ConcurrentQueue<System.Action> m_invokeQueue = new ConcurrentQueue<System.Action>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameObject prefab = ResourceManager.Instance.Local.ActivityInfoSet["MainTheme"].ActivityPrefab;
        GameManager.Instance.SetActivity(prefab);
    }

    private void Update()
    {
        ExcuteInvoke();
    }

    private void OnApplicationQuit()
    {
        if (m_playersocket != null)
        {
            if (m_playersocket.IsAlive)
            {
                m_playersocket.Close();
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
    private void M_playersocket_OnOpen(object sender, System.EventArgs e)
    {
        PushInvoke(() =>
        {
            try
            {
                if (currentActivity != null)
                    currentActivity.OnConnect();
            }
            catch(System.Exception ex) { Debug.LogError(ex); }
        });
    }
    private void M_playersocket_OnMessage(object sender, MessageEventArgs e)
    {
        PushInvoke(() =>
        {
            try
            {
                JObject jsonData = JObject.Parse(e.Data);
#if DEBUG
                Debug.Log(jsonData.ToString());
#endif
                if (currentActivity != null)
                    currentActivity.OnMessage(jsonData);
            }
            catch (System.Exception ex) { Debug.LogError(ex); }
        });
    }
    private void M_playersocket_OnClose(object sender, CloseEventArgs e)
    {
        PushInvoke(() =>
        {
            try
            {
                if (currentActivity != null)
                    currentActivity.OnDisconnect();
            }
            catch (System.Exception ex) { Debug.LogError(ex); }
        });
    }
    private void M_playersocket_OnError(object sender, ErrorEventArgs e)
    {
        try
        {
            if (currentActivity != null)
                currentActivity.OnConnectError();
        }
        catch (System.Exception ex) { Debug.LogError(ex); }
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
    private IEnumerator WaitConnect(PlayerSocket playersocket, System.Action callback)
    {
        float t = 0f;
        float delayTime = (float)playersocket.WaitTime.TotalSeconds;
        bool success = true;
        while (!playersocket.IsAlive)
        {
            t += Time.deltaTime;
            if (t > delayTime)
            {
                success = false;
                break;
            }
            yield return 0;
        }
        if(success)
            callback();
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
    public void SetActivity(GameObject activityPrefab, Object param = null)
    {
        if (this.currentActivity != null)
        {
            this.currentActivity.OnActivityDisabled();
            Destroy(this.currentActivity.gameObject);
        }

        GameObject activityObj = GameObject.Instantiate(activityPrefab, activitiesObj.transform);
        GameActivity activity = activityObj.GetComponent<GameActivity>();
        this.currentActivity = activity;
        activity.OnActivityEnabled(param);
    }
    public void SocketConnect(System.Action callback = null)
    {
        if (m_playersocket != null)
        {
            if (m_playersocket.IsAlive)
            {
                if(callback != null) callback();
                return;
            }
        }

        m_playersocket = new PlayerSocket(string.Format("ws://{0}:{1}/{2}", serverIP, serverPort, serverPath));
        m_playersocket.OnOpen += M_playersocket_OnOpen;
        m_playersocket.OnClose += M_playersocket_OnClose;
        m_playersocket.OnMessage += M_playersocket_OnMessage;
        m_playersocket.OnError += M_playersocket_OnError;
        m_playersocket.ConnectAsync();
        if(callback != null) StartCoroutine(WaitConnect(m_playersocket, callback));
    }
    public void Ping(System.Action<int> callback)
    {
        Ping ping = new Ping(serverIP);
        //HARDCODE 最高3s延迟
        StartCoroutine(WaitPing(ping, 3f, callback));
    }
    public void PlayerLogin(string playerId, string password)
    {
        //1保存用于登陆的凭据 方便重连
        m_playerId = playerId;
        m_password = password;
        //2
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
        if (m_playersocket != null)
        {
            if (m_playersocket.IsAlive)
            {
#if DEBUG
                Debug.Log(jsonData.ToString());
#endif
                m_playersocket.Send(jsonData.ToString());
            }
        }
    }
    #endregion


}
