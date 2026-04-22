using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    private static Dictionary<Collider2D, Ball> ballColliders = new();

    [field: SerializeField] public Rigidbody2D RB;
    [SerializeField] private new CircleCollider2D collider;
    [SerializeField] private CircleCollider2D trigger;
    [SerializeField] private new SpriteRenderer renderer;
    [SerializeField] private SpriteRenderer stickRenderer;

    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite invincibleSprite;

    [field: SerializeField] public float MaxSpeed { get; private set; }
    [field: SerializeField] public float MinSpeed { get; private set; }
    [SerializeField] private float initialSpeed;

    public Vector3 GetPosition => RB.position;
    public bool IsInvincible => invincibilityRemainingDuration > 0f;

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
        ballColliders.Add(trigger, this);

        invincibilityRemainingDuration = 0f;
        UpdateInvincibilityStatus();
    }

    private void Update()
    {
        invincibilityRemainingDuration = Mathf.Max(0f, invincibilityRemainingDuration - Time.deltaTime);
        UpdateInvincibilityStatus();
    }

    private void OnDestroy()
    {
        ballColliders.Remove(collider);
        ballColliders.Remove(trigger);

        OnBallDestroyed(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("KillZone")) return;
        Kill();
    }

    public void Launch(Vector3 direction)
    {
        RB.simulated = true;
        RB.AddForce(initialSpeed * direction.normalized, ForceMode2D.Impulse);
        SetStickyState(false);
    }

    public void AddInvincibilityDuration(float duration)
    {
        invincibilityRemainingDuration += duration;
    }

    public void SetStickyState(bool sticky)
    {
        stickRenderer.enabled = sticky;
    }

    private void UpdateInvincibilityStatus()
    {
        Sprite desiredSprite = IsInvincible ? invincibleSprite : defaultSprite;
        if (renderer.sprite != desiredSprite) renderer.sprite = desiredSprite;

        LayerMask desiredExcludeLayers = IsInvincible ? LayerMask.GetMask("Tile") : LayerMask.GetMask();
        if (collider.excludeLayers != desiredExcludeLayers) collider.excludeLayers = desiredExcludeLayers;
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}