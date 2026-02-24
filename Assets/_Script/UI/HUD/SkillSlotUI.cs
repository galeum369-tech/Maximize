using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI 연결 (화면에 그릴 컴포넌트)")]
    public Image iconImage; // 껍데기 역할을 할 실제 UI Image 컴포넌트
    public Image cooldownOverlay; // 쿨타임 가림막 (Radial 360)

    [Header("스킬 아이콘 세팅 (인스펙터에서 할당)")]
    public Sprite humanSkillIcon; // 인간 폼일 때 쓸 이미지
    public Sprite mechaSkillIcon; // 메카 폼일 때 쓸 이미지

    // 폼이 바뀔 때 외부(매니저)에서 호출해줄 함수
    public void ChangeForm(bool isMecha)
    {
        // 삼항 연산자로 메카면 메카 이미지, 아니면 인간 이미지로 iconImage.sprite를 교체
        iconImage.sprite = isMecha ? mechaSkillIcon : humanSkillIcon;
    }

    // 쿨타임은 외부(캐릭터 FSM이나 스킬 매니저)에서 진짜 데이터를 넘겨주면 UI만 갱신
    public void UpdateCooldownUI(float currentCooldown, float maxCooldown)
    {
        if (currentCooldown > 0f)
        {
            // 남은 쿨타임 비율에 맞춰 가림막 채우기
            cooldownOverlay.fillAmount = currentCooldown / maxCooldown;
        }
        else
        {
            // 쿨타임이 없으면 가림막 없애기
            cooldownOverlay.fillAmount = 0f;
        }
    }
}