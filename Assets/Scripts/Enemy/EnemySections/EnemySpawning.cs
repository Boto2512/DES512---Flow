using UnityEngine;

public class EnemySpawning : MonoBehaviour {
    [SerializeField] private SectionDatabase sectionDatabase;
    [SerializeField] private GameObject rangedObject;
    [SerializeField] private GameObject turretObject;
    [SerializeField] private Transform[] sectionParents;

    int currentSection;

    private void Awake() {
       SpawnEnemies();

        foreach (Transform child in sectionParents[0])
        {
            if (child.GetComponent<TurretEnemy>())
            {
                child.GetComponent<TurretEnemy>().enabled = true;
                Debug.Log("activating turret");
            }
            else if (child.GetComponent<EnemyController>())
            {
                child.GetComponent<EnemyController>().enabled = true;
                Debug.Log("activating enemys");
            }
        }
    }
     
    private void SpawnEnemies() {
        foreach(SectionData section in sectionDatabase.sections) {
            foreach (SpawnData enemyData in section.enemySpawnData) {
                if (enemyData.characterType == CharacterType.Default) {
                    Debug.Log("Attempted to spawn an default enemy. Dont do that");
                }
                else if (enemyData.characterType == CharacterType.Ranged) {
                    Instantiate(rangedObject, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[currentSection]);
                    Debug.Log("Spawning a ranged enemy");
                }
                else if (enemyData.characterType == CharacterType.Static) {
                    Instantiate(turretObject, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[currentSection]);
                    Debug.Log("Spawning a turret enemy");
                }
            }
            currentSection++;
        }
    }
}

