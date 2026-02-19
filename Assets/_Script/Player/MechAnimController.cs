using UnityEngine;

public class MechAnimController
{
    Animator anim;

    // 해시값 최적화
    readonly int hashMove = Animator.StringToHash("Move");
    readonly int hashAttack = Animator.StringToHash("Attack");
    readonly int hashMoveAttack = Animator.StringToHash("MoveAttack");
    readonly int hashDeath = Animator.StringToHash("Death");
    readonly int hashHit = Animator.StringToHash("Hit");
    readonly int hashSkillA = Animator.StringToHash("SkillA");
    readonly int hashSkillS = Animator.StringToHash("SkillS");

    // 점프/활공 관련
    readonly int hashGrounded = Animator.StringToHash("isGrounded");
    readonly int hashYVelocity = Animator.StringToHash("yVelocity");
    readonly int hashGliding = Animator.StringToHash("isGliding"); // [추가] 활공

    public MechAnimController(Animator anim)
    {
        this.anim = anim;
    }

    public void UpdateAerialState(bool isGrounded, float yVelocity, bool isGliding)
    {
        anim.SetBool(hashGrounded, isGrounded);
        anim.SetFloat(hashYVelocity, yVelocity);
        anim.SetBool(hashGliding, isGliding); // [추가]
    }

    public void PlayMove(bool isMoving)
    {
        anim.SetBool(hashMove, isMoving);
    }

    public void PlayAttack(bool isAttacking)
    {
        anim.SetBool(hashAttack, isAttacking);
    }

    public void PlayMoveAttack(bool isMoving, bool isAttacking)
    {
        anim.SetBool(hashMove, isMoving);
        anim.SetBool(hashMoveAttack, isAttacking);
    }

    public void PlaySkillA() => anim.SetTrigger(hashSkillA);
    public void PlaySkillS() => anim.SetTrigger(hashSkillS);
    public void PlayDeath() => anim.SetTrigger(hashDeath);
    public void PlayHit() => anim.SetTrigger(hashHit);
}