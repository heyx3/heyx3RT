using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
	//TODO: Fix Material/SkyMaterial.

	public abstract class RTMaterial : MonoBehaviour, Serialization.ISerializableRT
	{
		public static List<RTMaterial> Materials = new List<RTMaterial>();


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

		
		[Serializable]
		[HideInInspector]
		private Material myMat = null;
		[Serializable]
		[HideInInspector]
		private Shader myShader = null;


		public abstract string TypeName { get; }


		protected virtual void Awake()
		{
			Materials.Add(this);

			//Load the shader/material.
			{
				string shaderFile = "";
				try
				{
					shaderFile = GetNewGeneratedFileName("Shad", ".shader");
					File.WriteAllText(shaderFile, GenerateUnityShader());
					AssetDatabase.ImportAsset(shaderFile);
					myShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderFile);
				}
				catch (Exception e)
				{
					Debug.LogError("Unable to create shader file \"" + shaderFile + "\": " + e.Message);
				}
			}
			{
				string matFile = "";
				try
				{
					matFile = GetNewGeneratedFileName("Mat", ".material");
					Material m = new Material(myShader);
					SetUnityMatParams(m);
					AssetDatabase.CreateAsset(m, matFile);
					myMat = AssetDatabase.LoadAssetAtPath<Material>(matFile);
				}
				catch (Exception e)
				{
					Debug.LogError("Unable to create material file \"" + matFile + "\": " + e.Message);
				}
			}

			//Create/update the mesh renderer.
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null)
				mr = gameObject.AddComponent<MeshRenderer>();
			mr.sharedMaterial = myMat;
		}
		protected virtual void OnDestroy()
		{
			Materials.Remove(this);
			
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myMat));
			myMat = null;

			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(myShader));
			myShader = null;
		}

		public abstract string GenerateUnityShader();
		public abstract void SetUnityMatParams(Material m);

		public virtual void WriteData(Serialization.DataWriter writer) { }
		public virtual void ReadData(Serialization.DataReader reader) { }
	}
}