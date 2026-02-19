using UnityEngine;
using System.Collections.Generic;

public class MobSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class MobPool
    {
        public GameObject mobPrefab;
        [Range(0, 100)] public float spawnChance; // 출현 확률
    }

    public List<MobPool> mobPools;     // 해당 테마에서 나올 수 있는 몹 목록
    public Transform[] spawnPoints;   // 맵에 미리 배치한 스폰 지점들
    public float emptyChance = 20f;   // 아무것도 안 스폰될 확률 (빈 구역 생성)

    void Start()
    {
        SpawnMobs();
    }

    public void SpawnMobs()
    {
        foreach (var sp in spawnPoints)
        {
            // 빈 구역 체크
            if (Random.Range(0f, 100f) < emptyChance) continue;

            // 랜덤 몹 선택 (간단한 방식)
            int randomIndex = Random.Range(0, mobPools.Count);
            MobPool selected = mobPools[randomIndex];

            // 확률 기반 생성
            if (Random.Range(0f, 100f) <= selected.spawnChance)
            {
                Instantiate(selected.mobPrefab, sp.position, Quaternion.identity, sp);
            }
        }
    }
}