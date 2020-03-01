using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    public AudioData[] bgmList;
    public AudioData[] soundList;

    private AudioSource m_bgmAudioSource;
    private AudioSource m_soundAudioSource;


    private void Awake()
    {
        m_bgmAudioSource = this.transform.Find("bgm_player").gameObject.GetComponent<AudioSource>();
        m_soundAudioSource = this.transform.Find("sound_player").gameObject.GetComponent<AudioSource>();
    }

    public void PlayBGM(string audioName)
    {
        for (int i = 0; i < bgmList.Length; i++)
        {
            if (audioName == bgmList[i].audioName)
            {
                m_bgmAudioSource.clip = bgmList[i].audioClip;
                m_bgmAudioSource.loop = bgmList[i].loop;
                m_bgmAudioSource.Play();
                break;
            }
         }
    }

    public void StopBGM()
    {
        if (m_bgmAudioSource.isPlaying)
            m_bgmAudioSource.Pause();
    }

    public void PlaySound(string audioName)
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            if (audioName == soundList[i].audioName)
            {
                m_soundAudioSource.PlayOneShot(soundList[i].audioClip);
                break;
            }
        }
    }
}
