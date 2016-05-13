using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RTSerializer.SimpleData
{
	internal struct Vector2Writer : IWritable
	{
		public Vector2 Val;
		public Vector2Writer(Vector2 val) { Val = val; }
		public void WriteData(Writer wr)
		{
			wr.WriteFloat(Val.x, "x");
			wr.WriteFloat(Val.y, "y");
		}
	}
	internal struct Vector3Writer : IWritable
	{
		public Vector3 Val;
		public Vector3Writer(Vector3 val) { Val = val; }
		public void WriteData(Writer wr)
		{
			wr.WriteFloat(Val.x, "x");
			wr.WriteFloat(Val.y, "y");
			wr.WriteFloat(Val.z, "z");
		}
	}
	internal struct Vector4Writer : IWritable
	{
		public Vector4 Val;
		public Vector4Writer(Vector4 val) { Val = val; }
		public void WriteData(Writer wr)
		{
			wr.WriteFloat(Val.x, "x");
			wr.WriteFloat(Val.y, "y");
			wr.WriteFloat(Val.z, "z");
			wr.WriteFloat(Val.w, "w");
		}
	}
	internal struct QuaternionWriter : IWritable
	{
		public Quaternion Quat;
		public QuaternionWriter(Quaternion quat) { Quat = quat; }
		public void WriteData(Writer wr)
		{
			wr.WriteFloat(Quat.x, "x");
			wr.WriteFloat(Quat.y, "y");
			wr.WriteFloat(Quat.z, "z");
			wr.WriteFloat(Quat.w, "w");
		}
	}

	
	internal struct Vector2Reader : IReadable
	{
		public Action<Vector2> OnRead;
		public Vector2Reader(Action<Vector2> onRead) { OnRead = onRead; }
		public void ReadData(Reader rd)
		{
			OnRead(new Vector2(rd.ReadFloat("x"),
							   rd.ReadFloat("y")));
		}
	}
	internal struct Vector3Reader : IReadable
	{
		public Action<Vector3> OnRead;
		public Vector3Reader(Action<Vector3> onRead) { OnRead = onRead; }
		public void ReadData(Reader rd)
		{
			OnRead(new Vector3(rd.ReadFloat("x"),
							   rd.ReadFloat("y"),
							   rd.ReadFloat("z")));
		}
	}
	internal struct Vector4Reader : IReadable
	{
		public Action<Vector4> OnRead;
		public Vector4Reader(Action<Vector4> onRead) { OnRead = onRead; }
		public void ReadData(Reader rd)
		{
			OnRead(new Vector4(rd.ReadFloat("x"),
							   rd.ReadFloat("y"),
							   rd.ReadFloat("z"),
							   rd.ReadFloat("w")));
		}
	}
	internal struct QuaternionReader : IReadable
	{
		public Action<Quaternion> OnRead;
		public QuaternionReader(Action<Quaternion> onRead) { OnRead = onRead; }
		public void ReadData(Reader rd)
		{
			OnRead(new Quaternion(rd.ReadFloat("x"),
							      rd.ReadFloat("y"),
							      rd.ReadFloat("z"),
							      rd.ReadFloat("w")));
		}
	}


	internal struct TransformSerializer : ISerializable
	{
		public Transform Tr;
		public TransformSerializer(Transform tr) { Tr = tr; }
		public void WriteData(Writer wr)
		{
			wr.WriteVector3(Tr.localPosition, "Pos");
			wr.WriteQuaternion(Tr.localRotation, "QuaternionRot");
			wr.WriteVector3(Tr.localScale, "Scale");
		}
		public void ReadData(Reader rd)
		{
			Transform tr = Tr;
			tr.localPosition = rd.ReadVector3("Pos");
			tr.localRotation = rd.ReadQuaternion("QuaternionRot");
			tr.localScale = rd.ReadVector3("Scale");
		}
	}
}