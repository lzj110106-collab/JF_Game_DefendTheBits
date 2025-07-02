// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PlaySide/Unlit/VertexColor/Diffuse_NoFog" {
   	Properties {
	  [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0 // Back
      [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4.0 // LessEqual
      [MaterialToggle] _ZWrite ("Z Write", Float) = 1
	}
	SubShader {
	  Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
	 Pass
		{
			ZTest [_ZTest]
			ZWrite [_ZWrite] 
			Fog {Mode Off} 
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				UNITY_FOG_COORDS(n)
			};

			v2f vert (appdata_full v)
			{				
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float4 col = i.color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
