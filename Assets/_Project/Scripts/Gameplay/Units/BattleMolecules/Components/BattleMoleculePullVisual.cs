using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    /// <summary>
    ///     Управляет MaterialPropertyBlock для VertToMouse.shader на SpriteRenderer.
    ///     Преобразует мировую позицию мыши в Object Space и передаёт параметры деформации.
    ///
    /// Behaviour:
    ///   Owner: BattleMoleculePullVisual
    ///   Caller: BattleMoleculeAiming
    ///   Why not service/root: Перинстансный визуальный эффект материала
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BattleMoleculeAiming))]
    public class BattleMoleculePullVisual : MonoBehaviour
    {
        private static readonly int MousePositionId = Shader.PropertyToID("_MousePosition");
        private static readonly int PullStrengthId = Shader.PropertyToID("_PullStrength");
        private static readonly int PullFalloffId = Shader.PropertyToID("_PullFalloff");
        private static readonly int PullArcId = Shader.PropertyToID("_PullArc");
        private static readonly int PullTaperId = Shader.PropertyToID("_PullTaper");
        private static readonly int PullRadiusId = Shader.PropertyToID("_PullRadius");

        [Header("Shader Defaults")]
        [field: SerializeField] public float Strength { get; set; } = 0.5f;
        [field: SerializeField] public float Falloff { get; set; } = 1.0f;
        [field: SerializeField, Range(0, 360)] public float Arc { get; set; } = 180f;
        [field: SerializeField, Range(0, 1)] public float Taper { get; set; } = 0f;
        [field: SerializeField] public float Radius { get; set; } = 0f;

        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
        }

        private void Start()
        {
            // Ищем SpriteRenderer на себе или на первом ребёнке
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary> Включает эффект и выставляет константы (Strength, Falloff, Arc…). </summary>
        public void Show()
        {
            if (_spriteRenderer == null || _mpb == null)
                return;

            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(PullStrengthId, Strength);
            _mpb.SetFloat(PullFalloffId, Falloff);
            _mpb.SetFloat(PullArcId, Arc);
            _mpb.SetFloat(PullTaperId, Taper);
            _mpb.SetFloat(PullRadiusId, Radius);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }

        /// <summary> Обновляет позицию мыши (world → object space) для деформации. </summary>
        public void SetMouseWorldPosition(Vector3 worldPosition)
        {
            if (_spriteRenderer == null || _mpb == null)
                return;

            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetVector(MousePositionId, new Vector4(localPos.x, localPos.y, 0f, 0f));
            _spriteRenderer.SetPropertyBlock(_mpb);
        }

        /// <summary> Отключает эффект. </summary>
        public void Hide()
        {
            if (_spriteRenderer == null || _mpb == null)
                return;

            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetVector(MousePositionId, Vector4.zero);
            _mpb.SetFloat(PullStrengthId, 0f);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }
    }
}
