Shader "PlaySide/UI/Mask" {
     Properties {
         _MainTex ("Base (RGB)", 2D) = "white" {}
         _AlphaTex ("Alpha (A)", 2D) = "white" {}
     }
     SubShader {
             Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1"}
         ZWrite On
         ZTest LEqual
         
         ColorMask RGB
     
         Blend SrcAlpha OneMinusSrcAlpha
 
         Pass {
             SetTexture[_MainTex] { Combine texture }
             SetTexture[_AlphaTex] { Combine texture, previous * texture}
         }
     }
}
