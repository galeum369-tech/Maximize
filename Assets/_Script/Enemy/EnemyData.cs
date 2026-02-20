using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string monsterName;

    [Header("기본 스탯")]
    public float baseMaxHp = 50f;
    public float baseDamage = 10f;
    public float baseDefense = 5f;
    public float baseMoveSpeed = 2f;

    [Header("인식 설정")]
    public float attackRange = 1.5f;
    public float detectRange = 7f;

    [Header("보상")]
    public int dropMoneyMin = 10;
    public int dropMoneyMax = 50;
    // 나중에 아이템 드랍 리스트도 여기에 추가하면 됨
}