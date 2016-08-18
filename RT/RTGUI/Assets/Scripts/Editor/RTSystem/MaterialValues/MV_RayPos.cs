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
								  StringBuilder fragmentShaderBody)
		{
			MV_Inputs.RayStart.Emit(shaderlabProperties, cgDefinitions, fragmentShaderBody);
			MV_Inputs.RayDir.Emit(shaderlabProperties, cgDefinitions, fragmentShaderBody);

			fragmentShaderBody.Append("float3 ");
			fragmentShaderBody.Append(ShaderValueName);
			fragmentShaderBody.Append(" = ");
			fragmentShaderBody.Append(MV_Inputs.RayStart.ShaderValueName);
			fragmentShaderBody.Append(" + (");
			fragmentShaderBody.Append(MV_Inputs.RayStart.ShaderValueName);
			fragmentShaderBody.Append(" * ");
			fragmentShaderBody.Append(GetInputValue(0, OutputSizes.Three, true));
			fragmentShaderBody.AppendLine(");");
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