Shader "Sky Material/Vertical Gradient"
{
	Properties
	{
        _BottomCol ("Bottom Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _TopCol ("Top Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SkyDir ("Sky Dir", Vector) = (0.0, 1.0, 0.0, 0.0)
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
                float t : TEXCOORD0;
			};

            float4 _BottomCol, _TopCol;
			float4 _SkyDir;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.t = (0.5 + (0.5 * dot(v.vertex.xyz, normalize(_SkyDir.xyz))));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                return lerp(_BottomCol, _TopCol, i.t);
			}
			ENDCG
		}
	}
}
