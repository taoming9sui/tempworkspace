using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using anyID = System.UInt16;
using uint64 = System.UInt64;
using IntPtr = System.IntPtr;

public class TS3_Minimal_Mobile_Example : MonoBehaviour
{
    public string serverAddress = "Localhost";
    public int serverPort = 9987;
    public string serverPassword = "secret";
    public string nickName = "client";
    private string[] defaultChannel = new string[] { "Channel_1", "" };
    public string defaultChannelPassword = null;
    public bool pushCtrlToTalk = true;

    private static TeamSpeakClient ts_client;

    public static bool didNotFindServer = false;

    private static List<int> onTalkStatusChange_status = new List<int>();
    private static List<string> onTalkStatusChange_ClientName = new List<string>();
    private static string onTalkStatusChange_labelText = "";


    // Use this for initialization
    void Start()
    {
        //Getting the client		
        ts_client = TeamSpeakClient.GetInstance();

        //Attaching functions to the TeamSpeak callbacks.
        TeamSpeakCallbacks.onTalkStatusChangeEvent += onTalkStatusChangeEvent;

        //enabling logging of some pre defined errors.
        TeamSpeakClient.logErrors = true;

        //Example on how to use push to talk in unity
        if (pushCtrlToTalk)
        {
            ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_DEACTIVATED);
            ts_client.FlushClientSelfUpdates();
        }
    }

    void Update()
    {

        //Example on how to implement push to talk in unity
        if (pushCtrlToTalk)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_ACTIVE);
                ts_client.FlushClientSelfUpdates();
                Debug.Log("Talk enabled");
            }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_DEACTIVATED);
                ts_client.FlushClientSelfUpdates();
                Debug.Log("Talk disabled");
            }

#if (UNITY_IOS || UNITY_ANDROID)
            if (Input.GetMouseButtonDown(0))
            {
                ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_ACTIVE);
                ts_client.FlushClientSelfUpdates();
                Debug.Log("Talk enabled");
            }
            if (Input.GetMouseButtonUp(0))
            {
                ts_client.SetClientSelfVariableAsInt(ClientProperties.CLIENT_INPUT_DEACTIVATED, (int)InputDeactivationStatus.INPUT_DEACTIVATED);
                ts_client.FlushClientSelfUpdates();
                Debug.Log("Talk Disabled");
            }
#endif
        }
    }

    void OnGUI()
    {
        /*
            Simple Mobile Example
            Just tested on a Android Mobile Device 
        */

#if (UNITY_IOS || UNITY_ANDROID)

        if (GUI.Button(new Rect(Screen.width / 2 - 420, Screen.height - 300, 200, 200), "Connect to Server"))
        {
            Debug.Log(serverAddress + " " );
            ts_client.StartClient(serverAddress, (uint)serverPort, serverPassword, nickName, ref defaultChannel, defaultChannelPassword);
        }
        if (GUI.Button(new Rect(Screen.width / 2 - 210, Screen.height - 300, 200, 200), "Disconnect from Server"))
        {
            ts_client.StopClient();
        }
        if (GUI.Button(new Rect(Screen.width / 2 + 10, Screen.height - 300, 200, 200), "Toggle Push to Talk"))
        {
            if (pushCtrlToTalk == true)
            {
                pushCtrlToTalk = false;
            }
            else if (pushCtrlToTalk == false)
            {
                pushCtrlToTalk = true;
            }
            //pushCtrlToTalk = (pushCtrlToTalk == true) ? false : true;
        }
        if (GUI.Button(new Rect(Screen.width / 2 + 220, Screen.height - 300, 200, 200), "Quit"))
        {
            Application.Quit();
        }

#endif

    }

    void OnDisable()
    {
        ts_client.StopClient();
    }

    private static void onTalkStatusChangeEvent(uint64 serverConnectionHandlerID, int status, int isReceivedWhisper, anyID clientID)
    {
        string name = ts_client.GetClientVariableAsString(clientID, ClientProperties.CLIENT_NICKNAME);
        onTalkStatusChange_status.Add(status);
        onTalkStatusChange_ClientName.Add(name);
        if (onTalkStatusChange_status.Count > 10)
        {
            onTalkStatusChange_status.RemoveRange(0, onTalkStatusChange_status.Count - 10);
        }
        onTalkStatusChange_labelText = "";
        for (int i = 0; i < onTalkStatusChange_status.Count; i++)
        {
            onTalkStatusChange_labelText += onTalkStatusChange_ClientName[i];
            if (onTalkStatusChange_status[i] == 1)
            {
                onTalkStatusChange_labelText += " started talking.\n";
            }
            else
            {
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
