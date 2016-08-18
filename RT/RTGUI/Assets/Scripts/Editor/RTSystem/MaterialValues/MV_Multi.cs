using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace RT.MaterialValue
{
	/// <summary>
	/// A MaterialValue that has a variable number of children.
	/// </summary>
	[Serializable]
	public abstract class MV_MultiType : MV_Base
	{
		public override bool HasVariableNumberOfChildren { get { return true; } }
		public override OutputSizes OutputSize { get { return Inputs.Aggregate(OutputSizes.One, (o, i) => o.Max(i.OutputSize)); } }
		public override Color GUIColor { get { return new Color(0.95f, 0.95f, 0.95f); } }
		
		public override string GetInputName(int index) { return (index + 1).ToString(); }
	}

	[Serializable]
	public class MV_Arithmetic : MV_MultiType
	{
		public static MV_Arithmetic Add(MV_Base a, MV_Base b)
		{
			return new MV_Arithmetic(a, b, TypeName_Add, "Add", "+", 0.0f, false);
		}
		public static MV_Arithmetic Subtract(MV_Base a, MV_Base b)
		{
			return new MV_Arithmetic(a, b, TypeName_Subtract, "Subtract", "-", 0.0f, false);
		}
		public static MV_Arithmetic Multiply(MV_Base a, MV_Base b)
		{
			return new MV_Arithmetic(a, b, TypeName_Multiply, "Multiply", "*", 1.0f, true);
		}
		public static MV_Arithmetic Divide(MV_Base a, MV_Base b)
		{
			return new MV_Arithmetic(a, b, TypeName_Divide, "Divide", "/", 1.0f, true);
		}


		[SerializeField]
		private string typeName, prettyName, symbol;
		[SerializeField]
		private float identity;
		[SerializeField]
		private bool canAlwaysUse1DInputs;


		public override string TypeName { get { return typeName; } }
		public override string PrettyName {  get { return prettyName; } }


		private MV_Arithmetic(MV_Base a, MV_Base b,
							  string _typeName, string _prettyName, string _symbol,
							  float _identity, bool _canAlwaysUse1DInputs)
		{
			AddInput(a);
			AddInput(b);

			typeName = _typeName;
			prettyName = _prettyName;
			symbol = _symbol;
			identity = _identity;
			canAlwaysUse1DInputs = _canAlwaysUse1DInputs;
		}


		public override MV_Base GetDefaultInput(int inputIndex)
		{
			return new MV_Constant(new EditableVectorf(new Vectorf(identity, OutputSize),
													   false, OutputSizes.All,
													   float.NegativeInfinity, float.PositiveInfinity));
		}
		public override void Emit(StringBuilder shaderlabProperties, StringBuilder cgDefinitions, StringBuilder fragmentShaderBody)
		{
			OutputSizes outSize = OutputSize;

			fragmentShaderBody.Append(outSize.ToHLSLType());
			fragmentShaderBody.Append(" ");
			fragmentShaderBody.Append(ShaderValueName);
			fragmentShaderBody.Append(" = ");
			for (int i = 0; i < GetNInputs(); ++i)
			{
				if (i > 0)
				{
					fragmentShaderBody.Append(" ");
					fragmentShaderBody.Append(symbol);
					fragmentShaderBody.Append(" ");
				}
				fragmentShaderBody.Append(GetInputValue(i, outSize, canAlwaysUse1DInputs));
			}
			fragmentShaderBody.AppendLine(";");
		}
	}


	[Serializable]
	public class MV_MinMax : MV_MultiType
	{
		[SerializeField]
		private bool isMin;


		public override string TypeName { get { return (isMin ? TypeName_Min : TypeName_Max); } }
		public override string PrettyName { get { return (isMin ? "Min" : "Max"); } }


		public MV_MinMax(MV_Base a, MV_Base b, bool _isMin)
		{
			AddInput(a);
			AddInput(b);
			isMin = _isMin;
		}


		public override MV_Base GetDefaultInput(int inputIndex)
		{
			return new MV_Constant(new EditableVectorf(new Vectorf(isMin ? 0.0f : 1.0f),
													   false, OutputSizes.All,
													   float.NegativeInfinity, float.PositiveInfinity));
		}
		public override void Emit(StringBuilder shaderlabProperties, StringBuilder cgDefinitions, StringBuilder fragmentShaderBody)
		{
			OutputSizes outSize = OutputSize;

			fragmentShaderBody.Append(outSize.ToHLSLType());
			fragmentShaderBody.Append(" ");
			fragmentShaderBody.Append(ShaderValueName);
			fragmentShaderBody.Append(" = ");

			if (GetNInputs() == 0)
				fragmentShaderBody.Append(0.0f);
			else if (GetNInputs() == 1)
				fragmentShaderBody.Append(GetInput(0).ShaderValueName);
			else
			{
				for (int i = 0; i < GetNInputs(); ++i)
				{
					if (i > 0)
						fragmentShaderBody.Append(", ");
					if (i < GetNInputs() - 1)
					{
						fragmentShaderBody.Append(isMin ? "min" : "max");
						fragmentShaderBody.Append("(");
					}

					fragmentShaderBody.Append(GetInputValue(i, outSize, false));
				}
				for (int i = 0; i < GetNInputs() - 1; ++i)
					fragmentShaderBody.Append(")");
			}

			fragmentShaderBody.AppendLine(";");
		}
	}
}
