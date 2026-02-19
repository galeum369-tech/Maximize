using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum PortalType { ToVillage, ToDungeon, ToField }

    [Header("설정")]
    public PortalType portalType;
    public string targetSceneName;
    public GameState targetState;

    private PlayerInputHandler playerInput;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                playerInput.OnInteract += EnterPortal;
                // UI로 포탈 종류에 따른 텍스트 표시 가능 (예: "마을로 귀환", "던전 진입")
                Debug.Log($"{portalType} 상호작용 가능");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInput != null)
        {
            playerInput.OnInteract -= EnterPortal;
            playerInput = null;
        }
    }

    private void EnterPortal()
    {
        if (playerInput != null) playerInput.OnInteract -= EnterPortal;

        // 던전으로 갈 때만 현재 위치를 기억해둠
        if (portalType == PortalType.ToDungeon)
        {
            GameManager.Instance.SavePosition(playerInput.transform.position);
        }

        SceneControlManager.Instance.LoadTargetScene(targetSceneName, targetState);
    }
}