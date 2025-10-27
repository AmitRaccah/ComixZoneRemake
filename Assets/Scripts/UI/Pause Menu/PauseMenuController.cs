using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitButton;

    private bool _isPaused;
    private CursorLockMode _previousCursorLockMode;
    private bool _previousCursorVisible;

    void Awake()
    {
        menuRoot.SetActive(false);

        resumeButton.onClick.AddListener(HandleResumeClicked);
        mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        exitButton.onClick.AddListener(HandleExitClicked);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                ShowMenu();
            }
        }
    }

    void OnDestroy()
    {
        resumeButton.onClick.RemoveListener(HandleResumeClicked);
        mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        exitButton.onClick.RemoveListener(HandleExitClicked);

        if (_isPaused)
        {
            Time.timeScale = 1f;
            RestoreCursorState();
        }
    }

    void ShowMenu()
    {
        _previousCursorLockMode = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;

        Time.timeScale = 0f;
        menuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);

        _isPaused = true;
    }

    void ResumeGame()
    {
        menuRoot.SetActive(false);
        Time.timeScale = 1f;
        RestoreCursorState();
        _isPaused = false;
    }

    void HandleResumeClicked()
    {
        ResumeGame();
    }

    void HandleMainMenuClicked()
    {
        Time.timeScale = 1f;
        RestoreCursorState();
        SceneManager.LoadScene("OpeningScreen_2");
    }

    void HandleExitClicked()
    {
        Time.timeScale = 1f;
        RestoreCursorState();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void RestoreCursorState()
    {
        Cursor.lockState = _previousCursorLockMode;
        Cursor.visible = _previousCursorVisible;
    }
}
