using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TestSandbag : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    public float maxHp = 100f;
    public float currentHp;

    [Header("시각 효과")]
    public Color hitColor = Color.red;
    public float blinkDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Coroutine blinkCoroutine;

    void Start()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // 물리 설정 (너무 가볍지 않게)
        rb.mass = 2f;
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // 인터페이스 구현: 플레이어가 호출할 함수
    public void TakeDamage(float damage, float knockback, Vector2 hitDirection)
    {
        currentHp -= damage;
        Debug.Log($"[샌드백] {damage} 데미지! 남은 HP: {currentHp}");

        // 넉백 적용
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(hitDirection * knockback, ForceMode2D.Impulse);
        }

        // 깜빡임 효과
        if (spriteRenderer != null)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        if (currentHp <= 0) Die();
    }

    private IEnumerator BlinkRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(blinkDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("[샌드백] 파괴됨. 부활 중...");
        currentHp = maxHp;
        transform.position = Vector3.zero;
    }
}