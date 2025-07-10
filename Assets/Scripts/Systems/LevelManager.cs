using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [Header("Spawn")]
    [SerializeField] private bool spawnPlayerWithCustomRotation = false;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Vector3 playerSpawnVelocity;

    // player stuff
    private GameObject player;

    [Header("Level Comlpetion")]
    [SerializeField] private List<string> nextPossibleLevels;

    private void Awake() {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        player = GameObject.FindWithTag("Player");
        player.transform.position = playerSpawn.position;
        if (spawnPlayerWithCustomRotation) {
            Camera.main.gameObject.transform.rotation = playerSpawn.rotation;
        }
        player.GetComponent<IMomentumModifiable>().SetMomentum(playerSpawnVelocity);
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnDrawGizmos() {
        // spawn position and starting velocity
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerSpawn.position, playerSpawn.position + playerSpawnVelocity);
        Gizmos.DrawSphere(playerSpawn.position, 0.5f);
    }

    private void EnterLevel() {

    }

    private void ExitLevel() {
        string newLevel = SelectRandomLevel();
        Globals.LevelSceneManager.ChangeLevel(newLevel);

    }

    private string SelectRandomLevel() {
        int index = Random.Range(0, nextPossibleLevels.Count);
        return nextPossibleLevels[index];
    }
}
