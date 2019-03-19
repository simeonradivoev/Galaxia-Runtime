Shader "Galaxy/ParticleBillboard" 
{
	Properties 
	{
		_Color ("Overlay Color",COLOR) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}

		MySrcMode ("SrcMode", Float) = 0
		MyDstMode ("DstMode", Float) = 0
	}

	SubShader 
	{
		Pass
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			Blend [MySrcMode] [MyDstMode]
			Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
			LOD 200
		
			CGPROGRAM
				#pragma target 4.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityStandardBRDF.cginc"
				#include "UnityCG.cginc" 

				#define M_PI 3.1415926535897932384626433832795
				#define M_G 6.67384

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct appdata
				{
					uint index 					:SV_VertexID;
					float4	pos				: POSITION;
					float3	normal			: NORMAL;
					float2  info			: TEXCOORD0;
					float2  sheetPosition	: TEXCOORD1;
					float4  color			: COLOR;

				};

				struct GS_INPUT
				{
					float4	pos		: POSITION;
					float3	normal	: NORMAL;
					float4  info	: TEXCOORD0;
					float4  color	: COLOR;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float3	normal	: NORMAL;
					float4  color	: COLOR;
					float4 info		: TEXCOORD1;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				uniform sampler2D _CameraDepthTexture;
				Texture2D _MainTex;
				float4 _Color;
				SamplerState sampler_MainTex;
				float MaxScreenSize = 1;
				float TextureSheetPower;

				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(appdata v)
				{
					GS_INPUT output = (GS_INPUT)0;
					output.color = v.color;
					output.pos =  mul(unity_ObjectToWorld, v.pos);
					output.info =  float4(v.info,v.sheetPosition);
					return output;
				}

				float4x4 rotate(float3 r, float4 d)
				{
					float cx, cy, cz, sx, sy, sz;
					sincos(r.x, sx, cx);
					sincos(r.y, sy, cy);
					sincos(r.z, sz, cz);                   
					return float4x4( cy*cz,   -sz,    sy, d.x,
									   sz, cx*cz,   -sx, d.y,
									  -sy,    sx, cx*cy, d.z,
										0,     0,     0, d.w );                
				}

				float4x4 inverse(float4x4 input)
				 {
					 #define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
     
					 float4x4 cofactors = float4x4(
						  minor(_22_23_24, _32_33_34, _42_43_44), 
						 -minor(_21_23_24, _31_33_34, _41_43_44),
						  minor(_21_22_24, _31_32_34, _41_42_44),
						 -minor(_21_22_23, _31_32_33, _41_42_43),
         
						 -minor(_12_13_14, _32_33_34, _42_43_44),
						  minor(_11_13_14, _31_33_34, _41_43_44),
						 -minor(_11_12_14, _31_32_34, _41_42_44),
						  minor(_11_12_13, _31_32_33, _41_42_43),
         
						  minor(_12_13_14, _22_23_24, _42_43_44),
						 -minor(_11_13_14, _21_23_24, _41_43_44),
						  minor(_11_12_14, _21_22_24, _41_42_44),
						 -minor(_11_12_13, _21_22_23, _41_42_43),
         
						 -minor(_12_13_14, _22_23_24, _32_33_34),
						  minor(_11_13_14, _21_23_24, _31_33_34),
						 -minor(_11_12_14, _21_22_24, _31_32_34),
						  minor(_11_12_13, _21_22_23, _31_32_33)
					 );
					 #undef minor
					 return transpose(cofactors) / determinant(input);
				 }


				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					//inverse view matrix
					float4x4 i_v = inverse(UNITY_MATRIX_V);
					float4 pos = p[0].pos;
					float distance = length(_WorldSpaceCameraPos - pos);
					float3 look = normalize(_WorldSpaceCameraPos - pos);
					float3 up = float3(0,1,0);
					
					//the rotation matrix
					float4x4 rotateM = rotate(float3(0,0,p[0].info.y),float4(0,0,0,0));
					up = mul(rotateM,up);
					//rotate the up vector to match the camera up
					up =  mul(i_v,up);
					float3 right = cross(up, look);

					//individual size of particle
					float size = min(p[0].info.x,p[0].info.x * distance * MaxScreenSize);
					
					//quad vertices centered on 0,0 
					float4 v[4];
					v[0] = float4(right * 0.5f  - up * 0.5f,0);
					v[1] = float4(right * 0.5f + up * 0.5f, 0);
					v[2] = float4(- right * 0.5f - up * 0.5f, 0);
					v[3] = float4(- right * 0.5f + up * 0.5f, 0);
					
					float2 u[4];
					float uwidth = 1.0 / TextureSheetPower;
					float uheight = 1.0 / TextureSheetPower;
					float ux = fmod(p[0].info.z,TextureSheetPower) * uwidth;
					float uy = floor(p[0].info.z / TextureSheetPower) * uheight;
					u[0] = float2(ux + uwidth,uy);
					u[1] = float2(ux + uwidth,uy + uheight);
					u[2] = float2(ux,uy);
					u[3] = float2(ux,uy + uheight);

					FS_INPUT pIn;
					//use the look direction as the normal
					pIn.normal = look;
					pIn.color = p[0].color;
					//store the distance of each particle
					pIn.info = float4(distance,0,0,0);
					
					for(int i = 0;i < 4;i++)
					{
						pIn.pos = mul(UNITY_MATRIX_VP, pos + v[i] * size);
						pIn.tex0 = u[i];
						triStream.Append(pIn);
					}
				}

				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{
					float4 col = _MainTex.Sample(sampler_MainTex, input.tex0);
					return col * input.color * _Color;
				}

			ENDCG
		}
	} 
}
