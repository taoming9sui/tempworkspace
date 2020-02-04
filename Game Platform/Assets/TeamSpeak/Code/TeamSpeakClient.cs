using UnityEngine;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;
using uint64 = System.UInt64;
using anyID = System.UInt16;
using System.Collections.Generic;

public class TeamSpeakClient{

	public class TeamSpeakError{
		public uint code;
		public string message;

		public TeamSpeakError(uint code, string message){
			this.code = code;
			this.message = message;
		}
	};

	public class TeamSpeakSoundDevice{
		public string deviceName;
		public string deviceID;

		public TeamSpeakSoundDevice(string deviceName, string deviceID){
			this.deviceName = deviceName;
			this.deviceID = deviceID;
		}
	};

	//don't rename any of the values in the following enums as their string content is used

	public enum EncodeConfig{
		name,
		quality,
		bitrate,
	}

	public enum PlaybackConfig{
		volume_modifier,
		volume_factor_wave,
	}

	public enum PreProcessorConfig{
		name,					//Type of the used preprocessor. Currently this returns a constant string “Speex preprocessor”.
		denoise,				//Check if noise suppression is enabled. Returns “true” or “false”.
		vad,					//Check if Voice Activity Detection is enabled. Returns “true” or “false”.
		voiceactivation_level,	//Checks the Voice Activity Detection level in decibel. Returns a string with a numeric value, convert this to an integer.
		vad_extrabuffersize,	//Checks Voice Activity Detection extrabuffer size. Returns a string with a numeric value.
		agc,					//Check if Automatic Gain Control is enabled. Returns “true” or “false”.
		agc_level,				//Checks AGC level. Returns a string with a numeric value.
		agc_max_gain,			//Checks AGC max gain. Returns a string with a numeric value.
		echo_canceling,			//Checks if echo canceling is enabled. Returns a string with a boolean value.
	}

	public enum PreProcessorInfo{
		decibel_last_period,
	}

	public static bool logErrors = false;

	private static ClientUIFunctions cbs;
	private static ClientUIFunctionsRare cbs_rare;
	private static bool cbsInitialized = false;
	private static TeamSpeakClient instance = null;
	private string[] defaultChannel = new string[]{};	
	private static List<TeamSpeakError> errorMessages = new List<TeamSpeakError>();
	private List<uint64> scHandlerIDs = new List<uint64>();
	private List<string> serverAddresses = new List<string>();
	private static string serverAddress;
	private static uint serverPort;
	private static string serverPassword;
	private static string nickName;
	private static string defaultChannelPassword;
	private static uint64 scHandlerID = 0;
	private static string identity;
	public static bool started = false;
#if UNITY_ANDROID && !UNITY_EDITOR
	private AndroidJavaClass teamSpeakWrapper;
#endif

	public static TeamSpeakClient GetInstance(){
		if(instance == null){
			instance = new TeamSpeakClient();
		}
		return instance;
	}

	public bool NoErrors(){
		return errorMessages.Count == 0;
	}

	public static void ResetErrors(){
		errorMessages.Clear();
	}

	public static TeamSpeakError GetLastError(){
		if(errorMessages.Count > 0){
			return errorMessages[errorMessages.Count - 1];
		}else{
			return null;
		}
	}

	public static List<TeamSpeakError> GetErrors(){
		if(errorMessages.Count > 0){
			return errorMessages;
		}else{
			return null;
		}
	}

	public static void ClearTeamSpeakIdentity(){
		PlayerPrefs.SetString("ts3_identity","");
	}
	
	public string GetServerAddress(uint64 scHandlerID){
		int index = scHandlerIDs.IndexOf(scHandlerID);
		if(index >= 0){
			return serverAddresses[index];
		}else{
			return null;
		}
	}
	
	public uint64 GetServerConnectionHandlerID(string serverAddress){
		int index = serverAddresses.IndexOf(serverAddress);
		if(index >= 0){
			return scHandlerIDs[index];
		}else{
			return 0;
		}
	}

	// Use this for easy initialization on all platforms
	public void StartClient(string serverAddress, uint serverPort, string serverPassword, string nickName, ref string[] defaultChannel, string defaultChannelPassword) {
		if(!cbsInitialized){
			cbs = new ClientUIFunctions();
			cbs_rare = new ClientUIFunctionsRare();
			TeamSpeakCallbacks.Init(ref cbs);
			cbsInitialized = true;
		}
#if UNITY_IOS && !UNITY_EDITOR
		uint error = TeamSpeakInterface.ts3client_initClientLib(ref cbs, ref cbs_rare, (int)(LogTypes.LogType_FILE | LogTypes.LogType_CONSOLE), null,null);
#elif UNITY_ANDROID && !UNITY_EDITOR
		//Since normal callback doesn't work on Unity Android we start the client lib trough jni
		teamSpeakWrapper = new AndroidJavaClass("com.teamspeak.unity.TeamSpeakWrapper");
		teamSpeakWrapper.CallStatic("InitClientLib");
		uint error = public_errors.ERROR_ok;
#else
        uint error = TeamSpeakInterface.ts3client_initClientLib(ref cbs, ref cbs_rare, (int)(LogTypes.LogType_FILE | LogTypes.LogType_CONSOLE), null, Application.dataPath + System.IO.Path.DirectorySeparatorChar + "TeamSpeak" + System.IO.Path.DirectorySeparatorChar);		
#endif
		if (error != public_errors.ERROR_ok) {
			LogError(error);
			return;
		}
		error = TeamSpeakInterface.ts3client_spawnNewServerConnectionHandler(0, out scHandlerID);
		if (error != public_errors.ERROR_ok) {
			LogError(error);
			return;
		}

        //for Android and iOS we need to start custom sound device solutions
#if UNITY_IOS && !UNITY_EDITOR
		int AUDIO_SAMPLE_RATE = 48000;
		int AUDIO_NUM_CHANNELS = 2;
		int AUDIO_BIT_DEPTH_IN_BYTES = 2;
		int AUDIO_BIT_DEPTH = 16; // 8 * AUDIO_BIT_DEPTH_IN_BYTES
		int AUDIO_FORMAT_IS_NONINTERLEAVED = 0;
		string kWaveDeviceID = "iOS_WaveDeviceId";
		string kWaveDeviceDisplayName = "iOS AudioIO Device";
		error = TeamSpeakInterface.ts3client_registerCustomDevice(kWaveDeviceID,
                                                kWaveDeviceDisplayName,
                                                AUDIO_SAMPLE_RATE,
                                                AUDIO_NUM_CHANNELS,
                                                AUDIO_SAMPLE_RATE,
                                                AUDIO_NUM_CHANNELS);
		if(error != public_errors.ERROR_ok){
			LogError(error);
		}
#elif UNITY_ANDROID && !UNITY_EDITOR //#elif
		error = TeamSpeakInterface.ts3client_registerCustomDevice("Android", "Android device name", 48000, 1, 48000, 1);
		if(error != public_errors.ERROR_ok){
			LogError(error);
		}
#endif

#if UNITY_IOS && !UNITY_EDITOR
        error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, "custom", kWaveDeviceID);
#elif UNITY_ANDROID && !UNITY_EDITOR // #elif
		TeamSpeakInterface.ts3client_activateCaptureDevice(scHandlerID);
		error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, "custom", "Android");
#else
        error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, "", null);
#endif
		if (error != public_errors.ERROR_ok) {
			LogError(error);
		}

#if UNITY_IOS && !UNITY_EDITOR
		error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "custom", kWaveDeviceID);
#elif UNITY_ANDROID && !UNITY_EDITOR //#elif
		error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "custom", "Android");
#else
        error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "", null);
        /*error = TeamSpeakInterface.ts3client_registerCustomDevice("UnityAudio", "UnityAudio", 48000, 1, 48000, 1);
        error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "custom", "UnityAudio");*/
#endif
        if (error != public_errors.ERROR_ok) {
			LogError(error);
		}
		
		identity = PlayerPrefs.GetString("ts3_identity","");
		while(identity == ""){
			IntPtr identityPtr = IntPtr.Zero;
			error = TeamSpeakInterface.ts3client_createIdentity(out identityPtr);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return;
			}
			string tmp_identity = Marshal.PtrToStringAnsi(identityPtr);
			identity = string.Copy(tmp_identity);
			PlayerPrefs.SetString("ts3_identity",identity);
			TeamSpeakInterface.ts3client_freeMemory(identityPtr);  // Release dynamically allocated memory
		}

		TeamSpeakClient.serverAddress = serverAddress;
		TeamSpeakClient.serverPort = serverPort;
		TeamSpeakClient.nickName = nickName;
		TeamSpeakClient.defaultChannelPassword = defaultChannelPassword;
		TeamSpeakClient.serverPassword = serverPassword;

		if(defaultChannel != null && defaultChannel.Length > 0 && defaultChannel[defaultChannel.Length-1] != ""){
			errorMessages.Add(new TeamSpeakError(0,"Error: Can't use defaultChannel, string array doesn't end with an empty string"));
			if(logErrors){
				Debug.Log("Error: Can't use defaultChannel, string array doesn't end with an empty string");
			}
			defaultChannel = this.defaultChannel;
		}

		error = TeamSpeakInterface.ts3client_startConnection(TeamSpeakClient.scHandlerID, TeamSpeakClient.identity, TeamSpeakClient.serverAddress, TeamSpeakClient.serverPort, TeamSpeakClient.nickName, defaultChannel, TeamSpeakClient.defaultChannelPassword, TeamSpeakClient.serverPassword);
		if (error != public_errors.ERROR_ok) {
			LogError(error);
			return;
		}		
		
		scHandlerIDs.Add(scHandlerID);
		serverAddresses.Add(serverAddress);
		started = true;		
	}
	
	public void AddServerConnection(string serverAddress, uint serverPort, string serverPassword, string nickName, string[] defaultChannel, string defaultChannelPassword){
		if(started){
			uint64 scHandlerID = 0;
			/* Spawn a new server connection handler using the default port and store the server ID */
			uint error = TeamSpeakInterface.ts3client_spawnNewServerConnectionHandler(0, out scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return;
			}

#if UNITY_IOS && !UNITY_EDITOR
			int AUDIO_SAMPLE_RATE = 48000; //maybe double since it was float64
			int AUDIO_NUM_CHANNELS = 2;
			int AUDIO_BIT_DEPTH_IN_BYTES = 2;
			int AUDIO_BIT_DEPTH = 16; // 8 * AUDIO_BIT_DEPTH_IN_BYTES
			int AUDIO_FORMAT_IS_NONINTERLEAVED = 0;
			string kWaveDeviceID = "iOS_WaveDeviceId";
			string kWaveDeviceDisplayName = "iOS AudioIO Device";
			error = TeamSpeakInterface.ts3client_registerCustomDevice(kWaveDeviceID,
	                                                kWaveDeviceDisplayName,
	                                                AUDIO_SAMPLE_RATE,
	                                                AUDIO_NUM_CHANNELS,
	                                                AUDIO_SAMPLE_RATE,
	                                                AUDIO_NUM_CHANNELS);
			if(error != public_errors.ERROR_ok){
				LogError(error);
			}
#elif UNITY_ANDROID && !UNITY_EDITOR //#elif
			error = TeamSpeakInterface.ts3client_registerCustomDevice("Android", "Android device name", 48000, 1, 48000, 1);
			if(error != public_errors.ERROR_ok){
				LogError(error);
			}
#endif

#if UNITY_IOS && !UNITY_EDITOR
            			error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, "custom", kWaveDeviceID);
#elif UNITY_ANDROID && !UNITY_EDITOR //#elif
			TeamSpeakInterface.ts3client_activateCaptureDevice(scHandlerID);
			error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, "custom", "Android");
#else
            error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, "", null);
#endif
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}

#if UNITY_IOS && !UNITY_EDITOR
			error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "custom", kWaveDeviceID);
#elif UNITY_ANDROID && !UNITY_EDITOR //#elif
			error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "custom", "Android");
#else
            error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, "", null);
#endif
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
	
			string identity = PlayerPrefs.GetString("ts3_identity",null);
			if(identity == null){
				IntPtr identityPtr = IntPtr.Zero;
				error = TeamSpeakInterface.ts3client_createIdentity(out identityPtr);
				if (error != public_errors.ERROR_ok) {
					LogError(error);
					return;
				}
				string tmp_identity = Marshal.PtrToStringAnsi(identityPtr);
				identity = string.Copy(tmp_identity);
				PlayerPrefs.SetString("ts3_identity",identity);
				TeamSpeakInterface.ts3client_freeMemory(identityPtr); 
			}
			
			if(defaultChannel != null && defaultChannel.Length > 0 && defaultChannel[defaultChannel.Length-1] != ""){
				errorMessages.Add(new TeamSpeakError(0,"Error: Can't use defaultChannel, string array doesn't end with an empty string"));
				if(logErrors){
					Debug.Log("Error: Can't use defaultChannel, string array doesn't end with an empty string");
				}
				defaultChannel = this.defaultChannel;
			}

			error = TeamSpeakInterface.ts3client_startConnection(scHandlerID, identity, serverAddress, serverPort, nickName, defaultChannel, defaultChannelPassword, serverPassword);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return;
			}	
			scHandlerIDs.Add(scHandlerID);
			serverAddresses.Add(serverAddress);
		}
	}
	
	public void DropServerConnection(uint64 scHandlerID){
		int index = scHandlerIDs.IndexOf(scHandlerID);
		if(index >= 0){
			scHandlerIDs.RemoveAt(index);
			serverAddresses.RemoveAt(index);
			uint error = TeamSpeakInterface.ts3client_stopConnection(scHandlerID, "leaving");
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
			Thread.Sleep(200);
			error = TeamSpeakInterface.ts3client_destroyServerConnectionHandler(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		if(scHandlerIDs.Count == 0){
			StopClient();
		}
	}
	
	public void StopClient() {
		if(started){
			started = false;
			while(scHandlerIDs.Count > 0){
				DropServerConnection(scHandlerIDs[scHandlerIDs.Count-1]);
			}
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass teamSpeakWrapper = new AndroidJavaClass("com.teamspeak.unity.TeamSpeakWrapper");
			teamSpeakWrapper.CallStatic("StopAudio");
			teamSpeakWrapper.CallStatic("DestroyClientLib");
			uint error = public_errors.ERROR_ok;
#else
			uint error = TeamSpeakInterface.ts3client_destroyClientLib();
#endif
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return;
			}
			if(logErrors){
				Debug.Log("TeamSpeak client succesfully stopped");
			}
		}
	}
	
	public uint64 GetServerConnectionHandlerID(){
		if(started){
			return scHandlerIDs[0];
		}
		return 0;
	}

#if UNITY_ANDROID && !UNITY_EDITOR
	public void StartAudio(){
		if(started){
			teamSpeakWrapper.CallStatic("StopAudio");
		}
	}
	
	public void StopAudio(){
		if(started){
			teamSpeakWrapper.CallStatic("StopAudio");
		}
	}
#endif

	public int GetConnectionStatus(){
		if(started){
			return GetConnectionStatus(scHandlerIDs[0]);
		}
		return 0;
	}
	public int GetConnectionStatus(uint64 scHandlerID){
		if(started){
			int result = 0;
			uint error = TeamSpeakInterface.ts3client_getConnectionStatus(scHandlerID, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}
		return 0;
	}
	
	public uint64 GetClientLibVersionNumber(){
		if(started){
			uint64 result = 0;
			uint error = TeamSpeakInterface.ts3client_getClientLibVersionNumber(out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}
		return 0;
	}
	
	public void AllowWhispersFrom(anyID clientID){
		if(started){
			AllowWhispersFrom(scHandlerIDs[0],clientID);
		}
	}
	public void AllowWhispersFrom(uint64 scHandlerID, anyID clientID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_allowWhispersFrom(scHandlerID,clientID);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RemoveFromAllowedWhispersFrom(anyID clientID){
		if(started){
			RemoveFromAllowedWhispersFrom(scHandlerIDs[0],clientID);
		}
	}
	public void RemoveFromAllowedWhispersFrom(uint64 scHandlerID, anyID clientID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_removeFromAllowedWhispersFrom(scHandlerID,clientID);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestClientSetWhisperList(anyID clientID, uint64[] targetChannelIDArray, anyID[] targetClientIDArray){
		if(started){
			RequestClientSetWhisperList(scHandlerIDs[0],clientID,targetChannelIDArray,targetClientIDArray);
		}
	}
	public void RequestClientSetWhisperList(uint64 scHandlerID, anyID clientID, uint64[] targetChannelIDArray, anyID[] targetClientIDArray){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestClientSetWhisperList(scHandlerID,clientID,targetChannelIDArray,targetClientIDArray,IntPtr.Zero);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public List<uint64> GetServerConnectionHandlerList(){
		if(started){
			IntPtr intPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getServerConnectionHandlerList(out intPtr);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			int offset = 0;
			List<uint64> result = new List<uint64>();
			while(Marshal.ReadIntPtr(intPtr,offset) != IntPtr.Zero){
				result.Add((uint64)(Marshal.ReadInt64(intPtr,offset)));
				offset+=sizeof(uint64);
			}
			TeamSpeakInterface.ts3client_freeMemory(intPtr);
			return result;
		}
		return null;
	}
	
	public List<anyID> GetChannelClientList(uint64 channelID){
		if(started){
			return GetChannelClientList(scHandlerIDs[0],channelID);
		}
		return null;
	}
	public List<anyID> GetChannelClientList(uint64 scHandlerID, uint64 channelID){
		if(started){
			IntPtr intPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getChannelClientList(scHandlerID, channelID, out intPtr);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			int offset = 0;
			List<anyID> result = new List<anyID>();
			while(Marshal.ReadInt16(intPtr,offset) != 0){
				result.Add((anyID)(Marshal.ReadInt16(intPtr,offset)));
				offset+=sizeof(anyID);
			}
			TeamSpeakInterface.ts3client_freeMemory(intPtr);
			return result;
		}	
		return null;
	}

	public anyID GetClientID(){
		if(started){
			return GetClientID(scHandlerIDs[0]);
		}
		return 0;
	}
	public anyID GetClientID(uint64 scHandlerID){
		anyID clientID = 0;
		uint error = TeamSpeakInterface.ts3client_getClientID(scHandlerID, out clientID);  /* Calling some Client Lib function */
		if (error != public_errors.ERROR_ok) {
			LogError(error);
		}
		return clientID;
	}
	
	public  string GetClientLibVersion(){
		IntPtr versionPtr = IntPtr.Zero;
		uint error = TeamSpeakInterface.ts3client_getClientLibVersion(out versionPtr);
		if (error != public_errors.ERROR_ok) {
			LogError(error);
			return null;
		}
		string version = string.Copy(Marshal.PtrToStringAnsi(versionPtr));
		TeamSpeakInterface.ts3client_freeMemory(versionPtr);
		return version;
	}
	
	public uint64 GetChannelOfClient(anyID clientID){
		if(started){
			return GetChannelOfClient(scHandlerIDs[0],clientID);
		}
		return 0;
	}
	public uint64 GetChannelOfClient(uint64 scHandlerID, anyID clientID){
		if(started){
			uint64 channelID;
			uint error = TeamSpeakInterface.ts3client_getChannelOfClient(scHandlerID,clientID, out channelID);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
			}
			return channelID;
		}
		return 0;
	}
	
	public void RequestChannelDelete(uint64 channelID, int force){
		if(started){
			RequestChannelDelete(scHandlerIDs[0],channelID,force);
		}
	}
	public void RequestChannelDelete(uint64 scHandlerID, uint64 channelID, int force){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelDelete(scHandlerID,channelID,force,IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestChannelMove(uint64 channelID, uint64 newChannelParentID, uint64 newChannelOrder){
		if(started){
			RequestChannelMove(scHandlerIDs[0],channelID,newChannelParentID,newChannelOrder);
		}
	}
	public void RequestChannelMove(uint64 scHandlerID, uint64 channelID, uint64 newChannelParentID, uint64 newChannelOrder){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelMove(scHandlerID,channelID,newChannelParentID,newChannelOrder,IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestChannelDescription(uint64 channelID){
		if(started){
			RequestChannelDescription(scHandlerIDs[0],channelID);
		}
	}
	public void RequestChannelDescription(uint64 scHandlerID, uint64 channelID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelDescription(scHandlerID, channelID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public string GetChannelVariableAsString(uint64 channelID, ChannelProperties property){
		if(started){
			return GetChannelVariableAsString(scHandlerIDs[0],channelID,property);
		}
		return null;
	}
	public string GetChannelVariableAsString(uint64 scHandlerID, uint64 channelID, ChannelProperties property){
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getChannelVariableAsString(scHandlerID, channelID, property, out strPtr);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			string tmp_str = Marshal.PtrToStringAnsi(strPtr);
			string str = string.Copy(tmp_str); //copy to a string that is handled by the GarbageCollector 
			TeamSpeakInterface.ts3client_freeMemory(strPtr);  // Release dynamically allocated memory only if function succeeded
			return str;
		}else{
			return null;
		}
	}
	
	public uint64 GetChannelVariableAsUInt64(uint64 channelID, ChannelProperties property){
		if(started){
			return  GetChannelVariableAsUInt64(scHandlerIDs[0], channelID, property);
		}
		return 0;
	}
	public uint64 GetChannelVariableAsUInt64(uint64 scHandlerID, uint64 channelID, ChannelProperties property){
		if(started){
			uint64 result;
			uint error = TeamSpeakInterface.ts3client_getChannelVariableAsUInt64(scHandlerID,channelID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public int GetChannelVariableAsInt(uint64 channelID, ChannelProperties property){
		if(started){
			return GetChannelVariableAsInt(scHandlerIDs[0],channelID,property);
		}
		return 0;
	}
	public int GetChannelVariableAsInt(uint64 scHandlerID, uint64 channelID, ChannelProperties property){
		if(started){
			int result;
			uint error = TeamSpeakInterface.ts3client_getChannelVariableAsInt(scHandlerID, channelID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public void SetChannelVariableAsInt(uint64 channelID, ChannelProperties property, int newValue){
		if(started){
			SetChannelVariableAsInt(scHandlerIDs[0],channelID,property,newValue);
		}
	}
	public void SetChannelVariableAsInt(uint64 scHandlerID, uint64 channelID, ChannelProperties property, int newValue){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setChannelVariableAsInt(scHandlerID,channelID,property,newValue);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void SetChannelVariableAsUInt64(uint64 channelID, ChannelProperties property, uint64 newValue){
		if(started){
			SetChannelVariableAsUInt64(scHandlerIDs[0],channelID,property,newValue);
		}
	}
	public void SetChannelVariableAsUInt64(uint64 scHandlerID, uint64 channelID, ChannelProperties property, uint64 newValue){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setChannelVariableAsUInt64(scHandlerID,channelID,property,newValue);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void SetChannelVariableAsString(uint64 channelID, ChannelProperties property, string newValue){
		if(started){
			SetChannelVariableAsString(scHandlerIDs[0],channelID,property,newValue);
		}
	}
	public void SetChannelVariableAsString(uint64 scHandlerID, uint64 channelID, ChannelProperties property, string newValue){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setChannelVariableAsString(scHandlerID,channelID,property,newValue);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public List<uint64> GetChannelList(){
		if(started){
			return GetChannelList(scHandlerIDs[0]);
		}
		return null;
	}
	public List<uint64> GetChannelList(uint64 scHandlerID){
		if(started){
			IntPtr intPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getChannelList(scHandlerID,out intPtr);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			int offset = 0;
			List<uint64> result = new List<uint64>();
			while(Marshal.ReadIntPtr(intPtr,offset) != IntPtr.Zero){
				result.Add((uint64)(Marshal.ReadInt64(intPtr,offset)));
				offset+=sizeof(uint64);
			}
			TeamSpeakInterface.ts3client_freeMemory(intPtr);
			return result;
		}
		return null;
	}
	
	public uint64 GetChannelIDFromChannelNames(string[] names){
		if(started){
			return GetChannelIDFromChannelNames(scHandlerIDs[0],names);
		}
		return 0;
	}
	public uint64 GetChannelIDFromChannelNames(uint64 scHandlerID, string[] names){
		if(started){
			if(names == null || names.Length == 0){
				Debug.Log("Error:GetChannelIDFromChannelNames failed, names is empty");
				return uint64.MaxValue;
			}
			if(names[names.Length-1] != ""){
				Debug.Log("Error:GetChannelIDFromChannelNames failed, names is empty");
				return uint64.MaxValue;
			}
			uint64 result = 20;
			uint error = TeamSpeakInterface.ts3client_getChannelIDFromChannelNames(scHandlerID, names, out result);
			if(error != public_errors.ERROR_ok) {
				result = uint64.MaxValue;
				LogError(error);
			}
			return result;
		}
		return 0;
	}
	
	public void FlushChannelUpdates(uint64 channelID){
		if(started){
			FlushChannelUpdates(scHandlerIDs[0],channelID);
		}
	}
	public void FlushChannelUpdates(uint64 scHandlerID, uint64 channelID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_flushChannelUpdates(scHandlerID, channelID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void FlushChannelCreation(uint64 parentID){
		if(started){
			FlushChannelCreation(scHandlerIDs[0],parentID);
		}
	}
	public void FlushChannelCreation(uint64 scHandlerID, uint64 parentID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_flushChannelCreation(scHandlerID,parentID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public uint64 GetParentChannelOfChannel(uint64 channelID){
		if(started){
			return GetParentChannelOfChannel(scHandlerIDs[0],channelID);
		}
		return 0;
	}
	public uint64 GetParentChannelOfChannel(uint64 scHandlerID, uint64 channelID){
		if(started){
			uint64 parent;
			uint error = TeamSpeakInterface.ts3client_getParentChannelOfChannel(scHandlerID,channelID, out parent);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return parent;
		}
		return 0;
	}
	
	public void RequestChannelSubscribe(uint64[] channelIDs){
		if(started){
			RequestChannelSubscribe(scHandlerIDs[0],channelIDs);
		}
	}
	public void RequestChannelSubscribe(uint64 scHandlerID, uint64[] channelIDs){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelSubscribe(scHandlerID,channelIDs,IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestChannelUnsubscribe(uint64[] channelIDs){
		if(started){
			RequestChannelUnsubscribe(scHandlerIDs[0],channelIDs);
		}
	}
	public void RequestChannelUnsubscribe(uint64 scHandlerID, uint64[] channelIDs){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelUnsubscribe(scHandlerID,channelIDs,IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestChannelSubscribeAll(){
		if(started){
			RequestChannelSubscribeAll(scHandlerIDs[0]);
		}
	}
	public void RequestChannelSubscribeAll(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelSubscribeAll(scHandlerID,IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestChannelUnsubscribeAll(){
		if(started){
			RequestChannelUnsubscribeAll(scHandlerIDs[0]);
		}
	}
	public void RequestChannelUnsubscribeAll(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestChannelUnsubscribeAll(scHandlerID,IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestServerVariables(){
		if(started){
			RequestServerVariables(scHandlerIDs[0]);
		}
	}
	public void RequestServerVariables(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestServerVariables(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void RequestClientVariables(anyID clientID){	
		if(started){
			RequestClientVariables(scHandlerIDs[0],clientID);
		}
	}
	public void RequestClientVariables(uint64 scHandlerID, anyID clientID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestClientVariables(scHandlerID, clientID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public string GetClientVariableAsString(anyID clientID, ClientProperties property){
		if(started){
			return GetClientVariableAsString(scHandlerIDs[0],clientID,property);
		}
		return null;
	}
	public string GetClientVariableAsString(uint64 scHandlerID, anyID clientID, ClientProperties property){
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getClientVariableAsString(scHandlerID,clientID, property, out strPtr);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			string tmp_str = Marshal.PtrToStringAnsi(strPtr);
			string str = string.Copy(tmp_str); //copy to a string that is handled by the GarbageCollector 
			TeamSpeakInterface.ts3client_freeMemory(strPtr);  // Release dynamically allocated memory only if function succeeded
			return str;
		}else{
			return null;
		}
	}
	
	public string GetClientSelfVariableAsString(ClientProperties property){
		if(started){
			return GetClientSelfVariableAsString(scHandlerIDs[0],property);
		}
		return null;
	}
	public string GetClientSelfVariableAsString(uint64 scHandlerID, ClientProperties property){
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getClientSelfVariableAsString(scHandlerID, property, out strPtr);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			string tmp_str = Marshal.PtrToStringAnsi(strPtr);
			string str = string.Copy(tmp_str);
			TeamSpeakInterface.ts3client_freeMemory(strPtr);
			return str;
		}else{
			return null;
		}
	}
	
	public uint64 GetClientVariableAsUInt64(anyID clientID, ClientProperties property){
		if(started){
			return GetClientVariableAsUInt64(scHandlerIDs[0],clientID,property);
		}
		return 0;
	}
	public uint64 GetClientVariableAsUInt64(uint64 scHandlerID, anyID clientID, ClientProperties property){
		if(started){
			uint64 result;
			uint error = TeamSpeakInterface.ts3client_getClientVariableAsUInt64(scHandlerID,clientID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public int GetClientSelfVariableAsInt(ClientProperties property){
		if(started){
			return GetClientSelfVariableAsInt(scHandlerIDs[0],property);
		}
		return 0;
	}
	public int GetClientSelfVariableAsInt(uint64 scHandlerID, ClientProperties property){
		if(started){
			int result;
			uint error = TeamSpeakInterface.ts3client_getClientSelfVariableAsInt(scHandlerID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public int GetClientVariableAsInt(anyID clientID, ClientProperties property){
		if(started){
			return GetClientVariableAsInt(scHandlerIDs[0],clientID,property);
		}
		return 0;
	}
	public int GetClientVariableAsInt(uint64 scHandlerID, anyID clientID, ClientProperties property){
		if(started){
			int result;
			uint error = TeamSpeakInterface.ts3client_getClientVariableAsInt(scHandlerID,clientID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public void SetClientSelfVariableAsInt(ClientProperties property, int newValue){
		if(started){
			SetClientSelfVariableAsInt(scHandlerIDs[0],property,newValue);
		}
	}
	public void SetClientSelfVariableAsInt(uint64 scHandlerID, ClientProperties property, int newValue){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setClientSelfVariableAsInt(scHandlerID,property,newValue);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void SetClientSelfVariableAsString(ClientProperties property, string newValue){
		if(started){
			SetClientSelfVariableAsString(scHandlerIDs[0],property,newValue);
		}
	}
	public void SetClientSelfVariableAsString(uint64 scHandlerID, ClientProperties property, string newValue){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setClientSelfVariableAsString(scHandlerID,property,newValue);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void FlushClientSelfUpdates(){
		if(started){
			FlushClientSelfUpdates(scHandlerIDs[0]);
		}
	}
	public void FlushClientSelfUpdates(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_flushClientSelfUpdates(scHandlerID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
		
	public string GetServerVariableAsString(VirtualServerProperties property){
		if(started){
			return GetServerVariableAsString(scHandlerIDs[0],property);
		}
		return null;
	}
	public string GetServerVariableAsString(uint64 scHandlerID, VirtualServerProperties property){
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getServerVariableAsString(scHandlerID, property, out strPtr);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			string tmp_str = Marshal.PtrToStringAnsi(strPtr);
			string str = string.Copy(tmp_str); //copy to a string that is handled by the GarbageCollector 
			TeamSpeakInterface.ts3client_freeMemory(strPtr);  // Release dynamically allocated memory only if function succeeded
			return str;
		}else{
			return null;
		}
	}
	
	public uint64 GetServerVariableAsUInt64(VirtualServerProperties property){
		if(started){
			return GetServerVariableAsUInt64(scHandlerIDs[0],property);
		}
		return 0;
	}
	public uint64 GetServerVariableAsUInt64(uint64 scHandlerID, VirtualServerProperties property){
		if(started){
			uint64 result;
			uint error = TeamSpeakInterface.ts3client_getServerVariableAsUInt64(scHandlerID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public int GetServerVariableAsInt(VirtualServerProperties property){
		if(started){
			return GetServerVariableAsInt(scHandlerIDs[0],property);
		}
		return 0;
	}
	public int GetServerVariableAsInt(uint64 serverConnectionHandlerID, VirtualServerProperties property){
		if(started){
			int result;
			uint error = TeamSpeakInterface.ts3client_getServerVariableAsInt(serverConnectionHandlerID, property, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return 0;
			}
			return result;
		}else{
			return 0;
		}
	}
	
	public List<anyID> GetClientList(){
		if(started){
			GetClientList(scHandlerIDs[0]);
		}
		return null;
	}
	public List<anyID> GetClientList(uint64 scHandlerID){
		if(started){
			IntPtr intPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getClientList(scHandlerID, out intPtr);
			if(error != public_errors.ERROR_ok) {
				LogError(error);
				return null;
			}
			int offset = 0;
			List<anyID> result = new List<anyID>();
			while(Marshal.ReadInt16(intPtr,offset) != 0){
				result.Add((anyID)(Marshal.ReadInt16(intPtr,offset)));
				offset+=sizeof(anyID);
			}
			TeamSpeakInterface.ts3client_freeMemory(intPtr);
			return result;
		}	
		return null;
	}
	
	public void RequestClientKickFromChannel(anyID clientID, string kickReason){
		if(started){
			RequestClientKickFromChannel(scHandlerIDs[0],clientID,kickReason);
		}
	}
	public void RequestClientKickFromChannel(uint64 scHandlerID, anyID clientID, string kickReason){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestClientKickFromChannel(scHandlerID, clientID, kickReason, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestClientKickFromServer(anyID clientID, string kickReason){
		if(started){
			RequestClientKickFromServer(scHandlerIDs[0],clientID,kickReason);
		}
	}
	public void RequestClientKickFromServer(uint64 scHandlerID, anyID clientID, string kickReason){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestClientKickFromServer(scHandlerID, clientID, kickReason, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
		
	public void RequestClientMove(anyID clientID, uint64 newChannelID, string password){
		if(started){
			RequestClientMove(scHandlerIDs[0],clientID,newChannelID,password);
		}
	}
	public void RequestClientMove(uint64 scHandlerID, anyID clientID, uint64 newChannelID, string password){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestClientMove(scHandlerID, clientID, newChannelID, password, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestMuteClients(anyID[] clientIDs){
		if(started){
			RequestMuteClients(scHandlerIDs[0],clientIDs);
		}
	}
	public void RequestMuteClients(uint64 scHandlerID, anyID[] clientIDs){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestMuteClients(scHandlerID, clientIDs, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestUnmuteClients(anyID[] clientIDs){
		if(started){
			RequestUnmuteClients(scHandlerIDs[0],clientIDs);
		}
	}
	public void RequestUnmuteClients(uint64 scHandlerID, anyID[] clientIDs){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestUnmuteClients(scHandlerID, clientIDs, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestSendChannelTextMsg(string message, uint64 channelID){
		if(started){
			RequestSendChannelTextMsg(scHandlerIDs[0], message, channelID);
		}
	}
	public void RequestSendChannelTextMsg(uint64 scHandlerID, string message, uint64 channelID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestSendChannelTextMsg(scHandlerID, message, channelID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestSendServerTextMsg(string message){
		if(started){
			RequestSendServerTextMsg(scHandlerIDs[0], message);
		}
	}
	public void RequestSendServerTextMsg(uint64 scHandlerID, string message){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestSendServerTextMsg(scHandlerID, message, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void RequestSendPrivateTextMsg(string message, anyID clientID){
		if(started){
			RequestSendPrivateTextMsg(scHandlerIDs[0], message, clientID);
		}
	}
	public void RequestSendPrivateTextMsg(uint64 scHandlerID, string message, anyID clientID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestSendPrivateTextMsg(scHandlerID, message, clientID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void SystemSet3DListenerAttributes(Transform transform){
		if(started){
			SystemSet3DListenerAttributes(scHandlerIDs[0],transform.position,transform.forward,transform.up);
		}
	}
	public void SystemSet3DListenerAttributes(uint64 scHandlerID, Transform transform){
		if(started){
			SystemSet3DListenerAttributes(scHandlerID,transform.position,transform.forward,transform.up);
		}
	}
	public void SystemSet3DListenerAttributes(Vector3 position, Vector3 forward, Vector3 up){
		if(started){
			SystemSet3DListenerAttributes(scHandlerIDs[0],position,forward,up);
		}
	}
	public void SystemSet3DListenerAttributes(uint64 scHandlerID, Vector3 position, Vector3 forward, Vector3 up){
		if(started){
			TeamSpeakInterface.TS3_VECTOR position_ = new TeamSpeakInterface.TS3_VECTOR();
			position_.x = position.x;
			position_.y = position.y;
			position_.z = position.z;
			TeamSpeakInterface.TS3_VECTOR forward_ = new TeamSpeakInterface.TS3_VECTOR();
			forward_.x = forward.x;
			forward_.y = forward.y;
			forward_.z = forward.z;
			TeamSpeakInterface.TS3_VECTOR up_ = new TeamSpeakInterface.TS3_VECTOR();
			up_.x = up.x;
			up_.y = up.y;
			up_.z = up.z;
			
			uint error = TeamSpeakInterface.ts3client_systemset3DListenerAttributes(scHandlerID,out position_,out forward_,out up_);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void SystemSet3DSettings(float distanceFactor, float rolloffScale){
		if(started){
			SystemSet3DSettings(scHandlerIDs[0],distanceFactor,rolloffScale);
		}
	}
	public void SystemSet3DSettings(uint64 scHandlerID, float distanceFactor, float rolloffScale){
		if(started){
			uint error = TeamSpeakInterface.ts3client_systemset3DSettings(scHandlerID,distanceFactor,rolloffScale);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void ChannelSet3DAttributes(anyID clientID, Vector3 position){
		if(started){
			ChannelSet3DAttributes(scHandlerIDs[0],clientID,position);
		}
	}
	public void ChannelSet3DAttributes(uint64 scHandlerID, anyID clientID, Vector3 position){
		if(started){
			TeamSpeakInterface.TS3_VECTOR position_ = new TeamSpeakInterface.TS3_VECTOR();
			position_.x = position.x;
			position_.y = position.y;
			position_.z = position.z;
			uint error = TeamSpeakInterface.ts3client_channelset3DAttributes(scHandlerID, clientID, out position_);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void InitCLientLib(ref ClientUIFunctions clientUIFunctions, ref ClientUIFunctionsRare clientUIFunctionsRare, int logTypes, string logFileFolder, string resources){
		uint error = TeamSpeakInterface.ts3client_initClientLib(ref clientUIFunctions,ref clientUIFunctionsRare,logTypes,logFileFolder,resources);
		if(error != public_errors.ERROR_ok){
			LogError(error);
		}else{
			started = true;
		}
	}

	public void FreeMemory(IntPtr memoryPointer){
		if(started){
			TeamSpeakInterface.ts3client_freeMemory(memoryPointer);
		}
	}
	
	public void SpawnNewServerConnectionHandler(int port, out uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_spawnNewServerConnectionHandler(port, out scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}else{
			scHandlerID = 0;
		}
	}

	public void OpenCaptureDevice(string modeID, string captureDevice){
		if(started){
			OpenCaptureDevice(scHandlerIDs[0],modeID,captureDevice);
		}
	}
	public void OpenCaptureDevice(uint64 scHandlerID, string modeID, string captureDevice){
		if(started){
			uint error = TeamSpeakInterface.ts3client_openCaptureDevice(scHandlerID, modeID, captureDevice);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void OpenPlaybackDevice( string modeID, string playbackDevice){
		if(started){
			OpenPlaybackDevice(scHandlerIDs[0], modeID, playbackDevice);
		}
	}
	public void OpenPlaybackDevice(uint64 scHandlerID, string modeID, string playbackDevice){
		if(started){
			uint error = TeamSpeakInterface.ts3client_openPlaybackDevice(scHandlerID, modeID, playbackDevice);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void CreateIdentity(out string result){
		if(started){
			IntPtr identityPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_createIdentity(out identityPtr);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				result = null;
				return;
			}
			string tmp_result = Marshal.PtrToStringAnsi(identityPtr);
			result = string.Copy(tmp_result);
			TeamSpeakInterface.ts3client_freeMemory(identityPtr);
		}else{
			result = null;
		}
	}

	public void StartConnection(string identity, string ip, uint port, string nick, string[] defaultchannel, string defaultchannelpassword, string serverpassword){
		if(started){
			StartConnection(scHandlerIDs[0], identity, ip, port, nick, defaultchannel, defaultchannelpassword, serverpassword);
		}
	}
	public void StartConnection(uint64 scHandlerID, string identity, string ip, uint port, string nick, string[] defaultchannel, string defaultchannelpassword, string serverpassword){
		if(started){
			uint error = TeamSpeakInterface.ts3client_startConnection(scHandlerID, identity, serverAddress, serverPort, nickName, defaultChannel, defaultChannelPassword, serverPassword);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
				return;
			}
		}
	}

	public void StopConnection(string quitMessage){
		if(started){
			StopConnection(scHandlerIDs[0], quitMessage);
		}
	}
	public void StopConnection(uint64 scHandlerID, string quitMessage){
		if(started){
			uint error = TeamSpeakInterface.ts3client_stopConnection(scHandlerID, quitMessage);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void DestroyServerConnectionHandler(){
		if(started){
			DestroyServerConnectionHandler(scHandlerIDs[0]);
		}
	}
	public void DestroyServerConnectionHandler(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_destroyServerConnectionHandler(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void DestroyClientLib(){
		if(started){
			uint error = TeamSpeakInterface.ts3client_destroyClientLib();
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public string GetErrorMessage(uint error){
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			if(TeamSpeakInterface.ts3client_getErrorMessage(error,out strPtr) == public_errors.ERROR_ok) {
				string tmp_str = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_str);
				TeamSpeakInterface.ts3client_freeMemory(strPtr);
			}
		}
		return result;
	}

	public void AcquireCustomPlaybackData(string deviceID, short[] buffer, int samples){
		if(started){
			uint error = TeamSpeakInterface.ts3client_acquireCustomPlaybackData(deviceID,buffer,samples);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void ProcessCustomCaptureData(string deviceID, short[] buffer, int samples){
		if(started){
			uint error = TeamSpeakInterface.ts3client_processCustomCaptureData(deviceID,buffer,samples);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void ActivateCaptureDevice() {
		if(started){
			ActivateCaptureDevice(scHandlerIDs[0]);
		}
	}
	public void ActivateCaptureDevice(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_activateCaptureDevice(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void CloseCaptureDevice(){
		if(started){
			CloseCaptureDevice(scHandlerIDs[0]);
		}
	}
	public void CloseCaptureDevice(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_closeCaptureDevice(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void ClosePlaybackDevice(){
		if(started){
			ClosePlaybackDevice(scHandlerIDs[0]);
		}
	}
	public void ClosePlaybackDevice(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_closePlaybackDevice(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void CloseWaveFileHandle(uint64 waveHandler){
		if(started){
			CloseWaveFileHandle(scHandlerIDs[0], waveHandler);
		}
	}
	public void CloseWaveFileHandle(uint64 scHandlerID, uint64 waveHandler){
		if(started){
			uint error = TeamSpeakInterface.ts3client_closeWaveFileHandle(scHandlerID,waveHandler);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public string GetDefaultCaptureMode(){
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getDefaultCaptureMode(out strPtr);
			if(error == public_errors.ERROR_ok) {
				string tmp_str = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_str);
				TeamSpeakInterface.ts3client_freeMemory(strPtr);
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public TeamSpeakSoundDevice GetDefaultCaptureDevice(string modeID){
		TeamSpeakSoundDevice result = null;
		if(started){
			IntPtr array = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getDefaultCaptureDevice(modeID, out array);
			if(error == public_errors.ERROR_ok) {  /* Query printable error */
				int pointerSize = Marshal.SizeOf(array);
				IntPtr deviceNamePointer = Marshal.ReadIntPtr(array);
				IntPtr deviceIdPointer = Marshal.ReadIntPtr(array,pointerSize);
				string tmp_deviceName = Marshal.PtrToStringAnsi(deviceNamePointer);
				string tmp_deviceId = Marshal.PtrToStringAnsi(deviceIdPointer);
				string deviceName = string.Copy(tmp_deviceName);
				string deviceId = string.Copy(tmp_deviceId);
				result = new TeamSpeakSoundDevice(deviceName,deviceId);
				TeamSpeakInterface.ts3client_freeMemory(deviceNamePointer);  /* Release memory */
				TeamSpeakInterface.ts3client_freeMemory(deviceIdPointer);
				TeamSpeakInterface.ts3client_freeMemory(array);
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public List<string> GetCaptureModeList(){
		List<string> result = new List<string>();
		if(started){
			IntPtr array = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getCaptureModeList(out array);
			if(error == public_errors.ERROR_ok){
				int pointerSize = Marshal.SizeOf(array);
				IntPtr element = Marshal.ReadIntPtr(array);
				int i = 0;
				while(element != IntPtr.Zero && i < 100){
					string tmp_captureMode = Marshal.PtrToStringAnsi(element);
					string captureMode = string.Copy(tmp_captureMode);
					TeamSpeakInterface.ts3client_freeMemory(element);
					result.Add(captureMode);
					i++;
					element = Marshal.ReadIntPtr(array,i*pointerSize);
				}
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public List<TeamSpeakSoundDevice> GetCaptureDeviceList(string modeID){
		List<TeamSpeakSoundDevice> result = new List<TeamSpeakSoundDevice>();
		if(started){
			IntPtr array;
			uint error = TeamSpeakInterface.ts3client_getCaptureDeviceList(modeID, out array);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}else{
				int pointerSize = Marshal.SizeOf(array);
				int i = 0;
				IntPtr element = Marshal.ReadIntPtr(array);
				while(element != IntPtr.Zero && i < 100){
					IntPtr deviceNamePointer = Marshal.ReadIntPtr(element);
					string tmp_deviceName = Marshal.PtrToStringAnsi(deviceNamePointer);
					string deviceName = string.Copy(tmp_deviceName);
					IntPtr deviceIDPointer = Marshal.ReadIntPtr(element,pointerSize);
					string tmp_deviceID = Marshal.PtrToStringAnsi(deviceIDPointer);
					string deviceID = string.Copy(tmp_deviceID);
					result.Add(new TeamSpeakSoundDevice(deviceName,deviceID));
					TeamSpeakInterface.ts3client_freeMemory(deviceNamePointer);
					TeamSpeakInterface.ts3client_freeMemory(deviceIDPointer);
					TeamSpeakInterface.ts3client_freeMemory(element);
					i++;
					element = Marshal.ReadIntPtr(array,i*pointerSize);
				}
				TeamSpeakInterface.ts3client_freeMemory(array);
			}
		}
		return result;
	}

	public void GetCurrentCaptureDeviceName(out string deviceName, out int isDefault){
		if(started){
			GetCurrentCaptureDeviceName(scHandlerIDs[0], out deviceName, out isDefault);
		}else{
			deviceName = null;
			isDefault = 0;
		}
	}
	public void GetCurrentCaptureDeviceName(uint64 scHandlerID, out string deviceName, out int isDefault){
		deviceName = null;
		isDefault = 0;
		if(started){
			IntPtr deviceNamePointer = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getCurrentCaptureDeviceName(scHandlerID,out deviceNamePointer, out isDefault);
			if(error == public_errors.ERROR_ok){
				string tmp_deviceName = Marshal.PtrToStringAnsi(deviceNamePointer);
				deviceName = string.Copy(tmp_deviceName);
				TeamSpeakInterface.ts3client_freeMemory(deviceNamePointer);
			}else{
				LogError(error);
			}
		}
	}

	public string  GetCurrentCaptureMode(){
		if(started){
			return GetCurrentCaptureMode(scHandlerIDs[0]);
		}else{
			return null;
		}
	}
	public string GetCurrentCaptureMode(uint64 scHandlerID){
		string result = null;
		if(started){
			IntPtr captureModePointer = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getCurrentCaptureMode(scHandlerID, out captureModePointer);
			if (error == public_errors.ERROR_ok) {
				string tmp_captureMode = Marshal.PtrToStringAnsi(captureModePointer);
				result = string.Copy(tmp_captureMode);
				TeamSpeakInterface.ts3client_freeMemory(captureModePointer);
			}else{
				LogError(error);
			}
		}
		return result;
	}
	
	public string GetDefaultPlayBackMode(){
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getDefaultPlayBackMode(out strPtr);
			if(error == public_errors.ERROR_ok) {  /* Query printable error */
				string tmp_str = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_str); //copy to a string that is handled by the GarbageCollector 
				TeamSpeakInterface.ts3client_freeMemory(strPtr);  /* Release memory */
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public TeamSpeakSoundDevice GetDefaultPlaybackDevice(string modeID){
		TeamSpeakSoundDevice result = null;
		if(started){
			IntPtr array = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getDefaultPlaybackDevice(modeID, out array);
			if(error == public_errors.ERROR_ok) {  /* Query printable error */
				int pointerSize = Marshal.SizeOf(array);
				IntPtr deviceNamePointer = Marshal.ReadIntPtr(array);
				IntPtr deviceIdPointer = Marshal.ReadIntPtr(array,pointerSize);
				string tmp_deviceName = Marshal.PtrToStringAnsi(deviceNamePointer);
				string tmp_deviceId = Marshal.PtrToStringAnsi(deviceIdPointer);
				string deviceName = string.Copy(tmp_deviceName);
				string deviceId = string.Copy(tmp_deviceId);
				result = new TeamSpeakSoundDevice(deviceName,deviceId);
				TeamSpeakInterface.ts3client_freeMemory(deviceNamePointer);  /* Release memory */
				TeamSpeakInterface.ts3client_freeMemory(deviceIdPointer);
				TeamSpeakInterface.ts3client_freeMemory(array);
			}else{
				LogError(error);
			}
		}
		return result;
	}
	
	public List<string> GetPlaybackModeList(){
		List<string> result = new List<string>();
		if(started){
			IntPtr array = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getPlaybackModeList(out array);
			if(error == public_errors.ERROR_ok){
				int pointerSize = Marshal.SizeOf(array);
				IntPtr element = Marshal.ReadIntPtr(array);
				int i = 0;
				while(element != IntPtr.Zero && i < 100){
					string tmp_playbackMode = Marshal.PtrToStringAnsi(element);
					string playbackMode = string.Copy(tmp_playbackMode);
					TeamSpeakInterface.ts3client_freeMemory(element);
					result.Add(playbackMode);
					i++;
					element = Marshal.ReadIntPtr(array,i*pointerSize);
				}
			}else{
				LogError(error);
			}
		}
		return result;
	}
	
	public List<TeamSpeakSoundDevice> GetPlaybackDeviceList(string modeID){
		List<TeamSpeakSoundDevice> result = new List<TeamSpeakSoundDevice>();
		if(started){
			IntPtr array;
			uint error = TeamSpeakInterface.ts3client_getPlaybackDeviceList(modeID, out array);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}else{
				int pointerSize = Marshal.SizeOf(array);
				int i = 0;
				IntPtr element = Marshal.ReadIntPtr(array);
				while(element != IntPtr.Zero && i < 100){
					IntPtr deviceNamePointer = Marshal.ReadIntPtr(element);
					string tmp_deviceName = Marshal.PtrToStringAnsi(deviceNamePointer);
					string deviceName = string.Copy(tmp_deviceName);
					IntPtr deviceIDPointer = Marshal.ReadIntPtr(element,pointerSize);
					string tmp_deviceID = Marshal.PtrToStringAnsi(deviceIDPointer);
					string deviceID = string.Copy(tmp_deviceID);
					result.Add(new TeamSpeakSoundDevice(deviceName,deviceID));
					TeamSpeakInterface.ts3client_freeMemory(deviceNamePointer);
					TeamSpeakInterface.ts3client_freeMemory(deviceIDPointer);
					TeamSpeakInterface.ts3client_freeMemory(element);
					i++;
					element = Marshal.ReadIntPtr(array,i*pointerSize);
				}
				TeamSpeakInterface.ts3client_freeMemory(array);
			}
		}
		return result;
	}

	public void GetCurrentPlaybackDeviceName(out string deviceName, out int isDefault){
		if(started){
			GetCurrentPlaybackDeviceName(scHandlerIDs[0], out deviceName, out isDefault);
		}else{
			deviceName = null;
			isDefault = 0;
		}
	}
	public void GetCurrentPlaybackDeviceName(uint64 scHandlerID, out string deviceName, out int isDefault){
		deviceName = null;
		isDefault = 0;
		if(started){
			IntPtr deviceNamePointer = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getCurrentPlaybackDeviceName(scHandlerID,out deviceNamePointer, out isDefault);
			if(error == public_errors.ERROR_ok){
				string tmp_deviceName = Marshal.PtrToStringAnsi(deviceNamePointer);
				deviceName = string.Copy(tmp_deviceName);
				TeamSpeakInterface.ts3client_freeMemory(deviceNamePointer);
			}else{
				LogError(error);
			}
		}
	}

	public string GetCurrentPlayBackMode(){
		if(started){
			return GetCurrentPlayBackMode(scHandlerIDs[0]);
		}else{
			return null;
		}
	}
	public string GetCurrentPlayBackMode(uint64 scHandlerID){
		string result = null;
		if(started){
			IntPtr playbackModePointer = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getCurrentPlayBackMode(scHandlerID, out playbackModePointer);
			if (error == public_errors.ERROR_ok) {
				string tmp_playbackMode = Marshal.PtrToStringAnsi(playbackModePointer);
				result = string.Copy(tmp_playbackMode);
				TeamSpeakInterface.ts3client_freeMemory(playbackModePointer);
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public string GetEncodeConfigValue (EncodeConfig ident){
		if(started){
			return GetEncodeConfigValue (scHandlerIDs[0], ident);
		}else{
			return null;
		}
	}
	public string GetEncodeConfigValue (uint64 scHandlerID, EncodeConfig ident){
		string result = null;
		if(started){
			IntPtr valuePointer = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getEncodeConfigValue(scHandlerID,ident.ToString(),out valuePointer);
			if (error == public_errors.ERROR_ok) {
				string tmp_value = Marshal.PtrToStringAnsi(valuePointer);
				result = string.Copy(tmp_value);
				TeamSpeakInterface.ts3client_freeMemory(valuePointer);
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public float GetPlaybackConfigValueAsFloat(PlaybackConfig ident){
		if(started){
			return GetPlaybackConfigValueAsFloat(scHandlerIDs[0], ident);
		}else{
			return 0;
		}
	}
	public float GetPlaybackConfigValueAsFloat(uint64 scHandlerID,PlaybackConfig ident){
		float result = 0f;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getPlaybackConfigValueAsFloat(scHandlerID,ident.ToString(),out result);
			if (error == public_errors.ERROR_ok) {
				return result;
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public string GetPreProcessorConfigValue(PreProcessorConfig ident){
		if(started){
			return GetPreProcessorConfigValue(scHandlerIDs[0], ident);
		}else{
			return null;
		}
	}
	public string GetPreProcessorConfigValue(uint64 scHandlerID, PreProcessorConfig ident){
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getPreProcessorConfigValue(scHandlerID,ident.ToString(),out strPtr);
			if (error == public_errors.ERROR_ok) {
				string tmp_value = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_value);
				TeamSpeakInterface.ts3client_freeMemory(strPtr);
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public float GetPreProcessorInfoValueFloat(PreProcessorInfo ident){
		if(started){
			return GetPreProcessorInfoValueFloat(scHandlerIDs[0], ident);
		}else{
			return 0;
		}
	}
	public float GetPreProcessorInfoValueFloat(uint64 scHandlerID,PreProcessorInfo ident){
		float result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getPreProcessorInfoValueFloat(scHandlerID,ident.ToString(), out result);
			if (error == public_errors.ERROR_ok) {
				return result;
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public void InitiateGracefulPlaybackShutdown(){
		if(started){
			InitiateGracefulPlaybackShutdown(scHandlerIDs[0]);
		}
	}
	public void InitiateGracefulPlaybackShutdown(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_initiateGracefulPlaybackShutdown(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void LogMessage(string message, TeamSpeakInterface.LogLevel severity, string channel = "", uint64 logID = 0){
		if(started){
			uint error = TeamSpeakInterface.ts3client_logMessage(message,severity,channel,logID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void PauseWaveFileHandle(uint64 waveHandle, bool paused){
		if(started){
			PauseWaveFileHandle(scHandlerIDs[0],  waveHandle,  paused);
		}
	}
	public void PauseWaveFileHandle(uint64 scHandlerID, uint64 waveHandle, bool paused){
		if(started){
			int pausedInt = 0;
			if(paused){
				pausedInt = 1;
			}
			uint error = TeamSpeakInterface.ts3client_pauseWaveFileHandle(scHandlerID,waveHandle,pausedInt);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void PlayWaveFile(string path){
		if(started){
			PlayWaveFile(scHandlerIDs[0], path);
		}
	}
	public void PlayWaveFile(uint64 scHandlerID, string path){
		if(started){
			uint error = TeamSpeakInterface.ts3client_playWaveFile(scHandlerID, path);
			if (error != public_errors.ERROR_ok) {
				LogError(error);	
			}
		}
	}

	public void PlayWaveFileHandle(string path, bool loop, out uint64 waveHandle){
		if(started){
			PlayWaveFileHandle(scHandlerIDs[0], path, loop, out waveHandle);
		}else{
			waveHandle = 0;
		}
	}
	public void PlayWaveFileHandle(uint64 scHandlerID, string path, bool loop, out uint64 waveHandle){
		if(started){
			int loopInt = 0;
			if(loop){
				loopInt = 1;
			}
			uint error = TeamSpeakInterface.ts3client_playWaveFileHandle(scHandlerID,path,loopInt,out waveHandle);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}else{
			waveHandle = 0;
		}
	}
	
	public void RegisterCustomDevice(string deviceID, string deviceDisplayName, int capFrequency, int capChannels, int playFrequency, int playChannels){
		if(started){
			uint error = TeamSpeakInterface.ts3client_registerCustomDevice(deviceID, deviceDisplayName, capFrequency, capChannels, playFrequency, playChannels);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void UnregisterCustomDevice(string deviceID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_unregisterCustomDevice(deviceID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void Set3DWaveAttributes(uint64 waveHandle,Vector3 position){
		if(started){
			Set3DWaveAttributes(scHandlerIDs[0], waveHandle, position);
		}
	}
	public void Set3DWaveAttributes(uint64 scHandlerID,uint64 waveHandle,Vector3 position){
		if(started){
			TeamSpeakInterface.TS3_VECTOR position_ = new TeamSpeakInterface.TS3_VECTOR();
			position_.x = position.x;
			position_.y = position.y;
			position_.z = position.z;
			uint error = TeamSpeakInterface.ts3client_set3DWaveAttributes(scHandlerID,waveHandle,out position_);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetClientVolumeModifier(anyID clientID, float value){
		if(started){
			SetClientVolumeModifier(scHandlerIDs[0], clientID, value);
		}
	}
	public void SetClientVolumeModifier(uint64 scHandlerID, anyID clientID, float value){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setClientVolumeModifier(scHandlerID,clientID,value);
			if (error != public_errors.ERROR_ok){
				LogError(error);
			}
		}
	}

	public void SetLocalTestMode(bool enableTestMode){
		if(started){
			SetLocalTestMode(scHandlerIDs[0], enableTestMode);
		}
	}
	public void SetLocalTestMode(uint64 scHandlerID, bool enableTestMode){
		if(started){
			int status = 0;
			if(enableTestMode){
				status = 1;
			}
			uint error = TeamSpeakInterface.ts3client_setLocalTestMode(scHandlerID,status);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetLogVerbosity(TeamSpeakInterface.LogLevel verbosity){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setLogVerbosity(verbosity);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetPlaybackConfigValue(PlaybackConfig ident, string value){
		if(started){
			SetPlaybackConfigValue(scHandlerIDs[0], ident, value);
		}
	}
	public void SetPlaybackConfigValue(uint64 scHandlerID,PlaybackConfig ident, string value){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setPlaybackConfigValue(scHandlerID,ident.ToString(),value);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetPreProcessorConfigValue(PreProcessorConfig ident, string value){
		if(started){
			SetPreProcessorConfigValue(scHandlerIDs[0], ident, value);
		}
	}
	public void SetPreProcessorConfigValue(uint64 scHandlerID,PreProcessorConfig ident, string value){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setPreProcessorConfigValue(scHandlerID,ident.ToString(),value);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void StartVoiceRecording(){
		if(started){
			StartVoiceRecording(scHandlerIDs[0]);
		}
	}
	public void StartVoiceRecording(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_startVoiceRecording(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void StopVoiceRecording(){
		if(started){
			StopVoiceRecording(scHandlerID);
		}
	}
	public void StopVoiceRecording(uint64 scHandlerID){
		if(started){
			uint error = TeamSpeakInterface.ts3client_stopVoiceRecording(scHandlerID);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public int GetChannelEmptySecs(uint64 channelID){
		if(started){
			return GetChannelEmptySecs(scHandlerIDs[0], channelID);
		}else{
			return 0;
		}
	}
	public int GetChannelEmptySecs(uint64 serverConnectionHandlerID, uint64 channelID){
		int result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getChannelEmptySecs(serverConnectionHandlerID,  channelID, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return result;
	}

	public string GetTransferFileName(anyID transferID){
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getTransferFileName(transferID,out strPtr);
			if (error == public_errors.ERROR_ok) {
				string tmp_value = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_value);
				TeamSpeakInterface.ts3client_freeMemory(strPtr);
			}else{
				LogError(error);
			}
		}
		return result;
	}
	
	public string GetTransferFilePath(anyID transferID){
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getTransferFilePath(transferID,out strPtr);
			if (error == public_errors.ERROR_ok) {
				string tmp_value = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_value);
				TeamSpeakInterface.ts3client_freeMemory(strPtr);
			}else{
				LogError(error);
			}
		}
		return result;
	}
	
	public string GetTransferFileRemotePath(anyID transferID){ //The returned memory is dynamically allocated, remember to call ts3client_freeMemory() to release it
		string result = null;
		if(started){
			IntPtr strPtr = IntPtr.Zero;
			uint error = TeamSpeakInterface.ts3client_getTransferFileRemotePath(transferID,out strPtr);
			if (error == public_errors.ERROR_ok) {
				string tmp_value = Marshal.PtrToStringAnsi(strPtr);
				result = string.Copy(tmp_value);
				TeamSpeakInterface.ts3client_freeMemory(strPtr);
			}else{
				LogError(error);
			}
		}
		return result;
	}

	public uint64 GetTransferFileSize(anyID transferID){
		uint64 result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getTransferFileSize(transferID, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return result;
	}

	public uint64 GetTransferFileSizeDone(anyID transferID){
		uint64 result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getTransferFileSizeDone(transferID,out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return result;
	}
	
	public bool IsTransferSender(anyID transferID){ //1 == upload, 0 == download
		bool result = false;
		if(started){
			int intResult;
			uint error = TeamSpeakInterface.ts3client_isTransferSender(transferID, out intResult);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}else{
				result = (intResult == 1);
			}
		}
		return result;
	}
	
	public FileTransferState ts3client_getTransferStatus(anyID transferID){
		FileTransferState result = FileTransferState.FILETRANSFER_INITIALISING;
		if(started){
			int intResult = 0;
			uint error = TeamSpeakInterface.ts3client_getTransferStatus(transferID, out intResult);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}else{
				result = (FileTransferState)intResult;
			}
		}
		return result;
	}
	
	public float GetCurrentTransferSpeed(anyID transferID){ //bytes/second within the last few seconds
		float result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getCurrentTransferSpeed(transferID, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return result;
	}
	
	public float GetAverageTransferSpeed(anyID transferID){ //bytes/second since start of the transfer
		float result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getAverageTransferSpeed(transferID, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return result;
	}
	
	public uint64 GetTransferRunTime(anyID transferID){
		uint64 result = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getTransferRunTime(transferID, out result);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return result;
	}
	
	public void SendFile(uint64 channelID, string channelPW, string file, bool overwrite, bool resume, string sourceDirectory, out anyID transferID){
		if(started){
			SendFile(scHandlerIDs[0], channelID, channelPW, file, overwrite, resume, sourceDirectory, out transferID);
		}else{
			transferID = 0;
		}
	}
	public void SendFile(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string file, bool overwrite, bool resume, string sourceDirectory, out anyID transferID){
		if(started){
			int intOverwrite = 0;
			int intResume = 0;
			if(overwrite){
				intOverwrite = 1;
			}
			if(resume){
				intResume = 1;
			}
			uint error = TeamSpeakInterface.ts3client_sendFile(serverConnectionHandlerID, channelID, channelPW, file, intOverwrite, intResume, sourceDirectory, out transferID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}else{
			transferID = 0;
		}
	}

	public void RequestFile(uint64 channelID, string channelPW, string file, bool overwrite, bool resume, string destinationDirectory, out anyID transferID){
		if(started){
			RequestFile(scHandlerID, channelID, channelPW, file, overwrite, resume, destinationDirectory, out transferID);
		}else{
			transferID = 0;
		}
	}
	public void RequestFile(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string file, bool overwrite, bool resume, string destinationDirectory, out anyID transferID){
		if(started){
			int intOverwrite = 0;
			int intResume = 0;
			if(overwrite){
				intOverwrite = 1;
			}
			if(resume){
				intResume = 1;
			}
			uint error = TeamSpeakInterface.ts3client_requestFile(serverConnectionHandlerID, channelID, channelPW, file,  intOverwrite, intResume, destinationDirectory, out transferID, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}else{
			transferID = 0;
		}
	}

	public void HaltTransfer( anyID transferID, bool deleteUnfinishedFile){
		if(started){
			HaltTransfer(scHandlerID, transferID, deleteUnfinishedFile);
		}
	}
	public void HaltTransfer(uint64 serverConnectionHandlerID, anyID transferID, bool deleteUnfinishedFile){
		if(started){
			int intDeleteUnfinishedFile = 0;
			if(deleteUnfinishedFile){
				intDeleteUnfinishedFile = 1;
			}
			uint error = TeamSpeakInterface.ts3client_haltTransfer( serverConnectionHandlerID, transferID,  intDeleteUnfinishedFile, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void RequestFileList( uint64 channelID, string channelPW, string path){
		if(started){
			RequestFileList(scHandlerID, channelID, channelPW, path);
		}
	}
	public void RequestFileList(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string path){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestFileList(serverConnectionHandlerID, channelID, channelPW, path, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void RequestFileInfo(uint64 channelID, string channelPW, string file){
		if(started){
			RequestFileInfo(scHandlerID, channelID, channelPW, file);
		}
	}
	public void RequestFileInfo(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string file){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestFileInfo(serverConnectionHandlerID,  channelID,  channelPW,  file, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void RequestDeleteFile(uint64 channelID, string channelPW, string[] file){
		if(started){
			RequestDeleteFile(scHandlerID, channelID, channelPW, file);
		}
	}
	public void RequestDeleteFile(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string[] file){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestDeleteFile(serverConnectionHandlerID, channelID, channelPW, file, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void RequestCreateDirectory(uint64 channelID, string channelPW, string directoryPath){
		if(started){
			RequestCreateDirectory(scHandlerIDs[0], channelID, channelPW, directoryPath);
		}
	}
	public void RequestCreateDirectory(uint64 serverConnectionHandlerID, uint64 channelID, string channelPW, string directoryPath){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestCreateDirectory(serverConnectionHandlerID, channelID, channelPW, directoryPath, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void RequestRenameFile(uint64 fromChannelID, string fromChannelPW, uint64 toChannelID, string toChannelPW, string oldFile, string newFile){
		if(started){
			RequestRenameFile(scHandlerIDs[0], fromChannelID, fromChannelPW, toChannelID, toChannelPW, oldFile, newFile);
		}
	}
	public void RequestRenameFile(uint64 serverConnectionHandlerID, uint64 fromChannelID, string fromChannelPW, uint64 toChannelID, string toChannelPW, string oldFile, string newFile){
		if(started){
			uint error = TeamSpeakInterface.ts3client_requestRenameFile(serverConnectionHandlerID, fromChannelID, fromChannelPW, toChannelID, toChannelPW, oldFile, newFile, IntPtr.Zero);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public uint64 GetInstanceSpeedLimitUp(){
		uint64 limit = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getInstanceSpeedLimitUp(out limit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return limit;
	}
	
	public uint64 GetInstanceSpeedLimitDown(){
		uint64 limit = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getInstanceSpeedLimitDown(out limit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return limit;
	}

	public void GetServerConnectionHandlerSpeedLimitUp(){
		if(started){
			GetServerConnectionHandlerSpeedLimitUp(scHandlerIDs[0]);
		}
	}
	public uint64 GetServerConnectionHandlerSpeedLimitUp(uint64 serverConnectionHandlerID){
		uint64 limit = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getServerConnectionHandlerSpeedLimitUp(serverConnectionHandlerID, out limit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return limit;
	}

	public void GetServerConnectionHandlerSpeedLimitDown(){
		if(started){
			GetServerConnectionHandlerSpeedLimitDown(scHandlerIDs[0]);
		}
	}
	public uint64 GetServerConnectionHandlerSpeedLimitDown(uint64 serverConnectionHandlerID){
		uint64 limit = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getServerConnectionHandlerSpeedLimitDown(serverConnectionHandlerID, out limit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return limit;
	}

	public uint64 GetTransferSpeedLimit(anyID transferID){
		uint64 limit = 0;
		if(started){
			uint error = TeamSpeakInterface.ts3client_getTransferSpeedLimit(transferID, out limit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
		return limit;
	}

	public void SetInstanceSpeedLimitUp(uint64 newLimit){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setInstanceSpeedLimitUp(newLimit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetInstanceSpeedLimitDown(uint64 newLimit){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setInstanceSpeedLimitDown(newLimit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetServerConnectionHandlerSpeedLimitUp(uint64 newLimit){
		if(started){
			SetServerConnectionHandlerSpeedLimitUp(scHandlerIDs[0], newLimit);
		}
	}
	public void SetServerConnectionHandlerSpeedLimitUp(uint64 serverConnectionHandlerID, uint64 newLimit){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setServerConnectionHandlerSpeedLimitUp(serverConnectionHandlerID, newLimit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	public void SetServerConnectionHandlerSpeedLimitDown(uint64 newLimit){
		if(started){
			SetServerConnectionHandlerSpeedLimitDown(scHandlerIDs[0], newLimit);
		}
	}
	public void SetServerConnectionHandlerSpeedLimitDown(uint64 serverConnectionHandlerID, uint64 newLimit){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setServerConnectionHandlerSpeedLimitDown(serverConnectionHandlerID,newLimit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}
	
	public void SetTransferSpeedLimit(anyID transferID, uint64 newLimit){
		if(started){
			uint error = TeamSpeakInterface.ts3client_setTransferSpeedLimit(transferID,newLimit);
			if (error != public_errors.ERROR_ok) {
				LogError(error);
			}
		}
	}

	private static void LogError(uint error){
		IntPtr strPtr = IntPtr.Zero;
		if(TeamSpeakInterface.ts3client_getErrorMessage(error,out strPtr) == public_errors.ERROR_ok) {  /* Query printable error */
			string tmp_str = Marshal.PtrToStringAnsi(strPtr);
			string str = string.Copy(tmp_str); 
			TeamSpeakInterface.ts3client_freeMemory(strPtr);
			errorMessages.Add(new TeamSpeakError(error,str));
			if(logErrors){
				Debug.Log("Error: " + str);
			}
		}else{
			if(logErrors){
				Debug.Log("Error: " + error);
			}
		}
	}	

	private TeamSpeakClient(){
		#if UNITY_IOS && !UNITY_EDITOR
		TeamSpeakInterface.teamSpeakRemoteIOInit();
		#endif
	}
}
