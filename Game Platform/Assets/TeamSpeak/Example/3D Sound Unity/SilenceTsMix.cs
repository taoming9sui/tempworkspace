using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;

using anyID = System.UInt16;
using uint64 = System.UInt64;


public class SilenceTsMix : MonoBehaviour {

    private short[] silentArray = Enumerable.Repeat<short>(0, 2048).ToArray();

    void onEditMixedPlaybackVoiceData(uint64 serverConnectionHandlerID, IntPtr samples, int frameCount, int channels, uint[] channel_speaker_array, ref uint channel_fill_mask)
    {
        int sampleCount = frameCount * channels;
        if (silentArray.Length < sampleCount)
            Array.Resize(ref silentArray, sampleCount);

        Marshal.Copy(silentArray, 0, samples, sampleCount);
    }

    // Use this for initialization
    void Start () {
        TeamSpeakCallbacks.onEditMixedPlaybackVoiceDataEvent += onEditMixedPlaybackVoiceData;
    }

    private void OnDestroy()
    {
        TeamSpeakCallbacks.onEditMixedPlaybackVoiceDataEvent -= onEditMixedPlaybackVoiceData;
    }
}
