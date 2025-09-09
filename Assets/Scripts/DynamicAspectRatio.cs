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

    public void OnDrawGizmos()
    {
        Vector3 p1, p2, p3, p4;
        float z = camera.transform.position.z;
        p1 = camera.ViewportToWorldPoint(new Vector3(0, 0, z));
        p2 = camera.ViewportToWorldPoint(new Vector3(0, 1, z));
        p3 = camera.ViewportToWorldPoint(new Vector3(1, 1, z));
        p4 = camera.ViewportToWorldPoint(new Vector3(1, 0, z));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }

    public void UpdateCameraRect()
    {
        float currentScreenAspect = Screen.width / (float)Screen.height;
        float scale = currentScreenAspect / (targetAspectRatio.x / targetAspectRatio.y);

        if (scale < 1f) camera.rect = new Rect(0, 1f, 1f, scale);
        else camera.rect = new Rect((1f - scale) / 2f, 0, scale, 1f);
    }
}
