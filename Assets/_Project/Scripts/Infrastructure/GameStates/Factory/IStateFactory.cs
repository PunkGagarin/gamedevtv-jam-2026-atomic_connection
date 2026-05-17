using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;

namespace _Project.Scripts.Infrastructure.GameStates.Factory
{
    public interface IStateFactory
    {
        TState GetState<TState>() where TState : class, IExitableState;
    }
}
