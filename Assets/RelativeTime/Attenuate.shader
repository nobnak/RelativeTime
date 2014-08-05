Shader "Custom/Attenuate" {
	Properties {
		_Atten ("Attenuation", Float) = 0.01
	}
	SubShader {
		ZTest Always ZWrite Off Cull Off Fog { Mode Off }
		Blend Zero SrcAlpha
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Atten;
			
			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct vs2ps {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			vs2ps vert(appdata IN) {
				vs2ps OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}
			float4 frag(vs2ps IN) : COLOR {
				return float4(0.0, 0.0, 0.0, saturate(1.0 - _Atten));
			}
			ENDCG
		}
	} 
	FallBack off
}
