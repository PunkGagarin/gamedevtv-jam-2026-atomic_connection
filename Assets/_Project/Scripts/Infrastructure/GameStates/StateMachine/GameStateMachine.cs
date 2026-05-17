using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.StateMachine
{
    public class GameStateMachine : SimpleStateMachine<IGameState>, ITickable
    {
        public void Tick()
        {
            if (CurrentState is IUpdateable updateable)
                updateable.Update();
        }
    }
}
