using UnityEngine;
using KBCore.Refs;
using System;

public class Tile : MonoBehaviour
{
    [Flags]
    public enum DamageEffect
    {
        DamageLine = 1 << 0,
        DamageColumn = 1 << 1,
        DamageDiagonals = 1 << 2,
    }

    public enum InvulnerabilityType
    {
        Top,
        Bottom,
        Both,
        None
    }

    public enum VariantType
    {
        Common,
        CompletelyInvulnerable,
        TopInvulnerable,
        BottomInvulnerable,
    }

    [SerializeField, Self] private new SpriteRenderer renderer;
    [SerializeField, Self] private new BoxCollider2D collider;
    [SerializeField] private int hitsToBreak;
    [SerializeField] private InvulnerabilityType invulnerability;
    [SerializeField] private DamageEffect damageEffect;
    [SerializeField] private GameObject buffToDropPrefab;

    public Action<Tile, DamageEffect> OnTileDestroyed = delegate { };

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
        OnTileDestroyed(this, damageEffect);
    }

    public void Setup(Vector2Int gridPosition, TilesSpace tileSpace)
    {
        currentSpace = tileSpace;
        GridPosition = gridPosition;
        transform.localPosition = currentSpace.GetLocalPositionForTile(GridPosition);

        renderer.size = tileSpace.CellSize;
        collider.size = tileSpace.CellSize;
    }

    public void Hit(Vector2Int explodedTilePosition)
    {
        GetHit(explodedTilePosition);
    }

    private void GetHit(Vector2Int explodedTile)
    {
        bool blocked = false;
        switch (invulnerability)
        {
            case InvulnerabilityType.Top:
                blocked = GridPosition.y < explodedTile.y;
                break;
            case InvulnerabilityType.Bottom:
                blocked = GridPosition.y > explodedTile.y;
                break;
            case InvulnerabilityType.Both:
                blocked = true;
                return;
        }

        if (blocked) return;

        hitsReceived++;
        UpdateHits();
    }

    private void GetHit(Ball ball)
    {
        bool blocked = false;
        switch (invulnerability)
        {
            case InvulnerabilityType.Top:
                blocked = !ball.IsInvincible && ball.transform.position.y < transform.position.y;
                break;
            case InvulnerabilityType.Bottom:
                blocked = !ball.IsInvincible && ball.transform.position.y > transform.position.y;
                break;
            case InvulnerabilityType.Both:
                blocked = true;
                return;
        }

        if (blocked) return;

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
        float percentage = 1f - (hitsReceived / ((float)hitsToBreak));
        renderer.color = new Color(percentage, percentage, percentage);
    }

    private void GetDestroyed()
    {
        Destroy(gameObject);

        if (buffToDropPrefab == null) return;

        Instantiate(buffToDropPrefab, currentSpace.transform);
    }
}
