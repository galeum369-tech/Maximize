using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class StorageObject : MonoBehaviour
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

    // [핵심 해결책] 창고도 똑같이 추가!
    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnInteract -= OpenStorage;
            playerInput = null;
        }
    }

    private void OpenStorage()
    {
        Debug.Log("창고 UI 오픈!");
        if (InventoryUI.Instance != null) InventoryUI.Instance.OpenStorageUI();
    }
}