using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

using RT.Serialization;


namespace RT.MaterialValue
{
	/// <summary>
	/// A set of singletons for the various input data -- surface, shape, and ray.
	/// </summary>
	public class MV_Inputs : MV_Base
	{
		public static readonly MV_Inputs SurfacePos = new MV_Inputs(0),
										 SurfaceNormal = new MV_Inputs(1),
										 SurfaceTangent = new MV_Inputs(2),
										 SurfaceBitangent = new MV_Inputs(3),
										 SurfaceUV = new MV_Inputs(4),
										 RayStart = new MV_Inputs(5),
										 RayDir = new MV_Inputs(6),
										 ShapePos = new MV_Inputs(7),
										 ShapeRot = new MV_Inputs(8),
										 ShapeScale = new MV_Inputs(9);


		public static string[] OptionsCode = { RTSystem.Input_WorldPos, RTSystem.Input_WorldNormal,
											   RTSystem.Input_Tangent, RTSystem.Input_Bitangent,
											   RTSystem.Input_UV,
											   RTSystem.Input_CamPos, RTSystem.Input_RayDir,
											   RTSystem.Param_ShapePos, RTSystem.Param_ShapeRot,
											   RTSystem.Param_ShapeScale };
		public static string[] OptionsGUI = { "Surface Position", "Surface Normal",
											  "Surface Tangent", "Surface Bitangent",
											  "Surface UV",
											  "Ray Start", "Ray Dir",
											  "Shape Pos", "Shape Rot Axis-Angle", "Shape Scale" };
		public static string[] OptionsType = { TypeName_SurfPos, TypeName_SurfNormal,
											   TypeName_SurfTangent, TypeName_SurfBitangent,
											   TypeName_SurfUV,
											   TypeName_RayStartPos, TypeName_RayDir,
											   TypeName_ShapePos, TypeName_ShapeRot, TypeName_ShapeScale };

        
		private int selectedOption = 0;


		public override string TypeName { get { return OptionsType[selectedOption]; } }
		public override OutputSizes OutputSize
		{
			get
			{
				switch (OptionsType[selectedOption])
				{
					case TypeName_SurfUV:
						return OutputSizes.Two;
					case TypeName_SurfPos:
					case TypeName_SurfNormal:
					case TypeName_SurfTangent:
					case TypeName_SurfBitangent:
					case TypeName_RayStartPos:
					case TypeName_RayDir:
					case TypeName_ShapePos:
					case TypeName_ShapeScale:
						return OutputSizes.Three;
					case TypeName_ShapeRot:
						return OutputSizes.Four;

					default: throw new NotImplementedException(OptionsType[selectedOption]);
				}
			}
		}

		public override string ShaderValueName(Dictionary<MV_Base, uint> idLookup) { return OptionsCode[selectedOption]; }
		public override bool IsUsableInSkyMaterial
		{
			get
			{
				switch (TypeName)
				{
					case TypeName_RayStartPos:
					case TypeName_RayDir:
						return true;
					case TypeName_SurfPos:
					case TypeName_SurfUV:
					case TypeName_SurfNormal:
					case TypeName_SurfTangent:
					case TypeName_SurfBitangent:
					case TypeName_ShapePos:
					case TypeName_ShapeRot:
					case TypeName_ShapeScale:
						return false;

					default: throw new NotImplementedException(TypeName);
				}
			}
		}

		public override string PrettyName { get { return OptionsGUI[selectedOption]; } }
		public override UnityEngine.Color GUIColor { get { return new Color(1.0f, 1.0f, 0.85f); } }


		private MV_Inputs(int _selectedOption = 0) { selectedOption = _selectedOption; }


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			//If the shape data params haven't been defined yet, define them.
			if (!shaderlabProperties.ToString().Contains(RTSystem.Param_ShapePos))
			{
				shaderlabProperties.Append("\t\t\t");
				shaderlabProperties.Append(RTSystem.Param_ShapePos);
				shaderlabProperties.AppendLine(" (\"Shape Pos\", Vector) = (1.0,1.0,1.0,0.0)");
				
				shaderlabProperties.Append("\t\t\t");
				shaderlabProperties.Append(RTSystem.Param_ShapeRot);
				shaderlabProperties.AppendLine(" (\"Shape Rot\", Vector) = (0.0,1.0,0.0,0.0)");

				shaderlabProperties.Append("\t\t\t");
				shaderlabProperties.Append(RTSystem.Param_ShapeScale);
				shaderlabProperties.AppendLine(" (\"Shape Scale\", Vector) = (1.0,1.0,1.0,0.0)");

				cgDefinitions.Append("\t\t\t\tfloat3 ");
				cgDefinitions.Append(RTSystem.Param_ShapePos);
				cgDefinitions.Append(", ");
				cgDefinitions.Append(RTSystem.Param_ShapeScale);
				cgDefinitions.AppendLine(";");
				cgDefinitions.Append("\t\t\t\tfloat4 ");
				cgDefinitions.Append(RTSystem.Param_ShapeRot);
				cgDefinitions.AppendLine(";");
			}
		}
		public override void SetParams(Transform shapeTr, Material unityMat,
									   Dictionary<MV_Base, uint> idLookup)
		{
			//If we get a null reference exception accessing "shapeTr", we must be using a sky material.
			try
			{
				switch (OptionsType[selectedOption])
				{
					case TypeName_ShapePos:
						unityMat.SetVector(RTSystem.Param_ShapePos, shapeTr.position);
						break;
					case TypeName_ShapeScale:
						unityMat.SetVector(RTSystem.Param_ShapeScale, shapeTr.lossyScale);
						break;
					case TypeName_ShapeRot:
						Vector3 axis;
						float angle;
						shapeTr.rotation.ToAngleAxis(out angle, out axis);
						unityMat.SetVector(RTSystem.Param_ShapeRot,
										   new Vector4(axis.x, axis.y, axis.z,
													   Mathf.Deg2Rad * angle));
						break;
				}
			}
			catch (NullReferenceException)
			{
				Debug.LogError("Tried to use 'Shape' inputs in a Sky Material shader!");
			}
		}

		public override GUIResults DoCustomGUI()
		{
			GUIResults results = GUIResults.Nothing;

			int newChoice = EditorGUILayout.Popup(selectedOption, OptionsGUI,
												  GUILayout.MinWidth(100.0f));
			if (newChoice != selectedOption)
			{
				selectedOption = newChoice;
				results = GUIResults.Other;
			}

			return results;
		}

		//Note that we don't need to serialize which type of data is selected,
		//    because that is already determined by the node's type.
	}
}