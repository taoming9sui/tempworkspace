using UnityEngine;
using System.Collections;

public class TeamSpeakAndroidEventHandler : MonoBehaviour {
	
	void HandleEvent(string message){
#if UNITY_ANDROID && !UNITY_EDITOR
		TeamSpeakCallbacks.ParseEvent(message);
#endif
	}
	
	void OnApplicationPause(bool paused) {
#if UNITY_ANDROID && !UNITY_EDITOR
		if(paused){
			TeamSpeakClient.GetInstance().StopAudio();
		}else{
			TeamSpeakClient.GetInstance().StartAudio();
		}
#endif
    }
	
}
