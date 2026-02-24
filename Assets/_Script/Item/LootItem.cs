using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LootItem : MonoBehaviour
{
    [Header("아이템 정보")]
    public ItemData itemData; // 드롭될 아이템의 데이터 (SO)
    public int amount = 1;    // 떨어질 개수

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isCollected = false;

    [Header("자석 효과 설정")]
    public float pullRange = 5f;  // 플레이어를 감지할 범위
    public float pullSpeed = 10f; // 끌려가는 속도
    public float eatRange = 0.5f; // [추가] 이 거리 안으로 들어오면 획득됨

    private Transform playerTransform;
    private float magnetDelay = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // [수정] 트리거 강제 전환 삭제! 바닥이랑 정상적으로 충돌해서 튕기도록 냅둠
        // col.isTrigger = true; 
    }

    private void Start()
    {
        // 생성될 때 위로 살짝 튀어오르는 연출
        rb.AddForce(new Vector2(Random.Range(-2f, 2f), 5f), ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (isCollected) return;

        // 딜레이가 아직 안 끝났으면 대기
        if (magnetDelay > 0)
        {
            magnetDelay -= Time.deltaTime;
            return;
        }

        // 플레이어 타겟팅 (GameManager 적극 활용)
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            // 네가 만든 GameManager에 등록된 진짜 활성화 플레이어를 가져옴
            if (GameManager.Instance != null && GameManager.Instance.GetActivePlayer() != null)
            {
                playerTransform = GameManager.Instance.GetActivePlayer();
            }
            else
            {
                // 보험용 OverlapCircle
                Collider2D hit = Physics2D.OverlapCircle(transform.position, pullRange, LayerMask.GetMask("Player"));
                if (hit != null) playerTransform = hit.transform;
            }
        }
        else
        {
            // 플레이어 방향으로 이동 (자석 효과)
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, pullSpeed * Time.deltaTime);

            // [핵심] 트리거 콜라이더 대신, 실제 거리를 재서 먹어버림!
            if (Vector2.Distance(transform.position, playerTransform.position) <= eatRange)
            {
                TryCollectItem();
            }
        }
    }

    // 아이템 획득 시도 (OnTriggerEnter2D를 대체함)
    private void TryCollectItem()
    {
        bool isAdded = InventoryManager.Instance.AddItem(itemData, amount);

        if (isAdded)
        {
            isCollected = true;
            Debug.Log($"<color=yellow>{itemData.itemName}</color> {amount}개 획득!");

            // TODO: 획득 이펙트나 사운드 재생
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("인벤토리가 꽉 차서 먹을 수 없어!");

            // 인벤토리가 꽉 찼을 때 플레이어 몸에 비비적거리지 않게 살짝 밀어내거나 타겟을 초기화
            playerTransform = null;
            magnetDelay = 1f; // 1초 뒤에 다시 자석 켜짐
        }
    }
}