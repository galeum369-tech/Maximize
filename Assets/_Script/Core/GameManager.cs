using UnityEngine;
using System;

// 게임의 전체 상태 정의
public enum GameState { MainMenu, Village, Field, Dungeon, Pause, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태 관리")]
    public GameState currentState;
    public event Action<GameState> OnStateChanged; // 상태 변경 시 UI나 시스템에 알림

    [Header("월드/스테이지 데이터")]
    public ThemeStageData selectedTheme; // 현재 선택된 필드/던전 테마 (SO)
    public Vector3 savedFieldPosition;   // 던전 진입 전 필드 위치 기억
    public bool hasSavedPosition;

    [Header("플레이어 참조 (자석 효과용)")]
    private Transform activePlayerTransform; // 현재 활성화된 (인간 or 메카) 트랜스폼

    [Header("재화 관리")]
    public int currentMoney;
    public event Action<int> OnMoneyChanged;

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

    // --- [플레이어 관리] ---

    // 변신 시나 씬 로드 시 현재 조종 중인 플레이어를 등록 (LootItem이 참조함)
    public void RegisterPlayer(Transform player)
    {
        activePlayerTransform = player;
    }

    public Transform GetActivePlayer() => activePlayerTransform;

    // --- [위치 및 테마 관리] ---

    public void SetTheme(ThemeStageData theme)
    {
        selectedTheme = theme;
    }

    public void SavePosition(Vector3 pos)
    {
        savedFieldPosition = pos;
        hasSavedPosition = true;
        Debug.Log($"[GameManager] 필드 위치 저장됨: {pos}");
    }

    public void ClearSavedPosition()
    {
        hasSavedPosition = false;
    }

    // --- [상태 제어] ---

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // 상태별 시간 정지 등 공통 로직 처리
        Time.timeScale = (newState == GameState.Pause) ? 0f : 1f;

        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] 상태 변경: {newState}");
    }

    // --- [재화 제어] ---

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool UseMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }
}