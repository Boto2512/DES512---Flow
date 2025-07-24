using UnityEngine;
using UnityEngine.SceneManagement;

namespace secret {
    public class Teleport : MonoBehaviour
    {
        [SerializeField] string loadScene;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(loadScene);
            }
        }
    }

}

