using UnityEngine;

public class DroneState : MonoBehaviour
{
    public PlayerData data; // 인스펙터에서 연결

    [Header("드론 스탯")]
    public float currentAtk;

    [Header("스킬 쿨타임 관리")]
    public float skill1Cooldown = 5f; // 연사 스킬
    public float skill2Cooldown = 10f; // 거대 폭탄

    [HideInInspector] public float skill1Timer;
    [HideInInspector] public float skill2Timer;

    private void OnEnable() => RefreshStats();

    public void RefreshStats()
    {
        if (data != null) currentAtk = data.GetDroneAtk(); // PlayerData에서 계산된 공격력 가져옴
    }

    private void Update()
    {
        if (skill1Timer > 0) skill1Timer -= Time.deltaTime;
        if (skill2Timer > 0) skill2Timer -= Time.deltaTime;
    }
}