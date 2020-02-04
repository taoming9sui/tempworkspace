using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
 
public static class PostBuildHelper
{
    private static DirectoryInfo targetdir;
    private static string buildname;
    private static string buildDataDir;
    private static DirectoryInfo projectParent;
 
    // Name of folder in project directory containing files for build
    private static int filecount;
    private static int dircount;
 
    /// Processbuild Function
    [PostProcessBuild] // <- this is where the magic happens
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        UnityEngine.Debug.Log("Post Processing Build");
 
        // Get Required Paths
        buildname = Path.GetFileNameWithoutExtension(path);
        targetdir = Directory.GetParent(path);
        char divider = Path.DirectorySeparatorChar;
#if UNITY_STANDALONE_WIN
        	string dataMarker = "_Data"; // Specifically for Windows Standalone build
#elif UNITY_STANDALONE_OSX
			string dataMarker = Path.GetExtension(path) + divider + "Contents" ;
#endif
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
	        buildDataDir = targetdir.FullName + divider + buildname + dataMarker + divider;
			Directory.CreateDirectory(buildDataDir + "TeamSpeak" + divider + "soundbackends");
#if UNITY_STANDALONE_WIN
	        	File.Copy(Application.dataPath + divider + "TeamSpeak" + divider + "soundbackends" + divider + "directsound_win32.dll",buildDataDir + "TeamSpeak" + divider + "soundbackends" + divider + "directsound_win32.dll");
				File.Copy(Application.dataPath + divider + "TeamSpeak" + divider + "soundbackends" + divider + "directsound_win64.dll",buildDataDir + "TeamSpeak" + divider + "soundbackends" + divider + "directsound_win64.dll");
				File.Copy(Application.dataPath + divider + "TeamSpeak" + divider + "soundbackends" + divider + "windowsaudiosession_win32.dll",buildDataDir + "TeamSpeak" + divider + "soundbackends" + divider + "windowsaudiosession_win32.dll");
				File.Copy(Application.dataPath + divider + "TeamSpeak" + divider + "soundbackends" + divider + "windowsaudiosession_win64.dll",buildDataDir + "TeamSpeak" + divider + "soundbackends" + divider + "windowsaudiosession_win64.dll");
#elif UNITY_STANDALONE_OSX
				File.Copy(Application.dataPath + divider + "TeamSpeak" + divider + "soundbackends" + divider + "libcoreaudio_mac.dylib",buildDataDir + "TeamSpeak" + divider + "soundbackends" + divider + "libcoreaudio_mac.dylib");
#endif
#endif

#if UNITY_IOS
			Process proc = new Process();
			proc.EnableRaisingEvents=false; 
			proc.StartInfo.FileName = Application.dataPath + "/TeamSpeak/iOS/PostBuild/PostBuildTeamSpeakScript";
			string arguments = "'" + path + "'";
			if(EditorPrefs.GetBool("TeamSpeakAddBoost",true)){
				arguments += " 1";
			}else{
				arguments += " 0";
			}
			proc.StartInfo.Arguments =  arguments;
		
			UnityEngine.Debug.Log(proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
	
			proc.Start();
			proc.WaitForExit();
			UnityEngine.Debug.Log("TeamSpeak: build log file: " + System.IO.Directory.GetCurrentDirectory() + "/TeamSpeakBuildLogFile.txt");
			//for debugging purposes only
			UnityEngine.Debug.Log (proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
		
#endif
    }
}
