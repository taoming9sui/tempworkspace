using UnityEngine;
using DisruptorUnity3d;
using System;
using System.Runtime.InteropServices;

using anyID = System.UInt16;
using uint64 = System.UInt64;

public class TSObject : MainThreadInvoker
{

    private static TeamSpeakClient ts_client;

    private uint64 _connectionID;
    public uint64 connectionID { get { return _connectionID; } }

    private anyID _id;
    public anyID ID { get { return _id; } }

    private RingBuffer<short> ringBuffer;
    private const int kSampleRate = 48000;

    AudioSource audioSource;
    AudioReverbFilter audioReverbFilter;

    private TalkStatus _talkStatus = TalkStatus.STATUS_NOT_TALKING;
    public TalkStatus talkStatus
    {
        get { return _talkStatus; }
        set
        {
            if (talkStatus == value)
                return;

            _talkStatus = value;

            //Assign the changed color to the material.
            Invoke(() =>
            {
                Renderer renderer = gameObject.GetComponent<Renderer>();
                Color color = renderer.material.color;
                if (talkStatus == TalkStatus.STATUS_TALKING)
                {
                    color.r = 0;
                    color.g = 0;
                    color.b = 255;
                }
                else if (talkStatus == TalkStatus.STATUS_NOT_TALKING)
                {
                    color.r = 125;
                    color.g = 125;
                    color.b = 125;
                }
                renderer.material.color = color;
            });
        }
    }

    void AddReverbFilter()
    {
        audioReverbFilter = gameObject.AddComponent<AudioReverbFilter>();
        audioReverbFilter.reverbPreset = AudioReverbPreset.Forest;
    }

    void OnTalkStatusChanged(uint64 serverConnectionHandlerID, int status, int isReceivedWhisper, anyID clientID)
    {
        if (clientID == _id)
            talkStatus = (TalkStatus)status;
    }

    void OnEditPlaybackVoiceData(uint64 serverConnectionHandlerID, anyID clientID, IntPtr samples, int frameCount, int channels)
    {
        if (clientID != _id)
            return;

        int sampleCount = frameCount * channels;
        short[] samples_in_array = new short[sampleCount];
        Marshal.Copy(samples, samples_in_array, 0, sampleCount);

        int enqueuedCount = 0;
        foreach (short sample in samples_in_array)
        {
            if (ringBuffer.TryEnqueue(sample))
                enqueuedCount++;
            else
                Debug.Log("Couldn't enqueue sample with value " + sample);
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (talkStatus != TalkStatus.STATUS_TALKING)
            Array.Clear(data, 0, data.Length);
        else
        {
            int frameCount = data.Length / channels;
            for (int i = 0; i < frameCount; ++i)
            {
                short tmp;
                if (ringBuffer.TryDequeue(out tmp))
                {
                    float tmp_f = (float)tmp / (tmp < 0 ? System.Int16.MinValue * -1 : System.Int16.MaxValue);
                    tmp_f = Mathf.Clamp(tmp_f, -1f, 1f);
                    for (int channel = 0; channel < channels; ++channel)
                    {
                        data[i * channels + channel] *= tmp_f; // ToDo: while the channels are monaural, we don't receive the 1.0f as expected. Envelope for avoiding clicks? rollOff still pre (would be ok)?
                    }
                }
                else
                {
                    for (int channel = 0; channel < channels; ++channel)
                        data[i * channels + channel] = 0.0f;
                }
            }
        }
    }

    // Use this for creation
    public static TSObject Create(GameObject targetGameObject, uint64 connectionID, anyID id)
    {
        TSObject tsObject = targetGameObject.AddComponent<TSObject>();
        tsObject._connectionID = connectionID;
        tsObject._id = id;
        return tsObject;
    }

    // Use this for initialization
    void Awake()
    {
        ringBuffer = new RingBuffer<short>(kSampleRate);
        ts_client = TeamSpeakClient.GetInstance();
    }

    void Start()
    {
        Debug.Log("Started " + _id, this);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatializePostEffects = true;

        var dummy = AudioClip.Create("dummy", 1, 1, 48000, false);
        dummy.SetData(new float[] { 1 }, 0);
        audioSource.clip = dummy;

        audioSource.loop = true;
        audioSource.spatialize = true;
        audioSource.spatializePostEffects = true;

        audioSource.spatialBlend = 1;
        audioSource.Play();

        //AddReverbFilter();

        talkStatus = (TalkStatus)ts_client.GetClientVariableAsInt(_id, ClientProperties.CLIENT_FLAG_TALKING);

        TeamSpeakCallbacks.onTalkStatusChangeEvent += OnTalkStatusChanged;
        TeamSpeakCallbacks.onEditPlaybackVoiceDataEvent += OnEditPlaybackVoiceData;
    }

    private void OnDestroy()
    {
        TeamSpeakCallbacks.onTalkStatusChangeEvent -= OnTalkStatusChanged;
        TeamSpeakCallbacks.onEditPlaybackVoiceDataEvent -= OnEditPlaybackVoiceData;
    }
}
