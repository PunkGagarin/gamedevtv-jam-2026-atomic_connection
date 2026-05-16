using System;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Localization;

namespace _Project.Scripts.GameplayData
{
    [CreateAssetMenu(fileName = "Global Config Installer", menuName = "Game Resources/Global Config Installer")]
    public class GlobalConfigInstaller : ScriptableObjectInstaller
    {
        [field: SerializeField] private LanguageConfig LanguageConfig { get; set; }
        [field: SerializeField] private EnemySpawnerConfig EnemySpawnerConfig { get; set; }
        [field: SerializeField] private UnitClickConfig UnitClickConfig { get; set; }

        public override void InstallBindings()
        {
            BindConfig(LanguageConfig);
            BindConfig(EnemySpawnerConfig);
            BindConfig(UnitClickConfig);
        }

        private void BindConfig<TConfig>(TConfig config) where TConfig : ScriptableObject
        {
            if (config == null)
                throw new InvalidOperationException($"{typeof(TConfig).Name} is not assigned in {nameof(GlobalConfigInstaller)}.");

            Container
                .Bind<TConfig>()
                .FromInstance(config)
                .AsSingle();
        }
    }
}
