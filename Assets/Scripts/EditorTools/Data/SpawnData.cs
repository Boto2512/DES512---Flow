using UnityEngine;

[System.Serializable]
public struct SpawnData
{
    public Vector3 spawnPoint;
    public Quaternion spawnRotation;
    
    public CharacterType characterType;

    public GameObject tempGameWorldObject;
}
