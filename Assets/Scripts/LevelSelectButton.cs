using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private Image completionCrown;
    [SerializeField] private Image completionBorder;

    private LevelData levelData;

    public void Setup(LevelData levelData, int index, bool completed)
    {
        this.levelData = levelData;
        levelLabel.text = $"{index}";

        completionCrown.gameObject.SetActive(completed);
        completionBorder.gameObject.SetActive(completed);
    }

    public void LoadLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("Level Button was not setup.");
            return;
        }
        SceneManager.Instance.LoadLevel(levelData);
    }
}
