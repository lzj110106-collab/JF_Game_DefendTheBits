// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "PlaySide/Unlit/Alpha/Wave_Cutout" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	  _Amplitude("Amplitude", float) = 0.25
      _CutOff("Cut off", float) = 0.5
      [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
	}
	
	SubShader {
	  Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
	 Pass
		{
			ZWrite Off
			AlphaTest Off
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float _CutOff;
			uniform float _Amplitude;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR; 
				UNITY_FOG_COORDS(n)
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.color = v.color;
				
				float4 worldPos = mul (unity_ObjectToWorld, v.vertex);
				worldPos.y += (sin(_Time.y-worldPos.x)) * _Amplitude * o.color.a;


				//sin( _Time.y / (worldPos.x/worldPos.y) ) * o.color.a;


				o.pos = UnityObjectToClipPos(mul(unity_WorldToObject, worldPos) );
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.uv.xy);
				col.rgb *= i.color.rgb;
				UNITY_APPLY_FOG(i.fogCoord, col);

				half alpha = col.a;
				col.a = 1;
				if(alpha < _CutOff)
					col.a = 0;
					
				return col;
			}
			ENDCG
		}
	}
}
