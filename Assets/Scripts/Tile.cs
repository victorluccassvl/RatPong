using UnityEngine;
using KBCore.Refs;
using System;

public class Tile : MonoBehaviour
{
    public enum Variant
    {
        Common
    }

    [SerializeField, Self] private new SpriteRenderer renderer;
    [SerializeField, Self] private new BoxCollider2D collider;
    [SerializeField] private int hitsToBreak;
    [SerializeField] public Variant variant;

    public Action<Tile> OnTileDestroyed = delegate { };

    public Vector2Int GridPosition { get; private set; } = Vector2Int.zero;
    private TilesSpace currentSpace;
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

    private void OnDestroy()
    {
        OnTileDestroyed(this);
    }

    public void Setup(Vector2Int gridPosition, TilesSpace tileSpace)
    {
        currentSpace = tileSpace;
        GridPosition = gridPosition;
        transform.localPosition = currentSpace.GetLocalPositionForTile(GridPosition);

        renderer.size = tileSpace.CellSize;
        collider.size = tileSpace.CellSize;
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
