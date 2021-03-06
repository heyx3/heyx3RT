﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	public static class ShaderGenerator
	{
		/// <summary>
		/// Inserts the given Material Value hierarchy into the given Unity shader.
		/// </summary>
		/// <param name="shaderlabProperties">
		/// The code inside the "Properties" block.
		/// </param>
		/// <param name="cgDefinitions">
		/// The code inside the CGPROGRAM section that defines functions and variables.
		/// </param>
		/// <param name="cgFunctionBody">
		/// The code inside the function where the Material Value hierarchy is used.
		/// </param>
		public static void Insert(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup,
								  params MV_Base[] roots)
		{
			Graph g = new Graph(roots.ToList());
			foreach (MV_Base node in g.AllConnectedNodes)
				node.Emit(shaderlabProperties, cgDefinitions, cgFunctionBody, idLookup);
		}


		/// <summary>
		/// Generates a custom Standard Specular shader using the given PBR outputs.
		/// </summary>
		public static string GenerateShader_PBR(string name, Dictionary<MV_Base, uint> idLookup,
												MV_Base albedo, MV_Base metallic,
												MV_Base smoothness, MV_Base emissive)
		{
			//Insert the MaterialValues into various parts of the shader.

			StringBuilder shaderlabProperties = new StringBuilder();
			shaderlabProperties.AppendLine("\tProperties\n\t{");
			StringBuilder cgDefinitions = new StringBuilder();
			cgDefinitions.AppendLine("//------------Generated from MV_Base instances-------");
			StringBuilder funcBody = new StringBuilder();
			
			Insert(shaderlabProperties, cgDefinitions, funcBody, idLookup,
				   albedo, metallic, smoothness, emissive);

			shaderlabProperties.Append("\t}");
			cgDefinitions.Append("\t\t\t//---------------------------------------------------");

			
			//Generate the rest of the shader.

			StringBuilder shader = new StringBuilder();


			shader.Append("Shader \"");
			shader.Append(name);
			shader.AppendLine("\"");
			shader.AppendLine("{");
			shader.AppendLine(shaderlabProperties.ToString());

			shader.Append(@"
	SubShader
	{
		Tags { ""RenderType""=""Opaque"" }
		
		CGPROGRAM

		//Use Unity's Standard lighting model and full shadowing
		//Use a custom function to pass per-vertex data into the surface shader.
		#pragma surface surf Standard fullforwardshadows vertex:vert

		//Use shader model 4.0 target, to get more interpolators.
		#pragma target 4.0

		//Using appdata_tan struct for vertex data.
		#include ""UnityCG.cginc""

		");
		shader.AppendLine(cgDefinitions.ToString());
		shader.Append(@"
		struct Input
		{
			float4 vertex : SV_POSITION;

			float4 rtUV_ScreenPos : TEXCOORD0;
			float3 worldPos : TEXCOORD1;
			float3 worldNormal : TEXCOORD2;
			float4 tangent : TEXCOORD3;
		};

		void vert(inout appdata_full v, out Input o)
		{
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.worldNormal = UnityObjectToWorldNormal(v.normal);

			o.rtUV_ScreenPos = float4(v.texcoord.xy, o.vertex.xy);

			o.tangent = float4(normalize(mul(_Object2World,
										 float4(v.tangent.xyz, 0.0)).xyz),
							   v.tangent.w);
		}
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			//--------------Generated from Material Values-------------------------
			");
		shader.AppendLine(funcBody.ToString());
		shader.AppendLine(@"
			//--------------------------------------------------");
		shader.AppendLine();

		shader.Append("\t\t\t\to.Albedo = ");
		shader.Append(albedo.GetShaderValue(OutputSizes.Three, idLookup));
		shader.AppendLine(";");

		shader.Append("\t\t\t\to.Metallic = ");
		shader.Append(metallic.GetShaderValue(OutputSizes.One, idLookup));
		shader.AppendLine(";");

		shader.Append("\t\t\t\to.Smoothness = ");
		shader.Append(smoothness.GetShaderValue(OutputSizes.One, idLookup));
		shader.AppendLine(";");

		shader.Append("\t\t\t\to.Emission = ");
		shader.Append(emissive.GetShaderValue(OutputSizes.Three, idLookup));
		shader.AppendLine(";");

		shader.AppendLine("\t\t\t\to.Alpha = 1.0;");

		shader.Append(@"
		}

		ENDCG
	}
	Fallback ""Diffuse""
}");

			return shader.ToString().Replace("\r", "");
		}
		/// <summary>
		/// Generates a custom transparent/refracting shader using the given outputs.
		/// </summary>
		public static string GenerateShader_Refract(string name, Dictionary<MV_Base, uint> idLookup,
												    MV_Base albedo, MV_Base refractStrength)
		{
			StringBuilder shaderlabProperties = new StringBuilder();
			shaderlabProperties.AppendLine("\tProperties\n\t{");
			StringBuilder cgDefinitions = new StringBuilder();
			cgDefinitions.AppendLine("//------------Generated from MV_Base instances-------");
			StringBuilder funcBody = new StringBuilder();
			
			Insert(shaderlabProperties, cgDefinitions, funcBody, idLookup,
				   albedo, refractStrength);

			shaderlabProperties.Append("\t}");
			cgDefinitions.Append("\t\t\t//---------------------------------------------------");

			
			//Generate the rest of the shader.

			StringBuilder shader = new StringBuilder();

			shader.Append("Shader \"");
			shader.Append(name);
			shader.AppendLine("\"");
			shader.AppendLine("{");
			shader.AppendLine(shaderlabProperties.ToString());

			shader.Append(@"
	SubShader
	{
		Tags { ""Queue""=""Transparent"" ""RenderType""=""Opaque"" }
		
		GrabPass {
			Name ""BASE""
			Tags { ""LightMode"" = ""Always"" }
		}

		Pass {
			Name ""Base""
			Tags { ""LightMode"" = ""Always"" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fog
			#include ""UnityCG.cginc""

			");
			shader.AppendLine(cgDefinitions.ToString());
			shader.Append(@"
			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
			struct FragmentInput
			{
	            float4 vertex : SV_POSITION;
	            float4 uvgrab : TEXCOORD0;
                float3 myViewNormal : TEXCOORD2;
                float3 myWorldPos : TEXCOORD3;
	            UNITY_FOG_COORDS(4)
			};

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			FragmentInput vert(VertexInput v)
			{
				FragmentInput o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				o.myViewNormal = UnityObjectToWorldNormal(v.normal);
				o.myViewNormal = mul(UNITY_MATRIX_V, float4(o.myViewNormal, 0.0)).xyz;

				o.myWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				const float2 one_nOne = float2(1.0, -1.0);
			#if UNITY_UV_STARTS_AT_TOP
				const float2 scale = one_nOne.xy;
			#else
				const float2 scale = one_nOne.xx;
			#endif
				o.uvgrab.xy = 0.5 * (o.vertex.w + (scale * o.vertex.xy));
				o.uvgrab.zw = o.vertex.zw;

				UNITY_TRANSFER_FOG(o, o.vertex);

				return o;
			}
			half4 frag(FragmentInput i) : SV_Target
			{
			#if UNITY_SINGLE_PASS_STEREO
				i.uvgrab.xy = TransformStereoScreenSpaceTex(i.uvgrab.xy, i.uvgrab.w);
			#endif

				//Calculate perturbed coordinates
				float3 camToPos = normalize(_WorldSpaceCameraPos - i.myWorldPos),
					   camToPos_ViewSpace = mul(UNITY_MATRIX_VP, float4(camToPos, 1.0)).xyz;
				float2 bump = i.myViewNormal.xy / i.myViewNormal.z;
				float2 offset = bump * _GrabTexture_TexelSize.xy;

				//-------------------------------
				//MaterialValue graph:
				");
			shader.AppendLine(funcBody.ToString());
			shader.Append(@"
				//------------------------------

				offset *= ");
			shader.Append(refractStrength.ShaderValueName(idLookup));
			shader.Append(@";

			#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE
				i.uvgrab.xy = (offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uvgrab.z)) + i.uvgrab.xy;
			#else
				i.uvgrab.xy = (offset * i.uvgrab.z) + i.uvgrab.xy;
			#endif

				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				col *= ");
			shader.Append(albedo.ShaderValueName(idLookup));
			shader.AppendLine(@";
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	Fallback ""Diffuse""
}");

			return shader.ToString().Replace("\r", "");
		}
		/// <summary>
		/// Generates a custom fog shader using the given outputs.
		/// </summary>
		public static string GenerateShader_Fog(string name, Dictionary<MV_Base, uint> idLookup,
												MV_Base color, MV_Base density)
		{
			StringBuilder shaderlabProperties = new StringBuilder();
			shaderlabProperties.AppendLine("\tProperties\n\t{");
			StringBuilder cgDefinitions = new StringBuilder();
			cgDefinitions.AppendLine("//------------Generated from MV_Base instances-------");
			StringBuilder funcBody = new StringBuilder();
			
			Insert(shaderlabProperties, cgDefinitions, funcBody, idLookup, color, density);

			shaderlabProperties.Append("\t}");
			cgDefinitions.Append("\t\t\t//---------------------------------------------------");

			
			//Generate the rest of the shader.

			StringBuilder shader = new StringBuilder();

			shader.Append("Shader \"");
			shader.Append(name);
			shader.AppendLine("\"");
			shader.AppendLine("{");
			shader.AppendLine(shaderlabProperties.ToString());

			shader.Append(@"
	SubShader
	{
		Tags { ""Queue""=""Transparent+5"" ""RenderType""=""Transparent"" }
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include ""UnityCG.cginc""

			");
			shader.AppendLine(cgDefinitions.ToString());
			shader.Append(@"
			struct VertexInput
			{
				float4 vertex : POSITION;
			};
			struct FragmentInput
			{
	            float4 vertex : SV_POSITION;
			};

			FragmentInput vert(VertexInput v)
			{
				FragmentInput o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			half4 frag(FragmentInput i) : SV_Target
			{
				//-------------------------------
				//MaterialValue graph:
				");
			shader.AppendLine(funcBody.ToString());
			shader.Append(@"
				//------------------------------

				float strength = ");
			shader.Append(density.ShaderValueName(idLookup));
			shader.Append(" / (");
			shader.Append(density.ShaderValueName(idLookup));
			shader.Append(@" + 1.0);

				return float4(");
			shader.Append(color.ShaderValueName(idLookup));
			shader.Append(@", strength);
			}
			ENDCG
		}
	}
}");

			return shader.ToString().Replace("\r", "");
		}
		/// <summary>
		/// Sets the parameters of the given Unity PBR material,
		///     assumed to be using a shader generated by the given Material Values.
		/// </summary>
		/// <param name="shapeTr">
		/// The shape this material is used on.
		/// </param>
		public static void SetMaterialParams(Transform shapeTr, Material unityMat,
											 Dictionary<MV_Base, uint> idLookup,
											 params MV_Base[] mvRoots)
		{
			HashSet<MV_Base> usedMVs = new HashSet<MV_Base>();
			foreach (MV_Base mv in mvRoots.SelectMany(mvR => mvR.HierarchyRootFirst))
			{
				if (!usedMVs.Contains(mv))
				{
					usedMVs.Add(mv);
					mv.SetParams(shapeTr, unityMat, idLookup);
				}
			}
		}


		/// <summary>
		/// Generates a custom Unity shader using the given unlit color output.
		/// </summary>
		public static string GenerateShader(string name, MV_Base rgbOut,
											Dictionary<MV_Base, uint> idLookup,
											bool cullBackfaces = true)
		{
			//Insert the MaterialValues into various parts of the shader.

			StringBuilder shaderlabProperties = new StringBuilder();
			shaderlabProperties.AppendLine("\tProperties\n\t{");
			StringBuilder cgDefinitions = new StringBuilder();
			cgDefinitions.AppendLine("//------------Generated from MV_Base instances-------");
			StringBuilder funcBody = new StringBuilder();
			
			Insert(shaderlabProperties, cgDefinitions, funcBody, idLookup, rgbOut);

			shaderlabProperties.Append("\t}");
			cgDefinitions.Append("\t\t\t//---------------------------------------------------");

			
			//Generate the rest of the shader.

			StringBuilder shader = new StringBuilder();


			shader.Append("Shader \"");
			shader.Append(name);
			shader.AppendLine("\"");
			shader.AppendLine("{");
			shader.AppendLine(shaderlabProperties.ToString());

            shader.Append(@"
	SubShader
	{
		Tags { ""RenderType""=""Opaque"" }
		
        ");
            if (!cullBackfaces)
            {
                shader.AppendLine("Cull Off");
            }
            shader.Append(@"
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include ""UnityCG.cginc""

			");
			shader.AppendLine(cgDefinitions.ToString());
			shader.Append(@"

			struct Vertex
			{
				float4 vertex : POSITION;
			};
			struct Fragment
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};

			Fragment vert(Vertex v)
			{
				Fragment f;

				f.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				f.worldPos = mul(_Object2World, v.vertex).xyz;

				return f;
			}
			half4 frag(Fragment IN) : COLOR
			{
				//--------------Generated from Material Values-------------------------
				");
			shader.AppendLine(funcBody.ToString());
			shader.AppendLine("\t\t\t\t//--------------------------------------------------");
			shader.AppendLine();

			shader.Append("\t\t\t\treturn half4(");
			shader.Append(rgbOut.GetShaderValue(OutputSizes.Three, idLookup));
			shader.Append(@", 1.0);
			}

			ENDCG
		}
	}
	Fallback ""Diffuse""
}");

			return shader.ToString().Replace("\r", "");
		}
		/// <summary>
		/// Sets the parameters of the given Unity unlit material,
		///     assumed to be using a shader generated by the given Material Values.
		/// </summary>
		public static void SetMaterialParams(Material unityMat, Dictionary<MV_Base, uint> idLookup,
											 params MV_Base[] mvRoots)
		{
			HashSet<MV_Base> usedMVs = new HashSet<MV_Base>();
			foreach (MV_Base mv in mvRoots.SelectMany(mvR => mvR.HierarchyRootFirst))
			{
				if (!usedMVs.Contains(mv))
				{
					usedMVs.Add(mv);
					mv.SetParams(null, unityMat, idLookup);
				}
			}
		}
	}
}