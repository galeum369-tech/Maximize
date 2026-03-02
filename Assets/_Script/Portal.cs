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
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;

    private PlayerInputHandler playerInput;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                playerInput.OnInteract += EnterPortal;
                onPlayerEnter?.Invoke();
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
            onPlayerExit?.Invoke();
        }
    }

    // [핵심 해결책] 포탈도 추가!
    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnInteract -= EnterPortal;
            playerInput = null;
        }
    }

    private void EnterPortal()
    {
        if (playerInput != null) playerInput.OnInteract -= EnterPortal;
        onPlayerExit?.Invoke();

        if (portalType == PortalType.ToDungeon)
        {
            if (GameManager.Instance != null && GameManager.Instance.GetActivePlayer() != null)
            {
                GameManager.Instance.SavePosition(GameManager.Instance.GetActivePlayer().position);
            }
        }

        SceneControlManager.Instance.LoadTargetScene(targetSceneName, targetState);
    }
}