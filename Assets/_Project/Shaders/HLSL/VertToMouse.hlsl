// VertToMouse.hlsl
// Include: вертексная деформация спрайта — крайние вершины оттягиваются к мыши.
// Используется в VertToMouse.shader.
//
// Behaviour:
//   Owner: Shader/VertToMouse
//   Caller: VertToMouse.shader vertex shaders
//   Why not service/root: Вертексный эффект материала спрайта

#ifndef VERT_TO_MOUSE_INCLUDED
#define VERT_TO_MOUSE_INCLUDED

// Возвращает positionOS, смещённый к mousePositionOS.
//   positionOS      — исходная позиция вершины в Object Space
//   mousePositionOS — позиция мыши в Object Space
//   strength        — сила притяжения (0 = нет эффекта)
//   falloff         — степень расстояния от центра (1 = линейно, >1 = углы сильнее)
float3 VertToMouse_Apply_float(float3 positionOS, float3 mousePositionOS, float strength, float falloff)
{
    float distFromCenter = length(positionOS);

    float3 toMouse = mousePositionOS - positionOS;
    float toMouseLen = length(toMouse);

    float3 dirToMouse = toMouseLen > 0.0001f
        ? toMouse / toMouseLen
        : float3(0.0f, 0.0f, 0.0f);

    float weight = pow(distFromCenter, falloff);

    return positionOS + dirToMouse * weight * strength;
}

half3 VertToMouse_Apply_half(half3 positionOS, half3 mousePositionOS, half strength, half falloff)
{
    half distFromCenter = length(positionOS);

    half3 toMouse = mousePositionOS - positionOS;
    half toMouseLen = length(toMouse);

    half3 dirToMouse = toMouseLen > 0.0001h
        ? toMouse / toMouseLen
        : half3(0.0h, 0.0h, 0.0h);

    half weight = pow(distFromCenter, falloff);

    return positionOS + dirToMouse * weight * strength;
}

#endif // VERT_TO_MOUSE_INCLUDED
