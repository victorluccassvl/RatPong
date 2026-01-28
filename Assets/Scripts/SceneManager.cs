using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    public LevelData CurrentLevel { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToMainMenu()
    {
        CurrentLevel = null;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void LoadLevel(LevelData levelToLoad)
    {
        if (levelToLoad == null)
        {
            Debug.LogError("Cannot load null level");
            return;
        }

        CurrentLevel = levelToLoad;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}