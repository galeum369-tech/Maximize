using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("보유 재화")]
    public int money;

    [Header("공격스텟 레벨")]
    public int attackLevel = 1;
    public int critLevel = 1;

    [Header("생존/기동 스텟 레벨")]
    public int hpLevel = 1;
    public int defLevel = 1;
    public int spdLevel = 1;
    public int staminaLevel = 1;

    [Header("기본 스텟")]
    public float baseAtk = 10f;
    public float baseCrit = 0.1f; //치명타 확률
    public float baseHp = 100f;
    public float baseDef = 5f;
    public float baseSpd = 3f; //이동 속도
    public float baseStamina = 10f; //스태미나 최대치

    [Header("스텟 레벨당 증가량")]
    public float atkIncr = 2f; //절대값 증가
    public float critIncr = 0.05f; //절대값 증가 
    public float hpIncr = 20f;  //절대값 증가
    public float defIncr = 1f;  //절대값 증가
    public float spdIncr = 0.1f; //절대값 증가
    public float staminaIncr = 0.5f;  //절대값 증가

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
    public float GetCrit()
    {
        return baseCrit + (critLevel - 1) * critIncr;
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
    public float GetStamina()
    {
        return baseStamina + (staminaLevel - 1) * staminaIncr;
    }


}
