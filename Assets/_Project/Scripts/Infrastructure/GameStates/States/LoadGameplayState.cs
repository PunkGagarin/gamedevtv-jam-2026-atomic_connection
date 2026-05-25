using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Infrastructure.SceneManagement;
using _Project.Scripts.Utils.Pause;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class LoadGameplayState : IState, IGameState
    {
        [Inject] private SceneLoader _sceneLoader;
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private LoadingCurtain _loadingCurtain;
        [Inject] private PauseService _pauseService;
        [Inject] private AudioService _audio;

        public async void Enter()
        {
            _pauseService.SetPaused(false);
            _loadingCurtain.Show();
            //load resources
            await _sceneLoader.LoadScene(SceneEnum.Gameplay);
            _audio.PlayMusic(Sounds.intenseBgm, true);
            _loadingCurtain.Hide();

            _stateMachine.Enter<GameplayEnterState>();
        }

        public void Exit()
        {
        }
    }
}
