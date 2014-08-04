Shader "Custom/Life" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TEnd ("End Time", Float) = 60.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma target 5.0
		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;
		float _TEnd;

		#ifdef SHADER_API_D3D11
		struct Clock {
			float2 uv;
			float t;
		};
		StructuredBuffer<Clock> clocksCurr;
		int id;
		#endif

		struct Input {
			float2 uv_MainTex;
		};
		
		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			#ifdef SHADER_API_D3D11
			Clock c = clocksCurr[id];
			v.vertex.xyz += v.normal * 2.0 * saturate(c.t / _TEnd);
			#endif
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack off
}
