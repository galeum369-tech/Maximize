using UnityEngine;
using UnityEditor;

public class MapPieceEditorHelper
{
    // 하이어라키에서 오브젝트 우클릭 메뉴에 추가
    [MenuItem("GameObject/🛠️ Quick Setup/Map Piece 보정", false, 0)]
    public static void SetupMapPiece()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null) return;

        // 1. MapPiece 스크립트 없으면 추가
        if (!selected.GetComponent<MapPiece>())
        {
            selected.AddComponent<MapPiece>();
        }

        // 2. NextPoint(출구) 자동 생성 및 연결
        Transform nextPoint = selected.transform.Find("NextPoint");
        if (nextPoint == null)
        {
            GameObject npObj = new GameObject("NextPoint");
            npObj.transform.SetParent(selected.transform);
            npObj.transform.localPosition = new Vector3(20, 0, 0); // 기본값으로 오른쪽 20칸
            nextPoint = npObj.transform;
        }

        selected.GetComponent<MapPiece>().nextPoint = nextPoint;

        // 3. Enemy Spawn Group 오브젝트 생성
        if (selected.transform.Find("EnemySpawns") == null)
        {
            new GameObject("EnemySpawns").transform.SetParent(selected.transform);
        }

        Selection.activeGameObject = selected;
        Debug.Log($"{selected.name} 맵 피스 기본 세팅 완료!");
    }
}