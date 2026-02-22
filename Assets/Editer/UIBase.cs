using UnityEngine;
using TMPro; // TextMeshPro용

public class UIBase : MonoBehaviour
{
    [Header("자동 연결될 UI들")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI levelText;

    // 인스펙터에서 버튼 하나 누르면 자동으로 찾게 만들 수도 있어
    [ContextMenu("UI 자동 연결 시작")]
    public void BindUI()
    {
        // 자식 오브젝트들 중에서 이름이 일치하는 걸 찾아와
        // 규칙: 하이어라키에서 이름이 "Text_HP", "Text_Money" 이런 식이어야 함
        hpText = FindComponentByName<TextMeshProUGUI>("Text_HP");
        moneyText = FindComponentByName<TextMeshProUGUI>("Text_Money");
        levelText = FindComponentByName<TextMeshProUGUI>("Text_Level");

        Debug.Log("UI 바인딩 완료!");
    }

    private T FindComponentByName<T>(string objName) where T : Component
    {
        // 내 자식들 중에서 해당 이름의 오브젝트를 찾아서 컴포넌트 반환
        Transform target = transform.Find(objName);
        if (target != null) return target.GetComponent<T>();

        // 못 찾으면 전체 자식 탐색 (깊은 곳까지)
        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objName) return child.GetComponent<T>();
        }

        Debug.LogWarning($"{objName}을(를) 찾을 수 없습니다.");
        return null;
    }
}