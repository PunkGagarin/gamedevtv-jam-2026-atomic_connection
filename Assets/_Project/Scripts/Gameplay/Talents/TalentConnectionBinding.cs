namespace _Project.Scripts.Gameplay.Talents
{
    public readonly struct TalentConnectionBinding
    {
        public readonly TalentId ParentId;
        public readonly TalentId ChildId;
        public readonly TalentConnectionView View;

        public TalentConnectionBinding(TalentId parentId, TalentId childId, TalentConnectionView view)
        {
            ParentId = parentId;
            ChildId = childId;
            View = view;
        }
    }
}
