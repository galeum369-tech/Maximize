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
    public float rotationSpeed = 10f; // 드론 회전 속도 추가

    [Header("공격 프리팹")]
    public GameObject normalBulletPrefab;
    public GameObject bigCannonPrefab;

    [Header("자동 공격 설정")]
    public float fireRate = 0.5f;
    public float attackRange = 10f;
    public LayerMask targetLayer;

    private float fireTimer;
    private bool isExecutingSkill = false;
    private Transform currentTarget; // 현재 타겟 저장용

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

        UpdatePositionAndRotation();

        // 스킬 실행 중이 아닐 때만 자동 공격 타임 계산
        if (!isExecutingSkill)
        {
            HandleAutoAttack();
        }
    }

    private void UpdatePositionAndRotation()
    {
        // 1. 위치 이동 (부드러운 추적 + 부유 효과)
        Vector3 targetOffset = followOffset;
        if (owner.localScale.x < 0) targetOffset.x *= -1;

        Vector3 targetPos = owner.position + targetOffset;
        float floatingY = Mathf.Sin(Time.time * 2f) * 0.15f;
        transform.position = Vector3.Lerp(transform.position, targetPos + new Vector3(0, floatingY, 0), Time.deltaTime * followSpeed);

        // 2. 회전 (타겟이 있으면 타겟 방향, 없으면 플레이어 방향)
        Vector2 lookDir = GetFireDirection(currentTarget);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // 2D에서는 보통 오른쪽(X+)이 정면이므로 angle을 그대로 사용하거나 보정
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    private void HandleAutoAttack()
    {
        fireTimer += Time.deltaTime;

        // 매 프레임 찾지 않고 발사 타이밍에만 타겟 검색 (최적화)
        if (fireTimer >= fireRate)
        {
            currentTarget = FindNearestTarget();
            if (currentTarget != null)
            {
                FireNormal(currentTarget);
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
            currentTarget = FindNearestTarget();
            FireNormal(currentTarget);
            yield return new WaitForSeconds(0.1f);
        }

        fireTimer = 0f; // 스킬 직후 바로 평타 나가는 것 방지
        isExecutingSkill = false;
    }

    // --- 스킬 2: 거대 캐논 (Big Cannon) ---
    private void UseSkillBigBomb()
    {
        if (state.skill2Timer > 0 || isExecutingSkill) return;
        StartCoroutine(BigBombRoutine());
    }

    private IEnumerator BigBombRoutine()
    {
        isExecutingSkill = true; // 강한 공격 중엔 평타 중지
        state.skill2Timer = state.skill2Cooldown;

        currentTarget = FindNearestTarget();
        Vector2 fireDir = GetFireDirection(currentTarget);

        if (bigCannonPrefab != null)
        {
            // 발사 시점 이펙트나 딜레이가 필요하면 여기에 추가 가능
            GameObject cannon = Instantiate(bigCannonPrefab, transform.position, Quaternion.identity);
            var cannonScript = cannon.GetComponent<DroneCannon>();

            if (cannonScript != null)
            {
                cannonScript.Launch(state.currentAtk, fireDir);
            }
        }

        yield return new WaitForSeconds(0.3f); // 후딜레이
        fireTimer = 0f;
        isExecutingSkill = false;
    }

    private void FireNormal(Transform target)
    {
        if (normalBulletPrefab == null) return;

        // 1. 드론이 현재 조준하고 있는 방향을 가져옴 (타겟이 있으면 타겟 방향, 없으면 정면)
        Vector2 fireDir = GetFireDirection(target);

        // 2. 총알 생성 (드론의 현재 위치와 회전값 적용)
        GameObject bullet = Instantiate(normalBulletPrefab, transform.position, transform.rotation);
        var proj = bullet.GetComponent<HomingProjectile>();

        if (proj != null)
        {
            // 3. [수정] 공격력과 함께 발사 방향(fireDir)을 같이 전달함
            proj.Launch(state.currentAtk, fireDir);

            // 만약 유도탄 스크립트에 타겟을 직접 박아주는 기능이 있다면 추가로 호출 가능
            // (현재 HomingProjectile은 Launch 안에서 스스로 타겟을 찾도록 수정했음)
        }
    }

    private Vector2 GetFireDirection(Transform target)
    {
        if (target != null)
        {
            return (target.position - transform.position).normalized;
        }
        else
        {
            // 플레이어가 보는 방향 기준
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
            if (dst < minDst)
            {
                minDst = dst;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}