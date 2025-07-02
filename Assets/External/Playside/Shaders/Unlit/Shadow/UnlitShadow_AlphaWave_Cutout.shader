// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "PlaySide/Unlit/Shadow/Alpha/Wave_Cutout" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Amplitude("Amplitude", float) = 0.25
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader
	{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 100
	
		// Diffuse Pass
		Pass
		{
			Name "Diffuse"
			Tags { "LightMode" = "Vertex" }

			ZWrite On
			Alphatest Greater [_Cutoff]
			AlphaToMask True
			Cull Back
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _LightColor0; 
			uniform float _Cutoff;
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
				o.color.a = 1;

				o.pos = UnityObjectToClipPos(mul(unity_WorldToObject, worldPos) );
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.uv.xy)* i.color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				clip( col.a - _Cutoff );

				return col;
			}
			ENDCG
		}

		// Shadow Caster Pass
		Pass
		{
			Name "Caster"
			Tags { "LightMode" = "ShadowCaster" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f { 
				V2F_SHADOW_CASTER;
				float2  uv : TEXCOORD1;
			};

			uniform float4 _MainTex_ST;

			v2f vert( appdata_base v )
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			uniform sampler2D _MainTex;
			uniform fixed _Cutoff;
			uniform fixed4 _Color;

			float4 frag( v2f i ) : SV_Target
			{
				fixed4 texcol = tex2D( _MainTex, i.uv );
				clip( texcol.a*_Color.a - _Cutoff );
				
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}