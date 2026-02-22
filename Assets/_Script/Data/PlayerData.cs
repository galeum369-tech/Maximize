using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("보유 재화")]
    public int money;

    [Header("공격스텟 레벨")]
    public int attackLevel = 1;

    [Header("생존/기동 스텟 레벨")]
    public int hpLevel = 1;
    public int defLevel = 1;
    public int spdLevel = 1;

    [Header("기본 스텟")]
    public float baseAtk = 10f;
    public float baseHp = 100f;
    public float baseDef = 5f;
    public float baseSpd = 3f; //이동 속도

    [Header("=== 메카닉 전용 스탯 ===")]
    [Header("메카 기본 스탯")]
    public float mechaBaseHp = 500f;   // 훨씬 높은 체력
    public float mechaBaseAtk = 50f;   // 강력한 공격력
    public float mechaBaseDef = 20f;   // 단단한 방어력
    public float mechaBaseSpd = 8f;    // 이동 속도 (MoveController에서 참조)
    public float mechaFuelMax = 100f;  // (선택) 부스터/스킬용 연료

    [Header("=== 서브 드론 전용 스탯 ===")]
    public float droneBaseAtk = 5f;      // 드론 기본 공격력
    public float droneAtkIncr = 1.5f;   // 공격력 레벨당 증가량
    public float droneFireRate = 0.5f;  // 발사 간격 (낮을수록 빠름)
    public float droneRange = 10f;      // 탐색 사거리

    [Header("스텟 레벨당 증가량")]
    public float atkIncr = 2f; //절대값 증가
    public float hpIncr = 20f;  //절대값 증가
    public float defIncr = 1f;  //절대값 증가
    public float spdIncr = 0.1f; //절대값 증가

    [Header("메카 성장치 (레벨 공유 시)")]
    // 파일럿 레벨업 시 메카도 같이 강해지게 할 건지 결정해야 함.
    // 여기서는 파일럿의 레벨(attackLevel 등)을 계수로 사용하여 메카 스탯도 오르게 설정.
    public float mechaHpIncr = 50f;
    public float mechaAtkIncr = 5f;
    public float mechaDefIncr = 2f;


    //스텟 레벨업 비용
    public int GetUpgradeCost(int currentLevel)
    {
        // 레벨이 오를수록 비용이 비싸지는 공식 (예: 기본 100원 + 레벨당 50원씩 가산)
        return 100 + (currentLevel - 1) * 50;
    }

    //스텟 계산 공식
    public float GetAtk()
    {
        return baseAtk + (attackLevel - 1) * atkIncr;
    }
    public float GetHp()
    {
        return baseHp + (hpLevel - 1) * hpIncr;
    }
    public float GetDef()
    {
        return baseDef + (defLevel - 1) * defIncr;
    }
    public float GetSpd()
    {
        return baseSpd + (spdLevel - 1) * spdIncr;
    }

    // --- 메카 스탯 계산 메서드 ---
    public float GetMechaHp()
    {
        // 예: 기본 체력 + (체력 레벨 * 증가량)
        return mechaBaseHp + (hpLevel - 1) * mechaHpIncr;
    }

    public float GetMechaAtk()
    {
        return mechaBaseAtk + (attackLevel - 1) * mechaAtkIncr;
    }

    public float GetMechaDef()
    {
        return mechaBaseDef + (defLevel - 1) * mechaDefIncr;
    }

    public float GetMechaSpd()
    {
        // 속도는 레벨업으로 안 오르게 하거나 미세하게 증가
        return mechaBaseSpd;
    }

    // --- 드론 스탯 계산 메서드 ---
    public float GetDroneAtk()
    {
        // 유저의 attackLevel을 공유하여 드론의 공격력도 같이 성장하게 설정
        return droneBaseAtk + (attackLevel - 1) * droneAtkIncr;
    }
}
