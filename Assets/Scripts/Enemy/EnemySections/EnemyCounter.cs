using System.Collections.Generic;
using TMPro;
//using UnityEditor.Build;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounterText;

    [Tooltip("The transform of the object for the current section")]
    [SerializeField] private Transform currentSectionParent;
    [Tooltip("The transform of the object for the next section")]
    [SerializeField] private Transform nextSectionParent;
    [SerializeField] private float xRayThreshold;

    [SerializeField] private Vector3 doorEndPos;
    [SerializeField] private Transform door;
    private List<GameObject> nextEnemies = new List<GameObject>();
    private float enemyCount;
    private float totalCount;
    private ToggleRenderScript toggleXray;

    private bool isComplete;
    private void Start()
    {
        toggleXray = GetComponent<ToggleRenderScript>();

        int childCount = nextSectionParent.childCount;
        for (int currentChild = 0; currentChild < childCount; currentChild++) {
            nextEnemies.Add(nextSectionParent.GetChild(currentChild).gameObject);
        }

        enemyCount = currentSectionParent.childCount;
        totalCount = enemyCount;
    }

    private void Update()
    {
        enemyCount = currentSectionParent.childCount;
        enemyCounterText.text = enemyCount.ToString();
        if (enemyCount == 0)
        {
            isComplete = true;
            OpenDoor();

            foreach (GameObject enemy in nextEnemies)
            {
                if (enemy.GetComponent<TurretEnemy>())
                {
                    enemy.GetComponent<TurretEnemy>().enabled = true;
                }
                else if (enemy.GetComponent<EnemyController>())
                {
                    enemy.GetComponent<EnemyController>().enabled = true;
                }
            }
        }
        else if (enemyCount == totalCount) { toggleXray.ToggleXRay(false); }
        else if ((enemyCount/totalCount) * 100 <= xRayThreshold) 
        {
            toggleXray.ToggleXRay(true);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        toggleXray.ToggleXRay(true);
    }
    public void OnTriggerExit(Collider other)
    {
        toggleXray.ToggleXRay(false);

    }

    private void OpenDoor()
    {   
        door.position = Vector3.MoveTowards(door.position,doorEndPos, Time.deltaTime);
        toggleXray.ToggleXRay(false);
    }

}
