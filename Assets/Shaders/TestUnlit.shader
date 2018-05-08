Shader "Custom/TestUnlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)

		_DissolveTexture ("Cheese", 2D) = "white" {}
		_DissolveAmount ("Cheese cut out amount", Range(0,1)) = 1

		_ExtrudeAmount("Extrude amount", Range(-0.1,0.1)) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// Import properties from ShaderLab to CG
			sampler2D _MainTex;
			float4 _Color;
			sampler2D _DissolveTexture;
			float _DissolveAmount;
			float _ExtrudeAmount;

			// Build the object
			v2f vert (appdata IN)
			{
				v2f OUT;

				IN.vertex.xyz += IN.normal.xyz * _ExtrudeAmount * sin(_Time.y);

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv; // TRANSFORM_TEX(IN.uv, _MainTex);
				return OUT;
			}

			// Color the object
			fixed4 frag (v2f IN) : SV_Target
			{
				// sample the texture
				fixed4 textureColor = tex2D(_MainTex, IN.uv);
				// sample the chees texture
				fixed4 dissolveColor = tex2D(_DissolveTexture, IN.uv);
				// if the is negative, kill the pixel
				clip(dissolveColor.rgb - _DissolveAmount);
				// color it
				textureColor *= _Color;
				return textureColor;
			}
			ENDCG
		}
	}
}
