using UnityEngine;
using UnityEditor; // 에디터 기능을 쓰기 위해 필요

public class ItemDataBatchCreator : EditorWindow
{
    [MenuItem("Tools/아이템 데이터 10개 생성")] // 유니티 상단 메뉴바에 생김
    public static void CreateMultipleItems()
    {
        // 파일이 저장될 폴더 경로 (없으면 미리 만들어둬야 함)
        string folderPath = "Assets/Data/Items";

        // 폴더가 없으면 생성
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Items");
        }

        for (int i = 1; i <= 10; i++)
        {
            // ScriptableObject 인스턴스 생성
            ItemData newItem = ScriptableObject.CreateInstance<ItemData>();
            newItem.itemName = $"새 아이템 {i}";
            newItem.id = 1000 + i;

            // 실제 파일(.asset)로 저장
            string fullPath = $"{folderPath}/NewItem_{i}.asset";
            AssetDatabase.CreateAsset(newItem, fullPath);
        }

        AssetDatabase.SaveAssets(); // 변경사항 저장
        AssetDatabase.Refresh();    // 에디터 갱신
        Debug.Log("아이템 데이터 10개 생성 완료!");
    }
}