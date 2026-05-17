using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Infrastructure.GameStates.States;
using _Project.Scripts.Infrastructure.GameStates;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Units.Atom;
using _Project.Scripts.Gameplay.Units.BattleMolecule;
using _Project.Scripts.Gameplay.Units.Example;
using _Project.Scripts.Gameplay.Windows;
using _Project.Scripts.Infrastructure.AssetManagement;
using _Project.Scripts.Infrastructure.GameStates.Factory;
using _Project.Scripts.Utils.Pause;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindStateMachine();
            BindStateFactory();
            BindGameStates();
            BindAssetManagement();
            BindCameraProvider();
            BindCommonServices();
            BindPauseService();
            BindInputService();
            BindDragService();
            BindGameplayExample();
            BindWindowInfrastructure();
            BindGameplayBattleMolecule();
        }

        private void BindAssetManagement()
        {
            Container.Bind<IAssetProvider>().To<AssetProvider>().AsSingle();
        }

        private void BindInputService()
        {
            Container.Bind<IInputService>().To<StandaloneInputService>().AsSingle();
        }

        private void BindDragService()
        {
            Container.Bind<IDragService>().To<DragService>().AsSingle();
        }

        private void BindGameplayExample()
        {
            Container.Bind<ILevelStartPointProvider>().To<LevelStartPointProvider>().AsSingle();
            Container.Bind<IExampleUnitFactory>().To<ExampleUnitFactory>().AsSingle();
            Container.Bind<IExampleUnitClickService>().To<ExampleUnitClickService>().AsSingle();
            Container.Bind<IAtomFactory>().To<AtomFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
            Container.Bind<IEnemySpawner>().To<EnemySpawner>().AsSingle();
        }

        private void BindWindowInfrastructure()
        {
            Container.Bind<IWindowService>().To<WindowService>().AsSingle();
            Container.Bind<IWindowFactory>().To<WindowFactory>().AsSingle();
        }

        private void BindCameraProvider()
        {
            Container.BindInterfacesAndSelfTo<CameraProvider>().AsSingle();
        }

        private void BindCommonServices()
        {
            Container.Bind<ITimeService>().To<UnityTimeService>().AsSingle();
            Container.Bind<IPhysicsService>().To<PhysicsService>().AsSingle();
            Container.Bind<IRandomService>().To<UnityRandomService>().AsSingle();
        }

        private void BindGameplayBattleMolecule()
        {
            Container.Bind<IBattleMoleculeFactory>().To<BattleMoleculeFactory>().AsSingle();
        }

        private void BindPauseService()
        {
            Container.BindInterfacesAndSelfTo<PauseService>().AsSingle().NonLazy();
        }

        private void BindStateMachine()
        {
            Debug.Log(" BindStateMachine from installer");
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle().NonLazy();
        }

        private void BindStateFactory()
        {
            Container.BindInterfacesAndSelfTo<StateFactory>().AsSingle();
        }

        private void BindGameStates()
        {
            Container.BindInterfacesAndSelfTo<BootstrapState>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadMainMenuState>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadGameplayState>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplayEnterState>().AsSingle();
            Container.BindInterfacesAndSelfTo<MainMenuState>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplayState>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplayPauseState>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameOverOrParagonState>().AsSingle();
        }
    }

}
