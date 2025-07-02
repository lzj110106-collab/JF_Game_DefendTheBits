// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/PS_FX_OldScreen"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ChannelShift ("Shift", Float) = 0.001
		_Intensity ("Intensity", Float) = 1
		_Pinch ("Pinch", Float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 uvRB : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			half _ChannelShift;
			fixed _Intensity;
			fixed _Pinch;
			sampler2D _MainTex;

			v2f vert (appdata v)
			{ 
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = v.uv;

				float uR = v.uv.x + _ChannelShift;
				float uB = v.uv.x - _ChannelShift;
				o.uvRB = float4(uR, v.uv.y, uB, v.uv.y);
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
				col.a = 1;

				// Vignnette mask
				half2 coords = i.uv;
				coords = (coords - 0.5) * 2.0;		
				half coordDot = dot (coords,coords);
				float mask = 1-pow(1.0 - coordDot * _Pinch, (_Intensity*coordDot) ); 

				// UV Fisheye offset bulge
				float2 uvBulge = float2(i.uv.x, i.uv.y) - 0.5;
				uvBulge = uvBulge * (mask - 1) * _Intensity;

				// Chromatic Abberation
				fixed colR = tex2D(_MainTex, i.uvRB.xy + uvBulge).r;
				fixed colG = tex2D(_MainTex, i.uv + uvBulge).g;
				fixed colB = tex2D(_MainTex, i.uvRB.zw + uvBulge).b;

				col.rgb = float3(colR, colG, colB);

				// Multiply vignette
				col.rgb *= 1-mask;

				return col;
			}
			ENDCG
		}
	}
}
