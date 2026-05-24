Shader "Custom/VertToMouseShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Diffuse", 2D) = "white" {}
        [PerRendererData] _MaskTex("Mask", 2D) = "white" {}
        [PerRendererData] _NormalMap("Normal Map", 2D) = "bump" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [Header(Vertex Pull to Mouse)]
        _MousePosition("Mouse Position (Object Space)", Vector) = (0,0,0,0)
        _PullStrength("Pull Strength", Float) = 0.5
        _PullFalloff("Pull Falloff", Float) = 1.0
        _PullArc("Pull Arc (degrees, 0-360)", Range(0, 360)) = 180
        _PullTaper("Pull Taper (0=flat, 1=teardrop)", Range(0, 1)) = 0.0
        _PullRadius("Mouse Dead Radius (fade near center)", Float) = 0.0

        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        // ────────────────────────────────────────────────────────
        // Pass 1 — Universal2D (Lit)
        // ────────────────────────────────────────────────────────
        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color        : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MousePosition;
                float _PullStrength;
                float _PullFalloff;
                float _PullArc;
                float _PullTaper;
                float _PullRadius;
            CBUFFER_END

            // ── Вертексная деформация ──

            // Смещает positionOS в сторону mousePositionOS.
            //   strength — общая сила
            //   falloff  — степень расстояния от центра (1=линейно, >1=углы сильнее центра)
            //   pullArc  — арка в градусах: 0 = только вершина строго в направлении мыши,
            //               180 = все на стороне мыши, 360 = все вершины.
            //   taper    — 0 = все вершины внутри арки тянутся одинаково;
            //               1 = капля: чем меньше угол к мыши, тем сильнее.
            //   radius   — радиус от центра: эффект плавно угасает, если мышь ближе radius.
            //
            // Расчёт: определяем угол между направлением на вершину и на мышь через dot.
            // Если вершина внутри арки (cos(angle) > cos(halfArc)) — тянем.
            // Внутри арки taper создаёт градиент от мыши к краям арки.
            // Вне арки — оставляем на месте.
            float3 ApplyMousePull(float3 positionOS, float3 mousePositionOS, float strength, float falloff, float pullArc, float taper, float radius)
            {
                float dist = length(positionOS);
                float3 dirFromCenter = dist > 0.0001f ? positionOS / dist : float3(0, 0, 0);

                float mouseDist = length(mousePositionOS);
                float3 dirToMouse = mouseDist > 0.0001f ? mousePositionOS / mouseDist : float3(0, 0, 0);

                float alignment = dot(dirFromCenter, dirToMouse);

                // Половина арки в радианах
                float halfArc = pullArc * 0.5f * 3.14159265f / 180.0f;

                // Мягкий край: 2° плавного перехода
                const float EDGE_DEG = 2.0f * 3.14159265f / 180.0f;
                float cosLow  = cos(halfArc + EDGE_DEG);
                float cosHigh = cos(max(halfArc - EDGE_DEG, 0.0f));
                float edgeWeight = smoothstep(cosLow, cosHigh, alignment);

                // Taper: градиент pull-силы от направления мыши к краям арки
                float arcBoundary = cos(halfArc);
                float t = saturate((alignment - arcBoundary) / (1.0 - arcBoundary));
                float taperExp = 1.0f + taper * 2.0f; // [1, 3]
                float taperWeight = lerp(1.0f, pow(t, taperExp), taper);

                float angleWeight = edgeWeight * taperWeight;

                // Dead radius: эффект угасает, если мышь слишком близко к центру
                float radiusWeight = radius > 0.0001f
                    ? smoothstep(0.0f, radius, mouseDist)
                    : 1.0f;

                float3 toVertex = mousePositionOS - positionOS;
                float toLen = length(toVertex);
                float3 moveDir = toLen > 0.0001f ? toVertex / toLen : float3(0, 0, 0);

                float distWeight = pow(dist, falloff);

                return positionOS + moveDir * distWeight * angleWeight * radiusWeight * strength;
            }

            // ── Vertex / Fragment ──

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                input.positionOS = ApplyMousePull(input.positionOS, _MousePosition, _PullStrength, _PullFalloff, _PullArc, _PullTaper, _PullRadius);

                Varyings o = CommonLitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;

                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                return CommonLitFragment(input, input.color);
            }

            ENDHLSL
        }

        // ────────────────────────────────────────────────────────
        // Pass 2 — NormalsRendering
        // ────────────────────────────────────────────────────────
        Pass
        {
            Tags { "LightMode" = "NormalsRendering" }

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_NORMALS_INPUTS
                float4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_NORMALS_OUTPUTS
                half4   color       : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Normals2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MousePosition;
                float _PullStrength;
                float _PullFalloff;
                float _PullArc;
                float _PullTaper;
                float _PullRadius;
            CBUFFER_END

            float3 ApplyMousePull(float3 positionOS, float3 mousePositionOS, float strength, float falloff, float pullArc, float taper, float radius)
            {
                float dist = length(positionOS);
                float3 dirFromCenter = dist > 0.0001f ? positionOS / dist : float3(0, 0, 0);

                float mouseDist = length(mousePositionOS);
                float3 dirToMouse = mouseDist > 0.0001f ? mousePositionOS / mouseDist : float3(0, 0, 0);

                float alignment = dot(dirFromCenter, dirToMouse);

                float halfArc = pullArc * 0.5f * 3.14159265f / 180.0f;
                const float EDGE_DEG = 2.0f * 3.14159265f / 180.0f;
                float cosLow  = cos(halfArc + EDGE_DEG);
                float cosHigh = cos(max(halfArc - EDGE_DEG, 0.0f));
                float edgeWeight = smoothstep(cosLow, cosHigh, alignment);

                float arcBoundary = cos(halfArc);
                float t = saturate((alignment - arcBoundary) / (1.0 - arcBoundary));
                float taperExp = 1.0f + taper * 2.0f;
                float taperWeight = lerp(1.0f, pow(t, taperExp), taper);

                float angleWeight = edgeWeight * taperWeight;

                float radiusWeight = radius > 0.0001f
                    ? smoothstep(0.0f, radius, mouseDist)
                    : 1.0f;

                float3 toVertex = mousePositionOS - positionOS;
                float toLen = length(toVertex);
                float3 moveDir = toLen > 0.0001f ? toVertex / toLen : float3(0, 0, 0);

                float distWeight = pow(dist, falloff);
                return positionOS + moveDir * distWeight * angleWeight * radiusWeight * strength;
            }

            Varyings NormalsRenderingVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                input.positionOS = ApplyMousePull(input.positionOS, _MousePosition, _PullStrength, _PullFalloff, _PullArc, _PullTaper, _PullRadius);

                Varyings o = CommonNormalsVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;

                return o;
            }

            half4 NormalsRenderingFragment(Varyings input) : SV_Target
            {
                return CommonNormalsFragment(input, input.color);
            }

            ENDHLSL
        }

        // ────────────────────────────────────────────────────────
        // Pass 3 — UniversalForward (Unlit fallback)
        // ────────────────────────────────────────────────────────
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MousePosition;
                float _PullStrength;
                float _PullFalloff;
                float _PullArc;
                float _PullTaper;
                float _PullRadius;
            CBUFFER_END

            float3 ApplyMousePull(float3 positionOS, float3 mousePositionOS, float strength, float falloff, float pullArc, float taper, float radius)
            {
                float dist = length(positionOS);
                float3 dirFromCenter = dist > 0.0001f ? positionOS / dist : float3(0, 0, 0);

                float mouseDist = length(mousePositionOS);
                float3 dirToMouse = mouseDist > 0.0001f ? mousePositionOS / mouseDist : float3(0, 0, 0);

                float alignment = dot(dirFromCenter, dirToMouse);

                float halfArc = pullArc * 0.5f * 3.14159265f / 180.0f;
                const float EDGE_DEG = 2.0f * 3.14159265f / 180.0f;
                float cosLow  = cos(halfArc + EDGE_DEG);
                float cosHigh = cos(max(halfArc - EDGE_DEG, 0.0f));
                float edgeWeight = smoothstep(cosLow, cosHigh, alignment);

                float arcBoundary = cos(halfArc);
                float t = saturate((alignment - arcBoundary) / (1.0 - arcBoundary));
                float taperExp = 1.0f + taper * 2.0f;
                float taperWeight = lerp(1.0f, pow(t, taperExp), taper);

                float angleWeight = edgeWeight * taperWeight;

                float radiusWeight = radius > 0.0001f
                    ? smoothstep(0.0f, radius, mouseDist)
                    : 1.0f;

                float3 toVertex = mousePositionOS - positionOS;
                float toLen = length(toVertex);
                float3 moveDir = toLen > 0.0001f ? toVertex / toLen : float3(0, 0, 0);

                float distWeight = pow(dist, falloff);
                return positionOS + moveDir * distWeight * angleWeight * radiusWeight * strength;
            }

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                input.positionOS = ApplyMousePull(input.positionOS, _MousePosition, _PullStrength, _PullFalloff, _PullArc, _PullTaper, _PullRadius);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                return CommonUnlitFragment(input, input.color);
            }

            ENDHLSL
        }
    }
}
