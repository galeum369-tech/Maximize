using UnityEngine;

public class PlayerAnimController
{
    Animator anim;

    // 기존 해시값 유지 및 신규 추가
    readonly int hashMove = Animator.StringToHash("Move");
    readonly int hashAttack = Animator.StringToHash("Attack");
    readonly int hashComboIndex = Animator.StringToHash("ComboIndex");
    readonly int hashDeath = Animator.StringToHash("Death");
    readonly int hashSkillA = Animator.StringToHash("SkillA");
    readonly int hashSkillS = Animator.StringToHash("SkillS");

    // 공중 동작용 신규 해시값
    readonly int hashGrounded = Animator.StringToHash("isGrounded");
    readonly int hashYVelocity = Animator.StringToHash("yVelocity");

    public PlayerAnimController(Animator anim)
    {
        this.anim = anim;
    }

    // --- 공중 동작 제어 (핵심) ---
    public void UpdateAerialState(bool isGrounded, float yVelocity)
    {
        anim.SetBool(hashGrounded, isGrounded);
        anim.SetFloat(hashYVelocity, yVelocity);
    }

    // --- 기존 동작들 ---
    public void PlayMove(bool isMoving)
    {
        anim.SetBool(hashMove, isMoving);
    }

    public void PlayAttack(int comboIndex)
    {
        anim.SetInteger(hashComboIndex, comboIndex);
        anim.SetTrigger(hashAttack);
    }

    public void PlaySkillA()
    {
        anim.SetTrigger(hashSkillA);
    }

    public void PlaySkillS()
    {
        anim.SetTrigger(hashSkillS);
    }

    public void PlayDeath()
    {
        anim.SetTrigger(hashDeath);
    }
}