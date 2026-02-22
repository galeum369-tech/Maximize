using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public ThemeStageData currentTheme; // 현재 진행중인 테마 정보

    void Start()
    {
        // [수정] 주석 해제! GameManager에서 현재 선택된 테마를 무조건 받아와야 함
        currentTheme = GameManager.Instance.selectedTheme;

        // 테마 데이터가 비어있으면 경고 띄우고 정지 (에러 방지용)
        if (currentTheme == null)
        {
            Debug.LogError("[WorldGenerator] 선택된 테마가 없습니다! GameManager를 확인하세요.");
            return;
        }

        // GameManager의 현재 상태를 보고 알맞은 맵을 생성
        if (GameManager.Instance.currentState == GameState.Dungeon)
        {
            Debug.Log("던전 맵 생성 시작!");
            GenerateDungeon();
        }
        else if (GameManager.Instance.currentState == GameState.Field)
        {
            Debug.Log("필드 맵 생성 시작!");
            GenerateField();
        }
    }

    void GenerateField()
    {
        if (currentTheme.fieldPrefab != null)
        {
            // 필드는 단순히 테마에 지정된 큰 맵 하나를 생성
            Instantiate(currentTheme.fieldPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log($"{currentTheme.themeName} 필드 생성 완료!");
        }
        else
        {
            Debug.LogError("ThemeStageData에 Field Prefab이 등록되지 않았습니다!");
        }
    }

    void GenerateDungeon()
    {
        // MapGenerator가 같은 씬에 있다면 알아서 작동할 것이므로, 
        // 여기서 직접 MapGenerator 컴포넌트를 찾아서 실행시켜주는 것도 좋음.
        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        if (mapGen != null)
        {
            // MapGenerator의 Start()에서 자동으로 생성하도록 두거나, 여기서 수동 호출
            // mapGen.GenerateDungeon(); 
        }
    }
}