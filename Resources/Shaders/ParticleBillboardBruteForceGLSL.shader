///////////////////////////////////////////////////////
////////  Updated shader by Pantelis Lekakis  /////////
// This shader is used by OpenGL capable machines /////
// when directX is not present. ///////////////////////
///////////////////////////////////////////////////////

Shader "Galaxy/ParticleBillboardBruteForceGLSL" 
{
	Properties 
	{
		_Color ("Overlay Color", Color) = (1,1,1,1)
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
			ZWrite Off
			Cull Off
			LOD 200
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos: SV_POSITION;
				half2 uv: TEXCOORD0;
				fixed4 color: COLOR0;
			};

			half4 _Color;
			sampler2D _MainTex;
			float MaxScreenSize;
			float TextureSheetPower;
			
			v2f vert(appdata_full v)
			{
				v2f o;
				
				float4 pos =  mul(unity_ObjectToWorld, v.vertex);
				float4 info = v.texcoord;
				float3 localPos = v.normal;
				
				//individual size of particle
				float3 viewSpacePos = mul(UNITY_MATRIX_V, pos).xyz;
				float size = min(info.x,info.x * -viewSpacePos.z * MaxScreenSize);
				float3 expandedPos = viewSpacePos - 0.5 * size * float3(localPos.x, localPos.y, 0);
				
				// assume that all the galaxies will always be at origin, don't multiply with world to save a bit of time
				o.pos = mul(UNITY_MATRIX_P, float4(expandedPos, 1.0));
				
				o.uv = half2((localPos.x + 1.0) / 2.0,(localPos.y + 1.0) / 2.0);
				o.color = v.color;
				
				return o;
			}
			
			fixed4 frag(v2f i): SV_Target
			{
				clip(i.color.a - 0.022);
				
				fixed4 color = tex2D(_MainTex, i.uv);
				return color * i.color * _Color;
			}

			ENDCG
		}
	} 
}
