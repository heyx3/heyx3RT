using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Serialization;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	public class MV_Worley : MV_Base
	{
		public enum Ops { Add, Sub, Mul, Div, InvSub, InvDiv }
		public enum Params { One, Zero, D1, D2, InvD1, InvD2 }
		public enum DistFuncs { StraightLine, Manhattan }


		public override string TypeName { get { return TypeName_WorleyNoise; } }
		public override OutputSizes OutputSize {  get { return OutputSizes.One; } }

		public override string PrettyName { get { return "Worley Noise"; } }
		public override Color GUIColor { get { return new Color(0.55f, 0.55f, 0.9f); } }


		public Ops DistCombineOp = Ops.Add;
		public Params DistParam1 = Params.One,
					  DistParam2 = Params.Zero;
		public DistFuncs DistFunc = DistFuncs.StraightLine;


		public MV_Worley(MV_Base x, MV_Base variance) { AddInput(x); AddInput(variance); }

		
		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefs,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			//Define hashing functions if they haven't been already.
			//Just grab the ones from MV_Perlin.
			string cgDefsStr = cgDefs.ToString();
			if (!cgDefsStr.Contains("float hashValue1("))
				cgDefs.AppendLine(MV_Perlin.HashingFuncs);


			OutputSizes inputSize = GetInput(0).OutputSize.Max(GetInput(1).OutputSize);

			string sizeStr = inputSize.ToNumber().ToString();
			if (sizeStr == "1")
				sizeStr = "";

			//Define a custom Worley noise function.
			string worleyFuncName = "Worley" + idLookup[this].ToString();
			{
				//Get the distance function to use,
				//    assuming the variables are "p" and "worleyPos".
				StringBuilder distFunc = new StringBuilder();
				switch (DistFunc)
				{
					case DistFuncs.StraightLine:
						distFunc.Append("distance(p, worleyPos)");
						break;
					case DistFuncs.Manhattan:
						distFunc.Append("(");
						for (int i = 0; i < inputSize.ToNumber(); ++i)
						{
							if (i > 0)
								distFunc.Append(" + ");
							
							char component = ((uint)i + 1).ToOutputSize().ToComponent();
							distFunc.Append("abs(p.");
							distFunc.Append(component);
							distFunc.Append("- worleyPos.");
							distFunc.Append(component);
							distFunc.Append(")");
						}
						distFunc.Append(")");
						break;

					default: throw new NotImplementedException(DistFunc.ToString());
				}


				//Function signature.
				cgDefs.Append("float ");
				cgDefs.Append(worleyFuncName);
				cgDefs.Append("(float");
				cgDefs.Append(sizeStr);
				cgDefs.Append(" p, float");
				cgDefs.Append(sizeStr);
				cgDefs.AppendLine(" variance)");
				cgDefs.AppendLine("{");

				//Body.
				cgDefs.Append(@"
	#define N_CELLS (");
				int N_CELLS = 3;
				for (int i = 0; i < inputSize.ToNumber(); ++i)
				{
					if (i > 0)
					{
						cgDefs.Append(" * ");
						N_CELLS *= 3;
					}
					cgDefs.Append("3");
				}
				cgDefs.Append(@")
	float" + sizeStr + @" refPoses[3];
	refPoses[1] = floor(p);
	refPoses[0] = refPoses[1] - 1.0;
	refPoses[2] = refPoses[1] + 1.0;
	
	float distances[N_CELLS];
	float" + sizeStr + @" minWorleyPos = -variance + 0.5,
						  maxWorleyPos = variance + 0.5;
	float" + sizeStr + @" worleyPos, tempPos;

	");

				//Calculate all the distances.
				for (int i = 0; i < N_CELLS; ++i)
				{
					cgDefs.Append(
@"	worleyPos = float");
					cgDefs.Append(sizeStr);
					cgDefs.Append("(");
					switch (inputSize)
					{
						case OutputSizes.One:
							cgDefs.Append("refPoses[");
							cgDefs.Append(i);
							cgDefs.Append("]");
							break;

						case OutputSizes.Two: {
							int x = i % 3,
								y = i / 3;
							cgDefs.Append("refPoses[");
							cgDefs.Append(x);
							cgDefs.Append("].x, refPoses[");
							cgDefs.Append(y);
							cgDefs.Append("].y");
							} break;

						case OutputSizes.Three: {
							int x = i % 3,
								y = (i / 3) % 3,
								z = i / 9;
							cgDefs.Append("refPoses[");
							cgDefs.Append(x);
							cgDefs.Append("].x, refPoses[");
							cgDefs.Append(y);
							cgDefs.Append("].y, refPoses[");
							cgDefs.Append(z);
							cgDefs.Append("].z");
							} break;

						case OutputSizes.Four: {
							int x = i % 3,
								y = (i / 3) % 3,
								z = (i / 9) % 3,
								w = i / 27;
							cgDefs.Append("refPoses[");
							cgDefs.Append(x);
							cgDefs.Append("].x, refPoses[");
							cgDefs.Append(y);
							cgDefs.Append("].y, refPoses[");
							cgDefs.Append(z);
							cgDefs.Append("].z, refPoses[");
							cgDefs.Append(w);
							cgDefs.Append("].w");
							} break;

						default: throw new NotImplementedException(inputSize.ToString());
					}
					cgDefs.AppendLine(");");

					cgDefs.Append("	tempPos");
					if (inputSize == OutputSizes.One)
						cgDefs.AppendLine(" = hashValue1(worleyPos);");
					else
					{
						cgDefs.Append(".x = hashValue");
						cgDefs.Append(sizeStr);
						cgDefs.AppendLine("(worleyPos);");
						for (int j = 1; j < inputSize.ToNumber(); ++j)
						{
							cgDefs.Append("\ttempPos.");
							cgDefs.Append(((uint)(j + 1)).ToOutputSize().ToComponent());
							cgDefs.Append(" = hashValue1(tempPos.");
							cgDefs.Append(((uint)j).ToOutputSize().ToComponent());
							cgDefs.AppendLine(");");
						}
					}
					cgDefs.Append(
@"	worleyPos += lerp(minWorleyPos, maxWorleyPos, tempPos);
	distances[");
					cgDefs.Append(i);
					cgDefs.Append(
			  @"] = ");
					cgDefs.Append(distFunc);
					cgDefs.AppendLine(";\n");
				}

				//Get the 2 closest distances.
				cgDefs.Append(@"
	int closest1, closest2;
	int smallest, i;");
				for (int i = 0; i < 2; ++i)
				{
					cgDefs.Append(@"
	smallest = ");
					if (i == 0)
						cgDefs.Append("0;");
					else
						cgDefs.Append("closest1 == 0 ? 1 : 0;");

					cgDefs.Append(@"
	for (i = 1; i < N_CELLS; ++i)
		if (");
					if (i > 0)
						cgDefs.Append("i != closest1 && ");
					cgDefs.Append(
		@"distances[i] < distances[smallest])
			smallest = i;
	closest");
					cgDefs.Append(i + 1);
					cgDefs.AppendLine(" = smallest;");
				}

				//Combine the results.
				cgDefs.AppendLine(@"
	float d1 = distances[closest1];
	float d2 = distances[closest2];");
				switch (DistParam1)
				{
					case Params.One: break;
					case Params.Zero:
						cgDefs.AppendLine("\td1 = 0.0;");
						break;
					case Params.D1:
						cgDefs.AppendLine("\td1 *= distances[closest1];");
						break;
					case Params.D2:
						cgDefs.AppendLine("\td1 *= distances[closest2];");
						break;
					case Params.InvD1:
						cgDefs.AppendLine("\td1 /= distances[closest1];");
						break;
					case Params.InvD2:
						cgDefs.AppendLine("\td1 /= distances[closest2];");
						break;
					default: throw new NotImplementedException(DistParam1.ToString());
				}
				switch (DistParam2)
				{
					case Params.One: break;
					case Params.Zero:
						cgDefs.AppendLine("\td2 = 0.0;");
						break;
					case Params.D1:
						cgDefs.AppendLine("\td2 *= distances[closest1];");
						break;
					case Params.D2:
						cgDefs.AppendLine("\td2 *= distances[closest2];");
						break;
					case Params.InvD1:
						cgDefs.AppendLine("\td2 /= distances[closest1];");
						break;
					case Params.InvD2:
						cgDefs.AppendLine("\td2 /= distances[closest2];");
						break;
					default: throw new NotImplementedException(DistParam1.ToString());
				}

				switch (DistCombineOp)
				{
					case Ops.Add:
						cgDefs.AppendLine("\treturn d1 + d2;");
						break;
					case Ops.Sub:
						cgDefs.AppendLine("\treturn d1 - d2;");
						break;
					case Ops.InvSub:
						cgDefs.AppendLine("\treturn d2 - d1;");
						break;
					case Ops.Mul:
						cgDefs.AppendLine("\treturn d1 * d2;");
						break;
					case Ops.Div:
						cgDefs.AppendLine("\treturn d1 / d2;");
						break;
					case Ops.InvDiv:
						cgDefs.AppendLine("\treturn d2 / d1;");
						break;
					default: throw new NotImplementedException(DistCombineOp.ToString());
				}

				cgDefs.AppendLine("#undef N_CELLS\n}");
			}

			//Call out to the Worley Noise function.
			cgFunctionBody.Append("float ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = ");
			cgFunctionBody.Append(worleyFuncName);
			cgFunctionBody.Append("(");
			cgFunctionBody.Append(GetInput(0).ShaderValueName(idLookup));
			cgFunctionBody.Append(", ");
			cgFunctionBody.Append(GetInput(1).ShaderValueName(idLookup));
			cgFunctionBody.AppendLine(");");
		}

		public override string GetInputName(int index)
		{
			switch (index)
			{
				case 0: return "X";
				case 1: return "Variance";
				default: throw new NotImplementedException(index.ToString());
			}
		}
		public override MV_Base GetDefaultInput(int inputIndex)
		{
			switch (inputIndex)
			{
				case 0: return MV_Constant.MakeVec2(0.0f, 0.0f, false,
												    float.NegativeInfinity, float.PositiveInfinity,
													OutputSizes.All, true);
				case 1: return MV_Constant.MakeFloat(0.5f, false,
													 float.NegativeInfinity, float.PositiveInfinity,
													 OutputSizes.All, true);
				default: throw new NotImplementedException(inputIndex.ToString());
			}
		}

		public override GUIResults DoCustomGUI()
		{
			GUIResults result = base.DoCustomGUI();
			if (result != GUIResults.Nothing)
				return result;
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Dist Func:");
				DistFunc = (DistFuncs)EditorGUILayout.EnumPopup(DistFunc);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Combine Op:");
				DistCombineOp = (Ops)EditorGUILayout.EnumPopup(DistCombineOp);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Dist Param 1:");
				DistParam1 = (Params)EditorGUILayout.EnumPopup(DistParam1);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Dist Param 2:");
				DistParam2 = (Params)EditorGUILayout.EnumPopup(DistParam2);
			}
			GUILayout.EndHorizontal();

			return result;
		}

		public override void WriteData(DataWriter writer, string namePrefix,
									   Dictionary<MV_Base, uint> idLookup)
		{
			base.WriteData(writer, namePrefix, idLookup);

			writer.Byte((byte)DistFunc, namePrefix + "DistFunc");
			writer.Byte((byte)DistCombineOp, namePrefix + "DistCombine");
			writer.Byte((byte)DistParam1, namePrefix + "DistParam1");
			writer.Byte((byte)DistParam2, namePrefix + "DistParam2");
		}
		public override void ReadData(DataReader reader, string namePrefix,
									  Dictionary<MV_Base, List<uint>> childIDsLookup)
		{
			base.ReadData(reader, namePrefix, childIDsLookup);

			DistFunc = (DistFuncs)reader.Byte(namePrefix + "DistFunc");
			DistCombineOp = (Ops)reader.Byte(namePrefix + "DistCombine");
			DistParam1 = (Params)reader.Byte(namePrefix + "DistParam1");
			DistParam2 = (Params)reader.Byte(namePrefix + "DistParam2");
		}
	}
}
