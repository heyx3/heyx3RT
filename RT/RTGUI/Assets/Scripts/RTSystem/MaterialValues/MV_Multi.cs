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
	public abstract class MV_MultiType : MV_Base
	{
		public override bool HasVariableNumberOfChildren { get { return true; } }
		public override OutputSizes OutputSize { get { return Inputs.Aggregate(OutputSizes.One, (o, i) => (i == null ? o : o.Max(i.OutputSize))); } }
		public override Color GUIColor { get { return new Color(0.95f, 0.95f, 0.95f); } }

		public override string GetInputName(int index) { return (index + 1).ToString(); }
	}

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

		private string typeName, prettyName, symbol;
		private float identity;
		private bool canAlwaysUse1DInputs;

		public override string TypeName { get { return typeName; } }
		public override string PrettyName { get { return prettyName; } }

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
													   float.NegativeInfinity, float.PositiveInfinity),
								   true);
		}
		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			OutputSizes outSize = OutputSize;

			cgFunctionBody.Append(outSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = ");
			for (int i = 0; i < GetNInputs(); ++i)
			{
				if (i > 0)
				{
					cgFunctionBody.Append(" ");
					cgFunctionBody.Append(symbol);
					cgFunctionBody.Append(" ");
				}
				cgFunctionBody.Append(GetInput(i).GetShaderValue(outSize, idLookup,
																 canAlwaysUse1DInputs));
			}
			cgFunctionBody.AppendLine(";");
		}
	}


	public class MV_MinMax : MV_MultiType
	{
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
													   float.NegativeInfinity, float.PositiveInfinity),
								   true);
		}
		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			OutputSizes outSize = OutputSize;

			cgFunctionBody.Append(outSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = ");

			if (GetNInputs() == 0)
				cgFunctionBody.Append(0.0f);
			else if (GetNInputs() == 1)
				cgFunctionBody.Append(GetInput(0).ShaderValueName(idLookup));
			else
			{
				for (int i = 0; i < GetNInputs(); ++i)
				{
					if (i > 0)
						cgFunctionBody.Append(", ");
					if (i < GetNInputs() - 1)
					{
						cgFunctionBody.Append(isMin ? "min" : "max");
						cgFunctionBody.Append("(");
					}

					cgFunctionBody.Append(GetInput(i).GetShaderValue(outSize, idLookup, false));
				}
				for (int i = 0; i < GetNInputs() - 1; ++i)
					cgFunctionBody.Append(")");
			}

			cgFunctionBody.AppendLine(";");
		}
	}


	public class MV_Average : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Average; } }
		public override string PrettyName { get { return "Average"; } }

		public MV_Average(MV_Base a, MV_Base b)
		{
			AddInput(a);
			AddInput(b);
		}

		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			OutputSizes outSize = OutputSize;

			cgFunctionBody.Append(outSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = ");

			if (GetNInputs() == 0)
				cgFunctionBody.Append(0.0f);
			else if (GetNInputs() == 1)
				cgFunctionBody.Append(GetInput(0).ShaderValueName(idLookup));
			else
			{
				cgFunctionBody.Append("(1.0 / ");
				cgFunctionBody.Append(GetNInputs());
				cgFunctionBody.Append(".0) * (");
				for (int i = 0; i < GetNInputs(); ++i)
				{
					if (i > 0)
						cgFunctionBody.Append(" + ");
					cgFunctionBody.Append(GetInput(i).GetShaderValue(outSize, idLookup, false));
				}
				cgFunctionBody.Append(")");
			}

			cgFunctionBody.AppendLine(";");
		}
	}


	public class MV_Append : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Append; } }
		public override string PrettyName { get { return "Append"; } }
		public override OutputSizes OutputSize { get { return ((uint)Inputs.Sum(mv => (uint)mv.OutputSize)).ToOutputSize(); } }

		public MV_Append(MV_Base a, MV_Base b)
		{
			AddInput(a);
			AddInput(b);
		}

		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			OutputSizes outSize = OutputSize;

			cgFunctionBody.Append(outSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = ");

			if (GetNInputs() == 0)
				cgFunctionBody.Append(0.0f);
			else
			{
				cgFunctionBody.Append(outSize.ToHLSLType());
				cgFunctionBody.Append("(");
				for (int i = 0; i < GetNInputs(); ++i)
				{
					if (i > 0)
						cgFunctionBody.Append(", ");
					cgFunctionBody.Append(GetInput(i).ShaderValueName(idLookup));
				}
				cgFunctionBody.Append(")");
			}

			cgFunctionBody.AppendLine(";");
		}
	}
}
