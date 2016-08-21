using System;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace RT.MaterialValue
{
	/// <summary>
	/// A MaterialValue that takes three parameters.
	/// </summary>
	[Serializable]
	public class MV_Simple3 : MV_Base
	{
		public static MV_Simple3 Lerp(MV_Base a, MV_Base b, MV_Base t)
		{
			return new MV_Simple3("lerp($0, $1, $2)", "Lerp", TypeName_Lerp, "A", "B", "T", a, b, t,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All, float.NegativeInfinity, float.PositiveInfinity),
								  new EditableVectorf(new Vectorf(1.0f), false, OutputSizes.All, float.NegativeInfinity, float.PositiveInfinity),
								  new EditableVectorf(new Vectorf(0.5f), false, OutputSizes.All, float.NegativeInfinity, float.PositiveInfinity));
		}
		public static MV_Simple3 Clamp(MV_Base min, MV_Base max, MV_Base x)
		{
			return new MV_Simple3("clamp($2, $0, $1)", "Clamp", TypeName_Clamp, "Min", "Max", "X", min, max, x,
								  new EditableVectorf(new Vectorf(0.0f), false, OutputSizes.All, float.NegativeInfinity, float.PositiveInfinity),
								  new EditableVectorf(new Vectorf(1.0f), false, OutputSizes.All, float.NegativeInfinity, float.PositiveInfinity),
								  new EditableVectorf(new Vectorf(0.5f), false, OutputSizes.All, float.NegativeInfinity, float.PositiveInfinity));
		}

		
		[SerializeField]
		private string toDo, prettyName, typeName,
					   inputName1, inputName2, inputName3;
		[SerializeField]
		private EditableVectorf defaultValue1, defaultValue2, defaultValue3;


		public override string TypeName { get { return typeName; } }
		public override OutputSizes OutputSize { get { return GetInput(0).OutputSize.Max(GetInput(1).OutputSize.Max(GetInput(2).OutputSize)); } }

		public override string PrettyName { get { return prettyName; } }

		
		/// <summary>
		/// Creates a MaterialValue that does something with three parameters.
		/// </summary>
		/// <param name="_toDo">
		/// The expression to evaluate. Use '$0', '$1', and '$2' in place of the parameters.
		/// </param>
		/// <param name="x">The first parameter. Pass null to generate a default one.</param>
		/// <param name="y">The second parameter. Pass null to generate a default one.</param>
		/// <param name="z">The third parameter. Pass null to generate a default one.</param>
		private MV_Simple3(string _toDo, string _prettyName, string _typeName,
						   string _inputName1, string _inputName2, string _inputName3,
						   MV_Base x, MV_Base y, MV_Base z,
						   EditableVectorf _defaultValue1, EditableVectorf _defaultValue2, EditableVectorf _defaultValue3)
		{
			toDo = _toDo;
			prettyName = _prettyName;
			typeName = _typeName;
			inputName1 = _inputName1;
			inputName2 = _inputName2;
			inputName3 = _inputName3;
			defaultValue1 = _defaultValue1;
			defaultValue2 = _defaultValue2;
			defaultValue3 = _defaultValue3;

			if (x == null)
				AddInput(GetDefaultInput(0));
			else
				AddInput(x);

			if (y == null)
				AddInput(GetDefaultInput(1));
			else
				AddInput(y);

			if (z == null)
				AddInput(GetDefaultInput(2));
			else
				AddInput(z);
		}
		/// <summary>
		/// Creates a MaterialValue that does something with three parameters.
		/// </summary>
		/// <param name="_toDo">
		/// The expression to evaluate. Use '$0', '$1', and '$2' in place of the parameters.
		/// </param>
		/// <param name="x">The first parameter. Pass null to generate a default one.</param>
		/// <param name="y">The second parameter. Pass null to generate a default one.</param>
		/// <param name="z">The third parameter. Pass null to generate a default one.</param>
		private MV_Simple3(string _toDo, string _prettyName, string _typeName,
						   string _inputName1, string _inputName2, string _inputName3,
						   MV_Base x, MV_Base y, MV_Base z,
						   EditableVectorf _defaultValue)
			: this(_toDo, _prettyName, _typeName, _inputName1, _inputName2, _inputName3,
				   x, y, z, _defaultValue, _defaultValue, _defaultValue) { }


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody)
		{
			cgFunctionBody.Append(OutputSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName);
			cgFunctionBody.Append(" = (");
			cgFunctionBody.Append(toDo.Replace("$0", GetInput(0).GetShaderValue(OutputSize))
								      .Replace("$1", GetInput(1).GetShaderValue(OutputSize))
									  .Replace("$2", GetInput(2).GetShaderValue(OutputSize, true)));
			cgFunctionBody.AppendLine(");");
		}

		public override MV_Base GetDefaultInput(int inputIndex)
		{
			return new MV_Constant(inputIndex == 0 ? defaultValue1 : (inputIndex == 1 ? defaultValue2 : defaultValue3));
		}
		public override string GetInputName(int index) { return (index == 0 ? inputName1 : (index == 1 ? inputName2 : inputName3)); }

		
		//Note that this class's fields don't need to be serialized
		//    because all that extra data is inferred from the type name when deserializing.
	}
}