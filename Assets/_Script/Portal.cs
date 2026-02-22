using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    public enum PortalType { ToVillage, ToDungeon, ToField }

    [Header("설정")]
    public PortalType portalType;
    public string targetSceneName;
    public GameState targetState;

    [Header("UI 연출")]
    public UnityEvent onPlayerEnter; // "F키를 눌러 이동" 메시지 켜기
    public UnityEvent onPlayerExit;  // 메시지 끄기

    private PlayerInputHandler playerInput;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                playerInput.OnInteract += EnterPortal;
                onPlayerEnter?.Invoke(); // UI 표시
                Debug.Log($"{portalType} 진입: F키로 상호작용 가능");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInput != null)
        {
            playerInput.OnInteract -= EnterPortal;
            playerInput = null;
            onPlayerExit?.Invoke(); // UI 숨기기
        }
    }

    private void EnterPortal()
    {
        if (playerInput != null) playerInput.OnInteract -= EnterPortal;
        onPlayerExit?.Invoke();

        // 던전으로 갈 때만 현재 위치를 기억 (마을 복귀용)
        if (portalType == PortalType.ToDungeon)
        {
            GameManager.Instance.SavePosition(playerInput.transform.position);
        }

        SceneControlManager.Instance.LoadTargetScene(targetSceneName, targetState);
    }
}