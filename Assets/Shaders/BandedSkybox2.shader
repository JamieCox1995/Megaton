Shader "Skybox/Banded"
{
    Properties
    {
		[Toggle(ANGULAR_CORRECTION)] _UseAngularCorrection("Use Angular Correction", Int) = 1
		[Toggle(USE_GRADIENT_TEXTURE)] _UseAdvancedColors("Use Gradient Editor", Int) = 0
		[Toggle(USE_GROUND_REFLECTION)] _UseGroundReflection("Use Ground Reflection", Int) = 1
		[HDR] _SunColor("Sun Color", Color) = (1, 1, 1, 0)
		_SunRadius("Sun Radius (°)", Range(0, 90)) = 2
		_BandCount("Number of Bands", Int) = 12
        [HDR] _SkyColor1("Color A", Color) = (0, 0.25, 0.57, 0)
        [HDR] _SkyColor2("Color B", Color) = (0.3, 0.9, 0.95, 0)
		_GroundColor("Ground Color", Color) = (0.35, 0.25, 0.25, 0)
		_GroundBlend("Ground Blend Amount", Range(0, 0.05)) = 0.01
		_GroundReflectivity("Ground Reflectivity", Range(0, 1)) = 0.05
		_Gradient("Gradient", 2D) = "white" {}
    }

    CGINCLUDE

	#include "Lighting.cginc"
    #include "UnityCG.cginc"

	#pragma shader_feature ANGULAR_CORRECTION
	#pragma shader_feature USE_GRADIENT_TEXTURE
	#pragma shader_feature USE_GROUND_REFLECTION

	#define PI 3.14159265359
	#define ONE_OVER_PI 0.31830988618
	#define RADIANS_TO_DEGREES 180 * ONE_OVER_PI
	#define DEGREES_TO_RADIANS PI / 180

    struct appdata
    {
        float4 position : POSITION;
        float3 vertex : TEXCOORD0;
    };
    
    struct v2f
    {
        float4 position : SV_POSITION;
        float3 vertex : TEXCOORD0;
    };
    
	float4 _SunColor;
	float _SunRadius;
	int _BandCount;
	float4 _SkyColor1;
	float4 _SkyColor2;
	float4 _GroundColor;
	float _GroundBlend;

	#if defined(USE_GROUND_REFLECTION)
		float _GroundReflectivity;
	#endif

	#if defined(USE_GRADIENT_TEXTURE)
		uniform sampler2D _Gradient;
	#endif

	// quantize values in the range [0, 1] into one of n equal sized buckets
    float quantize(float t, int n)
	{
		int c = n - 1;

		#if defined(ANGULAR_CORRECTION)
			return round(t * n - 0.5) / c;
		#else
			return round(t * c) / c;
		#endif
	}

	// get the sky color at the specified point based on whether we are sampling the gradient texture or lerping
	#if defined(USE_GRADIENT_TEXTURE)
		#define getSkyColor(t) tex2D(_Gradient, float2(t, 0))
	#else
		#define getSkyColor(t) lerp(_SkyColor1, _SkyColor2, t)
	#endif

	// remap [-1, 1] to [0, 1] based on whether to use angular correction or not
	#if defined(ANGULAR_CORRECTION)
		#define remap01(t) (1 - acos(t) * ONE_OVER_PI)
	#else
		#define remap01(t) (t * 0.5 + 0.5)
	#endif

    v2f vert(appdata vertex)
    {
        v2f o;
        o.position = UnityObjectToClipPos(vertex.position);
        o.vertex = vertex.vertex;
        return o;
    }
    
    half4 frag(v2f i) : COLOR
    {
        float3 vertex = normalize(i.vertex);
		float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

		// used to drive the banded sky effect
		float t = dot(lightDir, vertex);

		#if defined(USE_GROUND_REFLECTION)
			// these values define the "mirror image" of the sky as reflected in the horizontal plane
			float3 reflectedLightDir = float3(lightDir.x, -lightDir.y, lightDir.z);
			float reflectedT = dot(reflectedLightDir, vertex);
		#endif

		// blend between the ground and the sky when the vertex direction is nearly horizontal
		float uprightness = dot(vertex, float3(0, 1, 0));
		float groundAlpha = smoothstep(-_GroundBlend, _GroundBlend, uprightness);
		
		// calculate whether the current vertex should be part of the sun
		float sunAngularRadius = cos(_SunRadius * DEGREES_TO_RADIANS);
		bool isSun = t >= sunAngularRadius;

		t = remap01(t);
		#if defined(USE_GROUND_REFLECTION)
			reflectedT = remap01(reflectedT);
		#endif

		float quantized = quantize(t, _BandCount);

		float4 skyColor = isSun ? _SunColor : getSkyColor(quantized);
		float4 groundColor;

		#if defined(USE_GROUND_REFLECTION)
			// apply the sky's reflection to the ground according to reflectivity and angle
			float4 reflectionColor = getSkyColor(reflectedT);
			float reflectAmount = lerp(0, _GroundReflectivity, 1 - abs(uprightness));
			
			groundColor = lerp(_GroundColor, reflectionColor, reflectAmount);
		#else
			// no reflection
			groundColor = _GroundColor;
		#endif

		return lerp(groundColor, skyColor, groundAlpha);
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" }
        Pass
        {
            ZWrite Off
            Cull Off
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
	CustomEditor "BandedSkyboxGUI"
}