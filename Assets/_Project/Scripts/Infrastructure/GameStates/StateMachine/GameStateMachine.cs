using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.StateMachine
{
    public class GameStateMachine : SimpleStateMachine<IGameState>, ITickable, IFixedTickable
    {
        public void Tick()
        {
            if (CurrentState is IUpdateable updateable)
                updateable.Update();
        }

        public void FixedTick()
        {
            if (CurrentState is IFixedUpdateable fixedUpdateable)
                fixedUpdateable.FixedUpdate();
        }
    }
}
