Shader "Hidden/ImageDepthFade"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_EffectTex("Effect Texture", 2D) = "white" {}
		_Depth ("Blend Depth", Float) = 0
		_Crossfade ("Crossfade Amount", Float) = 0
		_SkyDepthFrom ("Sky Depth From", Float) = 0
		_SkyDepthTo ("Sky Depth To", Float) = 1
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
			sampler2D _CameraDepthTexture;
			sampler2D _EffectTex;
			float _Depth;
			float _CrossfadeAmount;
			float _SkyDepthFrom;
			float _SkyDepthTo;

			fixed4 frag (v2f i) : SV_Target
			{
				float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				fixed4 main = tex2D(_MainTex, i.uv);
				fixed4 effect = tex2D(_EffectTex, i.uv);

				float blendAmount;

				if (depth < 1)
				{

					float x = lerp(-_CrossfadeAmount, 1 + _CrossfadeAmount, _Depth);

					blendAmount = smoothstep(x - _CrossfadeAmount, x + _CrossfadeAmount, depth);
				}
				else
				{
					blendAmount = 1 - smoothstep(_SkyDepthFrom, _SkyDepthTo, _Depth);
				}

				return lerp(main, effect, blendAmount);
			}
			ENDCG
		}
	}
}
