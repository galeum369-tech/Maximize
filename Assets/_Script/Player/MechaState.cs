using UnityEngine;

public class MechaState : MonoBehaviour
{
    [Header("데이터 참조")]
    public PlayerData baseData; // PlayerData SO (메카 스탯 포함)

    [Header("메카 전용 장착 슬롯")]
    public EquipmentData coreSlot;
    public EquipmentData frameSlot;
    public EquipmentData gearSlot;
    public EquipmentData chipSlot;

    // 최종 스탯 (외부에서 읽기 전용)
    public float FinalMaxHP { get; private set; }
    public float FinalAtk { get; private set; }
    public float FinalDef { get; private set; }
    public float FinalSpd { get; private set; }

    [Header("현재 상태")]
    public float currentHp;

    private void Awake()
    {
        // 시작 시 스탯 계산 및 체력 초기화
        RecalculateFinalStats();
        currentHp = FinalMaxHP;
    }

    // 장비 장착 (마을 UI에서 호출)
    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;

        switch (newItem.equipType)
        {
            case EquipType.Core: coreSlot = newItem; break;
            case EquipType.Frame: frameSlot = newItem; break;
            case EquipType.Gear: gearSlot = newItem; break;
            case EquipType.Chip: chipSlot = newItem; break;
        }

        // 장비가 바뀌었으니 스탯 재계산
        RecalculateFinalStats();
    }

    public void RecalculateFinalStats()
    {
        if (baseData == null) return;

        // PlayerData의 메카 기본 스탯 + 장비 보너스 합산
        FinalMaxHP = baseData.GetMechaHp() + GetTotalBonus("hp");
        FinalAtk = baseData.GetMechaAtk() + GetTotalBonus("atk");
        FinalDef = baseData.GetMechaDef() + GetTotalBonus("def");
        FinalSpd = baseData.GetMechaSpd() + GetTotalBonus("spd");

        // 체력이 줄어든 상태에서 장비 해제 등으로 최대 체력이 깎일 수 있으므로 보정
        currentHp = Mathf.Min(currentHp, FinalMaxHP);
    }

    // --- [추가] 데미지 처리 함수 ---
    public void TakeDamage(float damage)
    {
        // 1. 실질 데미지 계산 (데미지 - 방어력)
        // 방어력이 높아도 최소 1의 데미지는 입도록 설정 (취향에 따라 0으로 해도 됨)
        float actualDamage = Mathf.Max(damage - FinalDef, 1f);

        // 2. 체력 차감
        currentHp -= actualDamage;

        // 3. 파괴 체크
        if (currentHp <= 0)
        {
            currentHp = 0;
            HandleDestruction();
        }
    }

    // --- [추가] 메카 파괴 시 처리 ---
    private void HandleDestruction()
    {
        Debug.Log("메카 파괴됨! 파일럿 비상 탈출!");

        // 1. 변신 매니저가 있다면 강제로 인간형으로 전환
        if (PlayerTransformManager.Instance != null)
        {
            // true를 넘겨서 "파괴되어 변신 풀림"을 알릴 수도 있음 (연출용)
            PlayerTransformManager.Instance.ToHuman();
        }
    }

    // --- 장비 보너스 합산 로직 ---
    private float GetTotalBonus(string statType)
    {
        return GetBonus(coreSlot, statType) + GetBonus(frameSlot, statType) +
               GetBonus(gearSlot, statType) + GetBonus(chipSlot, statType);
    }

    private float GetBonus(EquipmentData item, string statType)
    {
        if (item == null) return 0;
        switch (statType)
        {
            case "hp": return item.hpBonus;
            case "atk": return item.atkBonus;
            case "crit": return item.critBonus;
            case "def": return item.defBonus;
            case "spd": return item.spdBonus;
            default: return 0;
        }
    }
}