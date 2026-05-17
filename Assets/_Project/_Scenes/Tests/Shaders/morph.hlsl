// morph.hlsl
// Vertex deformation for 2D sprites: wavy droplet contour in XY plane.
// Для использования в Shader Graph через Custom Function node (Vertex stage).
//
// Деформирует края спрайта в плоскости XY — контур становится волнистым,
// имитируя пульсирующую каплю. Вершины смещаются радиально от центра:
// вдоль направления (vertex.xy - center) наружу или внутрь.
//
// Составляющие деформации (суммируются):
//   wave1  — основная радиальная волна: sin(dist * freq - time * speed)
//   wave2  — вторая гармоника для сложного узора
//   drop   — гауссов бугор (капля) в центре: DropStrength * exp(-dist² * DropWidth)
//   noise  — псевдошум для органичности
//
// Подключение в Shader Graph:
//   1. Custom Function node → Type: File → File: morph.hlsl → Function: MorphDrop2D_float
//   2. Inputs:
//      - Position: Vector2 — от Vertex Position (Object Space)
//      - Center: Vector2 (0, 0) — центр деформации
//      - Speed, Amplitude, Frequency, DropStrength, DropWidth, NoiseMix: Float
//      - Time: Float — Time node
//   3. Outputs: OutPosition: Vector3 → Vertex Position
//
// Параметры:
//   Position    — позиция вершины XY в Object Space
//   Center      — центр деформации XY
//   Speed       — скорость волн (0.5–5)
//   Amplitude   — сила деформации контура (0.02–0.3)
//   Frequency   — количество волн (2–15)
//   DropStrength— вытяжение центра в каплю (0–0.5)
//   DropWidth   — ширина капли (0.5–10)
//   NoiseMix    — хаотичная рябь (0–0.2)
//   Time        — время анимации (Time node)

void MorphDrop2D_float(
    float2 Position,
    float2 Center,
    float Speed,
    float Amplitude,
    float Frequency,
    float DropStrength,
    float DropWidth,
    float NoiseMix,
    float Time,
    out float3 OutPosition
)
{
    float2 dir = Position - Center;
    float dist = length(dir);
    float2 dirNorm = dist > 0.0001 ? dir / dist : float2(0, 0);

    // Primary radial wave (основная радиальная волна)
    float wave1 = sin(dist * Frequency - Time * Speed) * Amplitude;

    // Secondary harmonic (вторая гармоника для сложности узора)
    float wave2 = sin(dist * Frequency * 2.17 - Time * Speed * 1.37) * Amplitude * 0.35;

    // Droplet bulge — гауссово вытяжение в центре (капля)
    float drop = DropStrength * exp(-dist * dist * DropWidth);

    // Noise-like perturbation — псевдошум из двух синусов
    float noise = sin(dist * 7.3 + Time * 0.5)
                * sin(dist * 11.7 + Time * 0.7)
                * NoiseMix;

    // Суммарное радиальное смещение
    float displacement = wave1 + wave2 + drop + noise;

    // Смещаем вершину вдоль направления от центра (в плоскости XY)
    OutPosition = float3(Position + dirNorm * displacement, 0);
}
