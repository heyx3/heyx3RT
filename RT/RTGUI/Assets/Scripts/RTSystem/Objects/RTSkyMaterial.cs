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
	public abstract class RTSkyMaterial : RTBaseMaterial, Serialization.ISerializableRT
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


		public float Distance = 2000.0f;

		public abstract string TypeName { get; }

		
		protected override void Start()
		{
			//Create/update the mesh filter.
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null)
				mf = gameObject.AddComponent<MeshFilter>();
			mf.sharedMesh = RTSystem.Instance.SkySphere;
			
			base.Start();
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
		public void OnValidate()
		{
			Update();
		}

		protected override string GenerateShader(string shaderName, Graph tempGraph, List<MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base rgb;
				GetUnityMaterialOutputs(tempGraph, out rgb);

				var tempNodes = new HashSet<MV_Base>(rgb.HierarchyRootFirst);
				foreach (var node in tempNodes.Where(n => !tempGraph.ContainsNode(n)))
					tempGraph.AddNode(node);


				outTopLevelMVs.Add(rgb);

				return MaterialValue.ShaderGenerator.GenerateShader(shaderName, rgb,
																	tempGraph.UniqueNodeIDs,
																	false);
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create shader \"" + shaderName + "\": " + e.Message);
				return null;
			}
		}

		/// <summary>
		/// Gets the final output color of the sky.
		/// Used to compile a Unity material for the sky.
		/// </summary>
		/// <param name="tempGraph">A copy of this instance's Graph that can be modified at will.</param>
		protected abstract void GetUnityMaterialOutputs(Graph tempGraph, out MaterialValue.MV_Base outRGB);
		
		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);
			writer.Float(Distance, "DrawDistance");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);
			Distance = reader.Float("DrawDistance");
		}
	}
}