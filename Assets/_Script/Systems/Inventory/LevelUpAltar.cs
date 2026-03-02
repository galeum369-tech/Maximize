using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class LevelUpAltar : MonoBehaviour
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
                playerInput.OnInteract += OpenAltar;
                onPlayerEnter?.Invoke();
                Debug.Log("레벨업 제단 접근: F키로 상호작용 가능");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInput != null)
        {
            playerInput.OnInteract -= OpenAltar;
            playerInput = null;
            onPlayerExit?.Invoke();
        }
    }

    // [핵심 해결책] 제단도 추가!
    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnInteract -= OpenAltar;
            playerInput = null;
        }
    }

    private void OpenAltar()
    {
        Debug.Log("레벨업 창 오픈!");
        if (LevelUpUI.Instance != null) LevelUpUI.Instance.OpenUI();
    }
}