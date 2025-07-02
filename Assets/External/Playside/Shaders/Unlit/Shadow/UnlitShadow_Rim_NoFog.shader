Shader "PlaySide/Lit/Diffuse_Rim_NoFog" {
   Properties {
	  _MainTex ("Texture", 2D) = "white" {}
	  _Color ("Main Color", Color) = (1,1,1,1)
      _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      _RimPower ("Rim Power", Float) = 1.0
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert noambient nofog 

      sampler2D _MainTex;
      sampler2D _BumpMap;
      float4 _RimColor;
      float _RimPower;

      struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float3 viewDir;
          float4 color: COLOR;
      };


      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
          half rim = 1.0 - saturate( dot(normalize(IN.viewDir ), o.Normal));
          o.Emission =  lerp(o.Albedo, _RimColor.rgb * ( pow(rim, _RimPower) / (1-_RimColor.a) ), rim);
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }
