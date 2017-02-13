using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


//Defines various data structures important to the Material Value graph system.


namespace RT.MaterialValue
{
	/// <summary>
	/// The different dimensionalities of MaterialValues' outputs.
	/// Can be used to represent a specific value, or a range of possible values.
	/// </summary>
	[Flags]
	public enum OutputSizes
	{
		/// <summary>
		/// Float
		/// </summary>
		One = 1,
		/// <summary>
		/// Vector2
		/// </summary>
		Two = 2,
		/// <summary>
		/// Vector3
		/// </summary>
		Three = 4,
		/// <summary>
		/// Vector4
		/// </summary>
		Four = 8,

		OneOrTwo = One | Two,
		OneOrThree = One | Three,
		OneOrFour = One | Four,
		All = One | Two | Three | Four,
	}
	public static class OutputSizesExtensions
	{
		public static OutputSizes ToOutputSize(this uint i)
		{
			switch (i)
			{
				case 1: return OutputSizes.One;
				case 2: return OutputSizes.Two;
				case 3: return OutputSizes.Three;
				case 4: return OutputSizes.Four;
				default: throw new NotImplementedException(i.ToString());
			}
		}
		public static uint ToNumber(this OutputSizes o)
		{
			switch (o)
			{
				case OutputSizes.One: return 1;
				case OutputSizes.Two: return 2;
				case OutputSizes.Three: return 3;
				case OutputSizes.Four: return 4;
				default: throw new NotImplementedException(o.ToString());
			}
		}
		public static string ToHLSLType(this OutputSizes o)
		{
			switch (o)
			{
				case OutputSizes.One: return "float";
				case OutputSizes.Two: return "float2";
				case OutputSizes.Three: return "float3";
				case OutputSizes.Four: return "float4";
				default: throw new NotImplementedException(o.ToString());
			}
		}

		public static bool IsSingleOption(this OutputSizes o)
		{
			switch (o)
			{
				case OutputSizes.One:
				case OutputSizes.Two:
				case OutputSizes.Three:
				case OutputSizes.Four:
					return true;
				default:
					return false;
			}
		}
		public static bool Contains(this OutputSizes o, OutputSizes o2) { return o.Intersection(o2) == o2; }

		public static OutputSizes Intersection(this OutputSizes o, OutputSizes o2) { return o & o2; }
		public static OutputSizes Union(this OutputSizes o, OutputSizes o2) { return o | o2; }
		public static OutputSizes Remove(this OutputSizes o, OutputSizes o2) { return o & ~o2; }
		
		public static OutputSizes Max(this OutputSizes o, OutputSizes o2) { return (OutputSizes)Math.Max((int)o, (int)o2); }
		public static OutputSizes Min(this OutputSizes o, OutputSizes o2) { return (OutputSizes)Math.Min((int)o, (int)o2); }
		public static OutputSizes MinIgnoring1D(this OutputSizes o, OutputSizes o2)
		{
			if (o == OutputSizes.One)
				return o2;
			else if (o2 == OutputSizes.One)
				return o;
			else
				return o.Min(o2);
		}

		public static IEnumerable<OutputSizes> GetSingleOptions(this OutputSizes o)
		{
			if (o.Contains(OutputSizes.One))
				yield return OutputSizes.One;
			if (o.Contains(OutputSizes.Two))
				yield return OutputSizes.Two;
			if (o.Contains(OutputSizes.Three))
				yield return OutputSizes.Three;
			if (o.Contains(OutputSizes.Four))
				yield return OutputSizes.Four;
		}
	}


	[Serializable]
	public class Vectorf : Serialization.ISerializableRT
	{
		public static readonly string[] ComponentStrings = { "x", "y", "z", "w" };


		public OutputSizes OutputSize;
		public Vector4 FullValue;


		public Vectorf() { }
		public Vectorf(Vectorf copy) { OutputSize = copy.OutputSize; FullValue = copy.FullValue; }
		public Vectorf(float f) { OutputSize = OutputSizes.One; FullValue = new Vector4(f, 0.0f, 0.0f, 0.0f); }
		public Vectorf(Vector2 v) { OutputSize = OutputSizes.Two; FullValue = new Vector4(v.x, v.y, 0.0f, 0.0f); }
		public Vectorf(Vector3 v) { OutputSize = OutputSizes.Three; FullValue = new Vector4(v.x, v.y, v.z, 0.0f); }
		public Vectorf(Vector4 v) { OutputSize = OutputSizes.Four; FullValue = new Vector4(v.x, v.y, v.z, v.w); }
		public Vectorf(float f, OutputSizes size) { OutputSize = size; FullValue = new Vector4(f, f, f, f); }


		public static implicit operator Vectorf(float f) { return new Vectorf(f); }
		public static implicit operator Vectorf(Vector2 v) { return new Vectorf(v); }
		public static implicit operator Vectorf(Vector3 v) { return new Vectorf(v); }
		public static implicit operator Vectorf(Vector4 v) { return new Vectorf(v); }
		
		public static implicit operator float(Vectorf v) { return v.FullValue.x; }
		public static implicit operator Vector2(Vectorf v)
		{
			return (v.OutputSize == OutputSizes.One ?
						new Vector2(v.FullValue.x, v.FullValue.x) :
						new Vector2(v.FullValue.x, v.FullValue.y));
		}
		public static implicit operator Vector3(Vectorf v)
		{
			return (v.OutputSize == OutputSizes.One ?
						new Vector3(v.FullValue.x, v.FullValue.x, v.FullValue.x) :
						new Vector3(v.FullValue.x, v.FullValue.y, v.FullValue.z));
		}
		public static implicit operator Vector4(Vectorf v)
		{
			return (v.OutputSize == OutputSizes.One ?
						new Vector4(v.FullValue.x, v.FullValue.x, v.FullValue.x, v.FullValue.x) :
						v.FullValue);
		}

		public static bool operator ==(Vectorf a, Vectorf b)
		{
			if (a.OutputSize != b.OutputSize)
				return false;
			for (uint i = 0; i < a.OutputSize.ToNumber(); ++i)
				if (a[i] != b[i])
					return false;
			return true;
		}
		public static bool operator !=(Vectorf a, Vectorf b) { return !(a == b); }
		public override bool Equals(object obj) { return obj is Vectorf && ((Vectorf)obj) == this; }
		public override int GetHashCode()
		{
			switch (OutputSize)
			{
				case OutputSizes.One: return FullValue.x.GetHashCode();
				case OutputSizes.Two: return new Vector2(FullValue.x, FullValue.y).GetHashCode();
				case OutputSizes.Three: return new Vector3(FullValue.x, FullValue.y, FullValue.z).GetHashCode();
				case OutputSizes.Four: return new Vector4(FullValue.x, FullValue.y, FullValue.z, FullValue.w).GetHashCode();
				default: throw new NotImplementedException(OutputSize.ToString());
			}
		}


		public float this[uint index]
		{
			get
			{
				switch (index)
				{
					case 0: return FullValue.x;
					case 1: return FullValue.y;
					case 2: return FullValue.z;
					case 3: return FullValue.w;
					default: throw new ArgumentOutOfRangeException(index.ToString());
				}
			}
			set
			{
				switch (index)
				{
					case 0: FullValue.x = value; break;
					case 1: FullValue.y = value; break;
					case 2: FullValue.z = value; break;
					case 3: FullValue.w = value; break;
					default: throw new ArgumentOutOfRangeException(index.ToString());
				}
			}
		}

		public Color RGB
		{
			get
			{
				switch (OutputSize)
				{
					case OutputSizes.One: return new Color(FullValue.x, FullValue.x, FullValue.x, 1.0f);
					case OutputSizes.Two: return new Color(FullValue.x, FullValue.y, 0.0f, 1.0f);
					case OutputSizes.Three:
					case OutputSizes.Four:
						return new Color(FullValue.x, FullValue.y, FullValue.z, 1.0f);
					default: throw new NotImplementedException(OutputSize.ToString());
				}
			}
			set
			{
				OutputSize = OutputSizes.Three;
				FullValue = new Vector4(value.r, value.g, value.b, 1.0f);
			}
		}
		public Color RGBA
		{
			get
			{
				switch (OutputSize)
				{
					case OutputSizes.One: return new Color(FullValue.x, FullValue.x, FullValue.x, FullValue.x);
					case OutputSizes.Two: return new Color(FullValue.x, FullValue.y, 0.0f, 1.0f);
					case OutputSizes.Three: return new Color(FullValue.x, FullValue.y, FullValue.z, 1.0f);
					case OutputSizes.Four: return new Color(FullValue.x, FullValue.y, FullValue.z, FullValue.w);
					default: throw new NotImplementedException(OutputSize.ToString());
				}
			}
			set
			{
				OutputSize = OutputSizes.Four;
				FullValue = new Vector4(value.r, value.g, value.b, value.a);
			}
		}


		public void WriteData(Serialization.DataWriter writer)
		{
			if (!OutputSize.IsSingleOption())
				throw new Serialization.DataWriter.WriteException("Output size is " + OutputSize.ToString());

			writer.UInt(OutputSize.ToNumber(), "Dimensions");
			for (uint i = 0; i < OutputSize.ToNumber(); ++i)
				writer.Float(this[i], ComponentStrings[i]);
		}
		public void ReadData(Serialization.DataReader reader)
		{
			OutputSize = reader.UInt("Dimensions").ToOutputSize();
			for (uint i = 0; i < OutputSize.ToNumber(); ++i)
				this[i] = reader.Float(ComponentStrings[i]);

			if (!OutputSize.IsSingleOption())
				throw new Serialization.DataReader.ReadException("Output size is " + OutputSize.ToString());
		}
	}


	[Serializable]
	public class EditableVectorf
	{
		public Vectorf V;

		/// <summary>
		/// A null value implies that the number of dimensions can't be changed.
		/// </summary>
		public OutputSizes AllowedDimensions;

		public float Min = 0.0f,
					 Max = 1.0f;

		public bool UseSliders = false;


		public EditableVectorf(Vectorf initialValue, bool useSliders, OutputSizes allowedDimensions,
							   float min = 0.0f, float max = 1.0f)
		{
			V = initialValue;
			AllowedDimensions = allowedDimensions;
			UseSliders = useSliders;
		}


		/// <summary>
		/// Writes this editor's data, not including the actual value.
		/// </summary>
		public void WriteData(Serialization.DataWriter writer, string prefix)
		{
			writer.Int((int)AllowedDimensions, prefix + "_AllowedDims");
			writer.Float(Min, prefix + "_Min");
			writer.Float(Max, prefix + "_Max");
			writer.Bool(UseSliders, prefix + "_UseSliders");
		}
		/// <summary>
		/// Reads this editor's data, not including the actual value.
		/// </summary>
		public void ReadData(Serialization.DataReader reader, string prefix)
		{
			AllowedDimensions = (OutputSizes)reader.Int(prefix + "_AllowedDims");
			Min = reader.Float(prefix + "_Min");
			Max = reader.Float(prefix + "_Max");
			UseSliders = reader.Bool(prefix + "_UseSliders");
		}


		/// <summary>
		/// Shows the GUI for editing this item.
		/// Returns whether anything changed.
		/// </summary>
		public bool DoGUI()
		{
			bool changed = false;

			//Edit the number of dimensions.
			if (AllowedDimensions != V.OutputSize)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label("N Dimensions");

				List<OutputSizes> allowedValues = AllowedDimensions.GetSingleOptions().ToList();
				int index = allowedValues.IndexOf(V.OutputSize);
				if (index > -1)
				{
					string[] content = allowedValues.Select(s => s.ToString()).ToArray();
					int newIndex = GUILayout.SelectionGrid(index, content, content.Length);
					if (newIndex != index)
					{
						V.OutputSize = allowedValues[newIndex];
						changed = true;
					}
				}

				GUILayout.EndHorizontal();
			}

			if (UseSliders)
			{
				//If the min is 0, max is 1, and number of inputs is 3 or 4,
				//    use a color editor.
				if (Min == 0.0f && Max == 1.0f && V.OutputSize == OutputSizes.Three)
				{
					Color newRGB = EditorGUILayout.ColorField(V.RGB);
					if (newRGB != V.RGB)
					{
						changed = true;
						V.RGB = newRGB;
					}
				}
				else if (Min == 0.0f && Max == 1.0f && V.OutputSize == OutputSizes.Four)
				{
					Color newRGBA = EditorGUILayout.ColorField(V.RGBA);
					if (newRGBA != V.RGBA)
					{
						changed = true;
						V.RGBA = newRGBA;
					}
				}
				//Otherwise, just use one slider for each component.
				else
				{
					GUILayout.BeginHorizontal();

					for (uint i = 0; i < V.OutputSize.ToNumber(); ++i)
					{
						if (i > 0)
							GUILayout.Space(15.0f);

						float newVal = GUILayout.HorizontalSlider(V[i], Min, Max,
																  GUILayout.MinWidth(20.0f));
						if (newVal != V[i])
						{
							changed = true;
							V[i] = newVal;
						}
					}

					GUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.BeginHorizontal();

				for (uint i = 0; i < V.OutputSize.ToNumber(); ++i)
				{
					string valueStr = GUILayout.TextField(V[i].ToString(),
														  GUILayout.MinWidth(30.0f));
					float f;
					if (float.TryParse(valueStr, out f) && f != V[i])
					{
						changed = true;
						V[i] = Mathf.Clamp(f, Min, Max);
					}
				}

				GUILayout.EndHorizontal();
			}

			return changed;
		}
	}
}