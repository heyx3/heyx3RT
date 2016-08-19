using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using RT.Serialization;

namespace RT.MaterialValue
{
	[Serializable]
	public class MV_RayPos : MV_Base
	{
		//Note that this class has a "secret" dependency on the RayStartPos and RayDir MaterialValues.


		public override string TypeName { get { return TypeName_RayPos; } }
		public override OutputSizes OutputSize { get { return OutputSizes.Three; } }

		public override string PrettyName { get { return "Ray pos at Time"; } }


		public MV_RayPos(MV_Base t) { AddInput(t); }


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody)
		{
			MV_Inputs.RayStart.Emit(shaderlabProperties, cgDefinitions, cgFunctionBody);
			MV_Inputs.RayDir.Emit(shaderlabProperties, cgDefinitions, cgFunctionBody);

			cgFunctionBody.Append("float3 ");
			cgFunctionBody.Append(ShaderValueName);
			cgFunctionBody.Append(" = ");
			cgFunctionBody.Append(MV_Inputs.RayStart.ShaderValueName);
			cgFunctionBody.Append(" + (");
			cgFunctionBody.Append(MV_Inputs.RayStart.ShaderValueName);
			cgFunctionBody.Append(" * ");
			cgFunctionBody.Append(GetInput(0).GetShaderValue(OutputSizes.Three, true));
			cgFunctionBody.AppendLine(");");
		}
		public override void SetParams(Transform shapeTr, Material unityMat)
		{
			MV_Inputs.RayStart.SetParams(shapeTr, unityMat);
			MV_Inputs.RayDir.SetParams(shapeTr, unityMat);
		}

		public override MV_Base GetDefaultInput(int inputIndex) { return MV_Constant.MakeFloat(0.0f); }
		public override string GetInputName(int index) { return "T"; }
	}
}