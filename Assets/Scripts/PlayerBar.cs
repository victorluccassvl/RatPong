using UnityEngine;
using UnityEngine.InputSystem;
using KBCore.Refs;
using System.Collections.Generic;
using System.Text;

public class PlayerBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Self] private Rigidbody2D RB;
    [SerializeField] private Transform barTransform;
    [SerializeField] private Transform leftGunPivot;
    [SerializeField] private Transform rightGunPivot;
    [SerializeField] private ParticleSystem leftGun;
    [SerializeField] private ParticleSystem rightGun;
    [SerializeField] private SpriteRenderer stickyRenderer;

    [Header("General Settings")]
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float minDeflectionAngleInDegrees;
    [SerializeField] private int gizmoDeflectionResolution;
    [SerializeField] private float defaultBarScale;

    private float BarCurrentScale => defaultBarScale * ((increaseSizeRemainingDuration > 0) ? GameManager.Instance.IncreaseSizeMultiplier : 1f);
    private bool IsSticky => stickyRemainingDuration > 0f;

    private InputAction moveAction;
    private InputAction launchAction;
    private float currentMoveSpeedX;
    private float minDeflectionAngleCos;
    private float increaseSizeRemainingDuration;
    private float stickyRemainingDuration;
    private float shootingWhenHitRemainingDuration;
    private bool forceLaunchBalls = false;
    private Dictionary<Ball, Vector3> capturedBalls = new();

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        launchAction = InputSystem.actions.FindAction("Launch");
        currentMoveSpeedX = 0f;
        increaseSizeRemainingDuration = 0f;
        stickyRemainingDuration = 0f;
        shootingWhenHitRemainingDuration = 0f;
        minDeflectionAngleCos = Mathf.Cos(minDeflectionAngleInDegrees * Mathf.Deg2Rad);
        UpdateSize();
    }

    private void Update()
    {
        stickyRemainingDuration = Mathf.Max(0f, stickyRemainingDuration - Time.deltaTime);
        increaseSizeRemainingDuration = Mathf.Max(0f, increaseSizeRemainingDuration - Time.deltaTime);
        shootingWhenHitRemainingDuration = Mathf.Max(0f, shootingWhenHitRemainingDuration - Time.deltaTime);
        UpdateGunStatus();
        UpdateStickyStatus();
    }

    private void FixedUpdate()
    {
        HandleLaunch();
        UpdateSize();
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        BuffCollectable buffCollectable = BuffCollectable.GetBuffCollectable(collider);
        if (!buffCollectable) return;

        GameManager.Instance.CollectBuff(buffCollectable);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Ball ball = Ball.GetBall(collision.collider);
        if (!ball) return;

        if (ball.RB.position.y <= transform.position.y) return;

        if (!IsSticky) DeflectBall(ball);
        else CaptureBall(ball);

        if (shootingWhenHitRemainingDuration > 0f)
        {
            AudioManager.Instance.PlayAudio("Shoot");
            leftGun.Play();
            rightGun.Play();
        }
    }

    public void OnDrawGizmos()
    {
        UpdateSize();
        minDeflectionAngleCos = Mathf.Cos(minDeflectionAngleInDegrees * Mathf.Deg2Rad);

        gizmoDeflectionResolution = Mathf.Max(1, gizmoDeflectionResolution);
        int arrows = gizmoDeflectionResolution + 2;

        float deflectedAngleCos;
        Vector2 reflectDirection;
        float globalBarPositionX;

        for (int arrow = 0; arrow < arrows; arrow++)
        {
            deflectedAngleCos = 2f * (arrow / (float)(arrows - 1)) - 1f;
            reflectDirection = GetDeflectionDirection(deflectedAngleCos);
            reflectDirection.Normalize();
            globalBarPositionX = RB.position.x + (BarCurrentScale / 2f) * deflectedAngleCos;
            Gizmos.DrawLine(new Vector3(globalBarPositionX, RB.position.y, 0f), new Vector3(globalBarPositionX + reflectDirection.x, RB.position.y + reflectDirection.y, 0f));
        }
    }

    public void AddIncreasedSizeDuration(float duration)
    {
        increaseSizeRemainingDuration += duration;
    }

    public void AddStickyDuration(float duration)
    {
        stickyRemainingDuration += duration;
    }

    public void AddShotWhenHitDuration(float duration)
    {
        shootingWhenHitRemainingDuration += duration;
    }

    private void UpdateSize()
    {
        if (Mathf.Approximately(barTransform.localScale.x, BarCurrentScale)) return;

        barTransform.localScale = new Vector3(BarCurrentScale, barTransform.localScale.y, 1f);
        leftGun.transform.position = leftGunPivot.position;
        rightGun.transform.position = rightGunPivot.position;
        forceLaunchBalls = true;
    }

    private void UpdateGunStatus()
    {
        bool isGunActive = leftGun.gameObject.activeSelf && rightGun.gameObject.activeSelf;
        bool shouldGunBeActive = shootingWhenHitRemainingDuration > 0;

        if (isGunActive != shouldGunBeActive)
        {
            leftGun.gameObject.SetActive(shouldGunBeActive);
            rightGun.gameObject.SetActive(shouldGunBeActive);
        }
    }

    private void UpdateStickyStatus()
    {
        if (IsSticky == stickyRenderer.enabled) return;

        stickyRenderer.enabled = IsSticky;
        forceLaunchBalls = true;
    }

    private void HandleLaunch()
    {
        if (!launchAction.IsPressed() && !forceLaunchBalls) return;

        foreach (Ball ball in capturedBalls.Keys)
        {
            GameManager.Instance.LaunchBall(ball, capturedBalls[ball]);
        }

        capturedBalls.Clear();
        forceLaunchBalls = false;
    }

    private void Move()
    {
        float moveValue = moveAction.ReadValue<float>();

        currentMoveSpeedX = moveValue * Mathf.MoveTowards(Mathf.Abs(currentMoveSpeedX), maxMoveSpeed, acceleration);
        float targetXPosition = RB.position.x + currentMoveSpeedX;
        RB.MovePosition(new Vector2(targetXPosition, RB.position.y));
    }

    public void CaptureBall(Ball ball)
    {
        Vector2 contactPoint = ball.RB.position - ball.RB.linearVelocity * Time.fixedDeltaTime;
        float contactNormalizedPosition = GetLocalBarPositionForCollision(ball.RB.position.x);
        float deflectionAngleCos = contactNormalizedPosition;

        ball.RB.linearVelocity = Vector3.zero;
        ball.RB.simulated = false;
        ball.transform.SetParent(transform, true);
        ball.transform.position = contactPoint;
        ball.SetStickyState(IsSticky);

        capturedBalls.Add(ball, GetDeflectionDirection(deflectionAngleCos));
    }

    private void DeflectBall(Ball ball)
    {
        float speed = ball.RB.linearVelocity.magnitude;
        float contactNormalizedPosition = GetLocalBarPositionForCollision(ball.RB.position.x);
        float deflectionAngleCos = contactNormalizedPosition;

        ball.RB.linearVelocity = Mathf.Min(speed, ball.MaxSpeed) * GetDeflectionDirection(deflectionAngleCos);
    }

    private float GetLocalBarPositionForCollision(float collisionGlobalPositionX)
    {
        float playerPositionX = RB.position.x;

        float contactPosition = Mathf.Clamp(collisionGlobalPositionX, playerPositionX - BarCurrentScale / 2f, playerPositionX + BarCurrentScale / 2f);
        float contactNormalizedPosition = Mathf.InverseLerp(playerPositionX - BarCurrentScale / 2f, playerPositionX + BarCurrentScale / 2f, contactPosition);
        return contactNormalizedPosition * 2f - 1;
    }

    private Vector2 GetDeflectionDirection(float deflectionAngleCos)
    {
        float deflectedAngleCos = deflectionAngleCos * minDeflectionAngleCos;
        return new Vector2(deflectedAngleCos, 1 - deflectedAngleCos * deflectedAngleCos).normalized;
    }
}
