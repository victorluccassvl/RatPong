using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LevelsData levelsData;

    [SerializeField] private ScrollRect levelsScrollRect;

    [SerializeField] private LevelSelectButton levelButtonPrefab;

    public void Awake() => UpdateLevelsList();

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
    Application.Quit();
#endif
    }

    private void UpdateLevelsList()
    {
        if (levelsData == null)
        {
            Debug.LogError("Could not find any level data to load");
            return;
        }

        for (int i = 0; i < levelsData.levels.Count; i++)
        {
            LevelData level = levelsData.levels[i];
            if (!level.enabled) continue;

            bool completed = PlayerPrefs.GetString(level.ID) != string.Empty;

            LevelSelectButton newLevelButton = Instantiate(levelButtonPrefab, levelsScrollRect.content);
            newLevelButton.Setup(level, i + 1, completed);
            newLevelButton.gameObject.SetActive(true);
        }
    }
}
