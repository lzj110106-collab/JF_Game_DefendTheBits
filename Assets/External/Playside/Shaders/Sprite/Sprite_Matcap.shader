// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:,iptp:0,cusa:True,bamd:0,lico:0,lgpr:1,limd:2,spmd:1,trmd:0,grmd:0,uamb:True,mssp:False,bkdf:False,hqlp:False,rprd:False,enco:True,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:1,fgcg:0.8470589,fgcb:0.5215687,fgca:1,fgde:0.01,fgrn:30.3,fgrf:145.6,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|diff-3229-OUT,spec-3076-OUT,gloss-923-OUT,normal-9854-RGB,emission-5937-OUT;n:type:ShaderForge.SFN_Tex2d,id:5894,x:31639,y:32530,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:node_5894,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9854,x:30841,y:32809,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_9854,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Multiply,id:7318,x:31828,y:33084,varname:node_7318,prsc:2|A-5671-RGB,B-3187-OUT;n:type:ShaderForge.SFN_Multiply,id:3076,x:32239,y:32709,varname:node_3076,prsc:2|A-5894-A,B-9094-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9094,x:32006,y:32743,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:node_9094,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:923,x:32378,y:32792,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:node_923,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:5937,x:32239,y:33073,varname:node_5937,prsc:2|A-3229-OUT,B-7318-OUT;n:type:ShaderForge.SFN_Color,id:3792,x:31639,y:32748,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_3792,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:3229,x:32006,y:32575,varname:node_3229,prsc:2|A-5894-RGB,B-3792-RGB;n:type:ShaderForge.SFN_ValueProperty,id:3187,x:31639,y:33224,ptovrint:False,ptlb:Reflect Intensity,ptin:_ReflectIntensity,varname:_Specular_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:5671,x:31639,y:32988,ptovrint:False,ptlb:MatCap,ptin:_MatCap,varname:node_5671,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False|UVIN-3537-OUT;n:type:ShaderForge.SFN_RemapRange,id:3537,x:31460,y:32988,varname:node_3537,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-4376-OUT;n:type:ShaderForge.SFN_ComponentMask,id:4376,x:31274,y:32988,varname:node_4376,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-1124-XYZ;n:type:ShaderForge.SFN_NormalVector,id:8128,x:30812,y:32986,prsc:2,pt:False;n:type:ShaderForge.SFN_Transform,id:1124,x:31103,y:32988,varname:node_1124,prsc:2,tffrom:0,tfto:3|IN-3192-OUT;n:type:ShaderForge.SFN_NormalBlend,id:3192,x:31014,y:33083,varname:node_3192,prsc:2|BSE-9854-RGB,DTL-8128-OUT;proporder:3792-5894-9854-9094-923-5671-3187;pass:END;sub:END;*/

Shader "PlaySide/Lit/Bump/MatCapReflect" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Specular ("Specular", Float ) = 1
        _Gloss ("Gloss", Float ) = 1
        _MatCap ("MatCap", 2D) = "black" {}
        _ReflectIntensity ("Reflect Intensity", Float ) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "CanUseSpriteAtlas"="True"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform float _Specular;
            uniform float _Gloss;
            uniform float4 _Color;
            uniform float _ReflectIntensity;
            uniform sampler2D _MatCap; uniform float4 _MatCap_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 normalLocal = _Normal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                float node_3076 = (_Diffuse_var.a*_Specular);
                float3 specularColor = float3(node_3076,node_3076,node_3076);
                float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b);
                float normTerm = (specPow + 2.0 ) / (2.0 * Pi);
                float3 directSpecular = attenColor * pow(max(0,dot(reflect(-lightDirection, normalDirection),viewDirection)),specPow)*normTerm*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 node_3229 = (_Diffuse_var.rgb*_Color.rgb);
                float3 diffuseColor = node_3229;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 node_3192_nrm_base = _Normal_var.rgb + float3(0,0,1);
                float3 node_3192_nrm_detail = i.normalDir * float3(-1,-1,1);
                float3 node_3192_nrm_combined = node_3192_nrm_base*dot(node_3192_nrm_base, node_3192_nrm_detail)/node_3192_nrm_base.z - node_3192_nrm_detail;
                float3 node_3192 = node_3192_nrm_combined;
                float2 node_3537 = (mul( UNITY_MATRIX_V, float4(node_3192,0) ).xyz.rgb.rg*0.5+0.5);
                float4 _MatCap_var = tex2D(_MatCap,TRANSFORM_TEX(node_3537, _MatCap));
                float3 emissive = (node_3229+(_MatCap_var.rgb*_ReflectIntensity));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
