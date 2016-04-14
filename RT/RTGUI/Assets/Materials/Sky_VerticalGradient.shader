Shader "Sky Material/Vertical Gradient"
{
	Properties
	{
        _BottomCol ("Bottom Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _TopCol ("Top Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
                float localY : TEXCOORD0;
			};

            float4 _BottomCol, _TopCol;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.localY = v.vertex.y;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                float t = (0.5 + (0.5 * i.localY));
                return lerp(_BottomCol, _TopCol, t);
			}
			ENDCG
		}
	}
}
