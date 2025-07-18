using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    [SerializeField] private SectionDatabase sectionDatabase;
    [SerializeField] private GameObject defaultRangedEnemy;
    [SerializeField] private GameObject homingRangedEnemy;
    [SerializeField] private GameObject turretObject;
    [SerializeField] private Transform[] sectionParents;

    private int currentSection = 1;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        SpawnSection(currentSection);
    }

    private void SpawnSection(int sectionIndex)
    {
        if (sectionIndex >= sectionDatabase.sections.Count) return;

        SectionData section = sectionDatabase.sections[sectionIndex];

        foreach (SpawnData enemyData in section.enemySpawnData)
        {
            GameObject enemy = null;

            switch (enemyData.characterType)
            {
                case CharacterType.Default:
                    enemy = Instantiate(defaultRangedEnemy, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[sectionIndex]);
                    break;
                case CharacterType.Ranged:
                    enemy = Instantiate(homingRangedEnemy, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[sectionIndex]);
                    break;
                case CharacterType.Static:
                    enemy = Instantiate(turretObject, enemyData.spawnPoint, enemyData.spawnRotation, sectionParents[sectionIndex]);
                    break;
            }

            if (enemy != null)
            {
                activeEnemies.Add(enemy);

                // Subscribe to OnDestroy to track death
                var deathTracker = enemy.AddComponent<EnemyTracker>();
                deathTracker.OnEnemyDeath += OnEnemyDied;
            }
        }

        // Activate enemies in section
        foreach (Transform child in sectionParents[sectionIndex])
        {
            if (child.TryGetComponent(out TurretEnemy turret))
            {
                turret.enabled = true;
            }
            else if (child.TryGetComponent(out EnemyController enemyController))
            {
                enemyController.enabled = true;
            }
        }
    }

    private void OnEnemyDied(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        if (activeEnemies.Count == 0)
        {
            currentSection++;
            SpawnSection(currentSection);
        }
    }
}
