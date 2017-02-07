using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
	[ExecuteInEditMode]
	public abstract class RTMaterial : MonoBehaviour, Serialization.ISerializableRT
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
		

		private const string GeneratedFolderPath = "Generated";
		private string GetNewGeneratedFileName(string namePrefix, string nameSuffix)
		{
			string fullDir = Path.Combine(Application.dataPath, GeneratedFolderPath);
			if (!Directory.Exists(fullDir))
				Directory.CreateDirectory(fullDir);

			int i = 0;
			while (File.Exists(Path.Combine(fullDir, namePrefix + i + nameSuffix)))
				i += 1;
			return Path.Combine(fullDir, namePrefix + i + nameSuffix).MakePathRelative("Assets");
		}

		
		[SerializeField]
		private Material myMat = null;
		[SerializeField]
		private Shader myShader = null;


		public abstract string TypeName { get; }
		protected abstract string GraphSerializationName { get; }
		
		public MaterialValue.Graph Graph
		{
			get
			{
				if (graph == null)
				{
					graph = new MaterialValue.Graph();
					InitGraph();
				}
				return graph;
			}
		}
		private MaterialValue.Graph graph = null;


		public virtual void Awake()
		{
			Materials.Add(this);

			//Create/update the mesh renderer.
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null)
				mr = gameObject.AddComponent<MeshRenderer>();
		}
		public virtual void Start()
		{
			RegenerateMaterial(GetComponent<MeshRenderer>());
		}
		protected virtual void OnDestroy()
		{
			Materials.Remove(this);
			
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myMat));
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myShader));
		}

		public void RegenerateMaterial(MeshRenderer mr)
		{
			//Clear out the old shader/material if they exist.
			if (myShader != null)
			{
				UnityEngine.Assertions.Assert.IsNotNull(myMat);
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myMat));
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myShader));
			}


			MaterialValue.MV_Base albedo, metallic, smoothness;
			GetUnityMaterialOutputs(out albedo, out metallic, out smoothness);
			HashSet<MaterialValue.MV_Base> tempNodes =
				new HashSet<MaterialValue.MV_Base>(albedo.HierarchyRootFirst
														 .Concat(metallic.HierarchyRootFirst)
														 .Concat(smoothness.HierarchyRootFirst)
														 .Where(n => !Graph.ContainsNode(n)));
			foreach (var node in tempNodes)
				Graph.AddNode(node);

			//Try loading the shader.
			string shaderFile = "";
			try
			{
				shaderFile = GetNewGeneratedFileName("Shad", ".shader");
				string shaderName = Path.GetFileNameWithoutExtension(shaderFile);

				string shaderText = MaterialValue.ShaderGenerator.GenerateShader(shaderName,
																				 Graph.UniqueNodeIDs,
																				 albedo,
																				 metallic,
																				 smoothness);

				File.WriteAllText(shaderFile, shaderText);
				AssetDatabase.ImportAsset(shaderFile);
				myShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderFile);

				UnityEngine.Assertions.Assert.IsNotNull(myShader, "Shader compilation failed!");
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create shader file \"" + shaderFile + "\": " + e.Message);
			}

			//Try loading the material.
			string matFile = "";
			try
			{
				matFile = GetNewGeneratedFileName("Mat", ".mat");
				myMat = new Material(myShader);
				MaterialValue.ShaderGenerator.SetMaterialParams(transform, myMat, graph.UniqueNodeIDs,
																albedo, metallic, smoothness);

				AssetDatabase.CreateAsset(myMat, matFile);
				myMat = AssetDatabase.LoadAssetAtPath<Material>(matFile);

				mr.sharedMaterial = myMat;
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create material file \"" + matFile + "\": " + e.Message);
			}

			//Clean up.
			foreach (var node in tempNodes)
				Graph.DeleteNode(node);
		}

		protected abstract void InitGraph();

		/// <summary>
		/// Gets the outputs of this material in terms of the Unity standard shader.
		/// </summary>
		/// <param name="toDelete">
		/// Any temporary MaterialValues that were created just for this method call.
		/// These should all have their Delete() method called once they're done being used.
		/// </param>
		protected abstract void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness);

		/// <summary>
		/// Gets the display name of the given root node.
		/// </summary>
		/// <param name="rootNodeIndex">
		/// The index of the node in "Graph.RootValues" to get the name of.
		/// </param>
		public abstract string GetRootNodeDisplayName(int rootNodeIndex);


		public virtual void WriteData(Serialization.DataWriter writer)
		{
			writer.Structure(Graph, GraphSerializationName);
		}
		public virtual void ReadData(Serialization.DataReader reader)
		{
			reader.Structure(Graph, GraphSerializationName);
		}
	}
}