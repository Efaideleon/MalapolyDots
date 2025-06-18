Shader "Unlit/ColorBlurShader"
{
    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        
        float _VerticalBlur;
        float _HorizontalBlur;

        uniform float3 _OffSetTexture;

        float4 BlurVertical(Varyings input) : SV_Target
        {
            // how many samples each side
            const int samples = 64;
            const float range = samples / 2.0;
            float2 uv = input.texcoord;

            // convert blur fraction to pixels
            float blurPixels = _VerticalBlur * _ScreenParams.y;
            float stepY = (blurPixels / _BlitTexture_TexelSize.w) / range;

            // start with “nothing”
            float3 outline = float3(0, 0, 0);

            // for each offset sample, pick the brighter (max) component-wise
            for (int i = -samples / 2; i <= samples / 2; ++i)
            {
                float2 offset = float2(0, stepY * i);
                float3 sample = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset).rgb;
                outline = max(outline, sample);
            }

            return float4(outline, 1);
        }

        float4 BlurHorizontal( Varyings input ) : SV_Target
        {
            const int samples = 64;
            const float range  = samples / 2.0;
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;
            float3 outline = float3(0,0,0);

            // convert blur fraction to pixels
            float blurPixels = _HorizontalBlur * _ScreenParams.x;
            float stepX      = (blurPixels / _BlitTexture_TexelSize.z) / range;

            for ( int i = -samples/2; i <= samples/2; ++i )
            {
                float2 offset = float2(stepX * i, 0);
                float3 sample = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset).rgb;
                outline = max(outline, sample);
            }

            return float4(outline, 1);
        }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "BlurPassVertical"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment BlurVertical
            
            ENDHLSL
        }
        
        Pass
        {
            Name "BlurPassHorizontal"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment BlurHorizontal
            
            ENDHLSL
        }
    }
}

