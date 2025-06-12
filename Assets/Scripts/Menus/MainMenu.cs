using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject levelSelectCanvas;

    private void Start() {
        mainMenuCanvas.SetActive(true);
    }

    #region -------------------------------- Open & Close Canvas --------------------------------
    public void OpenMainMenu() {
        mainMenuCanvas.SetActive(true);
        levelSelectCanvas.SetActive(false);
    }

    #endregion -------------------------------- Open & Close Canvas --------------------------------

    public void PlayLevel(string levelName) {
        SceneManager.LoadScene(levelName); 
    }
}
