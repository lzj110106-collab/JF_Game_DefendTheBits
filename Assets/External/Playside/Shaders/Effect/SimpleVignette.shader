// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/SimpleVignette" {
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", Float) = 1
	   [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend Mode", Float) = 0.0 // Zero
	   [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dest Blend Mode", Float) = 3.0 // SrcColor
	}

	Subshader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
	 	Pass {

		ZTest Always Cull Back ZWrite Off
		Fog { Mode off }      

		//Blend Zero SrcColor
		Blend [_SrcBlend] [_DstBlend]

		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};
		
		sampler2D _MainTex;
		fixed _Intensity;
		float4 _Color;

		v2f vert( appdata_full v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord.xy;
			o.color = v.color;

			return o;
		} 
			
		half4 frag(v2f i) : COLOR {
			half2 coords = i.uv;
			half2 uv = i.uv;
			
			coords = (coords - 0.5) * 2.0;		
			half coordDot = dot (coords,coords);

			float mask = 1.0 - coordDot * 0.1; 
			
			//fixed4 vignette = half4(0,0,0,1);
			//vignette = lerp (half4(1,1,1,1), vignette, saturate (_Intensity * coordDot));
			fixed4 col = _Color;
			col = lerp (half4(1,1,1,1), col, saturate (_Intensity * coordDot));
			col += 1-i.color.a;
			col.a = col.a * mask;

			return col;
		}
		ENDCG 
  	}
}
	Fallback off	
} 