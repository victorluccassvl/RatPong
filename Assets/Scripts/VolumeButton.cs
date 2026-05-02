using UnityEngine;
using UnityEngine.UI;

public class VolumeButton : MonoBehaviour
{
    [SerializeField] private Scrollbar volumeScroll;

    public void Start()
    {
        volumeScroll.value = AudioManager.Instance.GetVolume();
    }

    public void ToggleVolumeBar()
    {
        AudioManager.Instance.PlayAudio("Click");
        bool isOpen = volumeScroll.gameObject.activeSelf;

        volumeScroll.gameObject.SetActive(!isOpen);
    }

    public void UpdateVolume()
    {
        AudioManager.Instance.SetVolume(volumeScroll.value);
    }
}
