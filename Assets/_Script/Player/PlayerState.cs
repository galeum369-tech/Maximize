using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [Header("데이터 참조")]
    public PlayerData baseData; 

    [Header("장착 슬롯")]
    public EquipmentData coreSlot;
    public EquipmentData frameSlot;
    public EquipmentData gearSlot;
    public EquipmentData chipSlot;

    // 최종 계산된 스탯
    public float FinalMaxHP { get; private set; }
    public float FinalAtk { get; private set; }
    public float FinalCrit { get; private set; }
    public float FinalDef { get; private set; }
    public float FinalSpd { get; private set; }
    public float FinalMaxStamina { get; private set; }

    // 가변 수치 
    public float CurrentHP { get; private set; }
    public float CurrentStamina { get; private set; }

    private void Awake()
    {
        // 초기화 시 전체 스탯 계산
        RecalculateFinalStats();

        // 현재 수치를 최대치로 설정
        CurrentHP = FinalMaxHP;
        CurrentStamina = FinalMaxStamina;
    }

    private void Update()
    {
        // 예시: 스태미나 자동 회복 (최대치까지 서서히 차오름)
        if (CurrentStamina < FinalMaxStamina)
        {
            CurrentStamina += Time.deltaTime * 2f; // 초당 2씩 회복
            CurrentStamina = Mathf.Min(CurrentStamina, FinalMaxStamina);
        }
    }

    // 장비를 장착할 때 호출하는 메서드
    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;

        // 타입에 맞는 슬롯에 배정 (기존 장비는 덮어씌워짐)
        switch (newItem.equipType)
        {
            case EquipType.Core: coreSlot = newItem; break;
            case EquipType.Frame: frameSlot = newItem; break;
            case EquipType.Gear: gearSlot = newItem; break;
            case EquipType.Chip: chipSlot = newItem; break;
        }

        RecalculateFinalStats(); // 장착 후 스탯 즉시 갱신
    }


    #region 스탯 계산 로직
    // 모든 스탯을 다시 합산하는 핵심 로직
    public void RecalculateFinalStats()
    {
        // 1. 기본 수치 + 레벨업 증가량 가져오기
        float totalHp = baseData.GetHp();
        float totalAtk = baseData.GetAtk();
        float totalCrit = baseData.GetCrit();
        float totalDef = baseData.GetDef();
        float totalSpd = baseData.GetSpd();
        float totalStamina = baseData.GetStamina();

        // 2. 장착 슬롯 순회하며 보너스 합산
        totalHp += GetBonus(coreSlot, "hp") + GetBonus(frameSlot, "hp") + GetBonus(gearSlot, "hp") + GetBonus(chipSlot, "hp");
        totalAtk += GetBonus(coreSlot, "atk") + GetBonus(frameSlot, "atk") + GetBonus(gearSlot, "atk") + GetBonus(chipSlot, "atk");
        totalCrit += GetBonus(coreSlot, "crit") + GetBonus(frameSlot, "crit") + GetBonus(gearSlot, "crit") + GetBonus(chipSlot, "crit");
        totalDef += GetBonus(coreSlot, "def") + GetBonus(frameSlot, "def") + GetBonus(gearSlot, "def") + GetBonus(chipSlot, "def");
        totalSpd += GetBonus(coreSlot, "spd") + GetBonus(frameSlot, "spd") + GetBonus(gearSlot, "spd") + GetBonus(chipSlot, "spd");
        totalStamina += GetBonus(coreSlot, "stamina") + GetBonus(frameSlot, "stamina") + GetBonus(gearSlot, "stamina") + GetBonus(chipSlot, "stamina");

        // 3. 최종 값 적용
        FinalMaxHP = totalHp;
        FinalAtk = totalAtk;
        FinalCrit = totalCrit;
        FinalDef = totalDef;
        FinalSpd = totalSpd;
        FinalMaxStamina = totalStamina;

        Debug.Log($"[PlayerState] 스탯 갱신 완료! 공격력: {FinalAtk}, 속도: {FinalSpd}");
    }

    // 슬롯이 비어있는지 체크하며 수치를 가져오는 헬퍼 함수
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
            case "stamina": return item.staminaBonus;
            default: return 0;
        }
    }
    #endregion

    public bool ConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            return true; // 소모 성공
        }
        return false; // 스태미나 부족
    }

    // 데미지 처리 예시
    public void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage - FinalDef, 1f); // 방어력 적용
        CurrentHP -= actualDamage;
        if (CurrentHP <= 0) Debug.Log("플레이어 사망");
    }
}