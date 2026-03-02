using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    private static Dictionary<Collider2D, Ball> ballColliders = new();

    [field: SerializeField] public Rigidbody2D RB;
    [SerializeField] private new CircleCollider2D collider;
    [SerializeField] private PhysicsEventForward physicsEventForward;

    [field: SerializeField] public float MaxSpeed { get; private set; }
    [field: SerializeField] public float MinSpeed { get; private set; }
    [SerializeField] private float initialSpeed;

    public bool IsInvincible => invincibilityRemainingDuration > 0;

    private bool hasLimitedLifespam = false;
    private float lifespam = -1f;
    private float invincibilityRemainingDuration = 0f;

    public Action<Ball> OnBallDestroyed = delegate { };

    public static Ball GetBall(Collider2D collider)
    {
        if (!ballColliders.ContainsKey(collider)) return null;
        return ballColliders[collider];
    }

    private void Awake()
    {
        ballColliders.Add(collider, this);
        RB.AddForce(initialSpeed * Vector2.up, ForceMode2D.Impulse);

        invincibilityRemainingDuration = 0f;
        physicsEventForward.OnTriggerEnter2DEvent += RegisterKillZoneEntry;
    }

    private void Update()
    {
        if (invincibilityRemainingDuration > 0f) invincibilityRemainingDuration -= Time.deltaTime;
        if (!hasLimitedLifespam) return;

        lifespam -= Time.deltaTime;

        if (lifespam < 0) Kill();
    }

    private void OnDestroy()
    {
        ballColliders.Remove(collider);
        physicsEventForward.OnTriggerEnter2DEvent -= RegisterKillZoneEntry;

        OnBallDestroyed(this);
    }

    public void SetLifespam(float lifespam)
    {
        hasLimitedLifespam = true;
        this.lifespam = lifespam;
    }

    public void SetInvincibilityDuration(float duration)
    {
        invincibilityRemainingDuration = duration;
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