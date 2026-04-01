using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;

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
        if (goToMainMenuCoroutine != null) StopCoroutine(goToMainMenuCoroutine);
        goToMainMenuCoroutine = StartCoroutine(GoToMainMenuRoutine(0.5f));
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

    private Coroutine goToMainMenuCoroutine = null;
    private IEnumerator GoToMainMenuRoutine(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        CurrentLevel = null;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);

        goToMainMenuCoroutine = null;
    }
}