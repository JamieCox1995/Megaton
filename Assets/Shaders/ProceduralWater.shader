Shader "Environment/Procedural Water"
{
	Properties
	{
		[Header(Material Properties)] _Color("Color", Color) = (1, 1, 1, 1)
		_Tint("Tint", Color) = (1, 1, 1, .5)
		_MainTex("Main Texture", 2D) = "white" {}
		_NoiseTex("Extra Wave Noise", 2D) = "white" {}
		[Toggle(USE_FLOW_MAP)] _FlowMap("Use Flow Map?", Float) = 0
		_FlowTex("Flow Texture", 2D) = "bump" {}

		[Header(Animation Properties)] _Speed("Wave Speed", Range(0,1)) = 0.5
		_Amount("Wave Amount", Range(0,1)) = 0.5
		_Height("Wave Height", Range(0,1)) = 0.5
		_FlowRate("Flow Rate", Range(0, 20)) = 1
		_FlowFade("Flow Fadeout Time", Range(5, 30)) = 10

		[Header(Foam Properties)] _Foam("Water Depth Multiplier", Range(0,3)) = 0.5
		_CellSize("Cell Size", Float) = 8
		_Power("Cell Power", Range(0.5, 5)) = 2.4
		_CellMin("Cell Boundary Cutoff", Range(0, 1)) = 0.4
		_Turbulence("Turbulence", Range(0, 1)) = 0.1
		_Crest("Crest Amount", Range(0, 1)) = 0.8

	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#include "CellNoise.cginc"
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#pragma shader_feature USE_FLOW_MAP

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				#ifdef USE_FLOW_MAP
				float3 tangent : TANGENT;
				#endif
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				#ifdef USE_FLOW_MAP
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
				#endif
				float4 scrPos : TEXCOORD2;//
				float4 pos : TEXCOORD3;
				float offset : TEXCOORD4;
			};

			float4 _Color, _Tint;
			uniform sampler2D _CameraDepthTexture; //Depth Texture
			sampler2D _MainTex, _NoiseTex, _FlowTex;//
			float4 _MainTex_ST, _FlowTex_TexelSize;
			float _Speed, _Amount, _Height, _FlowRate, _FlowFade, _Foam, _CellSize, _Power, _CellMin, _Turbulence, _Crest;// 

			v2f vert(appdata v)
			{
				v2f o;
				float4 tex = tex2Dlod(_NoiseTex, float4(v.uv.xy, 0, 0));//extra noise tex
				float offset = sin(_Time.z * _Speed + (v.vertex.x * v.vertex.z * _Amount * tex)) * _Height; //movement
				o.offset = offset;
				//v.vertex.y += offset;
				v.vertex += offset * float4(v.normal, 1);
				o.pos = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				#ifdef USE_FLOW_MAP
				o.normal = v.normal;
				o.tangent = v.tangent;
				#endif
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.scrPos = ComputeScreenPos(o.vertex); // grab position on screen
				UNITY_TRANSFER_FOG(o,o.vertex);

				return o;
			}

			float sawtooth(float t, float length)
			{
				return 1 - (2 * abs((t % length) - (length * 0.5))) / length;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				#ifdef USE_FLOW_MAP
				float3 bitangent = cross(i.normal, i.tangent);
				float3 flowDirection = UnpackNormal(tex2D(_FlowTex, i.uv)); // get water flow vector

				float3 flow = flowDirection.x * i.tangent + flowDirection.y * bitangent;

				float3 pos1 = i.pos - ((_Time.y - (_FlowFade * 0.5)) % _FlowFade) * _FlowRate * flow; // positions for water foam
				float3 pos2 = i.pos - (_Time.y % _FlowFade) * _FlowRate * flow;

				float noise1 = pow(smoothstep(_CellMin, 1, cellNoise(pos1 / _CellSize)), _Power); // cell noise for highlights
				float noise2 = pow(smoothstep(_CellMin, 1, cellNoise(pos2 / _CellSize)), _Power);

				float noiseFactor = smoothstep(0.25, 0.75, sawtooth(_Time.y, _FlowFade)); // blend between the two sets of higlights to look continuous with minimal distortion
				float noise = lerp(noise1, noise2, noiseFactor);
				#else
				float noise = pow(smoothstep(_CellMin, 1, cellNoise(i.pos / _CellSize)), _Power);
				#endif

				float crest = 2 * (1 - _Crest) - 1;
				float highlight = _Turbulence + (1 - _Turbulence) * smoothstep(crest * _Height, _Height, i.offset); // highlight amount
				half4 col = lerp(_Color * tex2D(_MainTex, i.uv), _Tint, highlight * noise); // water with reflection highlights
				half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos))); // depth
				half4 foamLine = 1 - saturate(_Foam * (depth - i.scrPos.w)); // foam line by comparing depth and screenposition
				col = lerp(col, _Tint, foamLine); // add the foam line and tint to the texture
				return col;
			}
			ENDCG
		}
	}
	CustomEditor "ProceduralWaterGUI"
}