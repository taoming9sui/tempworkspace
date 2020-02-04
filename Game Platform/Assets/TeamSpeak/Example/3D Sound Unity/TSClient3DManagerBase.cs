using anyID = System.UInt16;
using uint64 = System.UInt64;

public class TSClient3DManagerBase : MainThreadInvoker {

    protected static TeamSpeakClient ts_client;

    protected virtual void OnClientMoveCommonMainThread(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility) {}
    private void onClientMoveCommon(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility)
    {
        Invoke(() => { OnClientMoveCommonMainThread(serverConnectionHandlerID, clientID, oldChannelID, newChannelID, visibility); });
    }

    void onClientMove(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, string moveMessage)
    {
        onClientMoveCommon(serverConnectionHandlerID, clientID, oldChannelID, newChannelID, visibility);
    }

    void onClientMoveMoved(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID moverID, string moverName, string moverUniqueIdentifier, string moveMessage)
    {
        onClientMoveCommon(serverConnectionHandlerID, clientID, oldChannelID, newChannelID, visibility);
    }

    void onClientMoveTimeout(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, string timeoutMessage)
    {
        onClientMoveCommon(serverConnectionHandlerID, clientID, oldChannelID, newChannelID, visibility);
    }

    void onConnectStatusChange(uint64 serverConnectionHandlerID, int newStatus, uint errorNumber)
    {
        if (newStatus == (int)ConnectStatus.STATUS_CONNECTION_ESTABLISHED)
        {
            anyID myID = ts_client.GetClientID(serverConnectionHandlerID);
            uint64 myChannel = ts_client.GetChannelOfClient(serverConnectionHandlerID, myID);
            onClientMoveCommon(serverConnectionHandlerID, myID, 0, myChannel, (int)Visibility.ENTER_VISIBILITY);
        }
    }

    // Use this for initialization
    protected virtual void onStart() { }
    void Start () {
        ts_client = TeamSpeakClient.GetInstance();

        TeamSpeakCallbacks.onConnectStatusChangeEvent += onConnectStatusChange;
        TeamSpeakCallbacks.onClientMoveEvent += onClientMove;
        TeamSpeakCallbacks.onClientMoveMovedEvent += onClientMoveMoved;
        TeamSpeakCallbacks.onClientMoveTimeoutEvent += onClientMoveTimeout;
        onStart();
    }

    private void OnDestroy()
    {
        TeamSpeakCallbacks.onConnectStatusChangeEvent -= onConnectStatusChange;
        TeamSpeakCallbacks.onClientMoveEvent -= onClientMove;
        TeamSpeakCallbacks.onClientMoveMovedEvent -= onClientMoveMoved;
        TeamSpeakCallbacks.onClientMoveTimeoutEvent -= onClientMoveTimeout;
    }
}
