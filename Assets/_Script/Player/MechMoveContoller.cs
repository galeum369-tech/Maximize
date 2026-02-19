using UnityEngine;

public class MechMoveController
{
    private Rigidbody2D rb;

    // 물리 설정값 (MechaController에서 조절 가능하도록 public으로 뺌)
    public float acceleration = 10f; // 가속도 (낮을수록 묵직함)
    public float deacceleration = 10f; // 감속도 (낮을수록 많이 미끄러짐)
    public float glideFallSpeed = -2f; // 활공 시 최대 낙하 속도

    public MechMoveController(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    // 1. 이동 (가속도 적용)
    public void Move(float xInput, float maxSpeed)
    {
        float targetSpeed = xInput * maxSpeed;
        float currentSpeed = rb.linearVelocity.x;

        // 목표 속도와의 차이
        float speedDif = targetSpeed - currentSpeed;

        // 입력이 있으면 가속, 없으면 감속 적용
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deacceleration;

        // ForceMode2D.Force를 사용하여 질량에 비례한 힘을 가함 (묵직한 느낌 핵심)
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    // 2. 점프
    public void Jump(float jumpForce)
    {
        // Y축 속도 초기화 후 점프 (반응성 향상)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 3. 활공 (핵심 로직)
    public void Glide()
    {
        // 이미 떨어지고 있을 때만 작동 (상승 중엔 활공 X)
        if (rb.linearVelocity.y < 0)
        {
            // 낙하 속도를 glideFallSpeed로 고정 (천천히 떨어짐)
            // Clamp를 써서 갑자기 툭 멈추지 않게 처리
            float newY = Mathf.Max(rb.linearVelocity.y, glideFallSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
        }
    }

    // 4. 정지 (감속 적용)
    public void Stop()
    {
        // 입력이 없을 때 마찰력에 의해 멈추는 느낌 구현
        Move(0, 0);
    }
}