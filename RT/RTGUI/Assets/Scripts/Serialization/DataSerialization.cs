using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace RT.Serialization
{
	public interface ISerializableRT
	{
		void ReadData(DataReader reader);
		void WriteData(DataWriter writer);
	}


	public abstract class DataWriter
	{
		public class WriteException : Exception { public WriteException(string msg) : base(msg) { } }


		public string ErrorMessage = "";


		public abstract void Bool(bool value, string name);
		public abstract void Byte(byte value, string name);
		public abstract void Int(int value, string name);
		public abstract void UInt(uint value, string name);
		public abstract void ULong(ulong value, string name);
		public abstract void Float(float value, string name);
		public abstract void Double(double value, string name);
		public abstract void String(string value, string name);
		public abstract void Bytes(byte[] value, string name);

		public abstract void Structure(ISerializableRT value, string name);
		

		public virtual void Vec2f(UnityEngine.Vector2 v, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", v.x);
			floats.Add("y", v.y);
			Structure(floats, name);
		}
		public virtual void Vec3f(UnityEngine.Vector3 v, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", v.x);
			floats.Add("y", v.y);
			floats.Add("z", v.z);
			Structure(floats, name);
		}
		public virtual void Vec4f(UnityEngine.Vector4 v, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", v.x);
			floats.Add("y", v.y);
			floats.Add("z", v.z);
			floats.Add("w", v.w);
			Structure(floats, name);
		}
		public virtual void Quaternion(UnityEngine.Quaternion q, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", q.x);
			floats.Add("y", q.y);
			floats.Add("z", q.z);
			floats.Add("w", q.w);
			Structure(floats, name);
		}
		public virtual void Rect(UnityEngine.Rect r, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", r.x);
			floats.Add("y", r.y);
			floats.Add("width", r.width);
			floats.Add("height", r.height);
			Structure(floats, name);
		}

		public delegate void ListElementWriter<T>(DataWriter w, T outVal, string name);
		public virtual void List<T>(List<T> data, string name, ListElementWriter<T> writeElementWithName)
		{
			ListSerializerWrapper<T> sList = new ListSerializerWrapper<T>();
			sList.Data = data;
			sList.ElementWriter = writeElementWithName;
			Structure(sList, name);
		}
	}

	public abstract class DataReader
	{
		public class ReadException : Exception { public ReadException(string msg) : base(msg) { } }


		public string ErrorMessage = "";

		
		public abstract bool Bool(string name);
		public abstract byte Byte(string name);
		public abstract int Int(string name);
		public abstract uint UInt(string name);
		public abstract ulong ULong(string name);
		public abstract float Float(string name);
		public abstract double Double(string name);
		public abstract string String(string name);
		public abstract byte[] Bytes(string name);

		public abstract void Structure(ISerializableRT outValue, string name);
		

		public virtual UnityEngine.Vector2 Vec2f(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Vector2(floats.ValuesByName[0].Value,
										   floats.ValuesByName[1].Value);
		}
		public virtual UnityEngine.Vector3 Vec3f(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("z", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Vector3(floats.ValuesByName[0].Value,
										   floats.ValuesByName[1].Value,
										   floats.ValuesByName[2].Value);
		}
		public virtual UnityEngine.Vector4 Vec4f(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("z", 0.0f);
			floats.Add("w", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Vector4(floats.ValuesByName[0].Value,
										   floats.ValuesByName[1].Value,
										   floats.ValuesByName[2].Value,
										   floats.ValuesByName[3].Value);
		}
		public virtual UnityEngine.Quaternion Quaternion(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("z", 0.0f);
			floats.Add("w", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Quaternion(floats.ValuesByName[0].Value,
											  floats.ValuesByName[1].Value,
											  floats.ValuesByName[2].Value,
											  floats.ValuesByName[3].Value);
		}
		public virtual UnityEngine.Rect Rect(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("width", 0.0f);
			floats.Add("height", 0.0f);
			Structure(floats, name);

			return new UnityEngine.Rect(floats.ValuesByName[0].Value, floats.ValuesByName[1].Value,
										floats.ValuesByName[2].Value, floats.ValuesByName[3].Value);
		}

		public delegate void ListElementReader<T>(DataReader r, ref T outVal, string name);
		public virtual List<T> List<T>(string name, ListElementReader<T> readElementWithName)
		{
			ListSerializerWrapper<T> sList = new ListSerializerWrapper<T>();
			sList.ElementReader = readElementWithName;
			Structure(sList, name);
			return sList.Data;
		}
	}

	
	#region Helpers for reading/writing special types.
	/// <summary>
	/// Helper class for DataReader/DataWriter. Please ignore.
	/// </summary>
	public class FloatSerializerWrapper : ISerializableRT
	{
		public List<KeyValuePair<string, float>> ValuesByName = new List<KeyValuePair<string, float>>();
		public void Add(string name, float val) { ValuesByName.Add(new KeyValuePair<string, float>(name, val)); }
		public void ReadData(DataReader reader)
		{
			for (int i = 0; i < ValuesByName.Count; ++i)
			{
				float f = reader.Float(ValuesByName[i].Key);
				ValuesByName[i] = new KeyValuePair<string, float>(ValuesByName[i].Key, f);
			}
		}
		public void WriteData(DataWriter writer)
		{
			foreach (KeyValuePair<string, float> kvp in ValuesByName)
				writer.Float(kvp.Value, kvp.Key);
		}
	}
	/// <summary>
	/// Helper class for DataReader/DataWriter. Please ignore.
	/// </summary>
	public class ListSerializerWrapper<T> : ISerializableRT
	{
		public List<T> Data = null;
		public DataWriter.ListElementWriter<T> ElementWriter = null;
		public DataReader.ListElementReader<T> ElementReader = null;
		public void ReadData(DataReader reader)
		{
			int size = (int)reader.UInt("NValues");

			Data = new List<T>(size);
			for (int i = 0; i < size; ++i)
			{
				T t = default(T);
				ElementReader(reader, ref t, (i + 1).ToString());
				Data.Add(t);
			}
		}
		public void WriteData(DataWriter writer)
		{
			writer.UInt((uint)Data.Count, "NValues");
			for (int i = 0; i < Data.Count; ++i)
				ElementWriter(writer, Data[i], (i + 1).ToString());
		}
	}
	#endregion
}
