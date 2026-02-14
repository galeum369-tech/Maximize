using UnityEngine;

// 상속을 위해 추상 클래스로 선언해두면 좋아
public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea] public string description;
    public int price; // 상점 판매가 등
}