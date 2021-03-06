﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
	[ExecuteInEditMode]
	public abstract class RTBaseMaterial : MonoBehaviour, Serialization.ISerializableRT
	{
		public const string GeneratedFolderPath = "Generated";
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
		private string myGraphJSON = "";
		[SerializeField]
		private Material myMat = null;
		[SerializeField]
		private Shader myShader = null;

		protected MaterialValue.Graph GraphCopy = null;
		protected List<MaterialValue.MV_Base> OutTopLevelMVs = new List<MaterialValue.MV_Base>();

		
		protected abstract string GraphSerializationName { get; }

		public string GraphJSON { get { return myGraphJSON; } }

		public MaterialValue.Graph Graph
		{
			get
			{
				if (graph == null)
				{
					ReloadGraph();
					SaveGraph();
				}
				return graph;
			}
		}
		private MaterialValue.Graph graph = null;
		public void ReloadGraph()
		{
			graph = new MaterialValue.Graph();

			//Try to load the graph.
			//If the graph doesn't exist, or loading fails, create a new one.
			if (myGraphJSON == "" || !LoadGraph())
			{
				InitGraph();
			}
		}
		public void ResetGraph()
		{
			graph.Clear(true);
			InitGraph();
		}


		protected virtual void Start()
		{
			//Create the mesh renderer.
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null)
				mr = gameObject.AddComponent<MeshRenderer>();

			RegenerateMaterial();
		}
		

		public void NullifyMaterial()
		{
			if (myMat != null)
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myMat));
				myMat = null;
			}
			if (myShader != null)
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myShader));
			}
		}
		public void RegenerateMaterial()
		{
			Undo.RecordObject(this, "Update material/graph");
			
			//Make sure nobody shares this object's material/shader.
			//This can happen if a copy/paste was done.
			if (myMat != null || myShader != null)
			{
				foreach (RTBaseMaterial mat in FindObjectsOfType<RTBaseMaterial>())
					if (mat != this)
					{
						if (myMat != null && mat.myMat == myMat)
							myMat = null;
						if (myShader != null && mat.myShader == myShader)
							myShader = null;
					}
			}

			//Clear out the old shader/material if they exist.
			NullifyMaterial();

			//If the graph doesn't exist yet, load it.
			if (graph == null)
				ReloadGraph();
			//Make sure it's up-to-date.
			SaveGraph();

			GraphCopy = Graph.Clone();

			//Try loading the shader.
			string shaderFile = "";
			OutTopLevelMVs = new List<MaterialValue.MV_Base>();
			try
			{
				shaderFile = GetNewGeneratedFileName("Shad", ".shader");
				string shaderName = Path.GetFileNameWithoutExtension(shaderFile);

				string shaderText = GenerateShader(shaderName, GraphCopy, OutTopLevelMVs);
				if (shaderText == null)
					return;
					
				File.WriteAllText(shaderFile, shaderText);
				AssetDatabase.ImportAsset(shaderFile);
				myShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderFile);

				UnityEngine.Assertions.Assert.IsNotNull(myShader, "Shader compilation failed!");
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create shader file \"" + shaderFile + "\": " +
							       e.Message + "\n\n" + e.StackTrace);
			}

			//Try loading the material.
			string matFile = "";
			try
			{
				matFile = GetNewGeneratedFileName("Mat", ".mat");
				myMat = new Material(myShader);
				MaterialValue.ShaderGenerator.SetMaterialParams(transform, myMat, GraphCopy.UniqueNodeIDs,
																OutTopLevelMVs.ToArray());

				AssetDatabase.CreateAsset(myMat, matFile);
				myMat = AssetDatabase.LoadAssetAtPath<Material>(matFile);

                GetComponent<MeshRenderer>().sharedMaterial = myMat;
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create material file \"" + matFile + "\": " +
							   e.Message + "\n\n" + e.StackTrace);
			}
		}
		/// <summary>
		/// Should do the following things:
		///     * Generate MV_Base instances representing the final outputs of the Unity material
		///     * Store those instances in "outTopLevelMVs"
		///     * Generate shader text for the Unity material using those outputs
		///     * Return that shader text, or "null" if something went wrong.
		/// </summary>
		/// <param name="shaderName">The name the shader should have.</param>
		/// <param name="tempGraph">
		/// A copy of this instance's Graph.
		/// This method can make any modifications it wants to the graph.
		/// </param>
		protected abstract string GenerateShader(string shaderName, MaterialValue.Graph tempGraph,
												 List<MaterialValue.MV_Base> outTopLevelMVs);
		
		protected abstract void InitGraph();
        private bool LoadGraph()
        {
            try
            {
                var reader = new Serialization.JSONReader(new StringReader(myGraphJSON));
                reader.Structure(graph, "graph");

				GraphCopy = new MaterialValue.Graph();
				graph.Clone(GraphCopy);

				return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't deserialize graph for GameObject \"" + gameObject.name +
							       "\": (" + e.GetType().Name + ") " +
								   e.Message + "\nStack: " + e.StackTrace);

				GraphCopy = null;
				OutTopLevelMVs.Clear();

                return false;
            }
        }
        private bool SaveGraph()
        {
            try
            {
                //Write the graph into "myGraphJSON".
				var json = new System.Text.StringBuilder();
				using (var writer = new Serialization.JSONWriter(new StringWriter(json)))
					writer.Structure(graph, "graph");

				string newJSON = json.ToString();
				if (myGraphJSON != newJSON)
				{
					myGraphJSON = newJSON;
					ReloadGraph();
				}

            return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't serialize graph for GameObject \"" + gameObject.name +
							       "\": (" + e.GetType().Name + ") " +
								   e.Message + "\nStack: " + e.StackTrace);
                return false;
            }
        }


		/// <summary>
		/// Gets the display name of the given root node.
		/// </summary>
		public abstract string GetRootNodeDisplayName(int rootNodeIndex);

		public virtual void WriteData(Serialization.DataWriter writer)
		{
			writer.Structure(Graph, GraphSerializationName);
		}
		public virtual void ReadData(Serialization.DataReader reader)
		{
			reader.Structure(Graph, GraphSerializationName);

			GraphCopy = new MaterialValue.Graph();
			Graph.Clone(GraphCopy);
		}
	}
}
