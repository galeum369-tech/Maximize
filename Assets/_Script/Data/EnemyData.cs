using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("비주얼 및 식별")]
    public string monsterName; // 몬스터 이름
    public GameObject prefab;  // [추가] 실제 맵에 생성될 몬스터 오브젝트 프리팹

    [Header("기본 스탯")]
    public float baseMaxHp = 50f;     // 최대 체력
    public float baseDamage = 10f;    // 기본 공격력
    public float baseDefense = 5f;    // 방어력 (%)
    public float baseMoveSpeed = 2f;  // 이동 속도

    [Header("인식 설정")]
    public float attackRange = 1.5f; // 공격 사거리
    public float detectRange = 7f;   // 플레이어 감지 범위

    [Header("보상")]
    public int dropMoneyMin = 10;    // 드랍 골드 최소치
    public int dropMoneyMax = 50;    // 드랍 골드 최대치

    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab; // 아이템 프리팹
        [Range(0, 100)] public float dropChance; // 드랍 확률
    }

    [Header("아이템 드랍 테이블")]
    public List<DropItem> lootTable; // 이 몬스터가 죽을 때 줄 수 있는 템 목록
}