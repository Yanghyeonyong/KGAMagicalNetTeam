using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class GuardChaseState : AIStateBase
{
    private GuardAI guard;

    //최적화_타이머
    private float reportTimer = 0f;    //신고 주기
    private float pathTimer = 0f;      //길찾기 연산 주기

    //최적화_컴포넌트 캐싱
    private PhotonView targetPV;

    //최적화_마지막 경로 타겟위치(가까우면 변경x)
    private Vector3 lastTargetPos;

    //최적화_사거리 제곱
    private float sqrAttackRange;

    private float stuckCheckTimer = 0f;   
    private float stuckDuration = 0f;      
    private Vector3 lastStuckCheckPos;


    public GuardChaseState(BaseAI ai, StateMachine machine) : base(ai, machine, BaseAI.AIStateID.Chase)
    {
        guard = ai as GuardAI;
        sqrAttackRange = guard.attackRange * guard.attackRange;
    }

    public override void Enter()
    {
        base.Enter();
        if (guard.Agent != null && guard.Agent.isOnNavMesh)
        {
            guard.Agent.updatePosition = true;
            guard.Agent.updateRotation = true;
            guard.Agent.isStopped = false;
            guard.Agent.ResetPath();
            guard.Agent.speed = guard.runSpeed;
        }

        //고착 체크 변수 초기화
        stuckDuration = 0f;
        lastStuckCheckPos = guard.transform.position;


        if (guard.targetPlayer != null)
        {
            //추격 시작시에 타겟 포톤뷰 찾아두기
            targetPV = guard.targetPlayer.GetComponent<PhotonView>();
            lastTargetPos = guard.targetPlayer.position;
            if (guard.Agent.isOnNavMesh)
            {
                Vector3 dir = (guard.targetPlayer.position - guard.transform.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    guard.transform.rotation = Quaternion.LookRotation(dir);
                }

                guard.Agent.SetDestination(guard.targetPlayer.position);
            }
            //발견 즉시 동료들에게 신고
            ReportToManager();
        }
    }
    public override void Execute()
    {
        //타겟 유효성 검사
        if (guard.targetPlayer == null)
        {
            stateMachine.ChangeState(new GuardPatrolState(guard, stateMachine));
            return;
        }

        //사거리체크
        Vector3 toTarget = guard.targetPlayer.position - guard.transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        //공격 사거리 안으로 들어왔는가?
        if (sqrDist <= sqrAttackRange)
        {
            //어택
            stateMachine.ChangeState(new GuardAttackState(guard, stateMachine));
            return;
        }

        //타겟이 0.25m 이상 움직였을 때만 재계산
        if (Vector3.SqrMagnitude(guard.targetPlayer.position - lastTargetPos) > 0.25f)
        {
            if (guard.Agent.isOnNavMesh)
            {
                guard.Agent.SetDestination(guard.targetPlayer.position);
                lastTargetPos = guard.targetPlayer.position;
            }
            pathTimer = 0f;
        }
        CheckIfStuck();

        //초마다 매니저에게 적 위치 공유
        reportTimer += Time.deltaTime;
        if (reportTimer > 1.0f)
        {
            reportTimer = 0f;
            ReportToManager();
        }
    }

    private void CheckIfStuck()
    {
        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer > 0.5f)
        {
            stuckCheckTimer = 0f;

            float movedDist = Vector3.Distance(guard.transform.position, lastStuckCheckPos);

            if (movedDist < 0.1f)
            {
                stuckDuration += 0.5f;
            }
            else
            {
                //움직였다면 초기화
                stuckDuration = 0f;
            }

            lastStuckCheckPos = guard.transform.position;
        }

        //3초 이상 제자리라면 탈출 시도
        if (stuckDuration >= 3.0f)
        {
            PerformUnstuck();
            stuckDuration = 0f;
        }
    }

    private void PerformUnstuck()
    {
        if (!guard.Agent.isOnNavMesh) return;

        guard.Agent.ResetPath();

        //현재 위치에서 랜덤한 곳으로 텔포 쪼끔
        Vector3 randomPoint = guard.transform.position + Random.insideUnitSphere * 1.5f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
        {
            guard.Agent.Warp(hit.position);
        }
        guard.Agent.SetDestination(guard.targetPlayer.position);
    }

    //플레이어 찾으면(무조건 조우시) 플레이어 정보 전달
    private void ReportToManager()
    {
        if (GuardManager.instance == null || targetPV == null) return;
        guard.ReportEnemy(guard.targetPlayer.position);
    }

}
