using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshPro damageText;

    [Header("연출 설정")]
    public float moveSpeed = 2f; // 위로 떠오르는 속도
    public float fadeSpeed = 2f; // 투명해지는 속도
    public float destroyTime = 1f; // 몇 초 뒤에 삭제할 건지

    private Color textColor;

    private void Start()
    {
        // 생성되고 일정 시간 뒤에 자동으로 삭제
        Destroy(gameObject, destroyTime);

        if (damageText == null) damageText = GetComponent<TextMeshPro>();
        textColor = damageText.color;
    }

    private void Update()
    {
        // 1. 위로 살살 올라감
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. 서서히 투명해짐 (알파값 감소)
        textColor.a -= fadeSpeed * Time.deltaTime;
        damageText.color = textColor;
    }

    // 생성할 때 데미지 숫자를 세팅해 주는 함수
    public void Setup(float damageAmount, bool isCritical = false)
    {
        if (damageText == null) damageText = GetComponent<TextMeshPro>();

        damageText.text = damageAmount.ToString("F0");

        // 크리티컬이면 글씨를 크고 빨갛게 만듦
        if (isCritical)
        {
            damageText.color = Color.red;
            damageText.fontSize *= 1.5f;
            textColor = Color.red;
        }
    }
}