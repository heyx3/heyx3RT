using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
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
		

		private const string GeneratedFolderPath = "Assets\\Generated";
		private string GetNewGeneratedFileName(string namePrefix, string nameSuffix)
		{
			int i = 0;
			while (File.Exists(Path.Combine(GeneratedFolderPath, namePrefix + i + nameSuffix)))
				i += 1;
			return Path.Combine(GeneratedFolderPath, namePrefix + i + nameSuffix);
		}

		
		[SerializeField]
		private Material myMat = null;
		[SerializeField]
		private Shader myShader = null;


		public abstract string TypeName { get; }

		public abstract IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs { get; }


		protected virtual void Awake()
		{
			Materials.Add(this);

			//Create/update the mesh renderer.
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null)
				mr = gameObject.AddComponent<MeshRenderer>();
			RegenerateMaterial(mr);
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

			//Try loading the shader.
			string shaderFile = "";
			try
			{
				shaderFile = GetNewGeneratedFileName("Shad", ".shader");
				string shaderName = Path.GetFileNameWithoutExtension(shaderFile);

				string shaderText =
					MaterialValue.ShaderGenerator.GenerateShader(shaderName, albedo, metallic, smoothness);

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
				MaterialValue.ShaderGenerator.SetMaterialParams(transform, myMat,
																albedo, metallic, smoothness);

				AssetDatabase.CreateAsset(myMat, matFile);
				myMat = AssetDatabase.LoadAssetAtPath<Material>(matFile);

				mr.sharedMaterial = myMat;
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create material file \"" + matFile + "\": " + e.Message);
			}
		}
		protected abstract void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness);

		public virtual void WriteData(Serialization.DataWriter writer) { }
		public virtual void ReadData(Serialization.DataReader reader) { }
	}
}