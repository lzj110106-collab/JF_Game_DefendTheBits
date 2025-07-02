// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/Shadow/Dissolve" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	  _Color ("Main Color", Color) = (1,1,1,1)
	  _TargetPos("Target Position", Vector) = (0,0,0,0)
	  _Progress("Evaporate Progress", Float) = 1.0
	  _MaxOffset("Max Offset", Float) = 1.0
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
			fixed3 _TargetPos;
			fixed _Progress;
			fixed _MaxOffset;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				SHADOW_COORDS(1)
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = v.vertex;
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.color = v.color;

				float dist = min(distance(_TargetPos, o.pos), _MaxOffset);
				float mask = (o.color.a *0.5 * dist);
				o.pos.y = clamp(o.pos.y * _Progress *mask , o.pos.y, 1+o.pos.y); 

				o.pos = UnityObjectToClipPos (o.pos);
				TRANSFER_SHADOW(o);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half3 shadow = 1+(SHADOW_ATTENUATION(i)-1) * (SHADOW_ATTENUATION(i)-1) * (UNITY_LIGHTMODEL_AMBIENT.rgb-1);
				half3 col = tex2D(_MainTex, i.uv.xy).rgb* i.color * shadow;


				return half4(col, 1);
			}
			ENDCG
		}
	}
	FallBack "VertexLit"
}


