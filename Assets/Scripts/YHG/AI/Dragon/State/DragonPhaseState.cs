using UnityEngine;
using System.Collections;
public class DragonPhaseState : BossStateBase
{
    private float phaseDuration = 5f; 
    private float timer = 0f;

    public DragonPhaseState(DragonAI dragon, StateMachine stateMachine) : 
        base(dragon, stateMachine) { }

    public override void Enter()
    {
        if (dragon.agent != null)
        {
            dragon.agent.isStopped = true;
            dragon.agent.velocity = Vector3.zero;
        }

        dragon.DisableWeaponHitbox();

        Collider bodyCol = dragon.GetComponent<Collider>();
        if (bodyCol != null) bodyCol.enabled = false;

        //2페진입
        dragon.isPhaseTwo = true;

        dragon.StartCoroutine(CoPhaseProcess());
    }

    public override void Execute() { }
    private IEnumerator CoPhaseProcess()
    {
        dragon.PlayAnimTrigger("Collapse");
        yield return CoroutineManager.waitForSeconds(phaseDuration);

        dragon.PlayAnimTrigger("Reassemble");

        yield return CoroutineManager.waitForSeconds(phaseDuration);

        stateMachine.ChangeState(new DragonFlightState(dragon, stateMachine));
    }
    public override void Exit()
    {
        //무적 해제
        Collider bodyCol = dragon.GetComponent<Collider>();
        if (bodyCol != null) bodyCol.enabled = true;

        if (dragon.agent != null)
        {
            dragon.agent.updatePosition = true;
            dragon.agent.updateRotation = true;
            dragon.agent.isStopped = false;
            dragon.agent.Warp(dragon.transform.position);
        }
    }
}
