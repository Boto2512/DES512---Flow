using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;


[Serializable]
public class Sound
{

    public string soundName;
    public AudioClip clip;// changed sound to clip


    [Tooltip("Lower number = higher priority")]
    [Range(0f, 256)] public int priority = 1;
    [Range(0f, 1)] public float volume = 1;
    [Range(0.1f, 3f)] public float pitch = 1;
    public bool loop = false;
    public bool mute = false;
    public AudioMixerGroup mixerGroup; //changed mixer to mixerGroup 
    private bool isPlaying;

    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{

    [SerializeField] private Sound[] soundList;
    public static AudioManager instance;
    private AudioSource referenced_audioSource;

    //objectpool system
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private const int INITIAL_POOL_SIZE = 5;
    private Transform audioPoolContainer;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); }
        else { instance = this; }
        DontDestroyOnLoad(instance);
        InitializePool();
    }
    private void InitializePool()
    {
        //create audio source object pool container(optimize performance)
        audioPoolContainer = new GameObject("AudioPool").transform;
        audioPoolContainer.SetParent(transform);
        //pre-create audio source
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            CreateNewAudioSourceInPool();
        }

    }
    private AudioSource CreateNewAudioSourceInPool()
    {
        GameObject sourceObj = new GameObject($"AudioSource_{audioSourcePool.Count}");
        sourceObj.transform.SetParent(audioPoolContainer);
        AudioSource newSource = sourceObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        audioSourcePool.Enqueue(newSource);
        return newSource;
    }
    private AudioSource GetAvailableAudioSource()
    {
        // try to get AvailableAudioSource from the pool
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        //  create a new one when pool is empty
        Debug.LogWarning("Audio pool exhausted! Creating new audio source.");
        return CreateNewAudioSourceInPool();
    }
    public void ReturnToPool(AudioSource source)
    {
        // reset audio source state
        source.Stop();
        source.clip = null;
        source.transform.SetParent(audioPoolContainer);
        source.gameObject.SetActive(false);

        // Recycling to the pool
        audioSourcePool.Enqueue(source);
    }
    private void ConfigureSource(AudioSource source, Sound sound)
    {
        // basic audio setting
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.loop = sound.loop;
        source.priority = sound.priority;

        // advanced audio setting
        source.outputAudioMixerGroup = sound.mixerGroup;
        source.mute = sound.mute;
    }

    public void Play(string soundName, Vector3? position = null)
    {
        Sound sound = Array.Find(soundList, s => s.soundName == soundName);
        if (sound == null)
        {
            Debug.LogError($"Sound {soundName} not found!");
            return;
        }
        AudioSource source = GetAvailableAudioSource();
        ConfigureSource(source, sound);

        // Sets the location (if provided)
        if (position.HasValue)
        {
            source.transform.position = position.Value;
            source.spatialBlend = 1.0f; // 3D sound
        }
        else
        {
            source.spatialBlend = 0.0f; // 2D sound
        }

        source.gameObject.SetActive(true);
        source.Play();

        // Automatically recycle non-looping audio after playback is complete
        if (!sound.loop)
        {
            StartCoroutine(ReturnToPoolAfterPlay(source, sound.clip.length));
        }
    }
    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnToPool(source);
    }

    public void PlaySound(string soundName, AudioSource audioSource = null)
    {
        instance.referenced_audioSource = audioSource;
        Sound s = Array.Find(soundList, Sound => Sound.soundName == soundName);
        audioSource.clip = s.clip;
        audioSource.priority = s.priority;
        audioSource.volume = s.volume;
        audioSource.pitch = s.pitch;
        audioSource.outputAudioMixerGroup = s.mixerGroup;
        audioSource.loop = s.loop;
        audioSource.mute = s.mute;
        audioSource.Play();
        Debug.Log($"Playing audio {s.soundName}");

        float clipLength = audioSource.clip.length;
    }
}
