using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
	[ExecuteInEditMode]
	public abstract class RTSkyMaterial : MonoBehaviour, Serialization.ISerializableRT
	{
		public static RTSkyMaterial Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<RTSkyMaterial>();
				return instance;
			}
		}
		private static RTSkyMaterial instance = null;

		protected const string TypeName_SimpleColor = "SimpleColor",
		                       TypeName_VerticalGradient = "VerticalGradient";
		

		public static void Serialize(RTSkyMaterial mat, string name, Serialization.DataWriter writer)
		{
			writer.String(mat.TypeName, name + "Type");
			writer.Structure(mat, name + "Value");
		}
		public static RTSkyMaterial Deserialize(GameObject obj, string name, Serialization.DataReader reader)
		{
			//Delete the old SkyMaterial component if it exists.
			RTSkyMaterial skyMat = Instance;
			if (skyMat != null)
			{
				if (skyMat.gameObject == obj)
					DestroyImmediate(skyMat);
				else
					DestroyImmediate(skyMat.gameObject);

			}

			//Get the type of Sky Material and create one of that type.
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


		public float Distance = 2000.0f;

		[SerializeField]
		private Material myMat = null;
		[SerializeField]
		private Shader myShader = null;


		public abstract string TypeName { get; }

		public abstract IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs { get; }
		
		
		public virtual void Awake()
		{
			Debug.Log("START");

			//Create/update the mesh filter.
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null)
				mf = gameObject.AddComponent<MeshFilter>();
			mf.sharedMesh = RTSystem.Instance.SkySphere;
			
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
			SceneView currentSV = SceneView.lastActiveSceneView;
			if (currentSV == null)
				return;

			Transform tr = transform;
			tr.parent = null;
			tr.position = currentSV.camera.transform.position;
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
				matFile = GetNewGeneratedFileName("Mat", ".mat");
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