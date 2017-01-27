using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RT.Serialization;

namespace RT.MaterialValue
{
	[Serializable]
	public class MV_PureNoise : MV_Base
	{
		[SerializeField]
		public byte NChannels;


		public override string TypeName { get { return TypeName_PureNoise; } }
		public override OutputSizes OutputSize
		{
			get
			{
				switch (NChannels)
				{
					case 1: return OutputSizes.One;
					case 2: return OutputSizes.Two;
					case 3: return OutputSizes.Three;
					case 4: return OutputSizes.Four;
					default: throw new NotImplementedException(NChannels.ToString());
				}
			}
		}

		public override string ShaderValueName
		{
			get
			{
				switch (OutputSize)
				{
					case OutputSizes.One: return base.ShaderValueName + ".r";
					case OutputSizes.Two: return base.ShaderValueName + ".rg";
					case OutputSizes.Three: return base.ShaderValueName + ".rgb";
					case OutputSizes.Four: return base.ShaderValueName;
					default: throw new NotImplementedException(OutputSize.ToString());
				}
			}
		}
		
		public override string PrettyName { get { return "Pure Noise"; } }
		public override Color GUIColor { get { return new Color(0.875f, 0.875f, 0.875f); } }


		public MV_PureNoise(byte nChannels) { NChannels = nChannels; }


		public override void Emit(System.Text.StringBuilder shaderlabProperties,
								  System.Text.StringBuilder cgDefinitions,
								  System.Text.StringBuilder cgFunctionBody)
		{
			//If the noise texture hasn't been declared yet, declare it.
			if (!cgDefinitions.ToString().Contains("\t\t\t\tsampler2D " + RTSystem.Param_PureNoiseTex))
			{
				shaderlabProperties.Append("\t\t\t");
				shaderlabProperties.Append(RTSystem.Param_PureNoiseTex);
				shaderlabProperties.AppendLine(" (\"Pure Noise\", 2D) = \"\" {}");

				cgDefinitions.Append("\t\t\t\tsampler2D ");
				cgDefinitions.Append(RTSystem.Param_PureNoiseTex);
				cgDefinitions.AppendLine(";");
			}

			//Combine various data to create the UV's to sample the noise texture with.
			cgFunctionBody.Append(OutputSize.ToHLSLType());
			cgFunctionBody.Append(" ");
			cgFunctionBody.Append(ShaderValueName);
			cgFunctionBody.Append(" = tex2D(");
			cgFunctionBody.Append(RTSystem.Param_PureNoiseTex);
			cgFunctionBody.Append(", ");
			cgFunctionBody.Append(RTSystem.Input_WorldPos);
			cgFunctionBody.Append(".xy + (5234.234 *");
			cgFunctionBody.Append(RTSystem.Input_ScreenPos);
			cgFunctionBody.Append(".xy) + (8282.2411 *");
			cgFunctionBody.Append(RTSystem.Input_UV);
			cgFunctionBody.Append("));");
		}
		public override void SetParams(Transform tr, Material unityMat)
		{
			unityMat.SetTexture(RTSystem.Param_PureNoiseTex, RTSystem.Instance.PureNoiseTex);
		}

		public override void WriteData(DataWriter writer, string namePrefix,
									   Dictionary<MV_Base, uint> idLookup)
		{
			base.WriteData(writer, namePrefix, idLookup);
			writer.Byte(NChannels, namePrefix + "Dimensions");
		}
		public override void ReadData(DataReader reader, string namePrefix,
									  Dictionary<MV_Base, List<uint>> childIDsLookup)
		{
			base.ReadData(reader, namePrefix, childIDsLookup);
			NChannels = reader.Byte(namePrefix + "Dimensions");
		}

		protected override GUIResults DoCustomGUI()
		{
			GUIResults result = GUIResults.Nothing;

			int newVal = EditorGUILayout.IntField("N Dimensions", NChannels);
			newVal = Math.Max(1, Math.Min(4, newVal));

			if (newVal != NChannels)
			{
				result = GUIResults.Other;
				NChannels = (byte)newVal;
			}

			return result;
		}
	}
}