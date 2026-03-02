using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalEventSystem : MonoBehaviour
{
    private static GlobalEventSystem instance;

    private void Awake()
    {
        // 이미 다른 씬에서 넘어온 이벤트 시스템이 있다면?
        if (instance != null && instance != this)
        {
            // 나는 짝퉁이므로 사라진다 (중복 방지)
            Destroy(gameObject);
            return;
        }

        // 내가 최초라면? 나를 전역으로 등록!
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}