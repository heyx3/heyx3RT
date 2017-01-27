using System;
using System.Collections.Generic;
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

		public abstract IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs { get; }


		public virtual void Awake()
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
			foreach (var nameAndVal in Outputs)
				nameAndVal.Value.Delete();

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


			HashSet<MaterialValue.MV_Base> toDelete = new HashSet<MaterialValue.MV_Base>();
			MaterialValue.MV_Base albedo, metallic, smoothness;
			GetUnityMaterialOutputs(out albedo, out metallic, out smoothness, toDelete);

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
				matFile = GetNewGeneratedFileName("Mat", ".mat");
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

			//Clean up.
			foreach (MaterialValue.MV_Base tempVal in toDelete)
				tempVal.Delete();
		}

		/// <summary>
		/// Gets the outputs of this material in terms of the Unity standard shader.
		/// </summary>
		/// <param name="toDelete">
		/// Any temporary MaterialValues that were created just for this method call.
		/// These should all have their Delete() method called once they're done being used.
		/// </param>
		protected abstract void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness,
														HashSet<MaterialValue.MV_Base> toDelete);

		/// <summary>
		/// Outputs into the given dictionary the MaterialValues that define this material.
		/// </summary>
		public abstract void GetMVs(Dictionary<string, MaterialValue.MV_Base> outVals);
		/// <summary>
		/// Tells the material (passed as an argument for convenience) to use the given MaterialValues.
		/// </summary>
		public abstract void SetMVs(Dictionary<string, MaterialValue.MV_Base> newVals);


		public virtual void WriteData(Serialization.DataWriter writer) { }
		public virtual void ReadData(Serialization.DataReader reader)
		{
			//Clean up the outputs before the new ones are read.
			foreach (var nameAndVal in Outputs)
				nameAndVal.Value.Delete();
		}
	}
}