using System;
using UnityEngine;


namespace RT
{
	public class RTVertex : RTSerializer.ISerializable
	{
		public Vector3 Pos, Normal, Tangent, Bitangent;
		public Vector2 UV;


		public RTVertex(Vector3 pos, Vector3 normal, Vector3 tangent, Vector3 bitangent, Vector2 uv)
		{
			Pos = pos;
			Normal = normal;
			Tangent = tangent;
			Bitangent = bitangent;
			UV = uv;
		}
		public RTVertex(Vector3 pos, Vector3 normal, Vector4 unityTangent, Vector2 uv)
		{
			Pos = pos;
			Normal = normal;
			Tangent = new Vector3(unityTangent.x, unityTangent.y, unityTangent.z);
			Bitangent = unityTangent.w * Vector3.Cross(Normal, Tangent);
			UV = uv;
		}

		public void WriteData(RTSerializer.Writer wr)
		{
			wr.WriteVector3(Pos, "Pos");
			wr.WriteVector3(Normal, "Normal");
			wr.WriteVector3(Tangent, "Tangent");
			wr.WriteVector3(Bitangent, "Bitangent");
			wr.WriteVector2(UV, "UV");
		}
		public void ReadData(RTSerializer.Reader rd)
		{
			Pos = rd.ReadVector3("Pos");
			Normal = rd.ReadVector3("Normal");
			Tangent = rd.ReadVector3("Tangent");
			Bitangent = rd.ReadVector3("Bitangent");
			UV = rd.ReadVector2("UV");
		}
	}
}
