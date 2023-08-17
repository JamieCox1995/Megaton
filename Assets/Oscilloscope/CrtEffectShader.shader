Shader "Hidden/CrtEffectShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SignalNoise ("Analog Noise", Vector) = (0, 0, 0, 0)
		_ScanNoise ("Scan Distortion", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			float3 _SignalNoise;
			float2 _ScanNoise;

			float rand(float2 co) {
				return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 T = float2(_SinTime.w, _CosTime.w);

				float2 n = float2(rand(i.uv + T), rand(i.uv + T + 1));
				float2 d = n * 2 - 1;

				float2 uv = saturate(i.uv + d * _ScanNoise);

				float3 col = tex2D(_MainTex, uv);

				//i.uv = saturate(i.uv + d * _ScanNoise);
				//float3 col = tex2D(_MainTex, i.uv);
				
				// http://www.equasys.de/colorconversion.html
				float3x3 toYPbPr = float3x3(float3(0.299, 0.587, 0.114),
											float3(-0.169, -0.331, 0.5),
											float3(0.5, -0.419, -0.081));

				float3 yPbPr = mul(toYPbPr, col);

				yPbPr.x += _SignalNoise.x * (rand(i.uv + T + 2) - 0.5);
				yPbPr.y += _SignalNoise.y * (rand(i.uv + T + 3) - 0.5);
				yPbPr.z += _SignalNoise.z * (rand(i.uv + T + 4) - 0.5);

				// http://www.equasys.de/colorconversion.html
				float3x3 toRGB = float3x3(float3(1, 0, 1.402),
										  float3(1, -0.344, -0.714),
										  float3(1, 1.772, 0));

				float3 final = mul(toRGB, yPbPr);

				return fixed4(final, 0);
			}
			ENDCG
		}
	}
}
