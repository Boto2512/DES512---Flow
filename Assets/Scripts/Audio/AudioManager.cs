using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;


[Serializable]
public class Sound
{

    public string soundName;
    public AudioClip sound;


    [Tooltip("Lower number = higher priority")]
    [Range(0f, 256)] public int priority = 1;
    [Range(0f, 1)] public float volume = 1;
    [Range(0.1f, 3f)] public float pitch = 1;
    public bool loop = false;
    public bool mute = false;
    public AudioMixerGroup mixer;
    private bool isPlaying;

    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{

    [SerializeField] private Sound[] soundList;
    public static AudioManager instance;
    private AudioSource referenced_audioSource;


    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); }
        else { instance = this; }
        DontDestroyOnLoad(instance);
    }

    public void PlaySound(string soundName, AudioSource audioSource = null)
    {
        instance.referenced_audioSource = audioSource;
        Sound s = Array.Find(soundList, Sound => Sound.soundName == soundName);
        audioSource.clip = s.sound;
        audioSource.priority = s.priority;
        audioSource.volume = s.volume;
        audioSource.pitch = s.pitch;
        audioSource.outputAudioMixerGroup = s.mixer;
        audioSource.loop = s.loop;
        audioSource.mute = s.mute;
        audioSource.Play();
        Debug.Log($"Playing audio {s.soundName}");

        float clipLength = audioSource.clip.length;
    }
}
