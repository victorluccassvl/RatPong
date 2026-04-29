using UnityEngine;

public class GoBackButton : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.Instance.GoToMainMenu();
    }
}
