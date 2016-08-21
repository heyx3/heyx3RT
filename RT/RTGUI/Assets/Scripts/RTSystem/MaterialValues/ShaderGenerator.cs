using System;
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
								  params MV_Base[] roots)
		{
			//Emit the code for all nodes in proper order.

			List<MV_Base> toProcess = new List<MV_Base>();
			Dictionary<MV_Base, bool> isNodeDoneYet = new Dictionary<MV_Base, bool>();
			foreach (MV_Base mv in roots)
			{
				toProcess.Add(mv);
				isNodeDoneYet.Add(mv, false);
			}

			while (toProcess.Count > 0)
			{
				MV_Base mv = toProcess[toProcess.Count - 1];

				//If this node hasn't been processed yet, add its inputs to the stack.
				if (!isNodeDoneYet[mv])
				{
					isNodeDoneYet[mv] = true;

					foreach (MV_Base inputMV in mv.Inputs)
					{
						//If the node is already in the stack, just move it to the top.
						if (isNodeDoneYet.ContainsKey(inputMV))
						{
							toProcess.Remove(inputMV);
							toProcess.Add(inputMV);
						}
						//Otherwise, add it to the top of the stack.
						else
						{
							toProcess.Add(inputMV);
							isNodeDoneYet.Add(inputMV, false);
						}
					}
				}
				//The node's inputs have already been processed and taken off the stack,
				//    so process the node itself.
				else
				{
					toProcess.RemoveAt(toProcess.Count - 1);
					mv.Emit(shaderlabProperties, cgDefinitions, cgFunctionBody);
				}
			}
		}


		/// <summary>
		/// Generates a custom Unity shader using the given PBR outputs.
		/// </summary>
		public static string GenerateShader(string name,
											MV_Base albedo, MV_Base metallic, MV_Base smoothness)
		{
			//Insert the MaterialValues into various parts of the shader.

			StringBuilder shaderlabProperties = new StringBuilder();
			shaderlabProperties.AppendLine("\tProperties\n\t{");
			StringBuilder cgDefinitions = new StringBuilder();
			cgDefinitions.AppendLine("//------------Generated from MV_Base instances-------");
			StringBuilder funcBody = new StringBuilder();
			
			Insert(shaderlabProperties, cgDefinitions, funcBody,
				   albedo, metallic, smoothness);

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
		
		Pass
		{
			CGPROGRAM

			//Use Unity's Standard lighting model and full shadowing
			//Use a custom function to pass per-vertex data into the surface shader.
			#pragma surface surf Standard fullforwardshadows vertex:vert

			//Use shader model 3.0 target, to get nice lighting.
			#pragma target 3.0

			//Using appdata_tan struct for vertex data.
			#include ""UnityCG.cginc""

			");
			shader.AppendLine(cgDefinitions.ToString());
			shader.Append(@"
			struct Input
			{
				//The following are filled in by Unity automatically.
				float4 screenPos;
				float3 worldPos;
				float3 worldNormal;
			
				//The following will be filled in via the vertex shader.
				float4 tangent;
				//UV will be packed into screenPos.zw.
			};

			void vert(inout appdata_tan v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.screenPos.zw = v.texcoord.xy;

				float3 worldTangent = normalize(mul(_Object2World,
													float4(v.tangent.xyz, 0.0)).xyz);
				o.tangent = float4(worldTangent, v.tangent.w);
			}
			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				//--------------Generated from Material Values-------------------------
				");
			shader.AppendLine(funcBody.ToString());
			shader.AppendLine("//--------------------------------------------------");
			shader.AppendLine();

			shader.Append("\t\t\t\to.Albedo = ");
			shader.Append(albedo.GetShaderValue(OutputSizes.Three));
			shader.AppendLine(";");

			shader.Append("\t\t\t\to.Metallic = ");
			shader.Append(metallic.GetShaderValue(OutputSizes.One));
			shader.AppendLine(";");

			shader.Append("\t\t\t\to.Smoothness = ");
			shader.Append(smoothness.GetShaderValue(OutputSizes.One));
			shader.AppendLine(";");

			shader.AppendLine("\t\t\t\to.Alpha = 1.0;");

			shader.Append(@"
			}

			ENDCG
		}
	}
	Fallback ""Diffuse""
}");

			return shader.ToString();
		}
		/// <summary>
		/// Sets the parameters of the given Unity PBR material,
		///     assumed to be using a shader generated by the given Material Values.
		/// </summary>
		/// <param name="shapeTr">
		/// The shape this material is used on.
		/// </param>
		public static void SetMaterialParams(Transform shapeTr, Material unityMat,
											   params MV_Base[] mvRoots)
		{
			HashSet<MV_Base> usedMVs = new HashSet<MV_Base>();
			foreach (MV_Base mv in mvRoots.SelectMany(mvR => mvR.Hierarchy))
			{
				if (!usedMVs.Contains(mv))
				{
					usedMVs.Add(mv);
					mv.SetParams(shapeTr, unityMat);
				}
			}
		}


		/// <summary>
		/// Generates a custom Unity shader using the given unlit color output.
		/// </summary>
		public static string GenerateShader(string name, MV_Base rgbOut)
		{
			//Insert the MaterialValues into various parts of the shader.

			StringBuilder shaderlabProperties = new StringBuilder();
			shaderlabProperties.AppendLine("\tProperties\n\t{");
			StringBuilder cgDefinitions = new StringBuilder();
			cgDefinitions.AppendLine("//------------Generated from MV_Base instances-------");
			StringBuilder funcBody = new StringBuilder();
			
			Insert(shaderlabProperties, cgDefinitions, funcBody, rgbOut);

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
			shader.Append(rgbOut.GetShaderValue(OutputSizes.Three));
			shader.Append(@", 1.0);
			}

			ENDCG
		}
	}
	Fallback ""Diffuse""
}");

			return shader.ToString();
		}
		/// <summary>
		/// Sets the parameters of the given Unity unlit material,
		///     assumed to be using a shader generated by the given Material Values.
		/// </summary>
		public static void SetMaterialParams(Material unityMat, params MV_Base[] mvRoots)
		{
			HashSet<MV_Base> usedMVs = new HashSet<MV_Base>();
			foreach (MV_Base mv in mvRoots.SelectMany(mvR => mvR.Hierarchy))
			{
				if (!usedMVs.Contains(mv))
				{
					usedMVs.Add(mv);
					mv.SetParams(null, unityMat);
				}
			}
		}
	}
}