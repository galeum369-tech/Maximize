using UnityEngine;
using System.Collections;

public class SubDrone : MonoBehaviour
{
    [Header("참조")]
    public Transform owner;
    private DroneState state;
    private PlayerInputHandler input;

    [Header("설정")]
    public Vector3 followOffset = new Vector3(-1.2f, 1.2f, 0);
    public float followSpeed = 6f;

    [Header("공격 프리팹")]
    public GameObject normalBulletPrefab;
    public GameObject bigCannonPrefab; // 직선 캐논 프리팹 (DroneCannon 스크립트 필요)

    [Header("자동 공격 설정")]
    public float fireRate = 0.5f;
    public float attackRange = 10f;
    public LayerMask targetLayer;

    private float fireTimer;
    private bool isExecutingSkill = false;

    private void Awake()
    {
        state = GetComponent<DroneState>();
        if (owner != null) input = owner.GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.OnSkill1 += UseSkillBarrage;
            input.OnSkill2 += UseSkillBigBomb;
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.OnSkill1 -= UseSkillBarrage;
            input.OnSkill2 -= UseSkillBigBomb;
        }
    }

    private void Update()
    {
        if (owner == null) return;

        UpdatePosition();

        if (!isExecutingSkill) HandleAutoAttack();
    }

    private void UpdatePosition()
    {
        Vector3 targetOffset = followOffset;
        if (owner.localScale.x < 0) targetOffset.x *= -1;

        Vector3 targetPos = owner.position + targetOffset;
        float floatingY = Mathf.Sin(Time.time * 2f) * 0.15f;
        transform.position = Vector3.Lerp(transform.position, targetPos + new Vector3(0, floatingY, 0), Time.deltaTime * followSpeed);
    }

    private void HandleAutoAttack()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            // 자동 공격은 타겟이 있을 때만 수행 (허공 사격 방지)
            Transform target = FindNearestTarget();
            if (target != null)
            {
                FireNormal(target);
                fireTimer = 0f;
            }
        }
    }

    // --- 스킬 1: 연사 (Barrage) ---
    private void UseSkillBarrage()
    {
        if (state.skill1Timer > 0 || isExecutingSkill) return;
        StartCoroutine(BarrageRoutine());
    }

    private IEnumerator BarrageRoutine()
    {
        isExecutingSkill = true;
        state.skill1Timer = state.skill1Cooldown;

        for (int i = 0; i < 10; i++)
        {
            Transform target = FindNearestTarget();
            // 타겟이 있으면 타겟 방향, 없으면 정면 발사
            FireNormal(target);
            yield return new WaitForSeconds(0.1f);
        }

        isExecutingSkill = false;
    }

    // --- [수정] 스킬 2: 거대 캐논 (Big Cannon) ---
    private void UseSkillBigBomb()
    {
        if (state.skill2Timer > 0 || isExecutingSkill) return;

        state.skill2Timer = state.skill2Cooldown;

        // 1. 발사 방향 결정
        Transform target = FindNearestTarget();
        Vector2 fireDir = GetFireDirection(target);

        // 2. 캐논 발사 (DroneCannon 로직 적용)
        if (bigCannonPrefab != null)
        {
            GameObject cannon = Instantiate(bigCannonPrefab, transform.position, Quaternion.identity);
            var cannonScript = cannon.GetComponent<DroneCannon>(); // 직선 발사용 스크립트

            if (cannonScript != null)
            {
                // 공격력 4배율 및 방향 전달
                cannonScript.Launch(state.currentAtk * 4f, fireDir);
            }
        }
    }

    // 일반 사격 (타겟 여부에 따른 로직 포함)
    private void FireNormal(Transform target)
    {
        if (normalBulletPrefab == null) return;

        GameObject bullet = Instantiate(normalBulletPrefab, transform.position, Quaternion.identity);
        var proj = bullet.GetComponent<HomingProjectile>();

        if (proj)
        {
            // 타겟이 있으면 유도, 없으면 정면 직선 발사
            if (target != null)
            {
                proj.Launch(state.currentAtk); // 기존 유도 로직
            }
            else
            {
                // 유도탄 스크립트에 정면 발사 기능이 없다면 방향을 강제로 설정해줘야 함
                // 여기서는 타겟 없이 발사할 때의 데미지만 전달
                proj.Launch(state.currentAtk);

                // 타겟이 없을 경우 탄환 자체의 forward를 플레이어 방향으로 고정
                Vector2 dir = GetFireDirection(null);
                bullet.transform.right = dir;
            }
        }
    }

    // [추가] 방향 계산 헬퍼 함수
    private Vector2 GetFireDirection(Transform target)
    {
        if (target != null)
        {
            return (target.position - transform.position).normalized;
        }
        else
        {
            // 타겟이 없으면 플레이어가 보는 방향(localScale.x) 기준 정면
            return owner.localScale.x > 0 ? Vector2.right : Vector2.left;
        }
    }

    private Transform FindNearestTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayer);
        Transform nearest = null;
        float minDst = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dst = Vector2.Distance(transform.position, enemy.transform.position);
            if (dst < minDst) { minDst = dst; nearest = enemy.transform; }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}