using UnityEngine;
using System.Collections.Generic;

public enum BuffType
{
    IncreaseSize,
    SpawnBalls,
    Sticky,
    ShootWhenHit,
    InvincibleBall
}

public class BuffCollectable : MonoBehaviour
{
    private static Dictionary<Collider2D, BuffCollectable> buffCollectableColliders = new();

    [field: SerializeField] public BuffType Buff { get; private set; }
    [SerializeField] private float fallSpeed;
    [field: SerializeField] public Rigidbody2D RB { get; private set; }
    [SerializeField] private new CircleCollider2D collider;
    [SerializeField] private PhysicsEventForward physicsEventForward;

    public static BuffCollectable GetBuffCollectable(Collider2D collider)
    {
        if (!buffCollectableColliders.ContainsKey(collider)) return null;
        return buffCollectableColliders[collider];
    }

    private void Awake()
    {
        buffCollectableColliders.Add(collider, this);
        RB.AddForce(fallSpeed * Vector2.down, ForceMode2D.Impulse);

        physicsEventForward.OnTriggerEnter2DEvent += RegisterKillZoneEntry;
    }

    private void OnDestroy()
    {
        buffCollectableColliders.Remove(collider);
        physicsEventForward.OnTriggerEnter2DEvent -= RegisterKillZoneEntry;
    }

    private void RegisterKillZoneEntry(Collider2D other)
    {
        if (!other.CompareTag("KillZone")) return;
        Kill();
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}
