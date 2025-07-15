using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject optionsCanvas;

    [SerializeField] private GameObject firstSelectedMain;
    [SerializeField] private GameObject firstSelectedSetting;

    private void Start() {
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstSelectedMain);
    }

    #region -------------------------------- Open & Close Canvas --------------------------------
    public void OpenMainMenu() {
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(firstSelectedMain);
    }

    public void OpenSettings()
    {
        optionsCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstSelectedSetting);
    }   
    #endregion -------------------------------- Open & Close Canvas --------------------------------

    public void PlayLevel(string levelName) {
        SceneManager.LoadScene(levelName); 
        TutorialManager.Instance.ResetTutorial();
    }
}
