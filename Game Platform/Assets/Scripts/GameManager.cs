using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;

    public string serverAddress = "118.113.201.76:8848/Fuck";
    public GameActivity currentActivity;
    public Camera defaultCamera;

    private WebSocket m_websocket;
    private string m_playerId;
    private string m_password;
    private ConcurrentQueue<System.Action> m_invokeQueue = new ConcurrentQueue<System.Action>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetActivity(currentActivity);
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

    public void SetUserCredential(string playerId, string password)
    {
        m_playerId = playerId;
        m_password = password;
    }
    public bool SocketConnect()
    {
        if(m_websocket != null)
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
    public void SetActivity(GameActivity gameActivity, Object param = null)
    {
        if(this.currentActivity != null)
        {
            this.currentActivity.OnActivityDisabled();
            this.currentActivity.gameObject.SetActive(false);
        }
        this.currentActivity = gameActivity;
        gameActivity.gameObject.SetActive(true);
        gameActivity.OnActivityEnabled(param);
    }
    #endregion

}
