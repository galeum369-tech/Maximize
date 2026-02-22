using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // 이제 인스펙터에서 직접 안 넣어도 됨 (테마 데이터에서 가져올 거니까 HideInInspector 처리해도 됨)
    [Header("Map Prefabs (Auto Loaded)")]
    private MapPiece startMapPrefab;
    private MapPiece endMapPrefab;
    private List<MapPiece> battleMapPrefabs;

    [Header("Generation Settings")]
    public int minBattleMaps = 2; // 최소 전투 맵 개수
    public int maxBattleMaps = 5; // 최대 전투 맵 개수

    private void Start()
    {
        // 1. GameManager에서 현재 선택된 테마를 가져옴
        ThemeStageData currentTheme = GameManager.Instance.selectedTheme;

        if (currentTheme != null)
        {
            // 2. 맵 프리팹들을 현재 테마의 던전 프리팹으로 덮어씌움
            startMapPrefab = currentTheme.startMap;
            endMapPrefab = currentTheme.endMap;
            battleMapPrefabs = currentTheme.battleMaps;
        }
        else
        {
            Debug.LogError("선택된 테마가 없어! GameManager를 확인해.");
            return;
        }

        // 3. 테마에 맞는 던전 생성 시작
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        Vector3 currentPosition = Vector3.zero;

        // 시작 맵 생성
        MapPiece startMap = Instantiate(startMapPrefab, currentPosition, Quaternion.identity);
        currentPosition = startMap.nextPoint.position;

        // 무작위 맵 개수 결정
        int totalBattleMaps = Random.Range(minBattleMaps, maxBattleMaps + 1);

        // 전투 맵 랜덤 생성 및 연결
        for (int i = 0; i < totalBattleMaps; i++)
        {
            MapPiece selectedPrefab = battleMapPrefabs[Random.Range(0, battleMapPrefabs.Count)];
            MapPiece newMap = Instantiate(selectedPrefab, currentPosition, Quaternion.identity);

            // 몬스터 스폰 초기화
            newMap.Initialize();

            currentPosition = newMap.nextPoint.position;
        }

        // 종료 맵 생성
        Instantiate(endMapPrefab, currentPosition, Quaternion.identity);
    }
}