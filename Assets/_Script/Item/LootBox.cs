using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class LootBox : MonoBehaviour
{
    // 몬스터 드랍 테이블과 동일한 구조의 클래스 선언
    [System.Serializable]
    public class BoxLoot
    {
        public GameObject itemPrefab; // 떨어뜨릴 아이템 프리팹 (LootItem)
        [Range(0, 100)] public float dropChance; // 드랍 확률 (0% ~ 100%)
    }

    [Header("보상 드랍 테이블")]
    public List<BoxLoot> lootTable; // 인스펙터에서 아이템과 확률을 세팅할 리스트

    [Header("비주얼 설정")]
    public SpriteRenderer spriteRenderer;
    public Sprite openSprite; // 열렸을 때 상자 이미지

    [Header("UI 연출")]
    public UnityEvent onPlayerEnter; // "F키를 눌러 열기" UI 띄우기
    public UnityEvent onPlayerExit;  // UI 끄기

    private PlayerInputHandler playerInput;
    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpened) return; // 이미 열렸으면 무시

        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                playerInput.OnInteract += OpenBox;
                onPlayerEnter?.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInput != null)
        {
            playerInput.OnInteract -= OpenBox;
            playerInput = null;
            onPlayerExit?.Invoke();
        }
    }

    private void OpenBox()
    {
        if (isOpened) return;

        isOpened = true; // 중복 오픈 방지

        // 상호작용 키 구독 해제
        if (playerInput != null)
        {
            playerInput.OnInteract -= OpenBox;
            playerInput = null;
        }

        onPlayerExit?.Invoke(); // 안내 UI 끄기

        // 상자 이미지 변경
        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        DropLoot(); // 드랍 로직 실행
    }

    private void DropLoot()
    {
        if (lootTable == null || lootTable.Count == 0) return;

        // 테이블에 등록된 모든 아이템에 대해 각각 확률을 굴림 (몬스터 로직과 동일!)
        foreach (var loot in lootTable)
        {
            // 0 ~ 100 사이의 랜덤 값을 뽑아서 설정한 확률보다 낮거나 같으면 드랍 성공
            if (Random.Range(0f, 100f) <= loot.dropChance)
            {
                // 아이템 생성 (상자 위치에서 살짝 위)
                GameObject droppedItem = Instantiate(loot.itemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

                // 사방으로 튀어오르는 물리 효과 부여
                Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 popDirection = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(3f, 5f));
                    rb.AddForce(popDirection, ForceMode2D.Impulse);
                }
            }
        }
    }
}