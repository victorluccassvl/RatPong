using KBCore.Refs;
using UnityEngine;

public class DynamicAspectRatio : MonoBehaviour
{
    [SerializeField] private Vector2 targetAspectRatio;
    [SerializeField, Self] private new Camera camera;

    public void Start()
    {
        UpdateCameraRect();
    }

    public void UpdateCameraRect()
    {
        float currentScreenAspect = Screen.width / (float)Screen.height;
        float scale = currentScreenAspect / (targetAspectRatio.x / targetAspectRatio.y);

        if (scale < 1f) camera.rect = new Rect(0, 1f, 1f, scale);
        else camera.rect = new Rect((1f - scale) / 2f, 0, scale, 1f);
    }
}
