using TMPro;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounterText;
    [SerializeField] private GameObject gameOver;
    private float enemyCount;

    private void Update()
    {
        enemyCount = transform.childCount;
        enemyCounterText.text = enemyCount.ToString();
        if (enemyCount == 0)
        {
            gameOver.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
