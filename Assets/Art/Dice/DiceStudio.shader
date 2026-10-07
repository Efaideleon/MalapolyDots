Shader "Malapoly/Dice/Studio"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _Smoothness ("Polish", Range(0,1)) = .3
        _Metallic ("Metallic", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; half3 normalVS : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalVS = TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Fixed camera-space studio lighting keeps the result readable on every board camera.
                half3 n = normalize(input.normalVS);
                half3 light = normalize(half3(-.45, .75, .65));
                half diffuse = saturate(dot(n, light));
                half highlight = pow(saturate(dot(n, normalize(light + half3(0,0,1)))), lerp(16, 80, _Smoothness));
                half3 color = _BaseColor.rgb * (.32 + .66 * diffuse);
                color += lerp(half3(1,1,1), _BaseColor.rgb, _Metallic) * highlight * (.045 + _Metallic * .2);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
