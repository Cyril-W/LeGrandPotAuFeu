Shader "Custom/HexMapElement" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Specular("Specular", Color) = (0.2, 0.2, 0.2)
		_BackgroundColor("Background Color", Color) = (0,0,0)
		[NoScaleOffset] _GridCoordinates("Grid Coordinates", 2D) = "white" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma surface surf StandardSpecular fullforwardshadows vertex:vert
#pragma target 3.0
#pragma multi_compile _ HEX_MAP_EDIT_MODE

#include "HexCellData.cginc"

		sampler2D _GridCoordinates;

		fixed4 _Color;
		half _Glossiness;
		fixed3 _Specular;
		half3 _BackgroundColor;

		struct Input {
			float2 visibility;
			float3 color : COLOR;
		};

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			float3 pos = mul(unity_ObjectToWorld, v.vertex);

			float4 gridUV = float4(pos.xz, 0, 0);
			gridUV.x *= 1 / (4 * 8.66025404);
			gridUV.y *= 1 / (2 * 15.0);
			float2 cellDataCoordinates = floor(gridUV.xy) + tex2Dlod(_GridCoordinates, gridUV).rg;
			cellDataCoordinates *= 2;

			float4 cellData = GetCellData(cellDataCoordinates);
			data.visibility.x = cellData.x;
			data.visibility.x = lerp(0.25, 1, data.visibility.x);
			data.visibility.y = cellData.y;
		}

		void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
			float explored = IN.visibility.y;
			o.Albedo = IN.color.rgb * 2 * (IN.visibility.x * explored) * _Color;

			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Occlusion = explored;
			o.Emission = _BackgroundColor * (1 - explored);
		}
		ENDCG
	}
		FallBack "Diffuse"
}