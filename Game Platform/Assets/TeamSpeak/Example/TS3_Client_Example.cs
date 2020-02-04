using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using anyID = System.UInt16;
using uint64 = System.UInt64;
using IntPtr = System.IntPtr;
using System.Runtime.InteropServices;

public class TS3_Client_Example : MonoBehaviour
{
    // gui objects
    public GameObject connection_Pannel;
    public GameObject connected_Pannel;
    public GameObject create_Pannel;
    public GameObject switch_Pannel;
    public static Text UI_Message;

    // connection parameter
    public string serverAddress = "127.0.0.1";
    public int serverPort = 9987;
    public string serverPassword = "secret";
    public string nickName = "client";
    private string[] defaultChannel = new string[] { "Channel_1", "" };
    public string defaultChannelPassword = null;
    public bool pushCtrlToTalk = false;

    // create channel parameter
    public string channelName = "";
    public string channelTopic = "";
    public string channelDescription = "";
    public string channelPassword = "";

    // switch channel parameter
    public int channelIDX = 1;
    public string channelPW = "secret";

    private static TeamSpeakClient ts_client;

    public static bool didNotFindServer = false;

    private static List<int> onTalkStatusChange_status = new List<int>();
    private static List<string> onTalkStatusChange_ClientName = new List<string>();
    private static string onTalkStatusChange_labelText = "";

    void Awake()
    {
        // Just for the UI 
        UI_Message = GameObject.FindGameObjectWithTag("Message").GetComponent<Text>();

        connection_Pannel = GameObject.FindGameObjectWithTag("con_01");
        connected_Pannel = GameObject.FindGameObjectWithTag("con_02");
        create_Pannel = GameObject.FindGameObjectWithTag("con_03");
        switch_Pannel = GameObject.FindGameObjectWithTag("con_04");

        connected_Pannel.SetActive(false);
        create_Pannel.SetActive(false);
        switch_Pannel.SetActive(false);
    }
    // Use this for initialization
    void Start()
    {
        //Getting the client		
        ts_client = TeamSpeakClient.GetInstance();

        //Attaching functions to the TeamSpeak callbacks.
        TeamSpeakCallbacks.onTalkStatusChangeEvent += onTalkStatusChangeEvent;

        //enabling logging of some pre defined errors.
        TeamSpeakClient.logErrors = true;
    }

    //Starting the Client
    private void connect()
    {
        ts_client.StartClient(serverAddress, (uint)serverPort, serverPassword, nickName, ref defaultChannel, defaultChannelPassword);
        // UI Elements
        if (TeamSpeakClient.started == true)
        {
            connection_Pannel.SetActive(false);
            connected_Pannel.SetActive(true);
        }
    }

    // Disconnect the Client
    private void disconnect()
    {
        var _leaveMessage = "Bye bye";
        ts_client.StopConnection(_leaveMessage);
        // UI Elements
        if (TeamSpeakClient.started == true)
        {
            connection_Pannel.SetActive(true);
            connected_Pannel.SetActive(false);
            create_Pannel.SetActive(false);
            switch_Pannel.SetActive(false);
        }
    }

    // an example how to switch between Push to Talk / Voice activation
    private void toggleVoice()
    {
        if (pushCtrlToTalk == true)
        {
            pushCtrlToTalk = false;
            ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_ACTIVE);
            ts_client.FlushClientSelfUpdates();
        }
        else
        {
            pushCtrlToTalk = true;
            ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_DEACTIVATED);
            ts_client.FlushClientSelfUpdates();
        }
    }

    // an example how to move between channels
    private void switchChannel()
    {
        ts_client.RequestClientMove(ts_client.GetClientID(), (ulong)channelIDX, channelPW);
    }

    //an example on how to create channel in unity
    private void createChannel(uint64 scHandlerID)
    {
        uint64 _channelID = 0;
        /* Set data of new channel. Use channelID of 0 for creating channels. */

        if (channelName != "")
            ts_client.SetChannelVariableAsString(scHandlerID, _channelID, ChannelProperties.CHANNEL_NAME, channelName);

        if (channelTopic != "")
            ts_client.SetChannelVariableAsString(scHandlerID, _channelID, ChannelProperties.CHANNEL_TOPIC, channelTopic);

        if (channelDescription != "")
            ts_client.SetChannelVariableAsString(scHandlerID, _channelID, ChannelProperties.CHANNEL_DESCRIPTION, channelDescription);

        ts_client.SetChannelVariableAsInt(scHandlerID, _channelID, ChannelProperties.CHANNEL_FLAG_PERMANENT, 1);
        ts_client.SetChannelVariableAsInt(scHandlerID, _channelID, ChannelProperties.CHANNEL_FLAG_SEMI_PERMANENT, 0);

        if (channelPassword != "")
            ts_client.SetChannelVariableAsString(scHandlerID, _channelID, ChannelProperties.CHANNEL_PASSWORD, channelPassword);

        /* Flush changes to server */
        ts_client.FlushChannelCreation(scHandlerID, 0);
    }

    // an example on how to use different TeamSpeakClient functions.
    private void debugTest()
    {
        //retrieving information.
        Debug.Log("Client nickname: " + ts_client.GetClientSelfVariableAsString(ClientProperties.CLIENT_NICKNAME));
        Debug.Log("Client version: " + ts_client.GetClientLibVersion());
        Debug.Log("Server name: " + ts_client.GetServerVariableAsString(VirtualServerProperties.VIRTUALSERVER_NAME));
        Debug.Log("Max number of clients" + ts_client.GetServerVariableAsInt(VirtualServerProperties.VIRTUALSERVER_MAXCLIENTS));
        ts_client.SetClientSelfVariableAsString(ClientProperties.CLIENT_NICKNAME, "newNickName");
        ts_client.FlushClientSelfUpdates();
        Debug.Log("New client nickname: " + ts_client.GetClientSelfVariableAsString(ClientProperties.CLIENT_NICKNAME));
        anyID clientID = ts_client.GetClientID();
        uint64 channelID = ts_client.GetChannelOfClient(clientID);

        List<uint64> serverConnectionHandlerList = ts_client.GetServerConnectionHandlerList();
        Debug.Log("Channel name: " + ts_client.GetChannelVariableAsString(serverConnectionHandlerList[0], channelID, ChannelProperties.CHANNEL_NAME));
        List<anyID> channelClientIDs = ts_client.GetChannelClientList(serverConnectionHandlerList[0], channelID);
        if (channelClientIDs != null)
        {
            foreach (anyID id in channelClientIDs)
            {
                Debug.Log("Client id in channel: " + id);
            }
        }

        ts_client.SetLogVerbosity(TeamSpeakInterface.LogLevel.LogLevel_INFO);

        List<string> captureModes = ts_client.GetCaptureModeList();
        foreach (string s in captureModes)
        {
            Debug.Log("available capture mode: " + s);
        }

        string defaultCaptureMode = ts_client.GetDefaultCaptureMode();
        Debug.Log("default capture mode: " + defaultCaptureMode);
        List<TeamSpeakClient.TeamSpeakSoundDevice> captureDevices = ts_client.GetCaptureDeviceList(defaultCaptureMode);
        foreach (TeamSpeakClient.TeamSpeakSoundDevice soundDevice in captureDevices)
        {
            Debug.Log("Capture Device: " + soundDevice.deviceID + "->" + soundDevice.deviceName);
        }
        TeamSpeakClient.TeamSpeakSoundDevice defaultCaptureDevice = ts_client.GetDefaultCaptureDevice(defaultCaptureMode);
        Debug.Log("Default capture device: " + defaultCaptureDevice.deviceID + "->" + defaultCaptureDevice.deviceName);

        string bitrate = ts_client.GetEncodeConfigValue(ts_client.GetServerConnectionHandlerID(), TeamSpeakClient.EncodeConfig.bitrate);
        Debug.Log("Encode bitrate: " + bitrate);

        ts_client.LogMessage("Test log message", TeamSpeakInterface.LogLevel.LogLevel_INFO, "client", ts_client.GetServerConnectionHandlerID());

        //Playing a wave file works on android but the StreamingAssets folder is compressed on Android, therefor the example doesn't support this.
        if (Application.platform != RuntimePlatform.Android)
        {
            string path = Application.streamingAssetsPath;
            path = System.IO.Path.Combine(path, "wavExample.wav");
            uint64 waveHandle;
            ts_client.PlayWaveFileHandle(ts_client.GetServerConnectionHandlerID(), path, true, out waveHandle);
            StartCoroutine(PauseWaveHandleIn5Seconds(waveHandle));
        }
    }

    #region Input Events
    public void set_Nickname(InputField _nick)
    {
        nickName = _nick.text;
        Debug.Log(nickName);
    }

    public void set_IP(InputField _ip)
    {
        serverAddress = _ip.text;
        Debug.Log(serverAddress);
    }

    public void set_port(InputField _port)
    {
        serverPort = int.Parse(_port.text);
        Debug.Log(serverPort);
    }

    public void set_password(InputField _pw)
    {
        serverPassword = _pw.text;
        Debug.Log(serverPassword);
    }

    public void set_channelName(InputField _cn)
    {
        channelName = _cn.text;
        Debug.Log(channelName);
    }

    public void set_channelTopic(InputField _ct)
    {
        channelTopic = _ct.text;
        Debug.Log(channelTopic);
    }

    public void set_channelDescription(InputField _cd)
    {
        channelDescription = _cd.text;
        Debug.Log(channelDescription);
    }

    public void set_channelPassword(InputField _pw)
    {
        channelPassword = _pw.text;
        Debug.Log(channelPassword);
    }

    public void switch_channelID(InputField _id)
    {
        channelIDX = int.Parse(_id.text);
        Debug.Log(channelIDX);
    }

    public void switch_channelPassword(InputField _pw)
    {
        channelPW = _pw.text;
        Debug.Log(channelPW);
    }

    #endregion


    #region UI_Buttons
    public void btn_Connect()
    {
        connect();
    }

    public void btn_Disconnect()
    {
        disconnect();
    }

    public void btn_ToggleCreateChannel()
    {
        create_Pannel.SetActive(!create_Pannel.activeInHierarchy);
    }

    public void btn_ToogleSwitchChannel()
    {
        switch_Pannel.SetActive(!switch_Pannel.activeInHierarchy);
    }

    public void btn_Push2Talk()
    {
        toggleVoice();
    }

    public void btn_CreateChannel()
    {
        createChannel(ts_client.GetServerConnectionHandlerID());
    }

    public void btn_SwitchChannel()
    {
        switchChannel();
    }

    public void btn_DebugTest()
    {
        debugTest();
    }

    public void Button_Quit()
    {
#if (UNITY_STANDALONE_WIN)
        if (!Application.isEditor)
        {
            Application.Quit();
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

#endif
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#endif


#if (UNITY_IOS || UNITY_ANDROID)
    Application.Quit();
#endif

    }

    #endregion

    void OnDisable()
    {
        ts_client.StopClient();
        //On windows the program sometimes freezes on closing the application. You can use the following hack to solve this.
        //#if UNITY_STANDALONE_WIN
        //        if (!Application.isEditor)
        //        {
        //            System.Diagnostics.Process.GetCurrentProcess().Kill();
        //        }
        //#endif
    }

    private static void onTalkStatusChangeEvent(uint64 serverConnectionHandlerID, int status, int isReceivedWhisper, anyID clientID)
    {
        string name = ts_client.GetClientVariableAsString(clientID, ClientProperties.CLIENT_NICKNAME);
        onTalkStatusChange_status.Add(status);
        onTalkStatusChange_ClientName.Add(name);
        if (onTalkStatusChange_status.Count > 1)
        {
            onTalkStatusChange_status.RemoveRange(0, onTalkStatusChange_status.Count - 1);
        }
        onTalkStatusChange_labelText = "";
        for (int i = 0; i < onTalkStatusChange_status.Count; i++)
        {
            onTalkStatusChange_labelText += onTalkStatusChange_ClientName[i];
            if (onTalkStatusChange_status[i] == 1)
            {
                Debug.Log(name + " started talking");
                onTalkStatusChange_labelText += " started talking.\n";
            }
            else
            {
                Debug.Log(name + " stopped talking");
                onTalkStatusChange_labelText += " stopped talking.\n";
            }
        }
    }

    IEnumerator PauseWaveHandleIn5Seconds(uint64 waveHandle)
    {
        yield return new WaitForSeconds(2f);
        ts_client.Set3DWaveAttributes(ts_client.GetServerConnectionHandlerID(), waveHandle, new Vector3(10, 10, 10));
        yield return new WaitForSeconds(3f);
        ts_client.PauseWaveFileHandle(ts_client.GetServerConnectionHandlerID(), waveHandle, true);
    }
}
