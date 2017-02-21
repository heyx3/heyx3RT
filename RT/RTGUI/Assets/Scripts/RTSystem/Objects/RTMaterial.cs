using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT
{
	[ExecuteInEditMode]
	public abstract class RTMaterial : RTBaseMaterial, Serialization.ISerializableRT
	{
		public static HashSet<RTMaterial> Materials = new HashSet<RTMaterial>();

		protected const string TypeName_Lambert = "Lambert",
		                       TypeName_Metal = "Metal";


		public static void Serialize(RTMaterial mat, string name, Serialization.DataWriter writer)
		{
			writer.String(mat.TypeName, name + "Type");
			writer.Structure(mat, name + "Value");
		}
		public static RTMaterial Deserialize(GameObject toAttachTo, Serialization.DataReader reader, string name)
		{
			RTMaterial mat = toAttachTo.GetComponent<RTMaterial>();
			if (mat != null)
				DestroyImmediate(mat);

			string typeName = reader.String(name + "Type");
			switch (typeName)
			{
				case TypeName_Lambert:
					mat = toAttachTo.AddComponent<RTMaterial_Lambert>();
					break;
				case TypeName_Metal:
					mat = toAttachTo.AddComponent<RTMaterial_Metal>();
					break;
					
				default:
					throw new Serialization.DataReader.ReadException("Unknown material type: " + typeName);
			}

			reader.Structure(mat, name + "Value");

			return mat;
		}
		
		public abstract string TypeName { get; }
		

		public virtual void Awake()
		{
			Materials.Add(this);
		}
		private void OnDestroy()
		{
			Materials.Remove(this);
		}

		protected override string GenerateShader(string shaderName, Graph tempGraph,
												 List<MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base albedo, metallic, smoothness;
				GetUnityMaterialOutputs(tempGraph, out albedo, out metallic, out smoothness);

				var tempNodes = new HashSet<MV_Base>(albedo.HierarchyRootFirst
														   .Concat(metallic.HierarchyRootFirst)
														   .Concat(smoothness.HierarchyRootFirst)
														   .Where(n => !tempGraph.ContainsNode(n)));
				foreach (var node in tempNodes)
					tempGraph.AddNode(node);


				outTopLevelMVs.Add(albedo);
				outTopLevelMVs.Add(metallic);
				outTopLevelMVs.Add(smoothness);

				return MaterialValue.ShaderGenerator.GenerateShader(shaderName, tempGraph.UniqueNodeIDs,
																	albedo, metallic, smoothness);
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create shader \"" + shaderName + "\": " + e.Message);
				return null;
			}
		}
		/// <summary>
		/// Gets the outputs of this material in terms of the Unity standard shader.
		/// </summary>
		/// <param name="tempGraph">A copy of this instance's Graph that can be modified at will.</param>
		protected abstract void GetUnityMaterialOutputs(Graph tempGraph,
														out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness);
	}
}