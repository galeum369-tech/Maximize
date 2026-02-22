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
    private Transform playerTransform;

    // [추가] 튀어 오르는 걸 감상할 딜레이 시간
    private float magnetDelay = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 플레이어와 부딪혔을 때 물리적 충돌은 무시하고 판정만 하도록 설정
        col.isTrigger = true;
    }

    private void Start()
    {
        // 생성될 때 위로 살짝 튀어오르는 연출 (파밍하는 맛을 살림)
        rb.AddForce(new Vector2(Random.Range(-2f, 2f), 5f), ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (isCollected) return;

        // 딜레이가 아직 안 끝났으면 자석 효과 작동 안 함
        if (magnetDelay > 0)
        {
            magnetDelay -= Time.deltaTime;
            return;
        }

        // 플레이어가 주변에 있는지 확인
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, pullRange, LayerMask.GetMask("Player"));
            if (hit != null) playerTransform = hit.transform;
        }
        else
        {
            // 플레이어 방향으로 이동 (자석 효과)
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, pullSpeed * Time.deltaTime);
        }
    

        // 타겟이 꺼져있다면(변신 등으로 인해) 타겟 초기화
        if (playerTransform != null && !playerTransform.gameObject.activeInHierarchy)
        {
            playerTransform = null;
        }

        if (playerTransform == null)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, pullRange, LayerMask.GetMask("Player"));
            if (hit != null) playerTransform = hit.transform;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, pullSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 먹은 아이템이거나, 부딪힌 게 플레이어가 아니면 무시
        if (isCollected || !collision.CompareTag("Player")) return;

        // 인벤토리 매니저를 통해 아이템 추가 시도
        bool isAdded = InventoryManager.Instance.AddItem(itemData, amount);

        if (isAdded)
        {
            isCollected = true;
            Debug.Log($"<color=yellow>{itemData.itemName}</color> {amount}개 획득!");

            // 획득 이펙트나 사운드를 여기서 재생하면 됨

            // 인벤토리에 잘 들어갔으면 필드에서 오브젝트 삭제
            Destroy(gameObject);
        }
        else
        {
            // 인벤토리가 꽉 찼을 경우
            Debug.Log("인벤토리가 꽉 차서 먹을 수 없어!");
        }
    }
}