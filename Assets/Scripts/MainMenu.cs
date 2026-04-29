using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LevelsData levelsData;

    [SerializeField] private ScrollRect levelsScrollRect;

    [SerializeField] private LevelSelectButton levelButtonPrefab;
    [SerializeField] private string musicID;

    public void Awake() => UpdateLevelsList();

    public void Start()
    {
        float normalizedVolume = PlayerPrefs.HasKey("NormalizedVolume") ? PlayerPrefs.GetFloat("NormalizedVolume") : 1f;
        AudioManager.Instance.SetVolume(normalizedVolume);
        AudioManager.Instance.PlayMusic(musicID);
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
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

            bool completed = PlayerPrefs.GetString(level.ID, "NotFound") != "NotFound";

            LevelSelectButton newLevelButton = Instantiate(levelButtonPrefab, levelsScrollRect.content);
            newLevelButton.Setup(level, i + 1, completed);
            newLevelButton.gameObject.SetActive(true);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainMenu))]
public class MainMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Delete Player Preferences"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
#endif