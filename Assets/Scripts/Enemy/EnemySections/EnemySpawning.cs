using UnityEngine;

public class EnemySpawning : MonoBehaviour {
    [SerializeField] private SectionDatabase sectionDatabase;
    [SerializeField] private GameObject rangedObject;
    [SerializeField] private GameObject turretObject;
    [SerializeField] private Transform[] sectionParents;

    int currentSection;

    private void Start() {
       SpawnEnemies();
    }
     
    private void SpawnEnemies() {
        foreach(SectionData section in sectionDatabase.sections) {
            foreach (SpawnData enemyData in section.enemySpawnData) {
                if (enemyData.characterType == CharacterType.Default) {
                    Debug.LogWarning("Attempted to spawn an default enemy. Dont do that");
                }
                else if (enemyData.characterType == CharacterType.Ranged) {
                    Instantiate(rangedObject, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[currentSection]);
                }
                else if (enemyData.characterType == CharacterType.Static) {
                    Instantiate(rangedObject, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[currentSection]);
                }
            }
            currentSection++;
        }
    }
}

