using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [Header("데이터 참조")]
    public PlayerData baseData;

    // 최종 스탯
    public float FinalMaxHP { get; private set; }
    public float FinalAtk { get; private set; }
    public float FinalDef { get; private set; }
    public float FinalSpd { get; private set; }

    [Header("현재 상태")]
    public float currentHp;

    [Header("일시적 버프 수치")]
    public float buffAtk = 0f;
    public float buffDef = 0f;
    public float buffSpd = 0f;

    private void Start()
    {
        RecalculateFinalStats();
        currentHp = FinalMaxHP;
        // 시작 시 체력바 초기화
        UIManager.Instance?.UpdateHP(currentHp, FinalMaxHP);
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged += RecalculateFinalStats;

        // [핵심 추가] 폼이 켜질 때마다(변신해서 돌아올 때) 최신 장비 스탯으로 강제 갱신!
        RecalculateFinalStats();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged -= RecalculateFinalStats;
    }

    public void RecalculateFinalStats()
    {
        if (baseData == null || InventoryManager.Instance == null) return;

        // [수정] 1. 계산 전의 기존 최대 체력을 기억해 둠
        float oldMaxHP = FinalMaxHP;

        FinalMaxHP = baseData.GetHp() + InventoryManager.Instance.GetTotalBonus("Player", "hp");
        FinalAtk = baseData.GetAtk() + InventoryManager.Instance.GetTotalBonus("Player", "atk") + buffAtk;
        FinalDef = baseData.GetDef() + InventoryManager.Instance.GetTotalBonus("Player", "def") + buffDef;
        FinalSpd = baseData.GetSpd() + InventoryManager.Instance.GetTotalBonus("Player", "spd") + buffSpd;

        // [수정] 2. 게임 처음 시작할 때(oldMaxHP가 0일 때)가 아니라면 체력 보정
        if (oldMaxHP > 0)
        {
            if (FinalMaxHP > oldMaxHP)
            {
                // 최대 체력이 늘어났다면 그 차이만큼 현재 체력도 올려줌 (장비 꼼수 방지)
                // (만약 레벨업/장착 시 무조건 풀피가 되길 원한다면 currentHp = FinalMaxHP; 로 바꿔!)
                currentHp += (FinalMaxHP - oldMaxHP);
            }
            else
            {
                // 장비를 빼서 최대 체력이 줄어들었다면, 상한선을 넘어가지 않게 깎아줌
                currentHp = Mathf.Min(currentHp, FinalMaxHP);
            }
        }

        if (gameObject.activeInHierarchy && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHp, FinalMaxHP);
        }
    }
}