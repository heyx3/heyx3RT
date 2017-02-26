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


		protected override string GenerateShader(string shaderName, Graph tempGraph,
												 List<MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base albedo, metallic, smoothness;
				GetUnityMaterialOutputs(tempGraph, out albedo, out metallic, out smoothness);

				tempGraph.AddNode(albedo);
				tempGraph.AddNode(metallic);
				tempGraph.AddNode(smoothness);

				outTopLevelMVs.Add(albedo);
				outTopLevelMVs.Add(metallic);
				outTopLevelMVs.Add(smoothness);

				return MaterialValue.ShaderGenerator.GenerateShader(shaderName, tempGraph.UniqueNodeIDs,
																	albedo, metallic, smoothness);
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create shader \"" + shaderName + "\": " +
							       e.Message + "\n" + e.StackTrace);
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

		private Vector3 lastPos = Vector3.zero;
		private Vector3 lastScale = Vector3.one;
		private Quaternion lastRot = Quaternion.identity;
		private void Update()
		{
			Transform tr = transform;
			if (tr.position != lastPos || tr.lossyScale != lastScale || tr.rotation != lastRot)
			{
				//Get the final material graph.
				MaterialValue.Graph tempGraph = Graph.Clone();
				MV_Base[] mvOuts = new MV_Base[3];
				GetUnityMaterialOutputs(tempGraph, out mvOuts[0], out mvOuts[1], out mvOuts[2]);

				ShaderGenerator.SetMaterialParams(tr, GetComponent<Renderer>().sharedMaterial, 
												  tempGraph.UniqueNodeIDs, mvOuts);

				//Remember this position/scale/rotation.
				lastPos = tr.position;
				lastScale = tr.lossyScale;
				lastRot = tr.rotation;
			}
		}
	}
}