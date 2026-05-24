using _Project.Scripts.Gameplay.Talents;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class NeedleMoleculeAttack : LineShotMoleculeAttack
    {
        protected override BattleMoleculeShotKind ShotKind => BattleMoleculeShotKind.Needle;
        protected override int BaseShotDamage => Config.NeedleMoleculeShotDamage;
        protected override TalentType DamageTalentType => TalentType.NeedleMoleculeDamage;
    }
}
