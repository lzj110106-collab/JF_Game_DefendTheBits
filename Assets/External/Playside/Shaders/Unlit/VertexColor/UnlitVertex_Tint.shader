// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/VertexColor/Tint" {
   	Properties {
   	  _Color ("Color", Color) = (1,1,1,1)
	  [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
      [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
      [MaterialToggle] _ZWrite ("Z Write", Float) = 1
	}
	SubShader {
	  Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
	 Pass
		{
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			Cull [_CullMode]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			float4 _Color;

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.color = v.color;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				return i.color *_Color;
			}
			ENDCG
		}
	}
}
