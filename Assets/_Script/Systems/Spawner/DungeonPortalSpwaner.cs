using UnityEngine;
using System.Collections.Generic;

public class DungeonPortalSpawner : MonoBehaviour
{
    [Header("포탈 프리팹")]
    public GameObject dungeonPortalPrefab;
    [Header("스폰 위치들")]
    public List<Transform> spawnPoints;
    [Header("설정")]
    public int portalCount = 3;

    void Start()
    {
        // [핵심 추가] 아까 저장한 시드값이 있다면, 랜덤 결과를 고정시킴
        if (PlayerPrefs.HasKey("CurrentFieldSeed"))
        {
            Random.InitState(PlayerPrefs.GetInt("CurrentFieldSeed"));
        }

        SpawnDungeonPortals();

        // 포탈 배치가 끝나면, 다음 랜덤 기능들에 영향을 주지 않도록 시간 기반 랜덤으로 다시 돌려놓음
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    void SpawnDungeonPortals()
    {
        if (spawnPoints.Count < portalCount) portalCount = spawnPoints.Count;
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < portalCount; i++)
        {
            // 이제 위에서 InitState로 고정했기 때문에, 
            // 던전에 갔다 와도 무조건 '아까 뽑았던 똑같은 위치(randomIndex)'가 뽑힘!
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];

            Instantiate(dungeonPortalPrefab, selectedPoint.position, Quaternion.identity);
            availablePoints.RemoveAt(randomIndex);
        }
    }
}