// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/VertexColor/Monochrome" {
   Properties {
	  [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
	  [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
	  [MaterialToggle] _ZWrite ("Z Write", Float) = 1
	}
	
	SubShader {
	  Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
	 Pass
		{
			Cull [_CullMode]
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//https://www.wikiwand.com/en/Relative_luminance
			const float3 _toLuminance = float3(0.2126, 0.7152, 0.0722);

			struct appdata {
				float4 position : POSITION;
				float4 colour : COLOR;
			};
			
			struct v2f {
				float4 position : SV_POSITION;
				float4 colour : COLOR;
			};

			v2f vert (appdata v)
			{				
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.colour = v.colour;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float lum = dot(i.colour.xyz, _toLuminance);
				return half4(lum, lum, lum, 1.0);
			}
			ENDCG
		}
	}
}
