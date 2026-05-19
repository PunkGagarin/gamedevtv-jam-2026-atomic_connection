namespace _Project.Scripts.Infrastructure.GameStates.StateInfrastructure
{
    public interface IState : IExitableState
    {
        void Enter();
    }

    public interface IExitableState
    {
        void Exit();
    }

    public interface IDeferredExitState : IExitableState
    {
        bool ExitCompleted { get; }
        void BeginExit();
        void EndExit();
    }

    public interface IPayloadState<TPayload> : IExitableState
    {
        void Enter(TPayload payload);
    }

    public interface IUpdateable
    {
        void Update();
    }

    public interface IFixedUpdateable
    {
        void FixedUpdate();
    }
    
    public interface IGameState : IExitableState
    {
    }
}
