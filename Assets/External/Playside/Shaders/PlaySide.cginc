// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#include "UnityCG.cginc"

float4 TransformPosition(appdata_full v)
{
	return UnityObjectToClipPos (v.vertex);
}
