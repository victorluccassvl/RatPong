using UnityEngine;
using UnityEngine.InputSystem;
using KBCore.Refs;

public class PlayerBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Self] private Rigidbody2D RB;
    [SerializeField] private Transform barTransform;
    [SerializeField] private Transform leftGunPivot;
    [SerializeField] private Transform rightGunPivot;
    [SerializeField] private ParticleSystem leftGun;
    [SerializeField] private ParticleSystem rightGun;

    [Header("General Settings")]
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float minDeflectionAngleInDegrees;
    [SerializeField] private int gizmoDeflectionResolution;
    [SerializeField] private float defaultBarScale;

    private float BarCurrentScale => defaultBarScale * ((increaseSizeRemainingDuration > 0) ? GameManager.Instance.IncreaseSizeMultiplier : 1f);

    private InputAction moveAction;
    private InputAction extraAction;
    private float currentMoveSpeedX;
    private float minDeflectionAngleCos;
    private float increaseSizeRemainingDuration;
    private float stickyRemainingDuration;
    private float shootingWhenHitRemainingDuration;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        extraAction = InputSystem.actions.FindAction("Extra");
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
    }

    private void FixedUpdate()
    {
        UpdateSize();
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        BuffCollectable buffCollectable = BuffCollectable.GetBuffCollectable(collider);
        if (!buffCollectable) return;

        GameManager.Instance.CollectBuff(buffCollectable);
        /*
            case BuffTypes.ShootWhenHit:
                if (CheckBuff(BuffTypes.ShootWhenHit))
                {
                    Buff updatedBuff = currentBuffs[BuffTypes.ShootWhenHit];
                    updatedBuff.duration = buff.duration + (updatedBuff.duration - (Time.time - updatedBuff.applicationTime));
                    currentBuffs[BuffTypes.ShootWhenHit] = updatedBuff;
                }
                else
                {
                    buff.applicationTime = Time.time;
                    currentBuffs.Add(BuffTypes.ShootWhenHit, buff);
                }
                break;

            case BuffTypes.InvincibleBall:
                break;
        }

        Destroy(buffCollectable.gameObject);
        */
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Ball ball = Ball.GetBall(collision.collider);
        if (!ball) return;

        DeflectBall(ball);

        if (shootingWhenHitRemainingDuration > 0f)
        {
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
        barTransform.localScale = new Vector3(BarCurrentScale, barTransform.localScale.y, 1f);
        leftGun.transform.position = leftGunPivot.position;
        rightGun.transform.position = rightGunPivot.position;
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

    private void Move()
    {
        float moveValue = moveAction.ReadValue<float>();

        currentMoveSpeedX = moveValue * Mathf.MoveTowards(Mathf.Abs(currentMoveSpeedX), maxMoveSpeed, acceleration);
        float targetXPosition = RB.position.x + currentMoveSpeedX;
        RB.MovePosition(new Vector2(targetXPosition, RB.position.y));
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
