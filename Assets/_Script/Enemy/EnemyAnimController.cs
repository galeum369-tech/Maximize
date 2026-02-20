using UnityEngine;

public class EnemyAnimController
{
    private Animator anim;

    public EnemyAnimController(Animator anim)
    {
        this.anim = anim;
    }

    // 이동 애니메이션 (속도에 따라 조절 가능)
    public void PlayMove(float speed)
    {
        anim.SetFloat("MoveSpeed", Mathf.Abs(speed));
    }

    // 공격 트리거
    public void PlayAttack()
    {
        anim.SetTrigger("Attack");
    }

    // 피격/사망 애니메이션
    public void PlayHit() => anim.SetTrigger("Hit");
    public void PlayDeath() => anim.SetTrigger("Die");
}