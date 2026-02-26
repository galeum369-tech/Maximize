using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class StorageObject : MonoBehaviour
{
    [Header("UI 연출")]
    public UnityEvent onPlayerEnter; // "F키를 눌러 창고 열기" 텍스트 띄우기용
    public UnityEvent onPlayerExit;

    private PlayerInputHandler playerInput;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                playerInput.OnInteract += OpenStorage;
                onPlayerEnter?.Invoke();
                Debug.Log("창고 접근: F키로 열기 가능");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInput != null)
        {
            playerInput.OnInteract -= OpenStorage;
            playerInput = null;
            onPlayerExit?.Invoke();
        }
    }

    private void OpenStorage()
    {
        // 상호작용 성공 시 인벤토리를 창고 모드로 염!
        Debug.Log("창고 UI 오픈!");
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.OpenStorageUI();
        }
    }
}