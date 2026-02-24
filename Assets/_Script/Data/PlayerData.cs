using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("보유 재화")]
    public int money;

    // [수정] 개별 스탯 레벨 대신 폼(Form)별 통합 레벨로 변경!
    [Header("=== 통합 레벨 ===")]
    public int humanLevel = 1;
    public int mechaLevel = 1;
    public int droneLevel = 1;

    [Header("=== 인간 기본 스탯 ===")]
    public float baseAtk = 10f;
    public float baseHp = 100f;
    public float baseDef = 5f;
    public float baseSpd = 3f;
    public float baseEnergyGain = 2.5f; // 기본 게이지 획득량

    [Header("인간 레벨당 증가량")]
    public float atkIncr = 2f;
    public float hpIncr = 20f;
    public float defIncr = 1f;
    public float spdIncr = 0.1f;
    public float energyGainIncr = 0.5f;

    [Header("=== 메카닉 기본 스탯 ===")]
    public float mechaBaseHp = 500f;
    public float mechaBaseAtk = 50f;
    public float mechaBaseDef = 20f;
    public float mechaBaseSpd = 8f;
    public float mechaMaxEnergy = 100f; // 변신에 필요한 통 

    [Header("메카 레벨당 증가량")]
    public float mechaHpIncr = 50f;
    public float mechaAtkIncr = 5f;
    public float mechaDefIncr = 2f;

    [Header("=== 서브 드론 기본 스탯 ===")]
    public float droneBaseAtk = 5f;
    public float droneFireRate = 0.5f; // 발사 간격
    public float droneRange = 10f;

    [Header("드론 레벨당 증가량")]
    public float droneAtkIncr = 1.5f;
    // 필요하다면 발사 속도나 사거리 증가량도 추가 가능

    [Header("=== 업그레이드 비용 설정 ===")]
    // 인간 강화 비용
    public int humanBaseCost = 100;
    public int humanCostIncr = 50;

    // 메카 강화 비용 (메카는 강력하니까 더 비싸게!)
    public int mechaBaseCost = 500;
    public int mechaCostIncr = 200;

    // 드론 강화 비용
    public int droneBaseCost = 200;
    public int droneCostIncr = 100;

    // 인간 레벨업 비용 계산
    public int GetHumanUpgradeCost()
    {
        return humanBaseCost + (humanLevel - 1) * humanCostIncr;
    }

    // 메카 레벨업 비용 계산
    public int GetMechaUpgradeCost()
    {
        return mechaBaseCost + (mechaLevel - 1) * mechaCostIncr;
    }

    // 드론 레벨업 비용 계산
    public int GetDroneUpgradeCost()
    {
        return droneBaseCost + (droneLevel - 1) * droneCostIncr;
    }

    // --- [수정] 인간 스탯 (humanLevel 하나로 모든 스탯이 오름) ---
    public float GetAtk() => baseAtk + (humanLevel - 1) * atkIncr;
    public float GetHp() => baseHp + (humanLevel - 1) * hpIncr;
    public float GetDef() => baseDef + (humanLevel - 1) * defIncr;
    public float GetSpd() => baseSpd + (humanLevel - 1) * spdIncr;
    public float GetEnergyGainPerHit() => baseEnergyGain + (humanLevel - 1) * energyGainIncr;

    // --- [수정] 메카 스탯 (mechaLevel 하나로 모든 스탯이 오름) ---
    public float GetMechaHp() => mechaBaseHp + (mechaLevel - 1) * mechaHpIncr;
    public float GetMechaAtk() => mechaBaseAtk + (mechaLevel - 1) * mechaAtkIncr;
    public float GetMechaDef() => mechaBaseDef + (mechaLevel - 1) * mechaDefIncr;
    public float GetMechaSpd() => mechaBaseSpd; // 속도는 고정

    // --- [수정] 드론 스탯 (droneLevel 하나로 모든 스탯이 오름) ---
    public float GetDroneAtk() => droneBaseAtk + (droneLevel - 1) * droneAtkIncr;
}