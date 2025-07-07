using UnityEngine;
using System;
using LevelDesign;

public class SpeedrunTester : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;

    private DateTime startTime;

    private const string tag_player = "Player";



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime=DateTime.Now;
    }




    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag(tag_player))
        {
            Debug.LogError($"Arrived at the destination.\nTime:{DateTime.Now.Subtract(startTime).TotalSeconds}\nKilled Enemy:{spawner.SpawnedEnemy-spawner.LeftEnemy}/{spawner.SpawnedEnemy}");
        }
    }
}
