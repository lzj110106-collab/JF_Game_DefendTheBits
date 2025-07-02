// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "PlaySide/Unlit/VertexColor/Displace" {
   Properties {
   		_Tint ("Depth Tint", Color) = (1,1,1,1)
   		_Height ("Height", Float) = 1.0
   		_Offset ("Height Offset", Float) = 0.0
		_HeightMap ("Height Map", 2D) = "white" {}
		_Position ("Position", Vector) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
		[MaterialToggle] _ZWrite ("Z Write", Float) = 1
	}
	
	SubShader {
	  Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
	 Pass
		{
			Cull [_CullMode]
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Offset;
			float _Height;
			float4 _Tint;
			sampler2D _HeightMap;
			//float4 _HeightMap_ST; COMMENTED FOR UV OVERRIDE
			float2 _Position;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = v.vertex;
				//o.uv = TRANSFORM_TEX (v.texcoord, _HeightMap); COMMENTED FOR UV OVERRIDE
				o.color = v.color;

				// Override UV with 1/10th world position for seamless water
				float4 worldPos = mul(unity_ObjectToWorld, o.pos); 
				o.uv = _Position + worldPos.xz*.1;

				float texHeight = tex2Dlod(_HeightMap, float4(o.uv,0,0)).r;

				// Displace in Y
				o.pos.y += _Height * texHeight * o.color.a + _Offset;
				texHeight = 1-texHeight;
				
				// Tint troughs
				o.color.rgb = o.color.rgb*(1-texHeight*_Tint.a) + _Tint.rgb*(texHeight*_Tint.a);
				
				o.pos = UnityObjectToClipPos (o.pos);
				return o; 
			}
			
			half3 frag (v2f i) : COLOR
			{
				half3 col = i.color.rgb;// ;
				return col;
			}

			ENDCG
		}
	}
}
