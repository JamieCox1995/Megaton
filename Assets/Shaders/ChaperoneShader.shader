Shader "Custom/ChaperoneShader" {
	Properties {
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_WorldPosition ("Chaperone Focal Position", Vector) = (0,0,0,0)
		_MinDistance ("Full Visibility Distance", Float) = 0
		_MaxDistance ("Fadeout Distance", Float) = 1

		_MajorGridSpacing ("Major Grid Spacing", Float) = 1
		_MajorGridThickness ("Major Grid Thickness", Float) = 0.1

		_MinorGridSpacing("Minor Grid Spacing", Float) = 0.2
		_MinorGridThickness("Minor Grid Thickness", Float) = 0.025
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "ForceNoShadowCasting"="True" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard noshadow alpha:fade vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
			float3 worldTangent;
		};

		fixed4 _Color;
		float3 _WorldPosition;
		float _MinDistance;
		float _MaxDistance;
		float _MajorGridSpacing;
		float _MajorGridThickness;
		float _MinorGridSpacing;
		float _MinorGridThickness;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			o.uv_MainTex = v.texcoord;

			// Get position, normal, and tangent vectors in world space.
			o.worldPos = mul(v.vertex, unity_WorldToObject);
			o.worldNormal = mul(v.normal, unity_WorldToObject);
			o.worldTangent = mul(v.tangent, unity_WorldToObject);
		}

		bool posInGrid(float3 pos, float3 normal, float3 tangent, float spacing, float thickness)
		{
			float3 scaledPos = pos / spacing;

			float scaledThickness = thickness / (2 * spacing);

			normal = -normal;
			float3 binormal = cross(normal, tangent);

			// Get the world position as projected into the planar space of the polygon.
			float yPrime = dot(scaledPos, tangent);
			float xPrime = dot(scaledPos, binormal);

			float2 planarPos = float2(xPrime, yPrime);

			float2 posInGrid = frac(planarPos);

			// X or Y is inside a grid line.
			bool result = (posInGrid.x < scaledThickness || posInGrid.x > 1 - scaledThickness
				|| posInGrid.y < scaledThickness || posInGrid.y > 1 - scaledThickness);

			return result;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float d = distance(IN.worldPos, _WorldPosition);

			// Inverse lerp between _MinDistance and _MaxDistance to fade according to the distance from the target point.
			float alpha = 1 - saturate((d - _MinDistance) / (_MaxDistance - _MinDistance));

			float3 pos = IN.worldPos;
			float3 normal = normalize(IN.worldNormal);
			float3 tangent = normalize(IN.worldTangent);

			// Fragment is either in the major or minor grid.
			bool isInGrid = posInGrid(pos, normal, tangent, _MajorGridSpacing, _MajorGridThickness)
				|| posInGrid(pos, normal, tangent, _MinorGridSpacing, _MinorGridThickness);

			float4 color = isInGrid ? _Color : float4(0, 0, 0, 0);

			o.Emission = color.rgb;
			o.Alpha = smoothstep(0, 1, color.a * alpha);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
