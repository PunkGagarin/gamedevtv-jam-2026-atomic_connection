using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Tutorial
{
    [CreateAssetMenu(fileName = "GameplayTutorialConfig", menuName = "Game Resources/Configs/Gameplay Tutorial")]
    public class GameplayTutorialConfig : ScriptableObject
    {
        [field: Header("Prefab")]
        [field: SerializeField] public string PrefabResourcePath { get; private set; } = "Gameplay/UI/GameplayTutorialView";

        [field: Header("Screen Placement")]
        [field: SerializeField] public Vector2 CoreCursorOffset { get; private set; } = Vector2.zero;
        [field: SerializeField] public Vector2 DragCursorOffset { get; private set; } = new(0f, 48f);
        [field: SerializeField] public Vector2 ActiveMoleculeCursorOffset { get; private set; } = Vector2.zero;
        [field: SerializeField, Min(0f)] public float ScreenPadding { get; private set; } = 48f;

        [field: Header("Animation")]
        [field: SerializeField, Min(0.01f)] public float PulseDurationSeconds { get; private set; } = 0.55f;
        [field: SerializeField, Min(0.01f)] public float DragLoopDurationSeconds { get; private set; } = 1.4f;
        [field: SerializeField, Min(0.01f)] public float DragPressedFrameSeconds { get; private set; } = 0.35f;
        [field: SerializeField, Min(0.01f)] public float DragReleaseFrameSeconds { get; private set; } = 0.25f;
        [field: SerializeField, Min(0.01f)] public float CursorBaseScale { get; private set; } = 1f;
        [field: SerializeField, Min(0.01f)] public float CursorPulseScale { get; private set; } = 0.86f;
        [field: SerializeField] public Sprite DragPathSprite { get; private set; }
        [field: SerializeField] public Vector2 DragPathSize { get; private set; } = new(27f, 64f);
        [field: SerializeField] public float DragPathAngleOffsetDegrees { get; private set; } = -90f;

        [field: Header("Visuals")]
        [field: SerializeField] public GameplayTutorialStepVisual CoreClickVisual { get; private set; } = new();
        [field: SerializeField] public GameplayTutorialStepVisual AtomDragVisual { get; private set; } = new();
        [field: SerializeField] public GameplayTutorialStepVisual AttackVisual { get; private set; } = new();
        [field: SerializeField] public GameplayTutorialStepVisual ActiveMoleculeVisual { get; private set; } = new();

        [field: Header("Attack Hint")]
        [field: SerializeField] public Vector2 FallbackAttackPullDirection { get; private set; } = new(-1f, 0f);
        [field: SerializeField, Min(1f)] public float AttackPullDistancePixels { get; private set; } = 220f;
    }

    [Serializable]
    public class GameplayTutorialStepVisual
    {
        [field: SerializeField] public Sprite CursorSprite { get; private set; }
        [field: SerializeField] public Sprite PressedCursorSprite { get; private set; }
        [field: SerializeField] public Sprite ReleasedCursorSprite { get; private set; }
        [field: SerializeField] public Sprite CursorIconSprite { get; private set; }
        [field: SerializeField] public Sprite TargetIconSprite { get; private set; }
        [field: SerializeField] public Vector2 CursorIconOffset { get; private set; } = new(0f, -58f);
        [field: SerializeField] public Vector2 TargetIconOffset { get; private set; } = Vector2.zero;
        [field: SerializeField, Min(1f)] public float CursorIconSize { get; private set; } = 48f;
        [field: SerializeField, Min(1f)] public float TargetIconSize { get; private set; } = 72f;
    }
}
