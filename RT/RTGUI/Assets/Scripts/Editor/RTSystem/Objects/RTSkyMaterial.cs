using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
	public abstract class RTSkyMaterial : MonoBehaviour, Serialization.ISerializableRT
	{
		public static RTSkyMaterial Instance { get; private set; }

		protected const string TypeName_SimpleColor = "SimpleColor",
		                       TypeName_VerticalGradient = "VerticalGradient";
		

		public static void Serialize(RTSkyMaterial mat, string name, Serialization.DataWriter writer)
		{
			writer.String(mat.TypeName, name + "Type");
			writer.Structure(mat, name + "Value");
		}
		public static RTSkyMaterial Deserialize(GameObject obj, string name, Serialization.DataReader reader)
		{
			RTSkyMaterial skyMat = null;

			string typeName = reader.String(name + "Type");
			switch (typeName)
			{
				case TypeName_SimpleColor: skyMat = obj.AddComponent<RTSkyMaterial_SimpleColor>(); break;
				case TypeName_VerticalGradient: skyMat = obj.AddComponent<RTSkyMaterial_VerticalGradient>(); break;
				default: throw new NotImplementedException(typeName);
			}

			reader.Structure(skyMat, name + "Value");

			return skyMat;
		}
		
		
		private const string GeneratedFolderPath = "Assets\\Generated";
		private string GetNewGeneratedFileName(string namePrefix, string nameSuffix)
		{
			int i = 0;
			while (File.Exists(Path.Combine(GeneratedFolderPath, namePrefix + i + nameSuffix)))
				i += 1;
			return Path.Combine(GeneratedFolderPath, namePrefix + i + nameSuffix);
		}


		public float Distance = 2000.0f;

		[SerializeField]
		private Material myMat = null;
		[SerializeField]
		private Shader myShader = null;


		public abstract string TypeName { get; }

		public abstract IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs { get; }
		
		
		protected virtual void Awake()
		{
			if (Instance != null)
				Destroy(Instance.gameObject);
			Instance = this;

			//Create/update the mesh renderer.
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null)
				mr = gameObject.AddComponent<MeshRenderer>();
			RegenerateMaterial(mr);
		}
		protected virtual void OnDestroy()
		{
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myMat));
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myShader));
		}
		private void Update()
		{
			Transform tr = transform;
			tr.parent = null;
			tr.position = SceneView.currentDrawingSceneView.camera.transform.position;
			tr.localScale = Vector3.one * Distance;
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

			MaterialValue.MV_Base outRGB;
			GetUnityMaterialOutputs(out outRGB);

			//Try loading the shader.
			string shaderFile = "";
			try
			{
				shaderFile = GetNewGeneratedFileName("Shad", ".shader");
				string shaderName = Path.GetFileNameWithoutExtension(shaderFile);

				string shaderText =
					MaterialValue.ShaderGenerator.GenerateShader(shaderName, outRGB);

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
				matFile = GetNewGeneratedFileName("Mat", ".material");
				myMat = new Material(myShader);
				MaterialValue.ShaderGenerator.SetMaterialParams(myMat, outRGB);

				AssetDatabase.CreateAsset(myMat, matFile);
				myMat = AssetDatabase.LoadAssetAtPath<Material>(matFile);

				mr.sharedMaterial = myMat;
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create material file \"" + matFile + "\": " + e.Message);
			}
		}
		protected abstract void GetUnityMaterialOutputs(out MaterialValue.MV_Base outRGB);

		public virtual void WriteData(Serialization.DataWriter writer) { }
		public virtual void ReadData(Serialization.DataReader reader) { }
	}
}