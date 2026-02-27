using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ShopObject : MonoBehaviour
{
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

    // [핵심 추가] 씬이 넘어가거나 오브젝트가 꺼질 때 강제로 구독 해제!
    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnInteract -= OpenShop;
            playerInput = null;
        }
    }

    private void OpenShop()
    {
        Debug.Log("상점 UI 오픈!");
        if (InventoryUI.Instance != null) InventoryUI.Instance.OpenShopUI();
    }
}