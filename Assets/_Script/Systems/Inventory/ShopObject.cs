using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ShopObject : MonoBehaviour
{
    [Header("UI 연출")]
    public UnityEvent onPlayerEnter; // "F키를 눌러 상점 열기"
    public UnityEvent onPlayerExit;

    private PlayerInputHandler playerInput;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                playerInput.OnInteract += OpenShop;
                onPlayerEnter?.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInput != null)
        {
            playerInput.OnInteract -= OpenShop;
            playerInput = null;
            onPlayerExit?.Invoke();
        }
    }

    private void OpenShop()
    {
        Debug.Log("상점 UI 오픈!");
        if (InventoryUI.Instance != null)
        {
            // 인벤토리를 상점 모드로 열기
            InventoryUI.Instance.OpenShopUI();
        }
    }
}