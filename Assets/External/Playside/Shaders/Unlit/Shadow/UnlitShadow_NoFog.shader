// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/Shadow/Diffuse_NoFog" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	  _Color ("Main Color", Color) = (1,1,1,1)
	}
	
	SubShader {
	  Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
	 Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            uniform float4 _LightColor0; 
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed3 _Color;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				half3 color : COLOR;
				SHADOW_COORDS(1)
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex); 
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.color = v.color.rgb;
				TRANSFER_SHADOW(o);
				return o;
			}
			
			half3 frag (v2f i) : COLOR
			{
				half3 shadow = 1+(SHADOW_ATTENUATION(i)-1) * (SHADOW_ATTENUATION(i)-1) * (UNITY_LIGHTMODEL_AMBIENT.rgb-1);
				half3 col = tex2D(_MainTex, i.uv.xy).rgb* i.color * shadow;

				return col;
			}
			ENDCG
		}
	}
	FallBack "VertexLit"
}


