using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using RT.Serialization;

namespace RT.MaterialValue
{
	/// <summary>
	/// A MaterialValue that just outputs a constant value.
	/// </summary>
	[Serializable]
	public class MV_Constant : MV_Base
	{
		public static MV_Constant MakeFloat(float f,
											bool useSlider = true, float min = 0.0f, float max = 1.0f,
											OutputSizes allowable = OutputSizes.One,
											bool isInline = false)
		{
			return new MV_Constant(new EditableVectorf(f, useSlider, allowable, min, max), isInline);
		}
		
		public static MV_Constant MakeVec2(Vector2 v,
										   bool useSlider = true, float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.One | OutputSizes.Two,
										   bool isInline = false)
		{
			return MakeVec2(v, useSlider, min, max, allowable, isInline);
		}
		public static MV_Constant MakeVec2(float x, float y,
										   bool useSlider = true, float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.One | OutputSizes.Two,
										   bool isInline = false)
		{
			return MakeVec2(new Vector2(x, y), useSlider, min, max, allowable, isInline);
		}

		public static MV_Constant MakeVec3(float x, float y, float z,
										   float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.OneOrThree,
										   bool isInline = false)
		{
			return MakeVec3(new Vector3(x, y, z), min, max, allowable, isInline);
		}
		public static MV_Constant MakeVec3(Vector3 v,
										   float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.OneOrThree,
										   bool isInline = false)
		{
			return new MV_Constant(new EditableVectorf(v, false, allowable, min, max), isInline);
		}

		public static MV_Constant MakeRGB(Color col)
		{
			Vectorf val = new Vectorf();
			val.RGB = col;
			return new MV_Constant(new EditableVectorf(val, true, OutputSizes.Three, 0.0f, 1.0f));
		}
		public static MV_Constant MakeRGBA(Color col)
		{
			Vectorf val = new Vectorf();
			val.RGBA = col;
			return new MV_Constant(new EditableVectorf(val, true, OutputSizes.Four, 0.0f, 1.0f));
		}


		public override string TypeName { get { return TypeName_Constant; } }
		public override OutputSizes OutputSize { get { return Value.OutputSize; } }
		public override string PrettyName { get { return "Constant"; } }

		public override string ShaderValueName
		{
			get
			{
				if (Value.OutputSize == OutputSizes.One)
					return Value.FullValue.x.ToString();

				System.Text.StringBuilder valStr = new System.Text.StringBuilder();
				valStr.Append(Value.OutputSize.ToHLSLType());
				valStr.Append("(");
				for (uint i = 0; i < Value.OutputSize.ToNumber(); ++i)
				{
					if (i > 0)
						valStr.Append(", ");
					valStr.Append(Value[i]);
				}
				valStr.Append(")");
				return valStr.ToString();
			}
		}


		/// <summary>
		/// If true, this instance is used by a single node
		///     and should not be given its own node in the editor GUI.
		/// </summary>
		public bool IsInline = false;

		[SerializeField]
		private EditableVectorf valueEditor;
		public EditableVectorf ValueEditor { get { return valueEditor; } }

		public Vectorf Value { get { return valueEditor.V; } set { valueEditor.V = value; } }
		public bool UseSliders { get { return valueEditor.UseSliders; } set { valueEditor.UseSliders = value; } }
		public float MinValue { get { return valueEditor.Min; } set { valueEditor.Min = value; } }
		public float MaxValue { get { return valueEditor.Max; } set { valueEditor.Max = value; } }
		public OutputSizes AllowableSizes { get { return valueEditor.AllowedDimensions; } set { valueEditor.AllowedDimensions = value; } }


		public MV_Constant(EditableVectorf initialValue, bool isInline = false)
		{
			valueEditor = initialValue;
			IsInline = isInline;
		}


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody) { }

		public override void WriteData(DataWriter writer, string namePrefix,
									   Dictionary<MV_Base, uint> idLookup)
		{
			base.WriteData(writer, namePrefix, idLookup);

			writer.Structure(Value, namePrefix + "Value");
			valueEditor.WriteData(writer, namePrefix + "Editor");

			writer.Bool(IsInline, "IsInline");
		}
		public override void ReadData(DataReader reader, string namePrefix,
									  Dictionary<MV_Base, List<uint>> childIDsLookup)
		{
			base.ReadData(reader, namePrefix, childIDsLookup);

			reader.Structure(Value, namePrefix + "Value");
			valueEditor.ReadData(reader, namePrefix + "Editor");

			IsInline = reader.Bool("IsInline");
		}
	}
}
