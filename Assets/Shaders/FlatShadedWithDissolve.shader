Shader "Particles/FX/Flat Shaded with Dissolve"
{
	Properties
	{
		[HDR] _TintColor("Color", Color) = (1, 1, 1, 1)
		_DissolveScale("Initial Scale", Float) = 1.0
		_DissolvePercentage("Dissolve Proportion", Range(0, 1)) = 0.0
		_Octaves ("Octaves", Int) = 3
		_Persistence ("Persistence", Range(0, 1)) = 0.67
		_Lacunarity ("Lacunarity", Float) = 0.2
	}
	SubShader
	{
		Tags { "RenderType"="Cutout" }
		Cull Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "CellNoise.cginc"
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 worldPos : TEXCOORD2;
			};

			float4 _TintColor;
			float _DissolveScale;
			float _DissolvePercentage;
			float _Octaves;
			float _Persistence;
			float _Lacunarity;

			float cellNoiseOctaves(float3 pt, int octaves, float persistence, float lacunarity)
			{
				float result = 0;
				float totalAmplitude = 0;
				for (int i = 0; i < octaves; i++)
				{
					float3 p = (pt + float3(i, i, i)) / pow(lacunarity, i);

					float amplitude = pow(persistence, i);

					result += cellNoise(p) * amplitude;
					totalAmplitude += amplitude;
				}

				return result / totalAmplitude;
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.worldPos = mul(unity_WorldToObject, v.vertex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float noise = cellNoiseOctaves(i.worldPos / _DissolveScale, _Octaves, _Persistence, _Lacunarity);

				clip(noise - _DissolvePercentage);

				// sample the texture
				fixed4 col = _TintColor * i.color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
