using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RT.MaterialValue
{
	public class MV_Map : MV_Base
	{
		public override string TypeName { get { return TypeName_Map; } }
		public override string PrettyName { get { return "Map"; } }

		public override OutputSizes OutputSize { get { return Inputs.Aggregate(OutputSizes.One, (o, i) => (i == null ? o : o.Max(i.OutputSize))); } }


		public MV_Map(MV_Base x, MV_Base srcMin, MV_Base srcMax, MV_Base destMin, MV_Base destMax)
		{
			AddInput(x);
			AddInput(srcMin);
			AddInput(srcMax);
			AddInput(destMin);
			AddInput(destMax);
		}


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			OutputSizes size = OutputSize;

			string x = GetInput(0).ShaderValueName(idLookup),
				   srcMin = GetInput(1).ShaderValueName(idLookup),
				   srcMax = GetInput(2).ShaderValueName(idLookup),
				   destMin = GetInput(3).ShaderValueName(idLookup),
				   destMax = GetInput(4).ShaderValueName(idLookup);

			cgFunctionBody.Append(size.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = lerp(");
			cgFunctionBody.Append(destMin);
			cgFunctionBody.Append(", ");
			cgFunctionBody.Append(destMax);
			cgFunctionBody.Append(", (");
			cgFunctionBody.Append(x);
			cgFunctionBody.Append(" - ");
			cgFunctionBody.Append(srcMin);
			cgFunctionBody.Append(") / (");
			cgFunctionBody.Append(srcMax);
			cgFunctionBody.Append(" - ");
			cgFunctionBody.Append(srcMin);
			cgFunctionBody.AppendLine("));");
		}

		public override string GetInputName(int index)
		{
			switch (index)
			{
				case 0: return "X";
				case 1: return "Src Min";
				case 2: return "Src Max";
				case 3: return "Dest Min";
				case 4: return "Dest Max";
				default: throw new NotImplementedException(index.ToString());
			}
		}
	}
}
