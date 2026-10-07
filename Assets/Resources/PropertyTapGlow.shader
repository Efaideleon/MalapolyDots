Shader "Malapoly/Property Tap Pulse"
{
    Properties
    {
        _EffectTime ("Animation Time", Float) = 0
        _Hovered ("Hovered", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _EffectTime;
                float _Hovered;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 uv : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Reuse the footprint's base band, leaving its monopoly walls untouched.
                clip(0.5 - input.uv.z);
                float edge = abs(input.uv.y);
                float core = exp2(-edge * edge * 32);
                float halo = exp2(-edge * edge * 5) * (1 - smoothstep(0.7, 1, edge));
                float pulse = lerp(0.5 + 0.22 * sin(_EffectTime * 2.6), 1, _Hovered);
                return half4(float3(0.85, 0.98, 1) * 1.8, (core * 0.8 + halo * 0.2) * pulse);
            }
            ENDHLSL
        }
    }
}
