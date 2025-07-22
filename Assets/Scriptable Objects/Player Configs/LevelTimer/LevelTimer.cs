using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer instance;

    List<float> levelTimes;
    float currentTimer;
    bool isTiming;
    bool once;
    float levelIndex;

    Scene currentScne;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); }
        else { instance = this; }
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (isTiming) { 
            currentTimer += Time.deltaTime; 
        }
        else if (!once) {
            levelTimes.Add(currentTimer);
            once = true;
            currentTimer = 0;
        }
        else { }
    }

    public void SaveTime()
    {
        isTiming = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isTiming = true;
        once = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
