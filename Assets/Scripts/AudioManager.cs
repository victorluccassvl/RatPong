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
        public AudioSource source;
    }

    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer masterMixer;

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

    public void PlayAudio(string ID)
    {
        Audio audio = musics.Find(m => m.ID == ID);
        if (audio == null) audio = sfx.Find(s => s.ID == ID);

        if (audio == null) return;

        audio.source.Stop();
        audio.source.generator = audio.audioAsset.definition;
        audio.source.Play();
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