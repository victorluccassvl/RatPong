using System.Collections.Generic;
using UnityEngine;
using KBCore.Refs;

public class Ball : MonoBehaviour
{
    private static Dictionary<Collider2D, Ball> ballColliders = new();

    [field: SerializeField, Child] public Rigidbody2D RB;
    [SerializeField] new CircleCollider2D collider;
    [SerializeField, Child] private PhysicsEventForward physicsEventForward;

    [field: SerializeField] public float MaxSpeed;
    [field: SerializeField] private float InitialSpeed;

    public static Ball GetBall(Collider2D collider)
    {
        if (!ballColliders.ContainsKey(collider)) return null;
        return ballColliders[collider];
    }

    private void Awake()
    {
        ballColliders.Add(collider, this);
        RB.AddForce(InitialSpeed * new Vector2(Random.value * 2f - 1f, Random.value * 2f - 1f).normalized, ForceMode2D.Impulse);

        physicsEventForward.OnTriggerEnter2DEvent += RegisterKillZoneEntry;
    }

    private void OnDestroy()
    {
        ballColliders.Remove(collider);
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