// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3176903,fgcg:0.4698783,fgcb:0.8308824,fgca:1,fgde:0.01,fgrn:8,fgrf:23,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-8520-OUT,alpha-1756-A;n:type:ShaderForge.SFN_Tex2d,id:1188,x:31998,y:32611,ptovrint:False,ptlb:_MainTex,ptin:__MainTex,varname:node_1188,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_VertexColor,id:1756,x:32092,y:32901,varname:node_1756,prsc:2;n:type:ShaderForge.SFN_Blend,id:3247,x:32315,y:32824,varname:node_3247,prsc:2,blmd:6,clmp:False|SRC-1756-RGB,DST-6450-OUT;n:type:ShaderForge.SFN_Color,id:8568,x:32111,y:32461,ptovrint:False,ptlb:Flash,ptin:_Flash,varname:node_8568,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:6450,x:32538,y:32609,varname:node_6450,prsc:2|A-1188-RGB,B-3415-RGB;n:type:ShaderForge.SFN_Color,id:3415,x:31942,y:32821,ptovrint:False,ptlb:Tint,ptin:_Tint,varname:node_3415,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Blend,id:8520,x:32523,y:32824,varname:node_8520,prsc:2,blmd:6,clmp:True|SRC-3247-OUT,DST-8568-RGB;proporder:8568-1188-3415;pass:END;sub:END;*/

Shader "Shader Forge/UnlitVertexAlpha_Diffuse" {
    Properties {
        _Flash ("Flash", Color) = (0.5,0.5,0.5,1)
        __MainTex ("_MainTex", 2D) = "white" {}
        _Tint ("Tint", Color) = (0.5,0.5,0.5,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D __MainTex; uniform float4 __MainTex_ST;
            uniform float4 _Flash;
            uniform float4 _Tint;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 __MainTex_var = tex2D(__MainTex,TRANSFORM_TEX(i.uv0, __MainTex));
                float3 node_3247 = (1.0-(1.0-i.vertexColor.rgb)*(1.0-(__MainTex_var.rgb*_Tint.rgb)));
                float3 emissive = saturate((1.0-(1.0-node_3247)*(1.0-_Flash.rgb)));
                float3 finalColor = emissive;
                return fixed4(finalColor,i.vertexColor.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
