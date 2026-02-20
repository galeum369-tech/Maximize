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

    // 타겟 방향으로 이동
    public void MoveTowards(Vector2 targetPos, float speed)
    {
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);

        // 이동 방향에 맞춰 스프라이트 반전
        Flip(dir.x);
    }

    // 정지
    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // 스프라이트 반전 로직
    public void Flip(float xDir)
    {
        if (Mathf.Abs(xDir) < 0.01f) return;
        float scaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(xDir > 0 ? scaleX : -scaleX, transform.localScale.y, 1);
    }
}