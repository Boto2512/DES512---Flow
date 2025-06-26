using System.Collections.Generic;
using UnityEngine;

namespace LevelDesign {
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<EnemyData> SpawnData = new List<EnemyData>();

        private int SpawningIndex = -1;
        private bool IsThereAnyEnemyExisting => transform.childCount > 0;


        public void Update(){
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Spawn();
            }
        }

        public void Spawn(){
            if (IsThereAnyEnemyExisting){
                Debug.Log("Haven't killed all enemies.");
                return;
            }

            SpawningIndex++;

            if (SpawningIndex>= SpawnData.Count){
                Debug.Log("You win!");
            }
            else{
                SpawnData[SpawningIndex].Spawn(transform);
            }

        }

        [System.Serializable]
        public class EnemyData{
            [SerializeField] private GameObject EnemyObj;
            [SerializeField] private List<Transform> SpawningPoints;

            public void Spawn(Transform parent){
                foreach (Transform t in SpawningPoints) {
                    Instantiate(EnemyObj,t.position,t.rotation, parent);
                }
            }
        }
    }

    

}
