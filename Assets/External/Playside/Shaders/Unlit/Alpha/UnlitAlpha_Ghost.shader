// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/Alpha/Ghost" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	  _Ramp ("Color Ramp", 2D) = "white" {}
	  _RampBlend ("Ramp Overlay", Range(0, 1)) = 1
	  _RimAmount ("Rim Amounty", Float) = 1
      [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend Mode", Float) = 5.0 // SrcAlpha
      [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dest Blend Mode", Float) = 10.0 // OneMinusSrcAlpha
      [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
      [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
	}
	
	SubShader {
	  Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
	  // Tags { "RenderType" = "Opaque" }
	 Pass
		{
			ZTest [_ZTest]
			ZWrite Off
			AlphaTest Off
			Blend [_SrcBlend] [_DstBlend]
			Cull [_CullMode]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Ramp;
			half _RampBlend;
			half _RimAmount;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR; 
				half4 normal : TEXCOORD1;
				half3 viewDir : TEXCOORD2;
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex); 
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.color = v.color;
				o.normal = mul(unity_ObjectToWorld, v.normal);
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half2 uvramp = half2(saturate(1.0 - pow(dot(i.normal ,i.viewDir ), _RimAmount)), 0);
				//half2 uvramp = half2(i.pos.y, 0);
				half4 ramp = tex2D(_Ramp, uvramp.xy);
				half4 col = tex2D(_MainTex, i.uv.xy) * i.color;
				ramp.rgb += col.rgb;
				ramp.a *= col.a;
				col = lerp(col, ramp, _RampBlend);
				//col.rgb += uvramp.x;
				//col.a = 1;
				
				//col.rgb +=  i.rim;
				return col;
			}
			ENDCG
		}
	}
}
