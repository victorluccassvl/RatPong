using UnityEngine;

public class GoBackButton : MonoBehaviour
{
    public void GoBack()
    {
        GameManager.Instance.ExplicitOpenEndGamePopUp();
    }
}
