// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/SimpleVignetteAlpha" {
	Properties
	{
		_Intensity ("Intensity", Float) = 1
		_Pinch ("Pinch", Float) = 0.1

	   [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend Mode", Float) = 5.0 // SrcAlpha
	   [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dest Blend Mode", Float) = 10.0 // OneMinusSrcAlpha
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
			float4 scrPos : TEXCOORD0;
			float4 color : COLOR;
		};
		
		sampler2D _MainTex;
		fixed _Intensity;
		fixed _Pinch;
		float4 _Color;

		v2f vert( appdata_full v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			//o.uv = v.texcoord.xy;
			o.color = v.color;
			o.scrPos = ComputeScreenPos(o.pos);

			return o;
		} 
			
		half4 frag(v2f i) : COLOR
		{
			// Vignette usin UV coord edges
			half2 coords = (i.scrPos.xy/i.scrPos.w);//i.uv;
			coords = (coords - 0.5) * 2.0;	

			half coordDot = dot (coords,coords);
			float mask = lerp( 0, coordDot * _Pinch, saturate (_Intensity * coordDot) );

			return fixed4(i.color.rgb, mask*i.color.a);
		}
		ENDCG 
  	}
}
	Fallback off	
} 