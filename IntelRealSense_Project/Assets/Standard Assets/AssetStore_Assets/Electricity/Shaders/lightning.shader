// electricity/lightning shader
// pixel shader 2.0 based rendering of electric spark
// by Ori Hanegby
// Free for any kind of use.


Shader "FX/Lightning" {
Properties {
	_SparkDist  ("Spark Distribution", range(-1,1)) = 0
	_MainTex ("MainTex (RGB)", 2D) = "white" {}
	_Noise ("Noise", 2D) = "noise" {}	
	_StartSeed ("StartSeed", Float) = 0
	_SparkMag ("Spark Magnitude" , range(1,100)) = 1
	_SparkWidth ("Spark Width" , range(0.001,0.499)) = 0.25
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" }


	SubShader {		
 		
 		// Main pass: Take the texture grabbed above and use the bumpmap to perturb it
 		// on to the screen
 		Blend one one
 		ZWrite off
		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	float2 texcoord: TEXCOORD0;
};

struct v2f {	
	float4 vertex : POSITION;	
	float2 uvmain : TEXCOORD0;	
};

float _SparkDist;
float4 _Noise_ST;
float4 _MainTex_ST;
float4 _ObjectScale;

v2f vert (appdata_t v)
{
	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	
	o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
	return o;
}

sampler2D _GrabTexture;
float4 _GrabTexture_TexelSize;
sampler2D _Noise;
sampler2D _MainTex;
float _GlowSpread;
float _GlowIntensity;
float _StartSeed;
float _SparkMag;
float _SparkWidth;

half4 frag( v2f i ) : COLOR
{
	
	
	float2 noiseVec = float2(i.uvmain.y / 5,abs(sin(_Time.x + _StartSeed)) * 256);	
	float4 noiseSamp = tex2D( _Noise,noiseVec);
		
	float dvdr = 1.0 - abs(i.uvmain.y - 0.5) * 2;
	dvdr = clamp(dvdr+_SparkDist,0,1);
	
	float fullWidth = 1 - _SparkWidth * 2;
	// Center the scaled texture
	float scaledTexel = (i.uvmain.x - _SparkWidth) / fullWidth;
			
	float offs = scaledTexel + ((0.5 - noiseSamp.x)/2) * _SparkMag * dvdr;
	offs = clamp(offs,0,1);
			
	
	float2 texSampVec = float2(offs,i.uvmain.y);
	half4 col = tex2D( _MainTex, texSampVec);

	
	return col;
}
ENDCG
		}
	}


	// ------------------------------------------------------------------
	// Fallback for older cards 	
	SubShader {
		Blend one one
 		ZWrite off
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture }
		}
	}
}

}
