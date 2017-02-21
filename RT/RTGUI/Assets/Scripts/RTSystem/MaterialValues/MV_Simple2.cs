using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace RT.MaterialValue
{
	/// <summary>
	/// A MaterialValue that takes two parameters.
	/// </summary>
	public class MV_Simple2 : MV_Base
	{
		public static MV_Simple2 Distance(MV_Base a, MV_Base b)
		{
			return new MV_Simple2("distance($0, $1)", "Distance", TypeName_Distance, "A", "B", a, b,
								  new EditableVectorf(new Vectorf(0.0f, OutputSizes.Three),
													  false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple2 Atan2(MV_Base y, MV_Base x)
		{
			return new MV_Simple2("atan2($0, $1)", "Atan2", TypeName_Atan2, "Y", "X", y, x,
								  new EditableVectorf(new Vectorf(1.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple2 Step(MV_Base edge, MV_Base x)
		{
			return new MV_Simple2("step($0, $1)", "Step", TypeName_Step, "Edge", "X", edge, x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity),
								  new EditableVectorf(new Vectorf(0.5f), false, OutputSizes.All,
													  float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple2 Dot(MV_Base a, MV_Base b)
		{
			return new MV_Simple2("dot($0, $1)", "Dot Product", TypeName_Dot, "A", "B", a, b,
								  new EditableVectorf(new Vectorf(1.0f, OutputSizes.One),
													  false, OutputSizes.All));
		}

		
		private string toDo, prettyName, typeName, inputName1, inputName2;
		private EditableVectorf defaultValue1, defaultValue2;


		public override string TypeName { get { return typeName; } }
		public override OutputSizes OutputSize { get { return GetInput(0).OutputSize.Max(GetInput(1).OutputSize); } }

		public override string PrettyName { get { return prettyName; } }

		
		/// <summary>
		/// Creates a MaterialValue that does something with two parameters.
		/// </summary>
		/// <param name="_toDo">
		/// The expression to evaluate. Use '$0' and '$1' in place of the parameters.
		/// </param>
		/// <param name="x">The first parameter. Pass null to generate a default one.</param>
		/// <param name="y">The second parameter. Pass null to generate a default one.</param>
		private MV_Simple2(string _toDo, string _prettyName, string _typeName,
						   string _inputName1, string _inputName2, MV_Base x, MV_Base y,
						   EditableVectorf _defaultValue1, EditableVectorf _defaultValue2)
		{
			toDo = _toDo;
			prettyName = _prettyName;
			typeName = _typeName;
			inputName1 = _inputName1;
			inputName2 = _inputName2;
			defaultValue1 = _defaultValue1;
			defaultValue2 = _defaultValue2;

			if (x == null)
				AddInput(GetDefaultInput(0));
			else
				AddInput(x);

			if (y == null)
				AddInput(GetDefaultInput(1));
			else
				AddInput(y);
		}
		/// <summary>
		/// Creates a MaterialValue that does something with two parameters.
		/// </summary>
		/// <param name="_toDo">
		/// The expression to evaluate. Use '$0' and '$1' in place of the parameters.
		/// </param>
		/// <param name="x">The first parameter. Pass null to generate a default one.</param>
		/// <param name="y">The second parameter. Pass null to generate a default one.</param>
		private MV_Simple2(string _toDo, string _prettyName, string _typeName,
						   string _inputName1, string _inputName2, MV_Base x, MV_Base y,
						   EditableVectorf _defaultValue)
			: this(_toDo, _prettyName, _typeName, _inputName1, _inputName2,
				   x, y, _defaultValue, _defaultValue) { }


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			cgFunctionBody.Append(OutputSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = (");
			cgFunctionBody.Append(toDo.Replace("$0", GetInput(0).GetShaderValue(OutputSize, idLookup))
									  .Replace("$1", GetInput(1).GetShaderValue(OutputSize, idLookup)));
			cgFunctionBody.AppendLine(");");
		}

		public override MV_Base GetDefaultInput(int inputIndex)
		{
			return new MV_Constant(inputIndex == 0 ? defaultValue1 : defaultValue2,
								   true);
		}
		public override string GetInputName(int index) { return (index == 0 ? inputName1 : inputName2); }

		
		//Note that this class's fields don't need to be serialized
		//    because all that extra data is inferred from the type name when deserializing.
	}
}