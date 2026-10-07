Shader "Malapoly/Completed Property Group Glow"
{
    Properties
    {
        [HDR] _GroupColor ("Property Group Color", Color) = (0.1, 0.8, 1, 1)
        _EffectTime ("Animation Time", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" }
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
                float4 _GroupColor;
                float _EffectTime;
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
                float pulse = 0.82 + 0.18 * sin(_EffectTime * 2.2);
                float alpha;
                float highlight;
                if (input.uv.z < 0.5)
                {
                    float edge = abs(input.uv.y);
                    highlight = exp2(-edge * edge * 45);
                    float halo = exp2(-edge * edge * 6) * (1 - smoothstep(0.7, 1, edge));
                    alpha = (highlight * 0.7 + halo * 0.28) * pulse;
                }
                else
                {
                    float height = input.uv.y;
                    float fade = (1 - height) * (1 - height);
                    float streak = pow(saturate(sin(input.uv.x * 3 - _EffectTime * 0.8)), 14);
                    float rise = pow(saturate(sin(height * 10 - _EffectTime * 3 + input.uv.x)), 8);
                    highlight = streak * rise;
                    alpha = fade * (0.12 + highlight * 0.55) * pulse;
                }
                float3 tint = lerp(_GroupColor.rgb, float3(1, 0.92, 0.65), highlight * 0.35);
                return half4(tint * 2, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
