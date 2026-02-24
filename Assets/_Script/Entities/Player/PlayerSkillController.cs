using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public static PlayerSkillController Instance { get; private set; }

    [Header("데이터 참조")]
    public PlayerData playerData;

    [Header("실행기 참조 (인스펙터 할당)")]
    public SubDrone droneScript;
    public Mech mechScript;

    [Header("폼 상태")]
    public bool isMechaForm = false;
    private float currentMechaEnergy = 0f;

    [Header("스킬 쿨타임 (0:스킬1, 1:스킬2, 2:변신)")]
    public float[] humanMaxCooldowns = { 5f, 10f, 1f };
    private float[] humanCurrentCooldowns = { 0f, 0f, 0f };

    public float[] mechaMaxCooldowns = { 5f, 12f, 1f };
    private float[] mechaCurrentCooldowns = { 0f, 0f, 0f };

    [Header("에너지 소모량(초당)")]
    public float lessEnerge = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SwapSkillForm(false);
            UpdateCooldownUI();
            UpdateEnergyUI();
        }
    }

    private void Update()
    {
        // 1. 쿨타임 감소 (백그라운드 처리)
        ProcessCooldowns(humanCurrentCooldowns);
        ProcessCooldowns(mechaCurrentCooldowns);

        // 2. UI 지속 업데이트
        UpdateCooldownUI();

        // 3. 메카 폼일 때 에너지 지속 감소 로직 추가
        if (isMechaForm)
        {
            // 1초에 1씩 감소 (Time.deltaTime은 1프레임당 걸린 시간)
            currentMechaEnergy -= lessEnerge * Time.deltaTime;

            // 감소하는 에너지를 UI에 실시간 반영
            UpdateEnergyUI();

            // 에너지가 바닥나면 강제로 인간 폼으로 복귀
            if (currentMechaEnergy <= 0f)
            {
                currentMechaEnergy = 0f;
                ForceRevertToHuman();
            }
        }

        // 🛠️ [테스트용 치트키] 키보드 숫자 1 누르면 게이지 MAX
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!isMechaForm)
            {
                currentMechaEnergy = playerData.mechaMaxEnergy;
                UpdateEnergyUI();
                Debug.Log("🛠️ [치트] 메카닉 변신 게이지 MAX!");
            }
        }
    }

    public void AddMechaEnergy()
    {
        if (isMechaForm) return;

        currentMechaEnergy += playerData.GetEnergyGainPerHit();
        if (currentMechaEnergy > playerData.mechaMaxEnergy)
            currentMechaEnergy = playerData.mechaMaxEnergy;

        UpdateEnergyUI();
    }

    private void ProcessCooldowns(float[] cooldowns)
    {
        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (cooldowns[i] > 0) cooldowns[i] -= Time.deltaTime;
        }
    }

    private void UpdateCooldownUI()
    {
        if (UIManager.Instance == null) return;

        if (isMechaForm)
        {
            UIManager.Instance.UpdateAllSkillCooldowns(mechaCurrentCooldowns, mechaMaxCooldowns);
        }
        else
        {
            float[] displayCurrent = new float[3];
            float[] displayMax = new float[3];

            displayCurrent[0] = humanCurrentCooldowns[0]; displayMax[0] = humanMaxCooldowns[0];
            displayCurrent[1] = humanCurrentCooldowns[1]; displayMax[1] = humanMaxCooldowns[1];

            displayMax[2] = playerData.mechaMaxEnergy;
            displayCurrent[2] = playerData.mechaMaxEnergy - currentMechaEnergy;

            UIManager.Instance.UpdateAllSkillCooldowns(displayCurrent, displayMax);
        }
    }

    private void UpdateEnergyUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMechaEnergy(currentMechaEnergy, playerData.mechaMaxEnergy);
    }

    public void TryUseSkill(int slotIndex)
    {
        float[] currentCooldowns = isMechaForm ? mechaCurrentCooldowns : humanCurrentCooldowns;
        float[] maxCooldowns = isMechaForm ? mechaMaxCooldowns : humanMaxCooldowns;

        if (currentCooldowns[slotIndex] <= 0f)
        {
            bool isSuccess = false;

            if (isMechaForm)
            {
                if (slotIndex == 0) isSuccess = mechScript.ExecuteSkill1();
                else if (slotIndex == 1) isSuccess = mechScript.ExecuteSkill2();
            }
            else
            {
                if (slotIndex == 0) isSuccess = droneScript.ExecuteSkill1();
                else if (slotIndex == 1) isSuccess = droneScript.ExecuteSkill2();
            }

            if (isSuccess)
            {
                currentCooldowns[slotIndex] = maxCooldowns[slotIndex];
            }
        }
    }

    public void TryTransform()
    {
        int transformSlot = 2;

        if (!isMechaForm)
        {
            // 인간 -> 메카
            if (currentMechaEnergy >= playerData.mechaMaxEnergy)
            {
                // 변신할 때 에너지를 0으로 만들지 않고 유지함 (그래야 메카 상태에서 깎임)
                isMechaForm = true;

                UIManager.Instance.SwapSkillForm(true);
                UpdateEnergyUI();
                PlayerTransformManager.Instance.ToMecha();
            }
        }
        else
        {
            // 메카 -> 인간 (수동 탈출)
            if (mechaCurrentCooldowns[transformSlot] <= 0f)
            {
                mechaCurrentCooldowns[transformSlot] = mechaMaxCooldowns[transformSlot];
                ForceRevertToHuman(); // 중복 코드 방지를 위해 함수로 분리
            }
        }
    }

    // 에너지가 다 떨어지거나 수동으로 탈출할 때 호출되는 강제 해제 로직
    private void ForceRevertToHuman()
    {
        isMechaForm = false;

        // 인간으로 돌아와도 게이지 유지
        //currentMechaEnergy = 0f;
        UpdateEnergyUI();

        UIManager.Instance.SwapSkillForm(false);
        PlayerTransformManager.Instance.ToHuman();

        Debug.Log("메카닉 해제! 파일럿 복귀 완료.");
    }
}