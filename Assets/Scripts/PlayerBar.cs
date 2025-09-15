using UnityEngine;
using UnityEngine.InputSystem;
using KBCore.Refs;

public class PlayerBar : MonoBehaviour
{
    [SerializeField, Self] private Rigidbody2D RB;
    [SerializeField] private Transform barTransform;

    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float minDeflectionAngleInDegrees;
    [SerializeField] private int gizmoDeflectionResolution;
    [SerializeField] private float barScale;

    private InputAction moveAction;
    private float currentMoveSpeedX;
    private float minDeflectionAngleCos;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        currentMoveSpeedX = 0f;
        minDeflectionAngleCos = Mathf.Cos(minDeflectionAngleInDegrees * Mathf.Deg2Rad);
        UpdateSize();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Ball ball = Ball.GetBall(collision.collider);
        if (!ball) return;

        DeflectBall(ball);
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
            globalBarPositionX = RB.position.x + (barScale / 2f) * deflectedAngleCos;
            Gizmos.DrawLine(new Vector3(globalBarPositionX, RB.position.y, 0f), new Vector3(globalBarPositionX + reflectDirection.x, RB.position.y + reflectDirection.y, 0f));
        }
    }

    private void UpdateSize()
    {
        barTransform.localScale = new Vector3(barScale, barTransform.localScale.y, 1f);
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

        float contactPosition = Mathf.Clamp(collisionGlobalPositionX, playerPositionX - barScale / 2f, playerPositionX + barScale / 2f);
        float contactNormalizedPosition = Mathf.InverseLerp(playerPositionX - barScale / 2f, playerPositionX + barScale / 2f, contactPosition);
        return contactNormalizedPosition * 2f - 1;
    }

    private Vector2 GetDeflectionDirection(float deflectionAngleCos)
    {
        float deflectedAngleCos = deflectionAngleCos * minDeflectionAngleCos;
        return new Vector2(deflectedAngleCos, 1 - deflectedAngleCos * deflectedAngleCos).normalized;
    }
}
