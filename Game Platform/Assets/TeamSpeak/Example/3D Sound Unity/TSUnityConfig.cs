using UnityEngine;

public class TSUnityConfig : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        {
            AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
            Debug.Log("dspBufferSize: " + audioConfig.dspBufferSize + ", SpeakerMode: " + audioConfig.speakerMode.ToString());
        }
    }

    void Awake()
    {
        /*AudioSpeakerMode capability = AudioSettings.driverCapabilities;
        Debug.Log("Capability: " + capability.ToString());*/

        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.dspBufferSize = 480; // valid: 32, 64, 128, 256, 340, 480, 512, 1024, 2048, 4096, 8192
        /*if (capability == AudioSpeakerMode.Mode7point1)
        {
            config.speakerMode = AudioSpeakerMode.Mode7point1;
        }*/
        AudioSettings.Reset(config);
    }
}
