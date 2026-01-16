using UnityEngine;
using KBCore.Refs;

public class Tile : MonoBehaviour
{
    public enum Variant
    {
        Common
    }

    [SerializeField, Self] private new SpriteRenderer renderer;
    [SerializeField] private int hitsToBreak;
    [SerializeField] public Variant variant;

    private int hitsReceived;

    private void Awake()
    {
        hitsReceived = 0;
        UpdateVisuals();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Ball ball = Ball.GetBall(collision.collider);
        if (!ball) return;

        GetHit(ball);
    }

    private void GetHit(Ball ball)
    {
        hitsReceived++;
        UpdateHits();
    }

    private void UpdateHits()
    {
        if (hitsReceived >= hitsToBreak)
        {
            GetDestroyed();
        }
        else
        {
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        float percentage = 1f - (hitsReceived / ((float)hitsToBreak - 1));
        renderer.color = new Color(percentage, percentage, percentage);
    }

    private void GetDestroyed()
    {
        Destroy(gameObject);
    }
}
