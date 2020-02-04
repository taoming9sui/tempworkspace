using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using IntPtr = System.IntPtr;
using uint64 = System.UInt64;
using anyID = System.UInt16;

public class TeamSpeakInterface{
	public static uint64 BANDWIDTH_LIMIT_UNLIMITED = 0xFFFFFFFFFFFFFFFFL;
	public static int SPEAKER_FRONT_LEFT = 0x1;
	public static int SPEAKER_FRONT_RIGHT = 0x2;
	public static int SPEAKER_FRONT_CENTER = 0x4;
	public static int SPEAKER_LOW_FREQUENCY = 0x8;
	public static int SPEAKER_BACK_LEFT = 0x10;
	public static int SPEAKER_BACK_RIGHT = 0x20;
	public static int SPEAKER_FRONT_LEFT_OF_CENTER = 0x40;
	public static int SPEAKER_FRONT_RIGHT_OF_CENTER = 0x80;
	public static int SPEAKER_BACK_CENTER = 0x100;
	public static int SPEAKER_SIDE_LEFT = 0x200;
	public static int SPEAKER_SIDE_RIGHT = 0x400;
	public static int SPEAKER_TOP_CENTER = 0x800;
	public static int SPEAKER_TOP_FRONT_LEFT = 0x1000;
	public static int SPEAKER_TOP_FRONT_CENTER = 0x2000;
	public static int SPEAKER_TOP_FRONT_RIGHT = 0x4000;
	public static int SPEAKER_TOP_BACK_LEFT = 0x8000;
	public static int SPEAKER_TOP_BACK_CENTER = 0x10000;
	public static int SPEAKER_TOP_BACK_RIGHT = 0x20000;
	public static int SPEAKER_HEADPHONES_LEFT = 0x10000000;
	public static int SPEAKER_HEADPHONES_RIGHT = 0x20000000;
	public static int SPEAKER_MONO = 0x40000000;

	[StructLayout(LayoutKind.Sequential)]
	public struct TS3_VECTOR{
		public float x;
		public float y;
		public float z;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct VariablesExportItem{
		public byte itemIsValid;    /* This item has valid values. ignore this item if 0 */
		public byte proposedIsSet;  /* The value in proposed is set. if 0 ignore proposed */
		public string current;      /* current value (stored in memory) */
		public string proposed;     /* New value to change to (const, so no updates please) */
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct VariablesExport{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public VariablesExportItem[] items;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct ClientMiniExport{
		anyID ID;
		uint64 channel;
		string ident;
		string nickname;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct TransformFilePathExport{
		uint64 channel;
		string filename;
		int action;
		int transformedFileNameMaxSize;
		int channelPathMaxSize;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct TransformFilePathExportReturns{
		string transformedFileName;
		string channelPath;
		int logFileAction;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct FileTransferCallbackExport{
		anyID clientID;
		anyID transferID;
		anyID remoteTransferID;
		uint status;
		string statusMessage;
		uint64 remotefileSize;
		uint64 bytes;
		int isSender;
	}

	public enum LogLevel {
		LogLevel_CRITICAL = 0,
		LogLevel_ERROR,
		LogLevel_WARNING,
		LogLevel_DEBUG,
		LogLevel_INFO,
		LogLevel_DEVEL,
	};

    //#if UNITY_IOS && !UNITY_EDITOR
    //	private const string DLL_PATH = "__Internal";
    //#elif UNITY_ANDROID && !UNITY_EDITOR
    //	private const string DLL_PATH = "ts3client_android";
    //#else
    //	private const string DLL_PATH = "libts3client";
    //#endif

#if UNITY_IOS && !UNITY_EDITOR
	private const string DLL_PATH = "__Internal";
#elif UNITY_ANDROID && !UNITY_EDITOR
	private const string DLL_PATH = "ts3client_android";
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
    private const string DLL_PATH = "libts3client";
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
    private const string DLL_PATH = "libts3client";
#elif UNITY_EDITOR
    private const string DLL_PATH = "libts3client";
#endif

    [DllImport(DLL_PATH, EntryPoint = "ts3client_initClientLib")]
	public static extern uint ts3client_initClientLib(ref ClientUIFunctions arg0, ref ClientUIFunctionsRare arg1, int arg2, string arg3, string arg4);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientLibVersion")]
	public static extern uint ts3client_getClientLibVersion(out IntPtr arg0);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_freeMemory")]
	public static extern uint ts3client_freeMemory(IntPtr arg0);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_spawnNewServerConnectionHandler")]
	public static extern uint ts3client_spawnNewServerConnectionHandler(int port, out uint64 arg0);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_openCaptureDevice")]
	public static extern uint ts3client_openCaptureDevice(uint64 arg0, string arg1, string arg2);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_openPlaybackDevice")]
	public static extern uint ts3client_openPlaybackDevice(uint64 arg0, string arg1, string arg2);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_createIdentity")]
	public static extern uint ts3client_createIdentity(out IntPtr arg0);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_startConnection", CharSet = CharSet.Ansi)]
	public static extern uint ts3client_startConnection(uint64 arg0, string identity, string ip, uint port, string nick, string[] defaultchannelarray, string defaultchannelpassword, string serverpassword);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_stopConnection")]
	public static extern uint ts3client_stopConnection(uint64 arg0, string arg1);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_destroyServerConnectionHandler")]
	public static extern uint ts3client_destroyServerConnectionHandler(uint64 arg0);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_destroyClientLib")]
	public static extern uint ts3client_destroyClientLib();
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getErrorMessage")]
	public static extern uint ts3client_getErrorMessage(uint arg0,out IntPtr arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientLibVersionNumber")]
	public static extern uint ts3client_getClientLibVersionNumber(out uint64 arg0);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_allowWhispersFrom")]
	public static extern uint ts3client_allowWhispersFrom(uint64 arg0, anyID arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_removeFromAllowedWhispersFrom")]
	public static extern uint ts3client_removeFromAllowedWhispersFrom(uint64 arg0, anyID arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestClientSetWhisperList")]
	public static extern uint ts3client_requestClientSetWhisperList(uint64 arg0, anyID arg1, uint64[] arg2, anyID[] arg3, IntPtr arg4);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getConnectionStatus")]
	public static extern uint ts3client_getConnectionStatus(uint64 arg0, out int arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelOfClient")]
	public static extern uint ts3client_getChannelOfClient(uint64 arg0, anyID arg1, out uint64 arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelDescription")]
	public static extern uint ts3client_requestChannelDescription(uint64 arg0, uint64 arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelVariableAsString")]
	public static extern uint ts3client_getChannelVariableAsString(uint64 arg0, uint64 arg1, ChannelProperties arg2, out IntPtr arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelVariableAsUInt64")]
	public static extern uint ts3client_getChannelVariableAsUInt64(uint64 arg0, uint64 arg1, ChannelProperties arg2, out uint64 arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelVariableAsInt")]
	public static extern uint ts3client_getChannelVariableAsInt(uint64 arg0, uint64 arg1, ChannelProperties arg2, out int arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setChannelVariableAsString")]
	public static extern uint ts3client_setChannelVariableAsString(uint64 arg0, uint64 arg1, ChannelProperties arg2, string arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setChannelVariableAsUInt64")]
	public static extern uint ts3client_setChannelVariableAsUInt64(uint64 arg0, uint64 arg1, ChannelProperties arg2, uint64 arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setChannelVariableAsInt")]
	public static extern uint ts3client_setChannelVariableAsInt(uint64 arg0, uint64 arg1, ChannelProperties arg2, int arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_flushChannelUpdates")]
	public static extern uint ts3client_flushChannelUpdates(uint64 arg0, uint64 arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelClientList")]
	public static extern uint ts3client_getChannelClientList(uint64 arg0, uint64 arg1, out IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_flushChannelCreation")]
	public static extern uint ts3client_flushChannelCreation(uint64 arg0, uint64 arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelList")]
	public static extern uint ts3client_getChannelList(uint64 arg0, out IntPtr arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelIDFromChannelNames")]
	public static extern uint ts3client_getChannelIDFromChannelNames(uint64 arg0, string[] arg1 ,out uint64 arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getParentChannelOfChannel")]
	public static extern uint ts3client_getParentChannelOfChannel(uint64 arg0, uint64 arg1, out uint64 arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelSubscribe")]
	public static extern uint ts3client_requestChannelSubscribe(uint64 arg0, uint64[] arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelUnsubscribe")]
	public static extern uint ts3client_requestChannelUnsubscribe(uint64 arg0, uint64[] arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelSubscribeAll")]
	public static extern uint ts3client_requestChannelSubscribeAll(uint64 arg0, IntPtr arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelUnsubscribeAll")]
	public static extern uint ts3client_requestChannelUnsubscribeAll(uint64 arg0, IntPtr arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelDelete")]
	public static extern uint ts3client_requestChannelDelete(uint64 arg0, uint64 arg1,int arg2, IntPtr arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestChannelMove")]
	public static extern uint ts3client_requestChannelMove(uint64 arg0, uint64 arg1, uint64 arg2, uint64 arg3, IntPtr arg4);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestServerVariables")]
	public static extern uint ts3client_requestServerVariables(uint64 arg0);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getServerConnectionHandlerList")]
	public static extern uint ts3client_getServerConnectionHandlerList(out IntPtr arg0);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getServerVariableAsString")]
	public static extern uint ts3client_getServerVariableAsString(uint64 arg0, VirtualServerProperties arg1 , out IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getServerVariableAsUInt64")]
	public static extern uint ts3client_getServerVariableAsUInt64(uint64 arg0, VirtualServerProperties arg1 , out uint64 arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getServerVariableAsInt")]
	public static extern uint ts3client_getServerVariableAsInt(uint64 arg0, VirtualServerProperties arg1 , out int arg2);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientID")]
	public static extern uint ts3client_getClientID(uint64 arg0, out anyID arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientSelfVariableAsString")]
	public static extern uint ts3client_getClientSelfVariableAsString(uint64 arg0, ClientProperties arg2, out IntPtr arg3);
		
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientSelfVariableAsInt")]
	public static extern uint ts3client_getClientSelfVariableAsInt(uint64 arg0, ClientProperties arg2, out int arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientVariableAsString")]
	public static extern uint ts3client_getClientVariableAsString(uint64 arg0, anyID arg1, ClientProperties arg2, out IntPtr arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientVariableAsUInt64")]
	public static extern uint ts3client_getClientVariableAsUInt64(uint64 arg0, anyID arg1, ClientProperties arg2, out uint64 arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientVariableAsInt")]
	public static extern uint ts3client_getClientVariableAsInt(uint64 arg0, anyID arg1, ClientProperties arg2, out int arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestClientVariables")]
	public static extern uint ts3client_requestClientVariables(uint64 arg0, anyID arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setClientSelfVariableAsString")]
	public static extern uint ts3client_setClientSelfVariableAsString(uint64 arg0, ClientProperties arg2, string arg3);
		
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setClientSelfVariableAsInt")]
	public static extern uint ts3client_setClientSelfVariableAsInt(uint64 arg0, ClientProperties arg2, int arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_flushClientSelfUpdates")]
	public static extern uint ts3client_flushClientSelfUpdates(uint64 arg0, IntPtr arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getClientList")]
	public static extern uint ts3client_getClientList(uint64 arg0, out IntPtr arg1);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestClientKickFromChannel")]
	public static extern uint ts3client_requestClientKickFromChannel(uint64 arg0, anyID arg1, string arg2, IntPtr arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestClientKickFromServer")]
	public static extern uint ts3client_requestClientKickFromServer(uint64 arg0, anyID arg1, string arg2, IntPtr arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestClientMove")]
	public static extern uint ts3client_requestClientMove(uint64 arg0, anyID arg1, uint64 arg2, string arg3, IntPtr arg4);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestMuteClients")]
	public static extern uint ts3client_requestMuteClients(uint64 arg0, anyID[] arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestUnmuteClients")]
	public static extern uint ts3client_requestUnmuteClients(uint64 arg0, anyID[] arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestSendChannelTextMsg")]
	public static extern uint ts3client_requestSendChannelTextMsg(uint64 arg0, string arg1, uint64 arg2, IntPtr arg3);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestSendServerTextMsg")]
	public static extern uint ts3client_requestSendServerTextMsg(uint64 arg0, string arg1, IntPtr arg2);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestSendPrivateTextMsg")]
	public static extern uint ts3client_requestSendPrivateTextMsg(uint64 arg0, string arg1, anyID arg2, IntPtr arg3);
		
	[DllImport(DLL_PATH, EntryPoint = "ts3client_systemset3DListenerAttributes")]
	public static extern uint ts3client_systemset3DListenerAttributes(uint64 scHandlerID, out TS3_VECTOR position, out TS3_VECTOR forward, out TS3_VECTOR up);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_systemset3DSettings")]
	public static extern uint ts3client_systemset3DSettings(uint64 scHandlerID, float distanceFactor, float rolloffScale);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_channelset3DAttributes")]
	public static extern uint ts3client_channelset3DAttributes(uint64 scHandlerID, anyID clientID, out TS3_VECTOR position);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_acquireCustomPlaybackData")]
	public static extern uint ts3client_acquireCustomPlaybackData(string deviceID, short[] buffer, int samples);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_activateCaptureDevice")]
	public static extern uint ts3client_activateCaptureDevice(uint64 scHandlerID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_closeCaptureDevice")]
	public static extern uint ts3client_closeCaptureDevice(uint64 scHandlerID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_closePlaybackDevice")]
	public static extern uint ts3client_closePlaybackDevice(uint64 scHandlerID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_closeWaveFileHandle")]
	public static extern uint ts3client_closeWaveFileHandle(uint64 scHandlerID, uint64 waveHandle);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCaptureDeviceList")]
	public static extern uint ts3client_getCaptureDeviceList(string modeID, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCaptureModeList")]
	public static extern uint ts3client_getCaptureModeList(out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCurrentCaptureDeviceName")]
	public static extern uint ts3client_getCurrentCaptureDeviceName(uint64 scHandlerID, out IntPtr result, out int isDefault);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCurrentCaptureMode")]
	public static extern uint ts3client_getCurrentCaptureMode(uint64 scHandlerID, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCurrentPlaybackDeviceName")]
	public static extern uint ts3client_getCurrentPlaybackDeviceName(uint64 scHandlerID, out IntPtr result, out int isDefault);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCurrentPlayBackMode")]
	public static extern uint ts3client_getCurrentPlayBackMode(uint64 scHandlerID, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getDefaultCaptureDevice")	]
	public static extern uint ts3client_getDefaultCaptureDevice(string modeID, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getDefaultCaptureMode")]
	public static extern uint ts3client_getDefaultCaptureMode(out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getDefaultPlaybackDevice")]
	public static extern uint ts3client_getDefaultPlaybackDevice(string modeID, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getDefaultPlayBackMode")]
	public static extern uint ts3client_getDefaultPlayBackMode(out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getEncodeConfigValue")]
	public static extern uint ts3client_getEncodeConfigValue(uint64 scHandlerID, string ident, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getPlaybackConfigValueAsFloat")]
	public static extern uint ts3client_getPlaybackConfigValueAsFloat(uint64 scHandlerID, string ident, out float result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getPlaybackDeviceList")]
	public static extern uint ts3client_getPlaybackDeviceList(string modeID, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getPlaybackModeList")]
	public static extern uint ts3client_getPlaybackModeList(out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getPreProcessorConfigValue")]
	public static extern uint ts3client_getPreProcessorConfigValue(uint64 scHandlerID, string ident, out IntPtr result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_getPreProcessorInfoValueFloat")]
	public static extern uint ts3client_getPreProcessorInfoValueFloat(uint64 scHandlerID, string ident, out float result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_initiateGracefulPlaybackShutdown")]
	public static extern uint ts3client_initiateGracefulPlaybackShutdown(uint64 scHandlerID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_logMessage")]
	public static extern uint ts3client_logMessage(string message, LogLevel severity, string channel, uint64 logID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_pauseWaveFileHandle")]
	public static extern uint ts3client_pauseWaveFileHandle(uint64 scHandlerID,uint64 waveHandle, int pause);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_playWaveFile")]
	public static extern uint ts3client_playWaveFile(uint64 scHandlerID, string path);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_playWaveFileHandle")]
	public static extern uint ts3client_playWaveFileHandle(uint64 scHandlerID, string path, int loop, out uint64 waveHandle);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_processCustomCaptureData")]
	public static extern uint ts3client_processCustomCaptureData(string deviceID, short[] buffer, int samples);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_registerCustomDevice")]
	public static extern uint ts3client_registerCustomDevice(string deviceID, string deviceDisplayName, int capFrequency, int capChannels, int playFrequency, int playChannels);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_set3DWaveAttributes")]
	public static extern uint ts3client_set3DWaveAttributes(uint64 scHandlerID, uint64 waveHandle, out TS3_VECTOR position);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setClientVolumeModifier")]
	public static extern uint ts3client_setClientVolumeModifier(uint64 scHandlerID, anyID clientID, float value);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setLocalTestMode")]
	public static extern uint ts3client_setLocalTestMode(uint64 scHandlerID, int status);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setLogVerbosity")]
	public static extern uint ts3client_setLogVerbosity(LogLevel logVerbosity);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setPlaybackConfigValue")]
	public static extern uint ts3client_setPlaybackConfigValue(uint64 scHandlerID, string ident, string value);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_setPreProcessorConfigValue")]
	public static extern uint ts3client_setPreProcessorConfigValue(uint64 scHandlerID, string ident, string value);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_startVoiceRecording")]
	public static extern uint ts3client_startVoiceRecording(uint64 scHandlerID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_stopVoiceRecording")]
	public static extern uint ts3client_stopVoiceRecording(uint64 scHandlerID);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_unregisterCustomDevice")]
	public static extern uint ts3client_unregisterCustomDevice(string deviceID);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getChannelEmptySecs")]
	public static extern uint ts3client_getChannelEmptySecs(uint64 serverConnectionHandlerID, uint64 channelID, out int result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferFileName")]
	public static extern uint ts3client_getTransferFileName(anyID transferID, out IntPtr result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferFilePath")]
	public static extern uint ts3client_getTransferFilePath(anyID transferID, out IntPtr result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferFileRemotePath")]
	public static extern uint ts3client_getTransferFileRemotePath(anyID transferID, out IntPtr result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferFileSize")]
	public static extern uint ts3client_getTransferFileSize(anyID transferID, out uint64 result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferFileSizeDone")]
	public static extern uint ts3client_getTransferFileSizeDone(anyID transferID, out uint64 result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_isTransferSender")]
	public static extern uint ts3client_isTransferSender(anyID transferID, out int result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferStatus")]
	public static extern uint ts3client_getTransferStatus(anyID transferID, out int result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getCurrentTransferSpeed")]
	public static extern uint ts3client_getCurrentTransferSpeed(anyID transferID, out float result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getAverageTransferSpeed")]
	public static extern uint ts3client_getAverageTransferSpeed(anyID transferID, out float result);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferRunTime")]
	public static extern uint ts3client_getTransferRunTime(anyID transferID, out uint64 result);
	
	[DllImport(DLL_PATH, EntryPoint = "ts3client_sendFile")]
	public static extern uint ts3client_sendFile(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string file, int overwrite, int resume, string sourceDirectory, out anyID result, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestFile")]
	public static extern uint ts3client_requestFile(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string file, int overwrite, int resume, string destinationDirectory, out anyID result, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_haltTransfer")]
	public static extern uint ts3client_haltTransfer(uint64 serverConnectionHandlerID, anyID transferID, int deleteUnfinishedFile, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestFileList")]
	public static extern uint ts3client_requestFileList(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string path, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestFileInfo")]
	public static extern uint ts3client_requestFileInfo(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string file, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestDeleteFile", CharSet = CharSet.Ansi)]
	public static extern uint ts3client_requestDeleteFile(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string[] file, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestCreateDirectory")]
	public static extern uint ts3client_requestCreateDirectory(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string directoryPath, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_requestRenameFile")]
	public static extern uint ts3client_requestRenameFile(uint64 serverConnectionHandlerID, uint64 fromChannelID, string fromChannelPW, uint64 toChannelID, string toChannelPW, string oldFile, string newFile, IntPtr returnCode);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getInstanceSpeedLimitUp")]
	public static extern uint ts3client_getInstanceSpeedLimitUp(out uint64 limit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getInstanceSpeedLimitDown")]
	public static extern uint ts3client_getInstanceSpeedLimitDown(out uint64 limit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getServerConnectionHandlerSpeedLimitUp")]
	public static extern uint ts3client_getServerConnectionHandlerSpeedLimitUp(uint64 serverConnectionHandlerID, out uint64  limit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getServerConnectionHandlerSpeedLimitDown")]
	public static extern uint ts3client_getServerConnectionHandlerSpeedLimitDown(uint64 serverConnectionHandlerID, out uint64 limit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_getTransferSpeedLimit")]
	public static extern uint ts3client_getTransferSpeedLimit(anyID transferID, out uint64 limit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_setInstanceSpeedLimitUp")]
	public static extern uint ts3client_setInstanceSpeedLimitUp(uint64 newLimit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_setInstanceSpeedLimitDown")]
	public static extern uint ts3client_setInstanceSpeedLimitDown(uint64 newLimit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_setServerConnectionHandlerSpeedLimitUp")]
	public static extern uint ts3client_setServerConnectionHandlerSpeedLimitUp(uint64 serverConnectionHandlerID, uint64 newLimit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_setServerConnectionHandlerSpeedLimitDown")]
	public static extern uint ts3client_setServerConnectionHandlerSpeedLimitDown(uint64 serverConnectionHandlerID, uint64 newLimit);

	[DllImport(DLL_PATH, EntryPoint = "ts3client_setTransferSpeedLimit")]
	public static extern uint ts3client_setTransferSpeedLimit(anyID transferID, uint64 newLimit);
		
	#if UNITY_IOS && !UNITY_EDITOR
	[DllImport("__Internal", EntryPoint = "teamSpeakRemoteIOInit")]
	public static extern void teamSpeakRemoteIOInit();
	#endif
}
