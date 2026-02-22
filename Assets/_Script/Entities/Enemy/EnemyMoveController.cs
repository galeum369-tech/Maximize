using UnityEngine;

public class EnemyMoveController
{
    private Rigidbody2D rb;
    private Transform transform;

    public EnemyMoveController(Rigidbody2D rb, Transform transform)
    {
        this.rb = rb;
        this.transform = transform;
    }

    // 메서드 이름을 Move로 변경하여 EnemyBase와 일치시킴
    public void Move(float xDir, float speed)
    {
        rb.linearVelocity = new Vector2(xDir * speed, rb.linearVelocity.y);
        Flip(xDir);
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void Flip(float xDir)
    {
        if (Mathf.Abs(xDir) < 0.01f) return;
        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(xDir > 0 ? scaleX : -scaleX, transform.localScale.y, 1);
    }
}