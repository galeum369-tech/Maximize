using UnityEngine;

public class AreaSpawner : MonoBehaviour
{
    public GameObject[] prefabs; // 스폰할 프리팹들
    public int minCount = 2;
    public int maxCount = 5;
    public Vector2 areaSize = new Vector2(5, 5); // 스폰 영역 크기

    public void Spawn()
    {
        int count = Random.Range(minCount, maxCount + 1);
        for (int i = 0; i < count; i++)
        {
            // 영역 내 랜덤 좌표 계산
            float rx = Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float ry = Random.Range(-areaSize.y / 2, areaSize.y / 2);
            Vector3 spawnPos = transform.position + new Vector3(rx, ry, 0);

            Instantiate(prefabs[Random.Range(0, prefabs.Length)], spawnPos, Quaternion.identity, transform);
        }
    }

    // 에디터에서 영역을 보여줌
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0));
    }
}