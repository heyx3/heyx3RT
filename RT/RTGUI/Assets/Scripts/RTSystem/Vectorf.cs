using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RT
{
	public class Vectorf : RTSerializer.ISerializable
	{
		public static readonly string[] ComponentStrings = { "x", "y", "z", "w" };


		public uint NValues;
		public Vector4 Value;


		public Vectorf() { }
		public Vectorf(float f) { NValues = 1; Value = new Vector4(f, 0.0f, 0.0f, 0.0f); }
		public Vectorf(Vector2 v) { NValues = 2; Value = new Vector4(v.x, v.y, 0.0f, 0.0f); }
		public Vectorf(Vector3 v) { NValues = 3; Value = new Vector4(v.x, v.y, v.z, 0.0f); }
		public Vectorf(Vector4 v) { NValues = 4; Value = new Vector4(v.x, v.y, v.z, v.w); }
		public Vectorf(float f, uint nValues)
		{
			NValues = nValues;
			for (uint i = 0; i < 4; ++i)
				this[i] = f;
		}


		public static implicit operator Vectorf(float f) { return new Vectorf(f); }
		public static implicit operator Vectorf(Vector2 v) { return new Vectorf(v); }
		public static implicit operator Vectorf(Vector3 v) { return new Vectorf(v); }
		public static implicit operator Vectorf(Vector4 v) { return new Vectorf(v); }

		public static implicit operator float(Vectorf v) { return v.Value.x; }
		public static implicit operator Vector2(Vectorf v)
		{
			return (v.NValues == 1 ?
						new Vector2(v.Value.x, v.Value.x) :
						new Vector2(v.Value.x, v.Value.y));
		}
		public static implicit operator Vector3(Vectorf v)
		{
			return (v.NValues == 1 ?
						new Vector3(v.Value.x, v.Value.x, v.Value.x) :
						new Vector3(v.Value.x, v.Value.y, v.Value.z));
		}
		public static implicit operator Vector4(Vectorf v)
		{
			return (v.NValues == 1 ?
						new Vector4(v.Value.x, v.Value.x, v.Value.x, v.Value.x) :
						v.Value);
		}


		public float this[uint index]
		{
			get
			{
				switch (index)
				{
					case 0: return Value.x;
					case 1: return Value.y;
					case 2: return Value.z;
					case 3: return Value.w;
					default: throw new ArgumentOutOfRangeException(index.ToString());
				}
			}
			set
			{
				switch (index)
				{
					case 0: Value.x = value; break;
					case 1: Value.y = value; break;
					case 2: Value.z = value; break;
					case 3: Value.w = value; break;
					default: throw new ArgumentOutOfRangeException(index.ToString());
				}
			}
		}


		public Color ToRGB()
		{
			if (NValues == 1)
				return new Color(Value.x, Value.x, Value.x);
			else
				return new Color(Value.x, Value.y, Value.z);
		}
		public Color ToRGBA()
		{
			if (NValues == 1)
				return new Color(Value.x, Value.x, Value.x, Value.x);
			else if (NValues == 2)
				return new Color(Value.x, Value.x, Value.x, Value.y);
			else
				return new Color(Value.x, Value.y, Value.z, Value.w);
		}

		/// <summary>
		/// Shows the GUI for editing this Vectorf's components using sliders.
		/// </summary>
		/// <param name="allowedNValues">
		/// Pass "null" if the number of components shouldn't be editable.
		/// </param>
		public void DoGUI(float tabLevel, float sliderMin, float sliderMax,
						  GUIStyle sliderBar, GUIStyle sliderThumb,
						  uint[] allowedNValues = null)
		{
			DoGUI(tabLevel, allowedNValues,
				  (i) =>
				  {
					  this[i] = GUILayout.HorizontalSlider(this[i], sliderMin, sliderMax,
														   sliderBar, sliderThumb);
				  });
		}
		/// <summary>
		/// Shows the GUI for editing this Vectorf's component using text boxes.
		/// </summary>
		/// <param name="allowedNValues">
		/// Pass "null" if the number of components shouldn't be editable.
		/// </param>
		public void DoGUI(float tabLevel, GUIStyle style, uint[] allowedNValues = null)
		{
			DoGUI(tabLevel, allowedNValues,
				  (i) =>
				  {
					  string str = GUILayout.TextField(this[i].ToString(), style);
					  float f;
					  if (float.TryParse(str, out f))
						  this[i] = f;
				  });
		}

		private void DoGUI(float tabLevel, uint[] allowedNValues, Action<uint> doComponent)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(tabLevel);
			GUILayout.BeginVertical();
			{
				if (allowedNValues != null)
				{
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("N Dimensions:");

						int index = allowedNValues.IndexOf(NValues);
						if (index > -1)
						{
							string[] content = allowedNValues.Select(u => ComponentStrings[u]).ToArray();
							index = GUILayout.SelectionGrid(index, content, allowedNValues.Length);
							NValues = allowedNValues[index];
						}
					}
					GUILayout.EndHorizontal();
				}

				for (uint i = 0; i < NValues; ++i)
				{
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label(ComponentStrings[i]);
						doComponent(i);
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		public void WriteData(RTSerializer.Writer writer)
		{
			writer.WriteUInt(NValues, "Dimensions");
			for (uint i = 0; i < NValues; ++i)
			{
				writer.WriteFloat(this[i], ComponentStrings[i]);
			}
		}
		public void ReadData(RTSerializer.Reader reader)
		{
			NValues = reader.ReadUInt("Dimensions");
			for (uint i = 0; i < NValues; ++i)
			{
				this[i] = reader.ReadFloat(ComponentStrings[i]);
			}
		}
	}
}