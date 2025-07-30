using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SectionsEvents : MonoBehaviour
{
    [SerializeField] private List<Transform> sections;

    public UnityEvent onSectionsCompleted;

    private bool active;

    public int enemycount {
        get {
            int count = 0;
            foreach (var section in sections) {
                count += section.childCount;
            }

            return count;
        }
    }

    private void Start()
    {
        active = true;
    }

    private void Update()
    {
        if (active) {
            if (enemycount == 0)
            {
                onSectionsCompleted?.Invoke();
                Debug.Log("Clear");
                active = false;
            }
           
        }
        
    }
}
