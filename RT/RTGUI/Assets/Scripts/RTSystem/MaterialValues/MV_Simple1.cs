using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	/// <summary>
	/// A MaterialValue that only takes one parameter.
	/// </summary>
	public class MV_Simple1 : MV_Base
	{
		public static MV_Simple1 Normalize(MV_Base x)
		{
			return new MV_Simple1("normalize($0)", "Normalize", TypeName_Normalize, "X", x,
								  new EditableVectorf(new Vectorf(1.0f, OutputSizes.Three),
													  false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Length(MV_Base x)
		{
			return new MV_Simple1("length($0)", "Length", TypeName_Length, "X", x,
								  new EditableVectorf(new Vectorf(1.0f, OutputSizes.Three),
													  false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity),
								  true);
		}
		public static MV_Simple1 Sqrt(MV_Base x)
		{
			return new MV_Simple1("sqrt($0)", "Square Root", TypeName_Sqrt, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Sin(MV_Base x)
		{
			return new MV_Simple1("sin($0)", "Sine", TypeName_Sin, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Cos(MV_Base x)
		{
			return new MV_Simple1("cos($0)", "Cosine", TypeName_Cos, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Tan(MV_Base x)
		{
			return new MV_Simple1("tan($0)", "Tangent", TypeName_Tan, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Asin(MV_Base x)
		{
			return new MV_Simple1("asin($0)", "Inverse Sine", TypeName_Asin, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  -1.0f, 1.0f));
		}
		public static MV_Simple1 Acos(MV_Base x)
		{
			return new MV_Simple1("acos($0)", "Inverse Cosine", TypeName_Acos, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  -1.0f, 1.0f));
		}
		public static MV_Simple1 Atan(MV_Base x)
		{
			return new MV_Simple1("atan($0)", "Inverse Tangent", TypeName_Atan, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Smoothstep(MV_Base x)
		{
			return new MV_Simple1("smoothstep(0.0, 1.0, $0)", "Smoothstep", TypeName_Smoothstep, "T", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Smootherstep(MV_Base x)
		{
			return new MV_Simple1("smoothstep(0.0, 1.0, smoothstep(0.0, 1.0, $0))",
								  "Smootherstep", TypeName_Smootherstep, "T", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Floor(MV_Base x)
		{
			return new MV_Simple1("floor($0)", "Floor", TypeName_Floor, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Ceil(MV_Base x)
		{
			return new MV_Simple1("ceil($0)", "Ceiling", TypeName_Ceil, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple1 Abs(MV_Base x)
		{
			return new MV_Simple1("abs($0)", "Absolute Value", TypeName_Abs, "X", x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}

		
		private string toDo, prettyName, typeName, inputName;
		private EditableVectorf defaultValue;
		private bool outputSizeIsOne;


		public override string TypeName { get { return typeName; } }
		public override OutputSizes OutputSize
		{
			get
			{
				if (outputSizeIsOne)
					return OutputSizes.One;
				else
					return GetInput(0).OutputSize;
			}
		}

		public override string PrettyName { get { return prettyName; } }


		/// <summary>
		/// Creates a MaterialValue that does something with a parameter.
		/// </summary>
		/// <param name="_toDo">The expression to evaluate. Use '$0' in place of the parameter.</param>
		/// <param name="x">The parameter. Pass null to generate a default one.</param>
		/// <param name="_outputSizeIsOne">
		/// If false, the output size of this instance is equal to the output size of its input.
		/// If true, the output sizee of this instance is One.
		/// </param>
		private MV_Simple1(string _toDo, string _prettyName, string _typeName,
						   string _inputName, MV_Base x, EditableVectorf _defaultValue,
						   bool _outputSizeIsOne = false)
		{
			toDo = _toDo;
			prettyName = _prettyName;
			typeName = _typeName;
			inputName = _inputName;
			defaultValue = _defaultValue;

			outputSizeIsOne = _outputSizeIsOne;

			if (x == null)
				AddInput(GetDefaultInput(0));
			else
				AddInput(x);
		}


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			cgFunctionBody.Append(OutputSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = (");
			cgFunctionBody.Append(toDo.Replace("$0", GetInput(0).ShaderValueName(idLookup)));
			cgFunctionBody.AppendLine(");");
		}

		public override MV_Base GetDefaultInput(int inputIndex) { return new MV_Constant(defaultValue, true); }
		public override string GetInputName(int index) { return inputName; }


		//Note that this class's fields don't need to be serialized
		//    because all that extra data is inferred from the type name when deserializing.
	}
}