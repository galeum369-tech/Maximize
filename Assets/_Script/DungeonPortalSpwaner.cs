using UnityEngine;
using System.Collections.Generic;

public class DungeonPortalSpawner : MonoBehaviour
{
    [Header("포탈 프리팹")]
    public GameObject dungeonPortalPrefab;

    [Header("스폰 위치들")]
    public List<Transform> spawnPoints;

    [Header("설정")]
    public int portalCount = 3; // 필드에 생성할 던전 포탈 개수

    void Start()
    {
        SpawnDungeonPortals();
    }

    void SpawnDungeonPortals()
    {
        if (spawnPoints.Count < portalCount) portalCount = spawnPoints.Count;

        // 위치 리스트를 복사해서 랜덤하게 섞음
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < portalCount; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];

            // 포탈 생성
            GameObject portalObj = Instantiate(dungeonPortalPrefab, selectedPoint.position, Quaternion.identity);

            // 필요하다면 여기서 포탈의 목적지(targetSceneName)를 랜덤하게 설정 가능
            // Portal p = portalObj.GetComponent<Portal>();
            // p.targetSceneName = "Dungeon_Random_01";

            // 사용한 지점은 리스트에서 제거해서 중복 스폰 방지
            availablePoints.RemoveAt(randomIndex);
        }
    }
}