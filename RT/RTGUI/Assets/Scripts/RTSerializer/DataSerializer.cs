using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RTSerializer
{
	public interface IWritable
	{
		void WriteData(Writer writer);
	}
	public interface IReadable
	{
		void ReadData(Reader reader);
	}

	public interface ISerializable : IWritable, IReadable { }


	/// <summary>
	/// Serializes information to some stream.
	/// </summary>
	public abstract class Writer
	{
		public abstract void WriteBool(bool value, string name);
		public abstract void WriteByte(byte value, string name);
		public abstract void WriteInt(int value, string name);
		public abstract void WriteUInt(uint value, string name);
		public abstract void WriteFloat(float value, string name);
		public abstract void WriteDouble(double value, string name);
		public abstract void WriteString(string value, string name);
		public abstract void WriteBytes(byte[] bytes, string name);

		public abstract void WriteDataStructure(IWritable toWrite, string name);

		public virtual void WriteVector2(Vector2 v, string name)
		{
			WriteDataStructure(new SimpleData.Vector2Writer(v), name);
		}
		public virtual void WriteVector3(Vector3 v, string name)
		{
			WriteDataStructure(new SimpleData.Vector3Writer(v), name);
		}
		public virtual void WriteVector4(Vector4 v, string name)
		{
			WriteDataStructure(new SimpleData.Vector4Writer(v), name);
		}
		public virtual void WriteQuaternion(Quaternion q, string name)
		{
			WriteDataStructure(new SimpleData.QuaternionWriter(q), name);
		}

		public void WriteTransform(Transform tr, string name)
		{
			WriteDataStructure(new SimpleData.TransformSerializer(tr), name);
		}

		public void WriteList<T>(IList<T> toWrite, Action<T, string, Writer> writeElementFunc, string name)
		{
			WriteDataStructure(new ListWriter<T>(toWrite, writeElementFunc), name);
		}


		/// <summary>
		/// Helper struct for "WriteList()".
		/// </summary>
		private class ListWriter<T> : IWritable
		{
			public IList<T> ToWrite;
			public Action<T, string, Writer> WriterFunc;
			public ListWriter(IList<T> toWrite, Action<T, string, Writer> writerFunc)
			{
				ToWrite = toWrite;
				WriterFunc = writerFunc;
			}
			public void WriteData(Writer wr)
			{
				wr.WriteInt(ToWrite.Count, "NValues");
				for (int i = 0; i < ToWrite.Count; ++i)
					WriterFunc(ToWrite[i], (i + 1).ToString(), wr);
			}
		}
	}

	/// <summary>
	/// Deserializes information from some stream.
	/// </summary>
	public abstract class Reader
	{
		public abstract bool ReadBool(string name);
		public abstract byte ReadByte(string name);
		public abstract int ReadInt(string name);
		public abstract uint ReadUInt(string name);
		public abstract float ReadFloat(string name);
		public abstract double ReadDouble(string name);
		public abstract string ReadString(string name);
		public abstract byte[] ReadBytes(string name);

		/// <summary>
		/// IMPORTANT NOTE: the IReadable is passed in, so it must generally be a class!
		/// </summary>
		public abstract void ReadDataStructure(IReadable toRead, string name);
		
		public virtual Vector2 ReadVector2(string name)
		{
			Vector2 v = new Vector2();
			IReadable rd = new SimpleData.Vector2Reader((v2) => v = v2);
			ReadDataStructure(rd, name);
			return v;
		}
		public virtual Vector3 ReadVector3(string name)
		{
			Vector3 v = new Vector3();
			IReadable rd = new SimpleData.Vector3Reader((v2) => v = v2);
			ReadDataStructure(rd, name);
			return v;
		}
		public virtual Vector4 ReadVector4(string name)
		{
			Vector4 v = new Vector4();
			IReadable rd = new SimpleData.Vector4Reader((v2) => v = v2);
			ReadDataStructure(rd, name);
			return v;
		}
		public virtual Quaternion ReadQuaternion(string name)
		{
			Quaternion q = new Quaternion();
			IReadable rd = new SimpleData.QuaternionReader((q2) => q = q2);
			ReadDataStructure(rd, name);
			return q;
		}

		public void ReadTransform(Transform tr, string name)
		{
			SimpleData.TransformSerializer ts = new SimpleData.TransformSerializer(tr);
			ReadDataStructure(ts, name);
		}

		public void ReadList<T>(IList<T> toRead, Func<string, Reader, T> readElementFunc, string name)
		{
			IReadable lrd = new ListReader<T>(toRead, readElementFunc);
			ReadDataStructure(lrd, name);
		}


		/// <summary>
		/// Helper struct for "ReadList()".
		/// </summary>
		private class ListReader<T> : IReadable
		{
			public IList<T> ToRead;
			public Func<string, Reader, T> ReaderFunc;
			public ListReader(IList<T> toRead, Func<string, Reader, T> readerFunc)
			{
				ToRead = toRead;
				ReaderFunc = readerFunc;
			}
			public void ReadData(Reader rd)
			{
				int n = rd.ReadInt("NValues");
				for (int i = 0; i < n; ++i)
					ToRead.Insert(i, ReaderFunc((i + 1).ToString(), rd));
			}
		}
	}


	/// <summary>
	/// Thrown when a Writer or Reader encounters an error.
	/// </summary>
	public class SerializerException : Exception
	{
		public SerializerException() : base() { }
		public SerializerException(string _message) : base(_message) { }
		public SerializerException(string _message, Exception _inner) : base(_message, _inner) { }
	}
}