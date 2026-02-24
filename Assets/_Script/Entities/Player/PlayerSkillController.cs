using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Header("데이터 참조")]
    public PlayerData playerData; // 플레이어 데이터 (에너지 최대치 등)

    [Header("폼 상태")]
    public bool isMechaForm = false;
    private float currentMechaEnergy = 0f; // 현재 모인 변신 게이지

    [Header("스킬 쿨타임 (0:스킬1, 1:스킬2, 2:변신)")]
    public float[] humanMaxCooldowns = { 3f, 8f, 1f }; // 변신(2번) 쿨타임은 UI 연출용으로 안 씀, 기본 1초 등 짧게 세팅
    private float[] humanCurrentCooldowns = { 0f, 0f, 0f };

    public float[] mechaMaxCooldowns = { 5f, 12f, 1f }; // 메카에서 인간으로 내리는 쿨타임 (언제든 내릴 수 있게 1초로 짧게 세팅)
    private float[] mechaCurrentCooldowns = { 0f, 0f, 0f };

    private void Start()
    {
        // 시작 시 UI 초기화
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

        // 3. 플레이어 입력 체크
        HandleInput();
    }

    // 적 타격 시 호출 (UniversalHitbox 등에서)
    public void AddMechaEnergy()
    {
        if (isMechaForm) return;

        float gain = playerData.GetEnergyGainPerHit();
        currentMechaEnergy += gain;

        if (currentMechaEnergy > playerData.mechaMaxEnergy)
        {
            currentMechaEnergy = playerData.mechaMaxEnergy;
        }

        UpdateEnergyUI();
    }

    // ==========================================
    // [핵심 변경점] 쿨타임 UI 업데이트 로직
    // ==========================================
    private void UpdateCooldownUI()
    {
        if (UIManager.Instance == null) return;

        if (isMechaForm)
        {
            // 메카 폼일 때는 일반적인 쿨타임 그대로 전달
            UIManager.Instance.UpdateAllSkillCooldowns(mechaCurrentCooldowns, mechaMaxCooldowns);
        }
        else
        {
            // 인간 폼일 때: 1, 2번 슬롯은 쿨타임, 3번(변신) 슬롯은 게이지 기반으로 계산해서 전달
            float[] displayCurrent = new float[3];
            float[] displayMax = new float[3];

            // 1번, 2번 스킬은 기존 쿨타임 그대로
            displayCurrent[0] = humanCurrentCooldowns[0]; displayMax[0] = humanMaxCooldowns[0];
            displayCurrent[1] = humanCurrentCooldowns[1]; displayMax[1] = humanMaxCooldowns[1];

            // 3번 변신 스킬: 게이지가 0이면 가림막 100%, 게이지가 꽉 차면 가림막 0% (스킬 사용 가능 연출)
            displayMax[2] = playerData.mechaMaxEnergy;
            displayCurrent[2] = playerData.mechaMaxEnergy - currentMechaEnergy; // 부족한 에너지를 '남은 쿨타임'처럼 취급

            UIManager.Instance.UpdateAllSkillCooldowns(displayCurrent, displayMax);
        }
    }

    private void UpdateEnergyUI()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateMechaEnergy(currentMechaEnergy, playerData.mechaMaxEnergy);
    }

    private void ProcessCooldowns(float[] cooldowns)
    {
        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (cooldowns[i] > 0) cooldowns[i] -= Time.deltaTime;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.W)) TryUseSkill(1);
        if (Input.GetKeyDown(KeyCode.E)) TryTransform();
    }

    private void TryUseSkill(int slotIndex)
    {
        float[] currentCooldowns = isMechaForm ? mechaCurrentCooldowns : humanCurrentCooldowns;
        float[] maxCooldowns = isMechaForm ? mechaMaxCooldowns : humanMaxCooldowns;

        if (currentCooldowns[slotIndex] <= 0f)
        {
            currentCooldowns[slotIndex] = maxCooldowns[slotIndex];

            if (isMechaForm)
            {
                Debug.Log($"메카 스킬 {slotIndex} 사용!");
                // TODO: GetComponent<MechaState>().ChangeState(...) 호출
            }
            else
            {
                Debug.Log($"인간 스킬 {slotIndex} 사용!");
                // TODO: GetComponent<PlayerState>().ChangeState(...) 호출
            }
        }
    }

    private void TryTransform()
    {
        int transformSlot = 2;

        if (!isMechaForm)
        {
            // [인간 -> 메카] 에너지가 꽉 찼는지 확인
            if (currentMechaEnergy >= playerData.mechaMaxEnergy)
            {
                currentMechaEnergy = 0f; // 에너지 초기화

                isMechaForm = true;
                UIManager.Instance.SwapSkillForm(true);
                UpdateEnergyUI();

                Debug.Log("메카닉 폼 체인지 완료!");
                // TODO: FSM 교체 및 모델링 변경 로직
            }
        }
        else
        {
            // [메카 -> 인간] 언제든 쿨타임만 아니면 내릴 수 있음
            if (mechaCurrentCooldowns[transformSlot] <= 0f)
            {
                mechaCurrentCooldowns[transformSlot] = mechaMaxCooldowns[transformSlot];

                isMechaForm = false;
                UIManager.Instance.SwapSkillForm(false);

                Debug.Log("인간 폼으로 복귀!");
                // TODO: FSM 교체 및 모델링 변경 로직
            }
        }
    }
}