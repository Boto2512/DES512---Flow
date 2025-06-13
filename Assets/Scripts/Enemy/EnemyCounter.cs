using TMPro;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounterText;
    private float enemyCount;

    private void Update()
    {
        enemyCount = transform.childCount;
        enemyCounterText.text = enemyCount.ToString();
        if (enemyCount == 0)
        {
            //end game
        }
    }
}
