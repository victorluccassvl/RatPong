using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class DualChoicePopUp : MonoBehaviour
{
    public class Settings
    {
        public string message;
        public Color? messageColor;
        public string leftButtonLabel;
        public string rightButtonLabel;
    }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI messageRenderer;
    [SerializeField] private Button leftButton;
    [SerializeField] private TextMeshProUGUI leftButtonMessageRenderer;
    [SerializeField] private Button rightButton;
    [SerializeField] private TextMeshProUGUI rightButtonMessageRenderer;

    [Header("Settings")]
    [SerializeField] private bool pauseTime = false;

    private Action leftButtonAction = delegate { };
    private Action rightButtonAction = delegate { };

    private bool isOpen = false;
    private Settings defaultSettings = new();

    public void LeftButtonConfirm()
    {
        leftButtonAction?.Invoke();
        Close();
    }

    public void RightButtonConfirm()
    {
        rightButtonAction?.Invoke();
        Close();
    }

    private GameObject previouslySelectedElement = null;
    public void Open(string message, Action leftButtonAction, Action rightButtonAction)
    {
        Settings settings = new();
        settings.message = message;
        Open(settings, leftButtonAction, rightButtonAction);
    }
    public void Open(Settings settings, Action leftButtonAction, Action rightButtonAction)
    {
        if (isOpen) return;

        defaultSettings.message = messageRenderer.text;
        defaultSettings.messageColor = messageRenderer.color;
        defaultSettings.leftButtonLabel = leftButtonMessageRenderer.text;
        defaultSettings.rightButtonLabel = rightButtonMessageRenderer.text;

        if (settings.message != null) messageRenderer.text = settings.message;
        if (settings.messageColor != null) messageRenderer.color = settings.messageColor.Value;
        if (settings.leftButtonLabel != null) leftButtonMessageRenderer.text = settings.leftButtonLabel;
        if (settings.rightButtonLabel != null) rightButtonMessageRenderer.text = settings.rightButtonLabel;

        this.leftButtonAction = leftButtonAction;
        this.rightButtonAction = rightButtonAction;

        gameObject.SetActive(true);

        previouslySelectedElement = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(rightButton.gameObject);

        if (pauseTime) Time.timeScale = 0f;
        isOpen = true;
    }

    public void Close()
    {
        if (!isOpen) return;

        messageRenderer.text = defaultSettings.message;
        messageRenderer.color = defaultSettings.messageColor.Value;
        leftButtonMessageRenderer.text = defaultSettings.leftButtonLabel;
        rightButtonMessageRenderer.text = defaultSettings.rightButtonLabel;

        leftButtonAction = delegate { };
        rightButtonAction = delegate { };

        gameObject.SetActive(false);

        if (previouslySelectedElement) EventSystem.current.SetSelectedGameObject(previouslySelectedElement);
        if (pauseTime) Time.timeScale = 1f;

        isOpen = false;
    }
}
