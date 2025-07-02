// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/Alpha/Diffuse_NoFog" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
      [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend Mode", Float) = 5.0 // SrcAlpha
      [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dest Blend Mode", Float) = 10.0 // OneMinusSrcAlpha
      [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
      [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
	}
	
	SubShader {
	  Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
	 Pass
		{
			ZTest [_ZTest]
			ZWrite Off
			AlphaTest Off
			Blend [_SrcBlend] [_DstBlend]
			Cull [_CullMod]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
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
				
				return col;
			}
			ENDCG
		}
	}
}
