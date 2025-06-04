Shader "Hidden/TestMaskWriteRed" {
  SubShader {
    Tags { "RenderType"="Opaque" "LightMode"="UniversalForward" }
    Pass {
      ZWrite On
      ZTest LEqual
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      struct Attributes { float4 pos : POSITION; };
      struct Varyings { float4 posCS : SV_POSITION; };

      Varyings vert(Attributes v) {
        Varyings o;
        o.posCS = UnityObjectToClipPos(v.pos);
        return o;
      }

      fixed4 frag(Varyings i) : SV_Target {
        return fixed4(1, 0, 0, 1);
      }
      ENDHLSL
    }
  }
}