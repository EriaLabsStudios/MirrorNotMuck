Shader "Custom/StochasticSample"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		_Smoothness("Smoothness", 2D) = "white" {}
		[Toggle]_Stochastic("Stochastic", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows
			#pragma shader_feature
			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			fixed4 _Color;

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _Smoothness;

			uniform float _BumpScale;

			uniform float _Stochastic;

			struct Input
			{
				float2 uv_MainTex;
			};

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

				//hash for randomness
				float2 hash2D2D(float2 s)
				{
				//magic numbers
				return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159)) * 43758.5453);
			}

			//stochastic sampling
			float4 tex2DStochastic(sampler2D tex, float2 UV)
			{
				//triangle vertices and blend weights
				//BW_vx[0...2].xyz = triangle verts
				//BW_vx[3].xy = blend weights (z is unused)
				float4x3 BW_vx;

				//uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
				float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), UV * 3.464);

				//vertex IDs and barycentric coords
				float2 vxID = float2 (floor(skewUV));
				float3 barry = float3 (frac(skewUV), 0);
				barry.z = 1.0 - barry.x - barry.y;

				BW_vx = ((barry.z > 0) ?
					float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
					float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

				//calculate derivatives to avoid triangular grid artifacts
				float2 dx = ddx(UV);
				float2 dy = ddy(UV);

				//blend samples with calculated weights
				return mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
						mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
						mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
			}

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				float4 bumpSample;
				float4 albedoSample = 1;
				float4 smoothnessSample;

				if (_Stochastic)
				{
					albedoSample = tex2DStochastic(_MainTex, IN.uv_MainTex);
					bumpSample = tex2DStochastic(_BumpMap, IN.uv_MainTex);
					smoothnessSample = tex2DStochastic(_Smoothness, IN.uv_MainTex);
					//etc.
				}
				else
				{
					albedoSample = tex2D(_MainTex, IN.uv_MainTex);
					bumpSample = tex2D(_BumpMap, IN.uv_MainTex);
					smoothnessSample = tex2D(_Smoothness, IN.uv_MainTex);
					//etc.
				}

				o.Alpha = albedoSample.a;

				o.Albedo = albedoSample.rgb;
				o.Normal = UnpackScaleNormal(bumpSample, _BumpScale);
				o.Smoothness = smoothnessSample.r;
				//etc.
			}
			ENDCG
		}
			FallBack "Diffuse"
}