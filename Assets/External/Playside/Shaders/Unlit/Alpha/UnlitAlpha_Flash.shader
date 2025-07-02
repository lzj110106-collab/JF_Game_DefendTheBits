// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/Alpha/Diffuse_Flash" {
   Properties {
   _Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
		[MaterialToggle] _ZWrite ("Z Write", Float) = 1
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
	}
	
	SubShader {
	  Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
	 Pass
		{
			Cull [_CullMode]
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex); 
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.color = v.color;
				return o;
			}
			
			half4 frag (v2f i) : COLOR 
			{
				half4 col = tex2D(_MainTex, i.uv.xy) * i.color;
				col.rgb += _Color.rgb*_Color.a;				
				return col;
			}
			ENDCG
		}
	}
}
