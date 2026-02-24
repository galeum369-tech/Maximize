using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("보유 재화")]
    public int money;

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
    public float mechaBaseSpd = 8f;   // [추가됨] 메카 기본 속도
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

    [Header("=== 업그레이드 비용 설정 ===")]
    public int humanBaseCost = 100;
    public int humanCostIncr = 50;
    public int mechaBaseCost = 500;
    public int mechaCostIncr = 200;
    public int droneBaseCost = 200;
    public int droneCostIncr = 100;

    public int GetHumanUpgradeCost() => humanBaseCost + (humanLevel - 1) * humanCostIncr;
    public int GetMechaUpgradeCost() => mechaBaseCost + (mechaLevel - 1) * mechaCostIncr;
    public int GetDroneUpgradeCost() => droneBaseCost + (droneLevel - 1) * droneCostIncr;

    // --- 인간 스탯 ---
    public float GetAtk() => baseAtk + (humanLevel - 1) * atkIncr;
    public float GetHp() => baseHp + (humanLevel - 1) * hpIncr;
    public float GetDef() => baseDef + (humanLevel - 1) * defIncr;
    public float GetSpd() => baseSpd + (humanLevel - 1) * spdIncr;
    public float GetEnergyGainPerHit() => baseEnergyGain + (humanLevel - 1) * energyGainIncr;

    // --- 메카 스탯 ---
    public float GetMechaHp() => mechaBaseHp + (mechaLevel - 1) * mechaHpIncr;
    public float GetMechaAtk() => mechaBaseAtk + (mechaLevel - 1) * mechaAtkIncr;
    public float GetMechaDef() => mechaBaseDef + (mechaLevel - 1) * mechaDefIncr;
    public float GetMechaSpd() => mechaBaseSpd; // 속도는 레벨업 증가량 없이 고정 반환!

    // --- 드론 스탯 ---
    public float GetDroneAtk() => droneBaseAtk + (droneLevel - 1) * droneAtkIncr;
}