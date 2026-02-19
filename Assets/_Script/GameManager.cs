using UnityEngine;

public enum GameState { Lobby, Dungeon, Pause, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public GameState currentState;

    [Header("데이터 보관")]
    public Vector3 savedFieldPosition { get; private set; }
    public bool hasSavedPosition { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 위치 저장 함수
    public void SavePosition(Vector3 pos)
    {
        savedFieldPosition = pos;
        hasSavedPosition = true;
    }

    // 위치 데이터 초기화 함수
    public void ClearSavedPosition()
    {
        hasSavedPosition = false;
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        // ... (이전과 동일한 switch문)
    }
}