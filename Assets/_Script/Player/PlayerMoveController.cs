using UnityEngine;

public class PlayerMoveController
{
    Rigidbody2D rb;

    public PlayerMoveController(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    // 이동
    public void Move(float xInput, float speed)
    {
        //입력 방향 *속도로 이동
        rb.linearVelocity = new Vector2(xInput * speed, rb.linearVelocity.y);
    }

    public void Jump(float jumpPower)
    {
        //점프
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    public void Stop()
    {
        //멈춤
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

}
