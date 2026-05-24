using System;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.CurrencyDrops;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Feedback;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Tutorial;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Localization;

namespace _Project.Scripts.GameplayData
{
    [CreateAssetMenu(fileName = "Global Config Installer", menuName = "Game Resources/Global Config Installer")]
    public class GlobalConfigInstaller : ScriptableObjectInstaller
    {
        [field: SerializeField] private LanguageConfig LanguageConfig { get; set; }
        [field: SerializeField] private UiThemeConfig UiThemeConfig { get; set; }
        [field: SerializeField] private AtomCoreConfig AtomCoreConfig { get; set; }
        [field: SerializeField] private BattleMoleculeConfig BattleMoleculeConfig { get; set; }
        [field: SerializeField] private GameplayFeedbackAnimationConfig GameplayFeedbackAnimationConfig { get; set; }
        [field: SerializeField] private TalentConfig TalentConfig { get; set; }
        [field: SerializeField] private TalentTreeAnimationConfig TalentTreeAnimationConfig { get; set; }
        [field: SerializeField] private CurrencyConfig CurrencyConfig { get; set; }
        [field: SerializeField] private CurrencyPickupConfig CurrencyPickupConfig { get; set; }
        [field: SerializeField] private LevelCatalogConfig LevelCatalogConfig { get; set; }
        [field: SerializeField] private EnemySpawnerConfig EnemySpawnerConfig { get; set; }
        [field: SerializeField] private EnemyMergeConfig EnemyMergeConfig { get; set; }
        [field: SerializeField] private EnemyKillRewardConfig EnemyKillRewardConfig { get; set; }
        [field: SerializeField] private GameplayTutorialConfig GameplayTutorialConfig { get; set; }

        public override void InstallBindings()
        {
            BindConfig(LanguageConfig);
            BindConfig(UiThemeConfig);
            BindConfig(AtomCoreConfig);
            BindConfig(BattleMoleculeConfig);
            BindConfig(GameplayFeedbackAnimationConfig);
            BindConfig(TalentConfig);
            BindConfig(TalentTreeAnimationConfig);
            BindConfig(CurrencyConfig);
            BindConfig(CurrencyPickupConfig);
            BindConfig(LevelCatalogConfig);
            BindConfig(EnemySpawnerConfig);
            BindConfig(EnemyMergeConfig);
            BindConfig(EnemyKillRewardConfig);
            BindConfig(GameplayTutorialConfig);
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
