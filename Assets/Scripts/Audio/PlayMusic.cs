using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField] private string musicName;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Play();
    }

    void Play()
    {
        AudioManager.instance.PlaySound(musicName, audioSource);
    }
}
