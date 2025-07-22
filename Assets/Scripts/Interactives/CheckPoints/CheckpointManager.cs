using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] List<Transform> checkpoints;
    Transform currentCheckpoint;
    Transform player;

    private void Start()
    {
        checkpoints = new List<Transform>();
        player = Globals.PLAYER.transform;

        foreach (Transform child in transform)
        {
            checkpoints.Add(child);
            child.GetComponent<Checkpoint>().GetChildIndex(checkpoints.Count - 1);
        }
        currentCheckpoint = checkpoints[0];
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.K)) { MovePlayer(); }
    }

    void MovePlayer()
    {
        player.GetComponent<IMomentumModifiable>().SetMomentum(Vector3.zero);
        Vector3 spawnpostition = currentCheckpoint.position + (Vector3.up * 2);
        player.position = currentCheckpoint.position;
    }

    public void UpdateCheckpoint(int index)
    {
        currentCheckpoint = checkpoints[index];
        
    }
}
