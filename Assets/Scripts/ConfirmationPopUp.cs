using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ConfirmationPopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageRenderer;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action confirmAction = delegate { };
    private Action cancelAction = delegate { };

    public void Confirm()
    {
        confirmAction?.Invoke();
        Close();
    }

    public void Cancel()
    {
        cancelAction?.Invoke();
        Close();
    }

    private GameObject previouslySelectedElement = null;
    public void Open(string message, Action confirmAction, Action cancelAction)
    {
        messageRenderer.text = message;

        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;

        gameObject.SetActive(true);

        previouslySelectedElement = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
    }

    public void Close()
    {
        messageRenderer.text = "";

        confirmAction = delegate { };
        cancelAction = delegate { };

        gameObject.SetActive(false);

        if (previouslySelectedElement) EventSystem.current.SetSelectedGameObject(previouslySelectedElement);
    }
}
