//#define CUSTOM_PASSWORD_ENCRYPTION

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using anyID = System.UInt16;
using uint64 = System.UInt64;


#if UNITY_IOS &&!UNITY_EDITOR
public class MonoPInvokeCallbackAttribute : System.Attribute
{
    private Type type;
    public MonoPInvokeCallbackAttribute( Type t ) { type = t; }
}
#endif

#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onConnectStatusChangeEvent_type( uint64 serverConnectionHandlerID, int newStatus,  uint errorNumber);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onServerProtocolVersionEvent_type(uint64 serverConnectionHandlerID, int protocolVersion);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onNewChannelEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, uint64 channelParentID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onNewChannelCreatedEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, uint64 channelParentID, anyID invokerID, string invokerName, string invokerUniqueIdentifier);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onDelChannelEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, anyID invokerID, string invokerName, string invokerUniqueIdentifier);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelMoveEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, uint64 newChannelParentID, anyID invokerID, string invokerName, string invokerUniqueIdentifier);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onUpdateChannelEvent_type(uint64 serverConnectionHandlerID, uint64 channelID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onUpdateChannelEditedEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, anyID invokerID, string invokerName, string invokerUniqueIdentifier);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onUpdateClientEvent_type(uint64 serverConnectionHandlerID, anyID clientID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientMoveEvent_type(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, string moveMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientMoveSubscriptionEvent_type(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientMoveTimeoutEvent_type(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, string timeoutMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientMoveMovedEvent_type(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID moverID, string moverName, string moverUniqueIdentifier, string moveMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientKickFromChannelEvent_type(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID kickerID, string kickerName, string kickerUniqueIdentifier, string kickMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientKickFromServerEvent_type(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID kickerID, string kickerName, string kickerUniqueIdentifier, string kickMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientIDsEvent_type(uint64 serverConnectionHandlerID, string uniqueClientIdentifier, anyID clientID, string clientName);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientIDsFinishedEvent_type(uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onServerEditedEvent_type(uint64 serverConnectionHandlerID, anyID editerID, string editerName, string editerUniqueIdentifier);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onServerUpdatedEvent_type(uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onServerErrorEvent_type(uint64 serverConnectionHandlerID, string errorMessage, uint error, string returnCode, string extraMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onServerStopEvent_type(uint64 serverConnectionHandlerID, string shutdownMessage);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onTextMessageEvent_type(uint64 serverConnectionHandlerID, anyID targetMode, anyID toID, anyID fromID, string fromName, string fromUniqueIdentifier, string message);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onTalkStatusChangeEvent_type(uint64 serverConnectionHandlerID, int status, int isReceivedWhisper, anyID clientID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onIgnoredWhisperEvent_type(uint64 serverConnectionHandlerID, anyID clientID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onConnectionInfoEvent_type(uint64 serverConnectionHandlerID, anyID clientID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onServerConnectionInfoEvent_type(uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelSubscribeEvent_type(uint64 serverConnectionHandlerID, uint64 channelID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelSubscribeFinishedEvent_type(uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelUnsubscribeEvent_type(uint64 serverConnectionHandlerID, uint64 channelID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelUnsubscribeFinishedEvent_type(uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelDescriptionUpdateEvent_type(uint64 serverConnectionHandlerID, uint64 channelID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onChannelPasswordChangedEvent_type(uint64 serverConnectionHandlerID, uint64 channelID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onPlaybackShutdownCompleteEvent_type(uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onUserLoggingMessageEvent_type(string logmessage, int logLevel, string logChannel, uint64 logID, string logTime, string completeLogString);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onSoundDeviceListChangedEvent_type(string modeID, int playOrCap);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onEditPlaybackVoiceDataEvent_type(uint64 serverConnectionHandlerID, anyID clientID, IntPtr samples, int frameCount, int channels);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onEditPostProcessVoiceDataEvent_type(uint64 serverConnectionHandlerID, anyID clientID, IntPtr samples, int frameCount, int channels, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] channelSpeakerArray, ref uint channelFillMask); //const channelSpeakerArry
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onEditMixedPlaybackVoiceDataEvent_type(uint64 serverConnectionHandlerID, IntPtr samples, int frameCount, int channels, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)]  uint[] channelSpeakerArray, ref uint channelFillMask); //const channelSpeakerArray
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onEditCapturedVoiceDataEvent_type(uint64 serverConnectionHandlerID, IntPtr samples, int frameCount, int channels, ref int edited);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onCustom3dRolloffCalculationClientEvent_type(uint64 serverConnectionHandlerID, anyID clientID, float distance, ref float volume);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onCustom3dRolloffCalculationWaveEvent_type(uint64 serverConnectionHandlerID, uint64 waveHandle, float distance, ref float volume);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onProvisioningSlotRequestResultEvent_type(uint error, uint64 requestHandle, string connectionKey);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onCheckServerUniqueIdentifierEvent_type(uint64 serverConnectionHandlerID, string ServerUniqueIdentifier, ref int cancelConnect);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onClientPasswordEncryptEvent_type(uint64 serverConnectionHandlerID, string plaintext, IntPtr encryptedText, int encryptedTextByteSize);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onFileTransferStatusEvent_type(anyID transferID, uint status, string statusMessage, uint64 remotefileSize, uint64 serverConnectionHandlerID);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onFileListEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, string path, string name, uint64 size, uint64 datetime, int type, uint64 incompletesize, string returnCode);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onFileListFinishedEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, string path);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void onFileInfoEvent_type(uint64 serverConnectionHandlerID, uint64 channelID, string name, uint64 size, uint64 datetime);
#if !UNITY_IOS || UNITY_EDITOR
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
public delegate void dummy1_type();

[StructLayout(LayoutKind.Sequential)]
public struct ClientUIFunctionsRare{};

[StructLayout(LayoutKind.Sequential)]
public struct ClientUIFunctions {
	public onConnectStatusChangeEvent_type onConnectStatusChangeEvent_delegate;
    public onServerProtocolVersionEvent_type onServerProtocolVersionEvent_delegate;
	public onNewChannelEvent_type onNewChannelEvent_delegate;
	public onNewChannelCreatedEvent_type onNewChannelCreatedEvent_delegate;
	public onDelChannelEvent_type onDelChannelEvent_delegate;
	public onChannelMoveEvent_type onChannelMoveEvent_delegate;
	public onUpdateChannelEvent_type onUpdateChannelEvent_delegate;
	public onUpdateChannelEditedEvent_type onUpdateChannelEditedEvent_delegate;
	public onUpdateClientEvent_type onUpdateClientEvent_delegate;
	public onClientMoveEvent_type onClientMoveEvent_delegate;
	public onClientMoveSubscriptionEvent_type onClientMoveSubscriptionEvent_delegate;
	public onClientMoveTimeoutEvent_type onClientMoveTimeoutEvent_delegate;
	public onClientMoveMovedEvent_type onClientMoveMovedEvent_delegate;
	public onClientKickFromChannelEvent_type onClientKickFromChannelEvent_delegate;
	public onClientKickFromServerEvent_type onClientKickFromServerEvent_delegate;
	public onClientIDsEvent_type onClientIDsEvent_delegate;
	public onClientIDsFinishedEvent_type onClientIDsFinishedEvent_delegate;
	public onServerEditedEvent_type onServerEditedEvent_delegate;
	public onServerUpdatedEvent_type onServerUpdatedEvent_delegate;
	public onServerErrorEvent_type onServerErrorEvent_delegate;
	public onServerStopEvent_type onServerStopEvent_delegate;
	public onTextMessageEvent_type onTextMessageEvent_delegate;
	public onTalkStatusChangeEvent_type onTalkStatusChangeEvent_delegate;
    public onIgnoredWhisperEvent_type onIgnoredWhisperEvent_delegate;
	public onConnectionInfoEvent_type onConnectionInfoEvent_delegate;
    public onServerConnectionInfoEvent_type onServerConnectionInfoEvent_delegate;
	public onChannelSubscribeEvent_type onChannelSubscribeEvent_delegate;
	public onChannelSubscribeFinishedEvent_type onChannelSubscribeFinishedEvent_delegate;
	public onChannelUnsubscribeEvent_type onChannelUnsubscribeEvent_delegate;
	public onChannelUnsubscribeFinishedEvent_type onChannelUnsubscribeFinishedEvent_delegate;
	public onChannelDescriptionUpdateEvent_type onChannelDescriptionUpdateEvent_delegate;
	public onChannelPasswordChangedEvent_type onChannelPasswordChangedEvent_delegate;
    public onPlaybackShutdownCompleteEvent_type onPlaybackShutdownCompleteEvent_delegate;
	public onSoundDeviceListChangedEvent_type onSoundDeviceListChangedEvent_delegate;
	public onEditPlaybackVoiceDataEvent_type onEditPlaybackVoiceDataEvent_delegate; 
	public onEditPostProcessVoiceDataEvent_type onEditPostProcessVoiceDataEvent_delegate;
	public onEditMixedPlaybackVoiceDataEvent_type onEditMixedPlaybackVoiceDataEvent_delegate;
	public onEditCapturedVoiceDataEvent_type onEditCapturedVoiceDataEvent_delegate;
	public onCustom3dRolloffCalculationClientEvent_type onCustom3dRolloffCalculationClientEvent_delegate;
	public onCustom3dRolloffCalculationWaveEvent_type onCustom3dRolloffCalculationWaveEvent_delegate;
	public onUserLoggingMessageEvent_type onUserLoggingMessageEvent_delegate;
	public dummy1_type dummy100_delagate;
	public dummy1_type dummy101_delegate;
	public onProvisioningSlotRequestResultEvent_type onProvisioningSlotRequestResultEvent_delegate;
	public onCheckServerUniqueIdentifierEvent_type onCheckServerUniqueIdentifierEvent_delegate;
	public onClientPasswordEncryptEvent_type onClientPasswordEncryptEvent_delegate;
	public onFileTransferStatusEvent_type onFileTransferStatusEvent_delegate;
	public onFileListEvent_type onFileListEvent_delegate;
	public onFileListFinishedEvent_type onFileListFinishedEvent_delegate; 
	public onFileInfoEvent_type onFileInfoEvent_delegate; 
};

public static class TeamSpeakCallbacks {
	public static event onChannelDescriptionUpdateEvent_type onChannelDescriptionUpdateEvent;
	public static event onChannelMoveEvent_type onChannelMoveEvent;
	public static event onChannelPasswordChangedEvent_type onChannelPasswordChangedEvent;
	public static event onChannelSubscribeEvent_type onChannelSubscribeEvent;
	public static event onChannelSubscribeFinishedEvent_type onChannelSubscribeFinishedEvent;
	public static event onChannelUnsubscribeEvent_type onChannelUnsubscribeEvent;
	public static event onChannelUnsubscribeFinishedEvent_type onChannelUnsubscribeFinishedEvent;
	public static event onClientIDsEvent_type onClientIDsEvent;
	public static event onClientIDsFinishedEvent_type onClientIDsFinishedEvent;
	public static event onClientKickFromChannelEvent_type onClientKickFromChannelEvent;
	public static event onClientKickFromServerEvent_type onClientKickFromServerEvent;
	public static event onClientMoveEvent_type onClientMoveEvent;
	public static event onClientMoveMovedEvent_type onClientMoveMovedEvent;
	public static event onClientMoveSubscriptionEvent_type onClientMoveSubscriptionEvent;
	public static event onClientMoveTimeoutEvent_type onClientMoveTimeoutEvent;
	public static event onConnectionInfoEvent_type onConnectionInfoEvent;
	public static event onConnectStatusChangeEvent_type onConnectStatusChangeEvent;
	public static event onDelChannelEvent_type onDelChannelEvent;
	public static event onIgnoredWhisperEvent_type onIgnoredWhisperEvent;
	public static event onNewChannelCreatedEvent_type onNewChannelCreatedEvent;
	public static event onNewChannelEvent_type onNewChannelEvent;
	public static event onPlaybackShutdownCompleteEvent_type onPlaybackShutdownCompleteEvent;
	public static event onServerConnectionInfoEvent_type onServerConnectionInfoEvent;
	public static event onServerEditedEvent_type onServerEditedEvent;
	public static event onServerErrorEvent_type onServerErrorEvent;
	public static event onServerProtocolVersionEvent_type onServerProtocolVersionEvent;
	public static event onServerStopEvent_type onServerStopEvent;
	public static event onServerUpdatedEvent_type onServerUpdatedEvent;
	public static event onTalkStatusChangeEvent_type onTalkStatusChangeEvent;
	public static event onTextMessageEvent_type onTextMessageEvent;
	public static event onUpdateChannelEditedEvent_type onUpdateChannelEditedEvent;
	public static event onUpdateChannelEvent_type onUpdateChannelEvent;
	public static event onUpdateClientEvent_type onUpdateClientEvent;
	public static event onUserLoggingMessageEvent_type onUserLoggingMessageEvent;
	public static event onSoundDeviceListChangedEvent_type onSoundDeviceListChangedEvent;
	public static event onEditPlaybackVoiceDataEvent_type onEditPlaybackVoiceDataEvent; 
	public static event onEditPostProcessVoiceDataEvent_type onEditPostProcessVoiceDataEvent;
	public static event onEditMixedPlaybackVoiceDataEvent_type onEditMixedPlaybackVoiceDataEvent;
	public static event onEditCapturedVoiceDataEvent_type onEditCapturedVoiceDataEvent;
	public static event onCustom3dRolloffCalculationClientEvent_type onCustom3dRolloffCalculationClientEvent;
	public static event onCustom3dRolloffCalculationWaveEvent_type onCustom3dRolloffCalculationWaveEvent;
	public static event onProvisioningSlotRequestResultEvent_type onProvisioningSlotRequestResultEvent;
	public static event onCheckServerUniqueIdentifierEvent_type onCheckServerUniqueIdentifierEvent;
	public static event onClientPasswordEncryptEvent_type onClientPasswordEncryptEvent;
	public static event onFileTransferStatusEvent_type onFileTransferStatusEvent;
	public static event onFileListEvent_type onFileListEvent;
	public static event onFileListFinishedEvent_type onFileListFinishedEvent;
	public static event onFileInfoEvent_type onFileInfoEvent;
	
	public static void Init(ref ClientUIFunctions cbs){
			cbs.onChannelDescriptionUpdateEvent_delegate = new onChannelDescriptionUpdateEvent_type(TeamSpeakCallbacks.onChannelDescriptionUpdateEvent_);
			cbs.onChannelMoveEvent_delegate = new onChannelMoveEvent_type(TeamSpeakCallbacks.onChannelMoveEvent_);
			cbs.onChannelPasswordChangedEvent_delegate = new onChannelPasswordChangedEvent_type(TeamSpeakCallbacks.onChannelPasswordChangedEvent_);
			cbs.onChannelSubscribeEvent_delegate = new onChannelSubscribeEvent_type(TeamSpeakCallbacks.onChannelSubscribeEvent_);
			cbs.onChannelSubscribeFinishedEvent_delegate = new onChannelSubscribeFinishedEvent_type(TeamSpeakCallbacks.onChannelSubscribeFinishedEvent_);
			cbs.onChannelUnsubscribeEvent_delegate = new onChannelUnsubscribeEvent_type(TeamSpeakCallbacks.onChannelUnsubscribeEvent_);
			cbs.onChannelUnsubscribeFinishedEvent_delegate = new onChannelUnsubscribeFinishedEvent_type(TeamSpeakCallbacks.onChannelUnsubscribeFinishedEvent_);
			cbs.onClientIDsEvent_delegate = new onClientIDsEvent_type(TeamSpeakCallbacks.onClientIDsEvent_);
			cbs.onClientIDsFinishedEvent_delegate = new onClientIDsFinishedEvent_type(TeamSpeakCallbacks.onClientIDsFinishedEvent_);
			cbs.onClientKickFromChannelEvent_delegate = new onClientKickFromChannelEvent_type(TeamSpeakCallbacks.onClientKickFromChannelEvent_);
			cbs.onClientKickFromServerEvent_delegate = new onClientKickFromServerEvent_type(TeamSpeakCallbacks.onClientKickFromServerEvent_);
			cbs.onClientMoveEvent_delegate = new onClientMoveEvent_type(TeamSpeakCallbacks.onClientMoveEvent_);
			cbs.onClientMoveMovedEvent_delegate = new onClientMoveMovedEvent_type(TeamSpeakCallbacks.onClientMoveMovedEvent_);
			cbs.onClientMoveSubscriptionEvent_delegate = new onClientMoveSubscriptionEvent_type(TeamSpeakCallbacks.onClientMoveSubscriptionEvent_);
			cbs.onClientMoveTimeoutEvent_delegate = new onClientMoveTimeoutEvent_type(TeamSpeakCallbacks.onClientMoveTimeoutEvent_);
			cbs.onConnectionInfoEvent_delegate = new onConnectionInfoEvent_type(TeamSpeakCallbacks.onConnectionInfoEvent_);
			cbs.onConnectStatusChangeEvent_delegate = new onConnectStatusChangeEvent_type(TeamSpeakCallbacks.onConnectStatusChangeEvent_);
			cbs.onDelChannelEvent_delegate = new onDelChannelEvent_type(TeamSpeakCallbacks.onDelChannelEvent_);
			cbs.onIgnoredWhisperEvent_delegate = new onIgnoredWhisperEvent_type(TeamSpeakCallbacks.onIgnoredWhisperEvent_);
			cbs.onNewChannelCreatedEvent_delegate = new onNewChannelCreatedEvent_type(TeamSpeakCallbacks.onNewChannelCreatedEvent_);
			cbs.onNewChannelEvent_delegate = new onNewChannelEvent_type(TeamSpeakCallbacks.onNewChannelEvent_);
			cbs.onPlaybackShutdownCompleteEvent_delegate = new onPlaybackShutdownCompleteEvent_type(TeamSpeakCallbacks.onPlaybackShutdownCompleteEvent_);
			cbs.onServerConnectionInfoEvent_delegate = new onServerConnectionInfoEvent_type(TeamSpeakCallbacks.onServerConnectionInfoEvent_);
			cbs.onServerEditedEvent_delegate = new onServerEditedEvent_type(TeamSpeakCallbacks.onServerEditedEvent_);
			cbs.onServerErrorEvent_delegate = new onServerErrorEvent_type(TeamSpeakCallbacks.onServerErrorEvent_);
			cbs.onServerProtocolVersionEvent_delegate = new onServerProtocolVersionEvent_type(TeamSpeakCallbacks.onServerProtocolVersionEvent_);
			cbs.onServerStopEvent_delegate = new onServerStopEvent_type(TeamSpeakCallbacks.onServerStopEvent_);
			cbs.onServerUpdatedEvent_delegate = new onServerUpdatedEvent_type(TeamSpeakCallbacks.onServerUpdatedEvent_);
			cbs.onTalkStatusChangeEvent_delegate = new onTalkStatusChangeEvent_type(TeamSpeakCallbacks.onTalkStatusChangeEvent_);
			cbs.onTextMessageEvent_delegate = new onTextMessageEvent_type(TeamSpeakCallbacks.onTextMessageEvent_);
			cbs.onUpdateChannelEditedEvent_delegate = new onUpdateChannelEditedEvent_type(TeamSpeakCallbacks.onUpdateChannelEditedEvent_);
			cbs.onUpdateChannelEvent_delegate = new onUpdateChannelEvent_type(TeamSpeakCallbacks.onUpdateChannelEvent_);
			cbs.onUpdateClientEvent_delegate = new onUpdateClientEvent_type(TeamSpeakCallbacks.onUpdateClientEvent_);
			cbs.onUserLoggingMessageEvent_delegate = new onUserLoggingMessageEvent_type(TeamSpeakCallbacks.onUserLoggingMessageEvent_);
			cbs.onSoundDeviceListChangedEvent_delegate = new onSoundDeviceListChangedEvent_type(onSoundDeviceListChangedEvent_);
#if !UNITY_ANDROID || UNITY_EDITOR	
			cbs.onEditPlaybackVoiceDataEvent_delegate = new onEditPlaybackVoiceDataEvent_type(onEditPlaybackVoiceDataEvent_); 
			cbs.onEditPostProcessVoiceDataEvent_delegate = new onEditPostProcessVoiceDataEvent_type(onEditPostProcessVoiceDataEvent_);
			cbs.onEditMixedPlaybackVoiceDataEvent_delegate = new onEditMixedPlaybackVoiceDataEvent_type(onEditMixedPlaybackVoiceDataEvent_);
			cbs.onEditCapturedVoiceDataEvent_delegate = new onEditCapturedVoiceDataEvent_type(onEditCapturedVoiceDataEvent_);
			cbs.onCustom3dRolloffCalculationClientEvent_delegate = new onCustom3dRolloffCalculationClientEvent_type(onCustom3dRolloffCalculationClientEvent_);
			cbs.onCustom3dRolloffCalculationWaveEvent_delegate = new onCustom3dRolloffCalculationWaveEvent_type(onCustom3dRolloffCalculationWaveEvent_);
#endif
			cbs.onProvisioningSlotRequestResultEvent_delegate = new onProvisioningSlotRequestResultEvent_type(onProvisioningSlotRequestResultEvent_);
#if !UNITY_ANDROID || UNITY_EDITOR	
			cbs.onCheckServerUniqueIdentifierEvent_delegate = new onCheckServerUniqueIdentifierEvent_type(onCheckServerUniqueIdentifierEvent_);
	#if CUSTOM_PASSWORD_ENCRYPTION
			cbs.onClientPasswordEncryptEvent_delegate = new onClientPasswordEncryptEvent_type(onClientPasswordEncryptEvent_);
	#endif
#endif
			cbs.onFileTransferStatusEvent_delegate = new onFileTransferStatusEvent_type(onFileTransferStatusEvent_);
			cbs.onFileListEvent_delegate = new onFileListEvent_type(onFileListEvent_);
			cbs.onFileListFinishedEvent_delegate = new onFileListFinishedEvent_type(onFileListFinishedEvent_);
			cbs.onFileInfoEvent_delegate = new onFileInfoEvent_type(onFileInfoEvent_);
	}
	
#if UNITY_ANDROID && !UNITY_EDITOR
	private static char RECORD_SEPARATOR = '\x1e';
	public static void ParseEvent(string message){
		try{
			string[] parts = message.Split(RECORD_SEPARATOR);
			Debug.Log ("reveived message: " + parts[0]);
			switch(parts[0]){
			case "onConnectStatusChangeEvent":
				onConnectStatusChangeEvent_( uint64.Parse(parts[1]) , int.Parse(parts[2]), uint.Parse(parts[3]));
				break;
			case "onServerProtocolVersionEvent":
				onServerProtocolVersionEvent_(uint64.Parse(parts[1]), int.Parse(parts[2]));
				break;
			case "onNewChannelEvent":
				onNewChannelEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), uint64.Parse(parts[3]));
				break;
			case "onNewChannelCreatedEvent":
				onNewChannelCreatedEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), uint64.Parse(parts[3]), anyID.Parse(parts[4]), parts[5], parts[6]);
				break;
			case "onDelChannelEvent": 
				onDelChannelEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), anyID.Parse(parts[3]) , parts[4], parts[5]);
				break;
			case "onChannelMoveEvent":
				onChannelMoveEvent_(uint64.Parse(parts[1]) , uint64.Parse(parts[2]) , uint64.Parse(parts[3]) , anyID.Parse(parts[4]) , parts[5] , parts[6]);
				break;
			case "onUpdateChannelEvent":
				onUpdateChannelEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]));
				break;
			case "onUpdateChannelEditedEvent":
				onUpdateChannelEditedEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), anyID.Parse(parts[3]), parts[4], parts[5]);
				break;
			case "onUpdateClientEvent":
				onUpdateClientEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]));
				break;
			case "onClientMoveEvent":
				onClientMoveEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), uint64.Parse(parts[3]), uint64.Parse(parts[4]), int.Parse(parts[5]), parts[6]);
				break;
			case "onClientMoveSubscriptionEvent":
				onClientMoveSubscriptionEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), uint64.Parse(parts[3]), uint64.Parse(parts[4]), int.Parse(parts[5]));
				break;
			case "onClientMoveTimeoutEvent":
				onClientMoveTimeoutEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), uint64.Parse(parts[3]), uint64.Parse(parts[4]), int.Parse(parts[5]), parts[6]);
				break;
			case "onClientMoveMovedEvent":
				onClientMoveMovedEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), uint64.Parse(parts[3]), uint64.Parse(parts[4]), int.Parse(parts[5]), anyID.Parse(parts[6]), parts[7], parts[8], parts[9]);
				break;
			case "onClientKickFromChannelEvent":	
				onClientKickFromChannelEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), uint64.Parse(parts[3]), uint64.Parse(parts[4]), int.Parse(parts[5]), anyID.Parse(parts[6]), parts[7], parts[8], parts[9]);
				break;
			case "onClientKickFromServerEvent":
				onClientKickFromServerEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), uint64.Parse(parts[3]), uint64.Parse(parts[4]), int.Parse(parts[5]), anyID.Parse(parts[6]), parts[7], parts[8], parts[9]);
				break;
			case "onClientIDsEvent":
				onClientIDsEvent_(uint64.Parse(parts[1]), parts[2], anyID.Parse(parts[3]), parts[4]);
				break;
			case "onClientIDsFinishedEvent":
				onClientIDsFinishedEvent_(uint64.Parse(parts[1]));
				break;
			case "onServerEditedEvent":
				onServerEditedEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), parts[3], parts[4]);
				break;
			case "onServerUpdatedEvent": 
				onServerUpdatedEvent_(uint64.Parse(parts[1]));
				break;
			case "onServerErrorEvent":
				Debug.Log(message);
				onServerErrorEvent_(uint64.Parse(parts[1]), parts[2], uint.Parse(parts[3]), parts[4], parts[5]);
				break;
			case "SonerverStopEvent":
				onServerStopEvent_(uint64.Parse(parts[1]), parts[2]);
				break;
			case "onTextMessageEvent":
				onTextMessageEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]), anyID.Parse(parts[3]), anyID.Parse(parts[4]), parts[5], parts[6], parts[7]);
				break;
			case "onTalkStatusChangeEvent":
				onTalkStatusChangeEvent_(uint64.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), anyID.Parse(parts[4]));
				break;
			case "onIgnoredWhisperEvent":
				onIgnoredWhisperEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]));
				break;
			case "onConnectionInfoEvent":
				onConnectionInfoEvent_(uint64.Parse(parts[1]), anyID.Parse(parts[2]));
				break;
			case "onServerConnectionInfoEvent":
				onServerConnectionInfoEvent_(uint64.Parse(parts[1]));
				break;
			case "onChannelSubscribeEvent":
				onChannelSubscribeEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]));
				break;
			case "onChannelSubscribeFinishedEvent":
				onChannelSubscribeFinishedEvent_(uint64.Parse(parts[1]));
				break;
			case "onChannelUnsubscribeEvent":
				onChannelUnsubscribeEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]));
				break;
			case "onChannelUnsubscribeFinishedEvent":
				onChannelUnsubscribeFinishedEvent_(uint64.Parse(parts[1]));
				break;
			case "onChannelDescriptionUpdateEvent":
				onChannelDescriptionUpdateEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]));
				break;
			case "onChannelPasswordChangedEvent":
				onChannelPasswordChangedEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]));
				break;
			case "onPlaybackShutdownCompleteEvent":	
				onPlaybackShutdownCompleteEvent_(uint64.Parse(parts[1]));
				break;
			case "onUserLoggingMessageEvent":
				onUserLoggingMessageEvent_(parts[1], int.Parse(parts[2]), parts[3], uint64.Parse(parts[4]), parts[5], parts[6]);
				break;
			case "onSoundDeviceListChangedEvent":
				onSoundDeviceListChangedEvent_(parts[1], int.Parse(parts[2]));
				break;
			case "onProvisioningSlotRequestResultEvent":
				onProvisioningSlotRequestResultEvent_(uint.Parse(parts[1]), uint64.Parse(parts[2]), parts[3]);
				break;
			case "onFileTransferStatusEvent":
				onFileTransferStatusEvent_(anyID.Parse(parts[1]), uint.Parse(parts[2]), parts[3], uint64.Parse(parts[4]), uint64.Parse (parts[5]));
				break;
			case "onFileListEvent":
				onFileListEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), parts[3], parts[4], uint64.Parse(parts[5]), uint64.Parse (parts[6]),int.Parse(parts[7]),uint64.Parse(parts[8]),parts[9]);
				break;
			case "onFileListFinishedEvent":
				onFileListFinishedEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), parts[3]);
				break;
			case "onFileInfoEvent":
				onFileInfoEvent_(uint64.Parse(parts[1]), uint64.Parse(parts[2]), parts[3], uint64.Parse(parts[4]), uint64.Parse(parts[5]));
				break;
			}
		}catch(Exception exc){
			Debug.Log("Callback error: " + exc.Message);
		}
	}
#endif

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onConnectStatusChangeEvent_type))]
#endif
	private static void onConnectStatusChangeEvent_( uint64 serverConnectionHandlerID, int newStatus, uint errorNumber)
	{
		Debug.Log ("onConnectStatusChangeEvent: " + newStatus + " " + errorNumber);
		try {
			if(onConnectStatusChangeEvent != null){
				onConnectStatusChangeEvent(serverConnectionHandlerID,newStatus,errorNumber);
			}
			if(errorNumber == public_errors.ERROR_failed_connection_initialisation){
				if(TeamSpeakClient.logErrors){
					Debug.Log("Connection failed: could not reach server");
				}
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onServerProtocolVersionEvent_type))]
#endif	
	private static void onServerProtocolVersionEvent_(uint64 serverConnectionHandlerID, int protocolVersion)
	{
		try {
			if(onServerProtocolVersionEvent != null){
				onServerProtocolVersionEvent(serverConnectionHandlerID, protocolVersion);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onNewChannelEvent_type))]
#endif
	private static void onNewChannelEvent_(uint64 serverConnectionHandlerID, uint64 channelID, uint64 channelParentID)
	{
		try {
			if(onNewChannelEvent != null){
				onNewChannelEvent(serverConnectionHandlerID,channelID,channelParentID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onNewChannelCreatedEvent_type))]
#endif
	private static void onNewChannelCreatedEvent_(uint64 serverConnectionHandlerID, uint64 channelID, uint64 channelParentID, anyID invokerID, string invokerName, string invokerUniqueIdentifier)
	{
		try {
			if(onNewChannelCreatedEvent != null){
				onNewChannelCreatedEvent(serverConnectionHandlerID,channelID,channelParentID,invokerID,invokerName,invokerUniqueIdentifier);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}
#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onDelChannelEvent_type))]
#endif

	private static void onDelChannelEvent_(uint64 serverConnectionHandlerID, uint64 channelID, anyID invokerID, string invokerName, string invokerUniqueIdentifier)
	{
		try {
			if(onDelChannelEvent != null){
				onDelChannelEvent(serverConnectionHandlerID,channelID,invokerID,invokerName,invokerUniqueIdentifier);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelMoveEvent_type))]
#endif
	private static void onChannelMoveEvent_(uint64 serverConnectionHandlerID, uint64 channelID, uint64 newChannelParentID, anyID invokerID, string invokerName, string invokerUniqueIdentifier)
	{
		try {
			if(onChannelMoveEvent != null){
				onChannelMoveEvent(serverConnectionHandlerID,channelID,newChannelParentID,invokerID,invokerName,invokerUniqueIdentifier);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onUpdateChannelEvent_type))]
#endif
	private static void onUpdateChannelEvent_(uint64 serverConnectionHandlerID, uint64 channelID)
	{
		try {
			if(onUpdateChannelEvent != null){
				onUpdateChannelEvent(serverConnectionHandlerID,channelID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onUpdateChannelEditedEvent_type))]
#endif
	private static void onUpdateChannelEditedEvent_(uint64 serverConnectionHandlerID, uint64 channelID, anyID invokerID, string invokerName, string invokerUniqueIdentifier)
	{
		try {
			if(onUpdateChannelEditedEvent != null){
				onUpdateChannelEditedEvent(serverConnectionHandlerID,channelID,invokerID,invokerName,invokerUniqueIdentifier);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onUpdateClientEvent_type))]
#endif
	private static void onUpdateClientEvent_(uint64 serverConnectionHandlerID, anyID clientID)
	{
		try {
			if(onUpdateClientEvent != null){
				onUpdateClientEvent(serverConnectionHandlerID,clientID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientMoveEvent_type))]
#endif
	private static void onClientMoveEvent_(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, string moveMessage)
	{
		try {
			if(onClientMoveEvent != null){
				onClientMoveEvent(serverConnectionHandlerID,clientID,oldChannelID,newChannelID,visibility,moveMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientMoveSubscriptionEvent_type))]
#endif
	private static void onClientMoveSubscriptionEvent_(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility)
	{
		try {
			if(onClientMoveSubscriptionEvent != null){
				onClientMoveSubscriptionEvent(serverConnectionHandlerID,clientID,oldChannelID,newChannelID,visibility);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientMoveTimeoutEvent_type))]
#endif
	private static void onClientMoveTimeoutEvent_(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, string timeoutMessage)
	{
		try {
			if(onClientMoveTimeoutEvent != null){
				onClientMoveTimeoutEvent(serverConnectionHandlerID,clientID,oldChannelID,newChannelID,visibility,timeoutMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientMoveMovedEvent_type))]
#endif
	private static void onClientMoveMovedEvent_(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID moverID, string moverName, string moverUniqueIdentifier, string moveMessage)
	{
		try {
			if(onClientMoveMovedEvent != null){
				onClientMoveMovedEvent(serverConnectionHandlerID,clientID,oldChannelID,newChannelID,visibility,moverID,moverName,moverUniqueIdentifier,moveMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientKickFromChannelEvent_type))]
#endif
	private static void onClientKickFromChannelEvent_(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID kickerID, string kickerName, string kickerUniqueIdentifier, string kickMessage)
	{
		try {
			if(onClientKickFromChannelEvent != null){
				onClientKickFromChannelEvent(serverConnectionHandlerID,clientID,oldChannelID,newChannelID,visibility,kickerID,kickerName,kickerUniqueIdentifier,kickMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientKickFromServerEvent_type))]
#endif
	private static void onClientKickFromServerEvent_(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility, anyID kickerID, string kickerName, string kickerUniqueIdentifier, string kickMessage)
	{
		try {
			if(onClientKickFromServerEvent != null){
				onClientKickFromServerEvent(serverConnectionHandlerID,clientID,oldChannelID,newChannelID,visibility,kickerID,kickerName,kickerUniqueIdentifier,kickMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientIDsEvent_type))]
#endif
	private static void onClientIDsEvent_(uint64 serverConnectionHandlerID, string uniqueClientIdentifier, anyID clientID, string clientName)
	{
		try {
			if(onClientIDsEvent != null){
				onClientIDsEvent(serverConnectionHandlerID,uniqueClientIdentifier,clientID,clientName);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientIDsFinishedEvent_type))]
#endif
	private static void onClientIDsFinishedEvent_(uint64 serverConnectionHandlerID)
	{
		try {
			if(onClientIDsFinishedEvent != null){
				onClientIDsFinishedEvent(serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onServerEditedEvent_type))]
#endif
	private static void onServerEditedEvent_(uint64 serverConnectionHandlerID, anyID editerID, string editerName, string editerUniqueIdentifier)
	{
		try {
			if(onServerEditedEvent != null){
				onServerEditedEvent(serverConnectionHandlerID,editerID,editerName,editerUniqueIdentifier);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onServerUpdatedEvent_type))]
#endif
	private static void onServerUpdatedEvent_(uint64 serverConnectionHandlerID)
	{
		try {
			if(onServerUpdatedEvent != null){
				onServerUpdatedEvent(serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onServerErrorEvent_type))]
#endif
	private static void onServerErrorEvent_(uint64 serverConnectionHandlerID, string errorMessage, uint error, string returnCode, string extraMessage)
	{
		try {
			if(onServerErrorEvent != null){
				onServerErrorEvent(serverConnectionHandlerID,errorMessage,error,returnCode,extraMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onServerStopEvent_type))]
#endif
	private static void onServerStopEvent_(uint64 serverConnectionHandlerID, string shutdownMessage)
	{
		try {
			if(onServerStopEvent != null){
				onServerStopEvent(serverConnectionHandlerID,shutdownMessage);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onTextMessageEvent_type))]
#endif
	private static void onTextMessageEvent_(uint64 serverConnectionHandlerID, anyID targetMode, anyID toID, anyID fromID, string fromName, string fromUniqueIdentifier, string message)
	{
		try {
			if(onTextMessageEvent != null){
				onTextMessageEvent(serverConnectionHandlerID,targetMode,toID,fromID,fromName,fromUniqueIdentifier,message);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onTalkStatusChangeEvent_type))]
#endif
	private static void onTalkStatusChangeEvent_(uint64 serverConnectionHandlerID, int status, int isReceivedWhisper, anyID clientID)
	{
		try{
			if(onTalkStatusChangeEvent != null){
				onTalkStatusChangeEvent(serverConnectionHandlerID,status,isReceivedWhisper,clientID);
			}
		}catch(System.Exception exc){
			Debug.Log("error: " + exc.Message + "\n" + exc.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onIgnoredWhisperEvent_type))]
#endif
	private static void onIgnoredWhisperEvent_(uint64 serverConnectionHandlerID, anyID clientID)
	{
		try {
			if(onIgnoredWhisperEvent != null){
				onIgnoredWhisperEvent(serverConnectionHandlerID,clientID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onConnectionInfoEvent_type))]
#endif
	private static void onConnectionInfoEvent_(uint64 serverConnectionHandlerID, anyID clientID)
	{
		try {
			if(onConnectionInfoEvent != null){
				onConnectionInfoEvent(serverConnectionHandlerID,clientID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onServerConnectionInfoEvent_type))]
#endif
	private static void onServerConnectionInfoEvent_(uint64 serverConnectionHandlerID)
	{
		try {
			if(onServerConnectionInfoEvent != null){
				onServerConnectionInfoEvent(serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelSubscribeEvent_type))]
#endif
	private static void onChannelSubscribeEvent_(uint64 serverConnectionHandlerID, uint64 channelID)
	{
		try {
			if(onChannelSubscribeEvent != null){
				onChannelSubscribeEvent(serverConnectionHandlerID,channelID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelSubscribeFinishedEvent_type))]
#endif
	private static void onChannelSubscribeFinishedEvent_(uint64 serverConnectionHandlerID)
	{
		try {
			if(onChannelSubscribeFinishedEvent != null){
				onChannelSubscribeFinishedEvent(serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelUnsubscribeEvent_type))]
#endif
	private static void onChannelUnsubscribeEvent_(uint64 serverConnectionHandlerID, uint64 channelID)
	{
		try {
			if(onChannelUnsubscribeEvent != null){
				onChannelUnsubscribeEvent(serverConnectionHandlerID,channelID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelUnsubscribeFinishedEvent_type))]
#endif
	private static void onChannelUnsubscribeFinishedEvent_(uint64 serverConnectionHandlerID)
	{
		try {
			if(onChannelUnsubscribeFinishedEvent != null){
				onChannelUnsubscribeFinishedEvent(serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelDescriptionUpdateEvent_type))]
#endif
	private static void onChannelDescriptionUpdateEvent_(uint64 serverConnectionHandlerID, uint64 channelID)
	{
		try {
			if(onChannelDescriptionUpdateEvent != null){
				onChannelDescriptionUpdateEvent(serverConnectionHandlerID,channelID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onChannelPasswordChangedEvent_type))]
#endif
	private static void onChannelPasswordChangedEvent_(uint64 serverConnectionHandlerID, uint64 channelID)
	{
		try {
			if(onChannelPasswordChangedEvent != null){
				onChannelPasswordChangedEvent(serverConnectionHandlerID, channelID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onPlaybackShutdownCompleteEvent_type))]
#endif
	private static void onPlaybackShutdownCompleteEvent_(uint64 serverConnectionHandlerID)
	{
		try {
			if(onPlaybackShutdownCompleteEvent != null){
				onPlaybackShutdownCompleteEvent(serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onUserLoggingMessageEvent_type))]
#endif
	private static void onUserLoggingMessageEvent_(string logmessage, int logLevel, string logChannel, uint64 logID, string logTime, string completeLogString)
	{
		try {
			if(onUserLoggingMessageEvent != null){
				onUserLoggingMessageEvent(logmessage,logLevel,logChannel,logID,logTime,completeLogString);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onSoundDeviceListChangedEvent_type))]
	#endif
	private static void onSoundDeviceListChangedEvent_(string modeID, int playOrCap){
		try {
			if(onSoundDeviceListChangedEvent != null){
				onSoundDeviceListChangedEvent(modeID,playOrCap);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onEditPlaybackVoiceDataEvent_type))]
	#endif
	private static void onEditPlaybackVoiceDataEvent_(uint64 serverConnectionHandlerID, anyID clientID, IntPtr samples, int frameCount, int channels){
		try {
			if(onEditPlaybackVoiceDataEvent != null){
				onEditPlaybackVoiceDataEvent(serverConnectionHandlerID, clientID, samples, frameCount, channels);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onEditPostProcessVoiceDataEvent_type))]
	#endif
	private static void onEditPostProcessVoiceDataEvent_(uint64 serverConnectionHandlerID, anyID clientID, IntPtr samples, int frameCount, int channels, uint[] channelSpeakerArray, ref uint channelFillMask){
		try {
			if(onEditPostProcessVoiceDataEvent != null){
				onEditPostProcessVoiceDataEvent(serverConnectionHandlerID, clientID, samples, frameCount, channels, channelSpeakerArray, ref channelFillMask);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}
	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onEditMixedPlaybackVoiceDataEvent_type))]
	#endif
	private static void onEditMixedPlaybackVoiceDataEvent_(uint64 serverConnectionHandlerID, IntPtr samples, int frameCount, int channels, uint[] channelSpeakerArray, ref uint channelFillMask){
		try {
			if(onEditMixedPlaybackVoiceDataEvent != null){
				onEditMixedPlaybackVoiceDataEvent(serverConnectionHandlerID, samples, frameCount, channels, channelSpeakerArray, ref channelFillMask);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onEditCapturedVoiceDataEvent_type))]
	#endif
	private static void onEditCapturedVoiceDataEvent_(uint64 serverConnectionHandlerID, IntPtr samples, int frameCount, int channels, ref int edited){
		try {
			if(onEditCapturedVoiceDataEvent != null){
				onEditCapturedVoiceDataEvent(serverConnectionHandlerID, samples, frameCount, channels, ref edited);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onCustom3dRolloffCalculationClientEvent_type))]
	#endif
	private static void onCustom3dRolloffCalculationClientEvent_(uint64 serverConnectionHandlerID, anyID clientID, float distance, ref float volume){
		try {
			if(onCustom3dRolloffCalculationClientEvent != null){
				onCustom3dRolloffCalculationClientEvent(serverConnectionHandlerID, clientID, distance, ref volume);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onCustom3dRolloffCalculationWaveEvent_type))]
	#endif
	private static void onCustom3dRolloffCalculationWaveEvent_(uint64 serverConnectionHandlerID, uint64 waveHandle, float distance, ref float volume){
		try {
			if(onCustom3dRolloffCalculationWaveEvent != null){
				onCustom3dRolloffCalculationWaveEvent(serverConnectionHandlerID,  waveHandle,  distance, ref  volume);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onProvisioningSlotRequestResultEvent_type))]
	#endif
	private static void onProvisioningSlotRequestResultEvent_(uint error, uint64 requestHandle, string connectionKey){
		try {
			if(onProvisioningSlotRequestResultEvent != null){
				onProvisioningSlotRequestResultEvent(error, requestHandle, connectionKey);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onCheckServerUniqueIdentifierEvent_type))]
	#endif
	private static void onCheckServerUniqueIdentifierEvent_(uint64 serverConnectionHandlerID, string ServerUniqueIdentifier, ref int cancelConnect){
		try {
			if(onCheckServerUniqueIdentifierEvent != null){
				onCheckServerUniqueIdentifierEvent(serverConnectionHandlerID,  ServerUniqueIdentifier, ref cancelConnect);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onClientPasswordEncryptEvent_type))]
	#endif
	private static void onClientPasswordEncryptEvent_(uint64 serverConnectionHandlerID, string plaintext, IntPtr encryptedText, int encryptedTextByteSize){
		try {
			if(onClientPasswordEncryptEvent != null){
				onClientPasswordEncryptEvent(serverConnectionHandlerID, plaintext, encryptedText, encryptedTextByteSize);
			}else{
				//example of creating a copy of the plaintext into the encyptedtext
				byte[] chars = System.Text.Encoding.ASCII.GetBytes(plaintext + '\0');
				Marshal.Copy(chars,0,encryptedText,chars.Length);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onFileTransferStatusEvent_type))]
	#endif
	private static void onFileTransferStatusEvent_(anyID transferID, uint status, string statusMessage, uint64 remotefileSize, uint64 serverConnectionHandlerID){
		try {
			if(onFileTransferStatusEvent != null){
				onFileTransferStatusEvent(transferID, status, statusMessage, remotefileSize, serverConnectionHandlerID);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onFileListEvent_type))]
	#endif
	private static void onFileListEvent_(uint64 serverConnectionHandlerID, uint64 channelID, string path, string name, uint64 size, uint64 datetime, int type, uint64 incompletesize, string returnCode){
		try {
			if(onFileListEvent != null){
				onFileListEvent(serverConnectionHandlerID, channelID, path, name, size, datetime, type, incompletesize, returnCode);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onFileListFinishedEvent_type))]
	#endif
	private static void onFileListFinishedEvent_(uint64 serverConnectionHandlerID, uint64 channelID, string path){
		try {
			if(onFileListFinishedEvent != null){
				onFileListFinishedEvent(serverConnectionHandlerID, channelID, path);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[MonoPInvokeCallback (typeof (onFileInfoEvent_type))]
	#endif
	private static void onFileInfoEvent_(uint64 serverConnectionHandlerID, uint64 channelID, string name, uint64 size, uint64 datetime){
		try {
			if(onFileInfoEvent != null){
				onFileInfoEvent(serverConnectionHandlerID, channelID, name, size, datetime);
			}
		} catch (System.Exception ex) {
			Debug.Log("error: " + ex.StackTrace);
		}
	}

}
