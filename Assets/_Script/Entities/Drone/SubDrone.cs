using UnityEngine;
using System.Collections;

public class SubDrone : MonoBehaviour
{
    [Header("참조")]
    public Transform owner;
    private DroneState state;
    // input 참조 삭제 (PlayerSkillController가 관리)

    [Header("설정")]
    public Vector3 followOffset = new Vector3(-1.2f, 1.2f, 0);
    public float followSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("공격 프리팹")]
    public GameObject normalBulletPrefab;
    public GameObject bigCannonPrefab;

    [Header("자동 공격 설정")]
    public float fireRate = 0.5f;
    public float attackRange = 10f;
    public LayerMask targetLayer;

    private float fireTimer;
    private bool isExecutingSkill = false;
    private Transform currentTarget;

    private void Awake()
    {
        state = GetComponent<DroneState>();
    }

    private void Update()
    {
        if (owner == null) return;
        UpdatePositionAndRotation();
        if (!isExecutingSkill) HandleAutoAttack();
    }

    private void UpdatePositionAndRotation()
    {
        Vector3 targetOffset = followOffset;
        if (owner.localScale.x < 0) targetOffset.x *= -1;

        Vector3 targetPos = owner.position + targetOffset;
        float floatingY = Mathf.Sin(Time.time * 2f) * 0.15f;
        transform.position = Vector3.Lerp(transform.position, targetPos + new Vector3(0, floatingY, 0), Time.deltaTime * followSpeed);

        Vector2 lookDir = GetFireDirection(currentTarget);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    private void HandleAutoAttack()
    {
        fireTimer += Time.deltaTime;
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

    // --- [수정] 스킬 1: 연사 (bool 반환) ---
    public bool ExecuteSkill1()
    {
        if (isExecutingSkill) return false; // 이미 다른 스킬 중이면 실패 반환
        StartCoroutine(BarrageRoutine());
        return true; // 성공적으로 실행됨
    }

    private IEnumerator BarrageRoutine()
    {
        isExecutingSkill = true;
        for (int i = 0; i < 10; i++)
        {
            currentTarget = FindNearestTarget();
            FireNormal(currentTarget);
            yield return new WaitForSeconds(0.1f);
        }
        fireTimer = 0f;
        isExecutingSkill = false;
    }

    // --- [수정] 스킬 2: 거대 캐논 (bool 반환) ---
    public bool ExecuteSkill2()
    {
        if (isExecutingSkill) return false;
        StartCoroutine(BigBombRoutine());
        return true;
    }

    private IEnumerator BigBombRoutine()
    {
        isExecutingSkill = true;
        currentTarget = FindNearestTarget();
        Vector2 fireDir = GetFireDirection(currentTarget);

        if (bigCannonPrefab != null)
        {
            GameObject cannon = Instantiate(bigCannonPrefab, transform.position, Quaternion.identity);
            var cannonScript = cannon.GetComponent<DroneCannon>();
            if (cannonScript != null) cannonScript.Launch(state.currentAtk, fireDir);
        }

        yield return new WaitForSeconds(0.3f);
        fireTimer = 0f;
        isExecutingSkill = false;
    }

    private void FireNormal(Transform target)
    {
        if (normalBulletPrefab == null) return;
        Vector2 fireDir = GetFireDirection(target);
        GameObject bullet = Instantiate(normalBulletPrefab, transform.position, transform.rotation);
        var proj = bullet.GetComponent<HomingProjectile>();
        if (proj != null) proj.Launch(state.currentAtk, fireDir);
    }

    private Vector2 GetFireDirection(Transform target)
    {
        if (target != null) return (target.position - transform.position).normalized;
        else return owner.localScale.x > 0 ? Vector2.right : Vector2.left;
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