using UnityEngine;
//각도 기준 공격패턴 지정
public class DragonCombatState : BossStateBase
{
    private float attackCooldown = 3.0f;
    private float lastAttackTime = 0f;

    private bool isAttacking = false;
    private float attackEndTime = 0f;

    public DragonCombatState(DragonAI dragon, StateMachine stateMachine) 
        : base(dragon, stateMachine) { }

    public override void Enter()
    {
        if (dragon.agent != null)
        {
            dragon.agent.isStopped = true;
            dragon.agent.velocity = Vector3.zero;
            dragon.agent.updateRotation = false;
        }

        lastAttackTime = Time.time - 1.0f;
        dragon.PlayAnimTrigger("Idle");
    }

    public override void Execute()
    {
        if (dragon.targetPlayer == null)
        {
            stateMachine.ChangeState(new DragonChaseState(dragon, stateMachine));
            return;
        }

        if (isAttacking)
        {
            if (Time.time > attackEndTime)
            {
                isAttacking = false;
                lastAttackTime = Time.time;     
                dragon.PlayAnimTrigger("Idle");
                dragon.FindRandomTarget();
            }
            return; 
        }

        Vector3 toTarget = dragon.targetPlayer.position - dragon.transform.position;
        float dist = toTarget.magnitude;

        //멀면 추격
        if (dist > dragon.distCombatExit)
        {
            stateMachine.ChangeState(new DragonChaseState(dragon, stateMachine));
            return;
        }

        //쿨
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            float angle = Vector3.SignedAngle(dragon.transform.forward, toTarget, Vector3.up);
            DecideAttackPattern(Mathf.Abs(angle), dist);
            lastAttackTime = Time.time;
        }
    }

    //방향잡는로직인데 일단 보류
    private void RotateTowardsTarget()
    {
        if (dragon.targetPlayer == null) return;
        Vector3 dir = (dragon.targetPlayer.position - dragon.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            dragon.transform.rotation = Quaternion.Slerp(dragon.transform.rotation, targetRot, Time.deltaTime * 5.0f);
        }
    }

    //패턴
    private void DecideAttackPattern(float absAngle, float dist)
    {
        isAttacking = true;

        //후방
        if (absAngle > dragon.angleBackTail)
        {
            dragon.PlayAnimTrigger("TailWhip");
            attackEndTime = Time.time + 1.5f;
        }

        //전방
        else
        {
            //멀면
            if (dist > dragon.distLongRange)
            {
                dragon.PlayAnimTrigger("Breathe Fire");
                dragon.ShootFireball();
                attackEndTime = Time.time + 1.5f;
            }
            else if (dist > 8.0f)
            {
                if (absAngle <= dragon.angleFrontWide)
                {
                    dragon.PlayAnimCrossFade("Fire Head 2", 0.1f);
                    attackEndTime = Time.time + 3.0f;
                }
                else
                {
                    isAttacking = false;
                    stateMachine.ChangeState(new DragonChaseState(dragon, stateMachine));
                }
            }
            else
            {
                int rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    dragon.PlayAnimCrossFade("Attack1", 0.1f);
                    attackEndTime = Time.time + 1.2f;
                }
                else
                {
                    dragon.PlayAnimCrossFade("Attack2", 0.1f);
                    attackEndTime = Time.time + 1.2f;
                }
            }
        }
    }
    public override void Exit()
    {
        if (dragon.agent != null)
        {
            dragon.agent.isStopped = false;
            dragon.agent.updateRotation = true;
        }
    }
}
