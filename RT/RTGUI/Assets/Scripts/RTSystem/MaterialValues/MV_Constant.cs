﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;



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
											OutputSizes allowable = OutputSizes.One)
		{
			return new MV_Constant(new EditableVectorf(f, useSlider, allowable, min, max));
		}
		
		public static MV_Constant MakeVec2(Vector2 v,
										   bool useSlider = true, float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.One | OutputSizes.Two)
		{
			return MakeVec2(v, useSlider, min, max, allowable);
		}
		public static MV_Constant MakeVec2(float x, float y,
										   bool useSlider = true, float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.One | OutputSizes.Two)
		{
			return MakeVec2(new Vector2(x, y), useSlider, min, max, allowable);
		}

		public static MV_Constant MakeVec3(float x, float y, float z,
										   float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.OneOrThree)
		{
			return MakeVec3(new Vector3(x, y, z), min, max, allowable);
		}
		public static MV_Constant MakeVec3(Vector3 v,
										   float min = 0.0f, float max = 1.0f,
										   OutputSizes allowable = OutputSizes.OneOrThree)
		{
			return new MV_Constant(new EditableVectorf(v, false, allowable, min, max));
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


		[SerializeField]
		private EditableVectorf valueEditor;

		public EditableVectorf ValueEditor { get { return valueEditor; } }

		public Vectorf Value { get { return valueEditor.V; } set { valueEditor.V = value; } }
		public bool UseSliders { get { return valueEditor.UseSliders; } set { valueEditor.UseSliders = value; } }
		public float MinValue { get { return valueEditor.Min; } set { valueEditor.Min = value; } }
		public float MaxValue { get { return valueEditor.Max; } set { valueEditor.Max = value; } }
		public OutputSizes AllowableSizes { get { return valueEditor.AllowedDimensions; } set { valueEditor.AllowedDimensions = value; } }


		public MV_Constant(EditableVectorf initialValue)
		{
			valueEditor = initialValue;
		}


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody) { }

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);
			writer.Structure(Value, "Value");
			valueEditor.WriteData(writer, "Editor");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);
			reader.Structure(Value, "Value");
			valueEditor.ReadData(reader, "Editor");
		}
	}
}