// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Particles/PointCloud" {
        Properties
        {
            _Color ("MainColor", Color) = (1,1,1,1)
            _Size ("Point Size", Range(1,10)) = 1
        }
     SubShader {
         Tags { "RenderType"="Transparent" "Queue"="Transparent" }
         LOD 200
         ZWrite OFF
         ZTest LEqual
         Cull Off
         Blend SrcAlpha OneMinusSrcAlpha

     Pass {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #include "UnityCG.cginc"

         half4 _Color;
         half _Size;

         struct appdata {
             float4 v : POSITION;
             float4 color: COLOR;
         };

         struct v2f {
             float4 pos : SV_POSITION;
             float4 col : COLOR;
             float size : PSIZE;
         };

         v2f vert(appdata v) {
             v2f o;
             o.pos = UnityObjectToClipPos(v.v);
             o.col = v.color*_Color;
             o.size = _Size;
             return o;
         }

         float4 frag(v2f o) : COLOR {
             return o.col;
         }

         ENDCG
         } 
     }

 }