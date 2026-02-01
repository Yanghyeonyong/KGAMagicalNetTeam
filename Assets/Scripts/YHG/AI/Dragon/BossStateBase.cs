using UnityEngine;

public abstract class BossStateBase : IState
{
    protected DragonAI dragon;
    protected StateMachine stateMachine;

    public BossStateBase(DragonAI dragon, StateMachine stateMachine)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    //필수
    public abstract void Execute();
    public virtual void FixedExecute() { }
    public virtual void Exit() { }
}
