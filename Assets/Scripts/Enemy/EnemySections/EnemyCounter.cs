using System.Collections.Generic;
using TMPro;
using UnityEditor.Build;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounterText;

    [Tooltip("The transform of the object for the current section")]
    [SerializeField] private Transform currentSectionParent;
    [Tooltip("The transform of the object for the next section")]
    [SerializeField] private Transform nextSectionParent;
    private List<GameObject> nextEnemies = new List<GameObject>();
    private float enemyCount;

    private bool isComplete;
    private void Start()
    {
        int childCount = nextSectionParent.childCount;
        for (int currentChild = 0; currentChild < childCount; currentChild++) {
            nextEnemies.Add(nextSectionParent.GetChild(currentChild).gameObject);
        }
    }

    private void Update()
    {
        enemyCount = currentSectionParent.childCount;
        enemyCounterText.text = enemyCount.ToString();
        if (enemyCount == 0) {
            isComplete = true;
            OpenDoor();

            foreach (GameObject enemy in nextEnemies)
            {
                if (enemy.GetComponent<TurretEnemy>()) {
                    enemy.GetComponent<TurretEnemy>().enabled = true;
                } 
                else if (enemy.GetComponent<EnemyController>()) {
                    enemy.GetComponent<EnemyController>().enabled = true;
                }
            }
        }
    }

    private void OpenDoor()
    {
        //openDoor
    }
}
