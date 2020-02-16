using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    public AudioData[] bgmList;
    public AudioData[] soundList;

    private AudioSource bgmAudioSource;
    private AudioSource soundAudioSource;


    private void Awake()
    {
        bgmAudioSource = this.transform.Find("bgm_player").gameObject.GetComponent<AudioSource>();
        soundAudioSource = this.transform.Find("sound_player").gameObject.GetComponent<AudioSource>();

    }

    public void PlayBGM(string audioName)
    {
        for (int i = 0; i < bgmList.Length; i++)
        {
            if (audioName == bgmList[i].audioName)
            {
                bgmAudioSource.clip = bgmList[i].audioClip;
                bgmAudioSource.loop = bgmList[i].loop;
                bgmAudioSource.Play();
                break;
            }
         }
    }

    public void StopBGM(string audioName)
    {
        if (bgmAudioSource.isPlaying)
            bgmAudioSource.Pause();
    }

    public void PlaySound(string audioName)
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            if (audioName == soundList[i].audioName)
            {
                soundAudioSource.PlayOneShot(soundList[i].audioClip);
                break;
            }
        }
    }
}
