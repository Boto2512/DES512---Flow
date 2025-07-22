using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] List<Transform> checkpoints;
    Transform currentCheckpoint;

    private void Start()
    {
        checkpoints = new List<Transform>();
        foreach(Transform child in transform)
        {
            checkpoints.Add(child);
        }
    }

    void MovePlayer()
    {
        Globals.PLAYER.transform.position = currentCheckpoint.position;
    }

    public void UpdateCheckpoint(int index)
    {
        currentCheckpoint = checkpoints[index];
        
    }
}
