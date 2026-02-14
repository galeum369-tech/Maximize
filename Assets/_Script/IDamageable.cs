using UnityEngine;

// 모든 공격 가능한 오브젝트들의 공통 규약
public interface IDamageable
{
    // (데미지, 넉백 힘, 타격 방향)을 인자로 받음
    void TakeDamage(float damage, float knockback, Vector2 hitDirection);
}