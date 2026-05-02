using UnityEngine;

public class BallWall : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball = Ball.GetBall(collision.collider);
        if (!ball) return;

        AudioManager.Instance.PlayAudio("WallHit");
    }
}
