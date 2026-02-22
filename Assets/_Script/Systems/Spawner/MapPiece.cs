using UnityEngine;
using System.Collections.Generic;

public class MapPiece : MonoBehaviour
{
    [Header("Connections")]
    public Transform nextPoint;
    public GameObject exitPortal; // 다음 방으로 가는 포탈 (처음엔 꺼두기)

    [Header("Combat Settings")]
    [SerializeField] private List<Transform> enemySpawnPoints;
    [SerializeField] private List<EnemyData> spawnableEnemyData;

    // 현재 살아있는 몹들을 담는 리스트
    private List<EnemyBase> aliveEnemies = new List<EnemyBase>();

    public void Initialize()
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Count == 0) return;

        // 포탈이 있다면 처음엔 비활성화
        if (exitPortal != null) exitPortal.SetActive(false);

        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            if (Random.Range(0f, 100f) < 80f) // 소환 확률 80%로 상향
            {
                SpawnEnemy(spawnPoint);
            }
        }

        // 혹시라도 몹이 하나도 소환 안 됐다면 바로 문 열어주기
        CheckRoomClear();
    }

    private void SpawnEnemy(Transform spawnPoint)
    {
        if (spawnableEnemyData == null || spawnableEnemyData.Count == 0) return;

        EnemyData selectedData = spawnableEnemyData[Random.Range(0, spawnableEnemyData.Count)];
        GameObject enemyObj = Instantiate(selectedData.prefab, spawnPoint.position, Quaternion.identity);
        enemyObj.transform.SetParent(this.transform);

        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.data = selectedData;

            // 리스트에 추가하고 사망 이벤트 구독
            aliveEnemies.Add(enemy);
            enemy.OnDeathEvent += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath(EnemyBase enemy)
    {
        // 이벤트 구독 해제 및 리스트에서 제거
        enemy.OnDeathEvent -= HandleEnemyDeath;
        aliveEnemies.Remove(enemy);

        // 남은 적이 있는지 체크
        CheckRoomClear();
    }

    private void CheckRoomClear()
    {
        if (aliveEnemies.Count <= 0)
        {
            Debug.Log("<color=cyan>방 클리어!</color> 다음 지역 포탈 활성화");
            if (exitPortal != null) exitPortal.SetActive(true);
        }
    }
}