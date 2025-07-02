// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "PlaySide/Unlit/Shadow/3PointLight" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	  _Color ("Main Color", Color) = (1,1,1,1)
	  _ColorL ("Color LEFT", Color) = (1,1,1,1)
	  _ColorR ("Color RIGHT", Color) = (1,1,1,1)
	}
	
	SubShader {
	  Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
	 Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed3 _Color;
			fixed3 _ColorL;
			fixed3 _ColorR;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 color : COLOR;
				UNITY_FOG_COORDS(n)
				SHADOW_COORDS(1)
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				float3 worldNormal = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz;

				o.pos = UnityObjectToClipPos (v.vertex); 
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);

				// Tint X+/- ColR, tint Z+/- ColL, keep Y+/- white
				o.color = saturate	( abs(worldNormal.y) + v.color.rgb*
										(
											lerp( float3(1,1,1), _ColorR, abs(worldNormal.x) )
										* 	lerp( float3(1,1,1), _ColorL, abs(worldNormal.z) )
										)
									);
				TRANSFER_SHADOW(o);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half3 shadow = 1+(SHADOW_ATTENUATION(i)-1) * (SHADOW_ATTENUATION(i)-1) * (UNITY_LIGHTMODEL_AMBIENT.rgb-1);
				half3 col = tex2D(_MainTex, i.uv.xy).rgb* i.color * shadow;
				UNITY_APPLY_FOG(i.fogCoord, col);
				
				return half4(col, 1);
			}
			ENDCG
		}
	}
	FallBack "VertexLit"
}
