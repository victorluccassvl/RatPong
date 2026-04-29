using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class Audio
    {
        public string ID;
        public IAudioGenerator.Serializable audioAsset;
    }

    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private List<Audio> musics;
    [SerializeField] private List<Audio> sfx;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (this != Instance) return;

        PlayerPrefs.SetFloat("NormalizedVolume", GetVolume());
    }

    public void PlayMusic(string ID)
    {
        Audio music = musics.Find(m => m.ID == ID);
        if (music == null) return;

        musicSource.Stop();
        musicSource.generator = music.audioAsset.definition;
        musicSource.Play();
    }

    public void PlaySFX(string ID)
    {

    }

    public void SetVolume(float normalizedVolume)
    {
        float volumeInDecibels;
        if (Mathf.Approximately(0, normalizedVolume)) volumeInDecibels = -80f;
        else volumeInDecibels = Mathf.Log10(normalizedVolume) * 20f;

        masterMixer.SetFloat("Volume", volumeInDecibels);
    }

    public float GetVolume()
    {
        float volumeInDecibels;
        masterMixer.GetFloat("Volume", out volumeInDecibels);
        return Mathf.Pow(10f, volumeInDecibels / 20f);
    }
}