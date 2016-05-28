using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace RTSerializer
{
	public class JsonWriter : Writer
	{
		private JSONObject doc;


		public JsonWriter() { ClearData(); }


		/// <summary>
		/// Saves all written data out to a JSON file at the given path.
		/// Returns an error message, or null if the data was saved successfully.
		/// </summary>
		/// <param name="compact">Whether to minify the JSON to save space.</param>
		public string SaveData(string path, bool compact)
		{
			try
			{
				File.WriteAllText(path, GetData(compact));
				return null;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
		/// <summary>
		/// Gets the written data as a JSON string.
		/// </summary>
		/// <param name="compact">Whether to minify the JSON to save space.</param>
		public string GetData(bool compact)
		{
			return doc.ToString(true);
		}

		public void ClearData() { doc = new JSONObject(new Dictionary<string, string>()); }


		public override void WriteBool(bool value, string name) { doc.AddField(name, value); }
		public override void WriteByte(byte value, string name) { doc.AddField(name, (int)value); }
		public override void WriteInt(int value, string name) { doc.AddField(name, value); }
		public override void WriteUInt(uint value, string name)
		{
			if (value > (uint)int.MaxValue)
				throw new ArgumentOutOfRangeException("\"" + name + "\" is too large!");

			doc.AddField(name, (int)value);
		}
		public override void WriteFloat(float value, string name) { doc.AddField(name, value); }
		public override void WriteDouble(double value, string name) { doc.AddField(name, (float)value); }
		public override void WriteString(string value, string name) { doc.AddField(name, value); }
		public override void WriteBytes(byte[] bytes, string name) { WriteString(Convert.ToBase64String(bytes), name); }
		public override void WriteDataStructure(IWritable toWrite, string name)
		{
			JSONObject obj = new JSONObject(new Dictionary<string, string>());
			toWrite.WriteData(new JsonWriter(obj));
			doc.AddField(name, obj);
		}


		private JsonWriter(JSONObject parentObj) { doc = parentObj; }
	}

	public class JsonReader : Reader
	{
		private JSONObject doc = null;


		/// <summary>
		/// If there was an error parsing the given file,
		/// An error message (or "null" if everything went fine) will be output to the given string.
		/// </summary>\
		public JsonReader(string filePath, ref string outErrorMsg)
		{
			outErrorMsg = Reload(filePath);
		}


		/// <summary>
		/// Loads in new JSON data from the given file path, resetting this reaader.
		/// Returns an error message, or "null" if the file was loaded successfully.
		/// </summary>
		public string Reload(string filePath)
		{
			doc = null;
			string status = null;

			try
			{
				status = "reading file";
				string data = File.ReadAllText(filePath);

				status = "parsing file";
				doc = new JSONObject(data);

				return null;
			}
			catch (Exception e)
			{
				return "Error " + status + ": " + e.Message;
			}
		}

		
		public override bool ReadBool(string name)
		{
			bool b;
			if (!doc.GetField(out b, name, false))
				throw new Exception("Couldn't parse '" + name + "' to a bool");
			return b;
		}
		public override byte ReadByte(string name) { return (byte)ReadInt(name); }
		public override int ReadInt(string name)
		{
			int i;
			if (!doc.GetField(out i, name, -1))
				throw new Exception("Couldn't parse '" + name + "' to an int");
			return i;
		}
		public override uint ReadUInt(string name) { return (uint)ReadInt(name); }
		public override float ReadFloat(string name)
		{
			float f;
			if (!doc.GetField(out f, name, float.NaN))
				throw new Exception("Couldn't parse '" + name + "' to a float");
			return f;
		}
		public override double ReadDouble(string name) { return (double)ReadFloat(name); }
		public override string ReadString(string name)
		{
			JSONObject obj = doc.GetField(name);
			if (obj == null)
			{
				throw new Exception("Field '" + name + "' wasn't found");
			}
			if (obj.type != JSONObject.Type.STRING)
			{
				throw new Exception("Field '" + name + "' wasn't a string");
			}
			return obj.str;
		}
		public override byte[] ReadBytes(string name) { return Convert.FromBase64String(ReadString(name)); }
		public override void ReadDataStructure(IReadable toRead, string name)
		{
			JSONObject obj = doc.GetField(name);
			if (obj == null)
			{
				throw new Exception("Field '" + name + "' wasn't found");
			}
			if (obj.type != JSONObject.Type.OBJECT)
			{
				throw new Exception("Field '" + name + "' wasn't a data structure");
			}

			toRead.ReadData(new JsonReader(obj));
		}


		private JsonReader(JSONObject parentObj) { doc = parentObj; }
	}
}