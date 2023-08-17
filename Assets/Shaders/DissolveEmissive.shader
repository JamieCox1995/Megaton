// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "FX/Dissolve and Burn/Without Shadows" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		[PerRendererData] _MainTex("Albedo (RGB)", 2D) = "white" {}
		[PerRendererData] _MainTex_ST("Tiling/Offset", Vector) = (1,1,0,0)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		[PerRendererData][HDR] _EmissionColor("Dissolve Emission", Color) = (1, 1, 1, 1)
		[PerRendererData] _DissolvePropagation("Dissolve Propagation", Range(0, 1)) = 0.05
		[PerRendererData] _DissolvePercentage("Dissolve Proportion", Range(0, 1)) = 0.0
		[PerRendererData] _DissolveScale("Dissolve Scale", Float) = 1.0
		[MaterialToggle] _AlbedoDissolve("Show Dissolve Noise as Albedo", Range(0, 1)) = 0.0
		[MaterialToggle] _InvertEmission("Invert Emission Mask", Range(0, 1)) = 0.0
		[MaterialToggle] _InvertAlbedo("Invert Albedo Mask", Range(0, 1)) = 0.0

	}
	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" }
		LOD 200

		Lighting On
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off

		CGPROGRAM
		#include "CellNoise.cginc"

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _EmissionColor;
		half _DissolvePropagation;
		half _DissolvePercentage;
		half _DissolveScale;
		half _AlbedoDissolve;
		half _InvertEmission;
		half _InvertAlbedo;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float2 dist = cellNoise(IN.worldPos / _DissolveScale);
			half gradient = 0.0001 + _DissolvePropagation + saturate(dist.x) * (1 - _DissolvePropagation - 0.0001);
			clip(gradient - _DissolvePercentage);

			float maskBase = smoothstep(_DissolvePercentage, _DissolvePercentage + _DissolvePropagation, gradient);

			// _InvertEmission = 0 -> maskBase
			// _InvertEmission = 1 -> 1 - maskBase
			// (1 - 2 * maskBase) * _InvertEmission + maskBase

			float emissionMask = (1 - 2 * maskBase) * _InvertEmission + maskBase;

			o.Emission = _EmissionColor * emissionMask;

			float albedoMask = (1 - 2 * maskBase) * _InvertAlbedo + maskBase;

			fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = color.rgb * albedoMask;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = color.a;
		}
		ENDCG
	}
	FallBack "Standard"
	CustomEditor "DissolveGUI"
}