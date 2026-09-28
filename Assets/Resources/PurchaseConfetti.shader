Shader "Malapoly/Purchase Confetti"
{
    Properties
    {
        _ConfettiColor ("Confetti Color", Color) = (1, 1, 1, 1)
        _EmissionStrength ("Confetti Brightness", Range(1, 8)) = 3
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _ConfettiColor;
                float _EmissionStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Each emitter has its own hue, independent of particle vertex colors and lights.
                return half4(_ConfettiColor.rgb * _EmissionStrength, _ConfettiColor.a);
            }
            ENDHLSL
        }
    }
}
