using System;
using System.Xml;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace RT
{
	public class RTShape_Mesh : RTShape
	{
		public override string TypeName { get { return TypeName_Mesh; } }


		public string MeshGUID;

		private Mesh procMesh = null;

		

		protected override void DoMyGUI()
		{
#if UNITY_EDITOR
			Mesh m = GetUnityMesh();
			Mesh newM = (Mesh)EditorGUILayout.ObjectField(m, typeof(Mesh), false);
			if (newM != m)
			{
				MeshGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newM));
			}
#endif
		}

		public override Mesh GetUnityMesh()
		{
			if (procMesh != null)
				return procMesh;
			
#if UNITY_EDITOR
			string path = AssetDatabase.GUIDToAssetPath(MeshGUID);
			if (path != null)
			{
				return (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
			}
			else
			{
				return null;
			}
#else
			return null;
#endif
		}

		protected override void WriteCustomData(XmlElement parentNode)
		{
			Mesh m = GetUnityMesh();
			if (m == null)
			{
				Debug.LogError("No mesh is set for an RTShape_Mesh!");
				XmlUtil.SetAttr(parentNode, "Error", "No mesh was set!");
				return;
			}
			if (!m.isReadable)
			{
				Debug.LogError("Mesh is not marked as readable!");
				XmlUtil.SetAttr(parentNode, "Error", "Mesh file was not readable in Unity!");
				return;
			}

			int[] tris = m.triangles;
			int nTris = tris.Length / 3;
			
			XmlUtil.SetAttr(parentNode, "NTris", nTris.ToString());
			XmlUtil.SetAttr(parentNode, "GUID", MeshGUID);

			XmlElement triEl = parentNode.OwnerDocument.CreateElement("Tri");
			for (int i = 0; i < nTris; ++i)
			{
				if (i % 3 == 2)
				{
					parentNode.AppendChild(triEl);
					triEl = parentNode.OwnerDocument.CreateElement("Tri");
				}

				XmlElement vertEl = parentNode.OwnerDocument.CreateElement("Vert");
				triEl.AppendChild(vertEl);

				XmlUtil.SetAttr(vertEl, "Pos", XmlUtil.ToString(m.vertices[tris[i]]));
				XmlUtil.SetAttr(vertEl, "UVx", m.uv[tris[i]].x.ToString());
				XmlUtil.SetAttr(vertEl, "UVy", m.uv[tris[i]].y.ToString());
				XmlUtil.SetAttr(vertEl, "Normal", XmlUtil.ToString(m.normals[tris[i]]));
				XmlUtil.SetAttr(vertEl, "Tangent", XmlUtil.ToString((Vector3)m.tangents[tris[i]]));
				XmlUtil.SetAttr(vertEl, "Bitangent", XmlUtil.ToString(Vector3.Cross(m.normals[tris[i]],
				                                                                    (Vector3)m.tangents[tris[i]]) *
				                                                       m.tangents[tris[i]].w));
			}
		}
		protected override void ReadCustomData (XmlElement parentNode)
		{
			procMesh = null;
			MeshGUID = parentNode.GetAttribute("GUID");

			if (MeshGUID == null || AssetDatabase.GUIDToAssetPath(MeshGUID) == null)
			{
				//TODO: Provide a right-click option for RTShape_Mesh to save the procMesh to an asset file.
				Debug.LogWarning("A mesh shape had an invalid or missing \"GUID\" attribute, " +
				                 "so the data is being created procedurally! " +
				                 "This means that the mesh is not linked to any mesh asset in Unity.");

				string str;

				str = parentNode.GetAttribute("NTris");
				int nTris;
				if (str == null || !int.TryParse(str, out nTris))
				{
					Debug.LogError("Missing or invalid number of triangles in mesh");
					return;
				}

				//Read all the vertex data.

				Vector3[] poses = new Vector3[nTris],
				          normals = new Vector3[nTris];
				Vector4[] tangents = new Vector4[nTris];
				Vector2[] uvs = new Vector2[nTris];
				int[] indices = new int[nTris];

				int i = 0;
				XmlElement triEl = XmlUtil.FindElement(parentNode, "Tri");
				while (triEl != null)
				{
					XmlElement vertEl = XmlUtil.FindElement(triEl, "Vert");
					while (vertEl != null)
					{
						poses[i] = new Vector3();
						str = vertEl.GetAttribute("Pos");
						if (str == null || !XmlUtil.FromString(str, ref poses[i]))
						{
							Debug.LogError("Missing or invalid 'Pos' attribute in mesh vertex");
							return;
						}

						normals[i] = new Vector3();
						str = vertEl.GetAttribute("Normal");
						if (str == null || !XmlUtil.FromString(str, ref normals[i]))
						{
							Debug.LogError("Missing or invalid 'Normal' attribute in mesh vertex");
							return;
						}

						Vector3 tang = Vector3.zero,
						        bitang = Vector3.zero;
						str = vertEl.GetAttribute("Tangent");
						if (str == null || !XmlUtil.FromString(str, ref tang))
						{
							Debug.LogError("Missing or invalid 'Tangent' attribute in mesh vertex");
							return;
						}
						str = vertEl.GetAttribute("Bitangent");
						if (str == null || !XmlUtil.FromString(str, ref bitang))
						{
							Debug.LogError("Missing or invalid 'Bitangent' attribute in mesh vertex");
							return;
						}
						float w = (Vector3.Dot(Vector3.Cross(normals[i], tang), bitang) > 0.0f ? 1.0f : -1.0f);
						tangents[i] = new Vector4(tang.x, tang.y, tang.z, w);

						uvs[i] = new Vector2();
						str = vertEl.GetAttribute("UVx");
						if (str == null || !float.TryParse(str, out uvs[i].x))
						{
							Debug.LogError("Missing or invalid 'UVx' attribute in mesh vertex");
							return;
						}
						str = vertEl.GetAttribute("UVy");
						if (str == null || !float.TryParse(str, out uvs[i].y))
						{
							Debug.LogError("Missing or invalid 'UVy' attribute in mesh vertex");
							return;
						}

						indices[i] = i;
						i += 1;
					}

					triEl = XmlUtil.FindSiblingElement(triEl, "Tri");
				}

				procMesh = new Mesh();
				procMesh.vertices = poses;
				procMesh.normals = normals;
				procMesh.tangents = tangents;
				procMesh.uv = uvs;
				procMesh.triangles = indices;
				procMesh.UploadMeshData(false);
			}
			else
			{
				procMesh = null;
			}
		}
	}
}