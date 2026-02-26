using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 코루틴을 사용하기 위해 추가

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    private PlayerInputHandler currentInput;

    [Header("퀵슬롯 데이터 (3개)")]
    public ItemSlot[] quickSlots = new ItemSlot[3];

    [Header("UI 요소")]
    public Image[] iconImages;
    public TextMeshProUGUI[] countTexts;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < 3; i++) quickSlots[i] = new ItemSlot();
    }

    public void SetInputHandler(PlayerInputHandler newInput)
    {
        if (currentInput != null)
        {
            currentInput.OnUseItem1 -= UseItem1;
            currentInput.OnUseItem2 -= UseItem2;
            currentInput.OnUseItem3 -= UseItem3;
        }

        currentInput = newInput;

        if (currentInput != null)
        {
            currentInput.OnUseItem1 += UseItem1;
            currentInput.OnUseItem2 += UseItem2;
            currentInput.OnUseItem3 += UseItem3;
        }
    }

    private void UseItem1() => UseItem(0);
    private void UseItem2() => UseItem(1);
    private void UseItem3() => UseItem(2);

    private void UseItem(int index)
    {
        var slot = quickSlots[index];
        if (slot.item != null && slot.item is ItemData consumable)
        {
            // 현재 메카인지 인간인지 확인
            bool isMecha = PlayerTransformManager.Instance.IsMechaMode;

            // 아이템 타입에 따라 효과 분기 처리
            switch (consumable.consumableType)
            {
                case ConsumableType.HealHP:
                    ApplyHealHP(consumable, isMecha);
                    break;
                case ConsumableType.HealEnergy:
                    ApplyHealEnergy(consumable, isMecha);
                    break;
                case ConsumableType.Buff:
                    // 버프는 코루틴으로 실행하여 지속시간 후 해제되도록 함
                    StartCoroutine(BuffCoroutine(consumable, isMecha));
                    break;
            }

            // 공통 로직: 개수 차감 및 슬롯 비우기
            slot.count--;
            if (slot.count <= 0) slot.Clear();

            RefreshQuickSlotUI();

            // 인벤토리가 열려있다면 즉시 갱신
            if (InventoryUI.Instance != null && InventoryUI.Instance.isOpen)
                InventoryUI.Instance.RefreshUI();
        }
    }

    // --- 개별 효과 적용 함수들 ---

    private void ApplyHealHP(ItemData data, bool isMecha)
    {
        Debug.Log($"{data.itemName} 사용! 체력 {data.effectValue} 회복!");

        if (isMecha)
        {
            var mState = PlayerTransformManager.Instance.mechaObject.GetComponent<MechaState>();
            // 최대 체력을 넘지 않도록 Min으로 제한
            mState.currentHp = Mathf.Min(mState.currentHp + data.effectValue, mState.FinalMaxHP);
            UIManager.Instance?.UpdateHP(mState.currentHp, mState.FinalMaxHP);
        }
        else
        {
            var pState = PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>();
            pState.currentHp = Mathf.Min(pState.currentHp + data.effectValue, pState.FinalMaxHP);
            UIManager.Instance?.UpdateHP(pState.currentHp, pState.FinalMaxHP);
        }
    }

    private void ApplyHealEnergy(ItemData data, bool isMecha)
    {
        Debug.Log($"{data.itemName} 사용! 에너지 {data.effectValue} 회복!");
        // 차후 에너지(EP/SP) 시스템 연동 시 여기에 로직 추가
    }

    // 버프 지속을 처리하는 코루틴
    private IEnumerator BuffCoroutine(ItemData data, bool isMecha)
    {
        Debug.Log($"{data.itemName} 사용! {data.buffStatType} 스탯이 {data.effectDuration}초 동안 {data.effectValue} 증가!");

        // 1. 버프 수치 적용 및 스탯 강제 갱신
        ApplyBuffValue(data.buffStatType, data.effectValue, isMecha);
        InventoryManager.Instance.ForceStatUpdate();

        // 2. 지속 시간(초) 만큼 대기
        yield return new WaitForSeconds(data.effectDuration);

        // 3. 시간이 지나면 버프 수치를 다시 빼서 원상복구
        Debug.Log($"{data.itemName} 버프 종료!");
        ApplyBuffValue(data.buffStatType, -data.effectValue, isMecha);
        InventoryManager.Instance.ForceStatUpdate();
    }

    // 대상 폼에 맞춰 실제 버프 변수 증감 처리
    private void ApplyBuffValue(BuffStatType buffType, float amount, bool isMecha)
    {
        if (isMecha)
        {
            var mState = PlayerTransformManager.Instance.mechaObject.GetComponent<MechaState>();
            if (mState == null) return;

            switch (buffType)
            {
                case BuffStatType.Atk: mState.buffAtk += amount; break;
                case BuffStatType.Def: mState.buffDef += amount; break;
                case BuffStatType.Spd: mState.buffSpd += amount; break;
            }
        }
        else
        {
            var pState = PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>();
            if (pState == null) return;

            switch (buffType)
            {
                case BuffStatType.Atk: pState.buffAtk += amount; break;
                case BuffStatType.Def: pState.buffDef += amount; break;
                case BuffStatType.Spd: pState.buffSpd += amount; break;
            }
        }
    }

    public void RefreshQuickSlotUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (quickSlots[i].item != null && quickSlots[i].count > 0)
            {
                iconImages[i].sprite = quickSlots[i].item.icon;
                iconImages[i].enabled = true;
                countTexts[i].text = quickSlots[i].count > 1 ? quickSlots[i].count.ToString() : "";
            }
            else
            {
                iconImages[i].enabled = false;
                countTexts[i].text = "";
            }
        }
    }

    // [핵심 1] 인벤토리에서 퀵슬롯으로 장착
    public void EquipToQuickSlot(int index, ItemSlot bagSlot)
    {
        if (quickSlots[index].item != null)
        {
            ItemSlot temp = new ItemSlot();
            temp.item = quickSlots[index].item;
            temp.count = quickSlots[index].count;

            quickSlots[index].item = bagSlot.item;
            quickSlots[index].count = bagSlot.count;

            bagSlot.item = temp.item;
            bagSlot.count = temp.count;
        }
        else
        {
            quickSlots[index].item = bagSlot.item;
            quickSlots[index].count = bagSlot.count;
            bagSlot.Clear();
        }
        RefreshQuickSlotUI();
    }

    // [핵심 2] 퀵슬롯 해제 시 가방으로 반환
    public void UnequipQuickSlot(int index)
    {
        if (quickSlots[index].item != null)
        {
            bool success = InventoryManager.Instance.AddItem(quickSlots[index].item, quickSlots[index].count);
            if (success)
            {
                quickSlots[index].Clear();
                RefreshQuickSlotUI();
                Debug.Log($"퀵슬롯 {index + 1}번 해제! 인벤토리로 돌아감.");
            }
            else
            {
                Debug.Log("가방이 꽉 차서 퀵슬롯을 해제할 수 없어!");
            }
        }
    }

    // [핵심 3] 아이템 획득 시 퀵슬롯 우선 보충
    public int AddToQuickSlotFirst(ItemData data, int amount)
    {
        for (int i = 0; i < 3; i++)
        {
            if (quickSlots[i].item == data && quickSlots[i].count < data.GetMaxStack())
            {
                int canAdd = data.GetMaxStack() - quickSlots[i].count;
                int toAdd = Mathf.Min(canAdd, amount);
                quickSlots[i].count += toAdd;
                amount -= toAdd;

                RefreshQuickSlotUI();
                if (amount <= 0) break;
            }
        }
        return amount;
    }
}