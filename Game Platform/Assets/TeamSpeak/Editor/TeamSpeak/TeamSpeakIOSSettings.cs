using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class TeamSpeakIOSSettings : EditorWindow
{
	private static bool addBoost = true;


	// Add menu item named "My Window" to the Window menu
	[MenuItem("TeamSpeak/iOS Settings")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(TeamSpeakIOSSettings));
		if(!EditorPrefs.HasKey("TeamSpeakAddBoost")){
			EditorPrefs.SetBool("TeamSpeakAddBoost", true);
		}

		addBoost = EditorPrefs.GetBool("TeamSpeakAddBoost");
	}
	
	void OnFocus(){
		
	}
	
	void OnGUI()
	{
		GUILayout.Label ("TeamSpeak iOS Settings", EditorStyles.boldLabel);
		bool tmpAddBoost = (bool)EditorGUILayout.Toggle("Add boost framework to Xcode project", addBoost);
		if(tmpAddBoost != addBoost){
			EditorPrefs.SetBool("TeamSpeakAddBoost", tmpAddBoost);
			addBoost = tmpAddBoost;
		}
	}
}