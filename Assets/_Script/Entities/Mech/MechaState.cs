using UnityEngine;

public class MechaState : MonoBehaviour
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

    private void Awake()
    {
        RecalculateFinalStats();
        currentHp = FinalMaxHP;
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged += RecalculateFinalStats;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged -= RecalculateFinalStats;
    }

    public void RecalculateFinalStats()
    {
        // PlayerData에 적혀있는 메카 스탯을 기준으로 장비 보너스 합산
        FinalMaxHP = baseData.mechaBaseHp + InventoryManager.Instance.GetTotalBonus("Mecha", "hp");
        FinalAtk = baseData.mechaBaseAtk + InventoryManager.Instance.GetTotalBonus("Mecha", "atk");
        FinalDef = baseData.mechaBaseDef + InventoryManager.Instance.GetTotalBonus("Mecha", "def");
        FinalSpd = baseData.mechaBaseSpd + InventoryManager.Instance.GetTotalBonus("Mecha", "spd"); // PlayerData에 맞게 변수명 확인 필요

        currentHp = Mathf.Min(currentHp, FinalMaxHP);
        Debug.Log($"[Mecha] 스탯 갱신 완료! HP: {FinalMaxHP}");
    }

    public void TakeDamage(float damage)
    {
        float reduction = FinalDef / 100f;
        float actualDamage = damage * (1f - reduction);
        actualDamage = Mathf.Max(actualDamage, 1f);

        currentHp -= actualDamage;
        UIManager.Instance?.UpdateHP(currentHp, FinalMaxHP);

        if (currentHp <= 0)
        {
            currentHp = 0;
            PlayerTransformManager.Instance?.ToHuman();
        }
    }
}