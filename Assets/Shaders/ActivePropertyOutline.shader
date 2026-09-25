Shader "Malapoly/Active Property Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1, 0.425, 0.91, 1)
        _OutlineWidth("Outline Width (pixels)", Range(0, 8)) = 4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half4 _OutlineColor;
        float _OutlineWidth;
        CBUFFER_END
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings MaskVertex(Attributes input)
        {
            UNITY_SETUP_INSTANCE_ID(input);
            Varyings output;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            return output;
        }
        // Offset the silhouette in screen space so split normals and LOD seams cannot open gaps.
        Varyings OffsetVertex(Attributes input, float2 direction)
        {
            Varyings output = MaskVertex(input);
            output.positionCS.xy += direction * (2.0 * _OutlineWidth / _ScaledScreenParams.xy) * output.positionCS.w;
            return output;
        }
        Varyings OutlineVertex0(Attributes input) { return OffsetVertex(input, float2(1,0)); }
        Varyings OutlineVertex1(Attributes input) { return OffsetVertex(input, float2(-1,0)); }
        Varyings OutlineVertex2(Attributes input) { return OffsetVertex(input, float2(0,1)); }
        Varyings OutlineVertex3(Attributes input) { return OffsetVertex(input, float2(0,-1)); }
        Varyings OutlineVertex4(Attributes input) { return OffsetVertex(input, float2(0.7071,0.7071)); }
        Varyings OutlineVertex5(Attributes input) { return OffsetVertex(input, float2(-0.7071,0.7071)); }
        Varyings OutlineVertex6(Attributes input) { return OffsetVertex(input, float2(0.7071,-0.7071)); }
        Varyings OutlineVertex7(Attributes input) { return OffsetVertex(input, float2(-0.7071,-0.7071)); }
        half4 Fragment(Varyings input) : SV_Target { return _OutlineColor; }
        ENDHLSL
        Pass
        {
            Name "Selection Mask"
            Cull Off
            ZWrite Off
            ZTest LEqual
            ColorMask 0
            Stencil { Ref 1 ReadMask 1 WriteMask 1 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex MaskVertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 1"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex0
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 2"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex1
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 3"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex2
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 4"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex3
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 5"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex4
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 6"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex5
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 7"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex6
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Outline 8"
            Cull Off
            ZWrite Off
            ZTest LEqual
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex OutlineVertex7
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }
    }
}
