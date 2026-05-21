// DitherMonoBayer4x4_Screen.hlsl
// Цветной дизеринг с внешней текстурой паттерна.
// Scale — количество ячеек дизеринга по ширине экрана.
// Адаптируется к любому размеру/разрешению экрана автоматически.

float GetThresholdScreen(float2 UV, float Scale, UnityTexture2D PatternTex)
{
    float aspect = _ScreenParams.x / _ScreenParams.y;
    float2 blocks = float2(Scale, Scale / aspect);
    float2 cellUV = frac(UV * blocks);
    return SAMPLE_TEXTURE2D(PatternTex.tex, PatternTex.samplerstate, cellUV).r;
}

void DitherMonoBayer4x4_float(float3 Color, float2 UV, float Scale, UnityTexture2D PatternTex, float Threshold, float3 DitherColor, out float3 Out)
{
    float patternThreshold = GetThresholdScreen(UV, Scale, PatternTex);
    float gray = dot(Color, float3(0.299, 0.587, 0.114));
    // Threshold: 0 = норма, >0 = плотнее в тенях, <0 = плотнее в светах
    float adjustedThreshold = patternThreshold - Threshold * (1.0 - gray);
    Out = gray > adjustedThreshold ? Color : Color * DitherColor;
}