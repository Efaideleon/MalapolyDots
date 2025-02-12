Shader "Unlit/NewUnlitShaderOutlinePlayer"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1) // Outline color
        _OutlineWidth("Outline Width", Float) = 0.1          // Outline width
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // First pass: Regular unlit shader rendering
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Second pass: Outline rendering
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="Always" }
            Cull Front // Cull front faces to create outline behind object
            ZWrite On
            ZTest LEqual
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert_outline
            #pragma fragment frag_outline

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            v2f vert_outline(appdata v)
            {
                v2f o;
                // Transform object-space position to world-space
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // Transform object-space normal to world-space
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                // Offset vertex position along the normal
                worldPos += worldNormal * _OutlineWidth;
                // Transform world-space position to clip-space
                o.vertex = UnityWorldToClipPos(worldPos);
                return o;
            }

            fixed4 frag_outline(v2f i) : SV_Target
            {
                // Return the outline color
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
