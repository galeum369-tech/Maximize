using UnityEngine;

public class EnemyAnimController
{
    private Animator anim;

    readonly int hashMove = Animator.StringToHash("Move");
    readonly int hashAttack = Animator.StringToHash("Attack");
    readonly int hashHit = Animator.StringToHash("Hit");
    readonly int hashDeath = Animator.StringToHash("Die");

    public EnemyAnimController(Animator anim)
    {
        this.anim = anim;
    }

    // 이동 애니메이션 (속도에 따라 조절 가능)
    public void PlayMove(float speed)
    {
        anim.SetFloat(hashMove, Mathf.Abs(speed));
    }

    // 공격 트리거
    public void PlayAttack()
    {
        anim.SetTrigger(hashAttack);
    }

    // 피격/사망 애니메이션
    public void PlayHit() => anim.SetTrigger(hashHit);
    public void PlayDeath() => anim.SetTrigger(hashDeath);
}