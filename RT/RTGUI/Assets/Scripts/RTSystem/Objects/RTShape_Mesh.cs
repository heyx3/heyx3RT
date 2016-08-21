using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT
{
	[ExecuteInEditMode]
	public class RTShape_Mesh : RTShape
	{
		public override string TypeName { get { return TypeName_Mesh; } }
		public override Mesh UnityMesh { get { return myMesh; } }


		[SerializeField]
		private Mesh myMesh;

		private string myMeshGUID;


		public override void Awake()
		{
			base.Awake();
			OnValidate();
		}
		void OnValidate()
		{
			myMeshGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(myMesh));
			GetComponent<MeshFilter>().sharedMesh = myMesh;
		}

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);

			//Write out the GUID of the mesh alongside the actual RT-side data.
			writer.String(myMeshGUID, "GUID");

			//Turn mesh data into a list of vertices.
			if (!myMesh.isReadable)
				throw new Serialization.DataWriter.WriteException("Mesh " + myMesh.name + " isn't readable");
			int[] indices = myMesh.triangles;
			Vector3[] poses = myMesh.vertices,
					  normals = myMesh.normals;
			Vector4[] _tangents = myMesh.tangents;
			Vector2[] uvs = myMesh.uv;
			Vector3[] tangents = new Vector3[_tangents.Length],
					  bitangents = new Vector3[_tangents.Length];
			for (int i = 0; i < _tangents.Length; ++i)
			{
				tangents[i] = (Vector3)_tangents[i];
				bitangents[i] = _tangents[i].w * Vector3.Cross(normals[i], tangents[i]);
			}
			List<Vertex> verts = new List<Vertex>(indices.Length);
			for (int i = 0; i < indices.Length; ++i)
			{
				int vertI = indices[i];
				verts.Add(new Vertex(poses[vertI], normals[vertI],
									 tangents[vertI], bitangents[vertI],
									 uvs[vertI]));
			}

			//Write out the vertices.
			writer.List(verts, "Vertices", (wr, vert, name) => wr.Structure(vert, name));
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);

			//Try to get the GUID of the mesh file.
			try
			{
				myMeshGUID = reader.String("GUID");
			}
			catch (Serialization.DataReader.ReadException)
			{
				myMeshGUID = null;
			}

			//If it doesn't exist, create a new mesh asset for the data.
			if (myMeshGUID == null || AssetDatabase.GUIDToAssetPath(myMeshGUID) == null)
			{
				Debug.LogError("Mesh doesn't have a valid GUID, so a new mesh file will be generated with the data");

				List<Vertex> verts = reader.List("Vertices",
					(Serialization.DataReader rd, ref Vertex vert, string name) =>
					{
						vert = new Vertex();
						rd.Structure(vert, name);
					});

				//Convert the vertices to actual mesh data.
				Vector3[] poses = new Vector3[verts.Count],
						  normals = new Vector3[verts.Count];
				Vector4[] tangents = new Vector4[verts.Count];
				Vector2[] uvs = new Vector2[verts.Count];
				int[] indices = new int[verts.Count];
				for (int i = 0; i < verts.Count; ++i)
				{
					poses[i] = verts[i].Pos;
					normals[i] = verts[i].Normal;
					uvs[i] = verts[i].UV;
					indices[i] = i;

					tangents[i] = new Vector4(verts[i].Tangent.x, verts[i].Tangent.y, verts[i].Tangent.z, 1.0f);
					if (Vector3.Dot(normals[i],
									Vector3.Cross(verts[i].Tangent,
												  verts[i].Bitangent)) < 0.0001f)
					{
						tangents[i] = new Vector4(tangents[i].x, tangents[i].y, tangents[i].z, -1.0f);
					}
				}
				
				//Create the mesh.
				myMesh = new Mesh();
				myMesh.vertices = poses;
				myMesh.normals = normals;
				myMesh.tangents = tangents;
				myMesh.uv = uvs;
				myMesh.triangles = indices;
				myMesh.UploadMeshData(false);
				myMesh.RecalculateBounds();

				//Find an unused mesh file name.
				string meshFileName = "Assets\\RT Meshes\\0.asset";
				int meshI = 0;
				while (AssetDatabase.FindAssets(meshFileName).Length > 0)
				{
					meshI += 1;
					meshFileName = "Assets\\RT Meshes\\" + meshI + ".asset";
				}

				//Save the mesh to a file.
				AssetDatabase.CreateAsset(myMesh, meshFileName);
				AssetDatabase.SaveAssets();
				Debug.Log("Created mesh " + meshFileName);
				OnValidate();
			}
			//Otherwise, just get the mesh with that GUID.
			else
			{
				myMesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(myMeshGUID));
			}
		}
		#region Helper struct for serialization
		private class Vertex : Serialization.ISerializableRT
		{
			public Vector3 Pos, Normal, Tangent, Bitangent;
			public Vector2 UV;
			public Vertex() { }
			public Vertex(Vector3 pos, Vector3 norm, Vector3 tangent, Vector3 bitangent, Vector2 uv)
				{ Pos = pos; Normal = norm; Tangent = tangent; Bitangent = bitangent; UV = uv; }
			public void WriteData(Serialization.DataWriter writer)
			{
				writer.Vec3f(Pos, "Pos");
				writer.Vec3f(Normal, "Normal");
				writer.Vec3f(Tangent, "Tangent");
				writer.Vec3f(Bitangent, "Bitangent");
				writer.Vec2f(UV, "UV");
			}
			public void ReadData(Serialization.DataReader reader)
			{
				Pos = reader.Vec3f("Pos");
				Normal = reader.Vec3f("Normal");
				Tangent = reader.Vec3f("Tangent");
				Bitangent = reader.Vec3f("Bitangent");
				UV = reader.Vec2f("UV");
			}
		}
		#endregion
	}
}