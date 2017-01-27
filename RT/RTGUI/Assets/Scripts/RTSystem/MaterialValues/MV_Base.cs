using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	[Serializable]
	public abstract class MV_Base
	{
		private static Dictionary<uint, MV_Base> guidToValue = new Dictionary<uint, MV_Base>();

		public static MV_Base GetValue(uint guid)
			{ return (guidToValue.ContainsKey(guid) ? guidToValue[guid] : null); }


		public static void Write(MV_Base val, Dictionary<MV_Base, uint> idLookup, string name,
								 Serialization.DataWriter writer)
		{
			writer.String(val.TypeName, name + "Type");
			writer.UInt(idLookup[val], name + "ID");
			val.WriteData(writer, name, idLookup);
		}
		public static MV_Base Read(Dictionary<MV_Base, List<uint>> childIDLookup, string name,
								   Serialization.DataReader reader, out uint id)
		{
			string typeName = reader.String(name + "Type");
			MV_Base mv = null;
			switch (typeName)
			{
				case TypeName_Constant: mv = MV_Constant.MakeFloat(0.0f); break;
				case TypeName_Tex2D: mv = new MV_Tex2D(); break;
				case TypeName_Add: mv = MV_Arithmetic.Add(null, null); break;
				case TypeName_Subtract: mv = MV_Arithmetic.Subtract(null, null); break;
				case TypeName_Divide: mv = MV_Arithmetic.Divide(null, null); break;
				case TypeName_Normalize: mv = MV_Simple1.Normalize(null); break;
				case TypeName_Length: mv = MV_Simple1.Length(null); break;
				case TypeName_Distance: mv = MV_Simple2.Distance(null, null); break;
				case TypeName_Dot: mv = MV_Simple2.Dot(null, null); break;
				case TypeName_Sqrt: mv = MV_Simple1.Sqrt(null); break;
				case TypeName_Sin: mv = MV_Simple1.Sin(null); break;
				case TypeName_Cos: mv = MV_Simple1.Cos(null); break;
				case TypeName_Tan: mv = MV_Simple1.Tan(null); break;
				case TypeName_Asin: mv = MV_Simple1.Asin(null); break;
				case TypeName_Acos: mv = MV_Simple1.Acos(null); break;
				case TypeName_Atan: mv = MV_Simple1.Atan(null); break;
				case TypeName_Atan2: mv = MV_Simple2.Atan2(null, null); break;
				case TypeName_Step: mv = MV_Simple2.Step(null, null); break;
				case TypeName_Lerp: mv = MV_Simple3.Lerp(null, null, null); break;
				case TypeName_Smoothstep: mv = MV_Simple1.Smoothstep(null); break;
				case TypeName_Smootherstep: mv = MV_Simple1.Smootherstep(null); break;
				case TypeName_Clamp: mv = MV_Simple3.Clamp(null, null, null); break;
				case TypeName_Floor: mv = MV_Simple1.Floor(null); break;
				case TypeName_Ceil: mv = MV_Simple1.Ceil(null); break;
				case TypeName_Abs: mv = MV_Simple1.Abs(null); break;
				case TypeName_Min: mv = new MV_MinMax(null, null, true); break;
				case TypeName_Max: mv = new MV_MinMax(null, null, false); break;
				case TypeName_Swizzle: mv = new MV_Swizzle(null, MV_Swizzle.Components.X); break;
				case TypeName_PureNoise: mv = new MV_PureNoise(1); break;
				case TypeName_SurfUV: mv = MV_Inputs.SurfaceUV; break;
				case TypeName_SurfPos: mv = MV_Inputs.SurfacePos; break;
				case TypeName_SurfNormal: mv = MV_Inputs.SurfaceNormal; break;
				case TypeName_SurfTangent: mv = MV_Inputs.SurfaceTangent; break;
				case TypeName_SurfBitangent: mv = MV_Inputs.SurfaceBitangent; break;
				case TypeName_RayStartPos: mv = MV_Inputs.RayStart; break;
				case TypeName_RayDir: mv = MV_Inputs.RayDir; break;
				case TypeName_RayPos: mv = new MV_RayPos(null); break;
				case TypeName_ShapePos: mv = MV_Inputs.ShapePos; break;
				case TypeName_ShapeScale: mv = MV_Inputs.ShapeScale; break;
				case TypeName_ShapeRot: mv = MV_Inputs.ShapeRot; break;

				default:
					Debug.LogError("Unexpected MaterialValue type name \"" +
								   typeName + "\"");
					id = uint.MaxValue;
					return null;
			}

			mv.ReadData(reader, name, childIDLookup);

			id = reader.UInt(name + "ID");
			return mv;
		}


		#region "TypeName"s
		protected const string TypeName_Constant = "Constant",
							   TypeName_Tex2D = "Tex2D",
							   TypeName_Add = "Add",
							   TypeName_Subtract = "Subtract",
							   TypeName_Multiply = "Multiply",
							   TypeName_Divide = "Divide",
							   TypeName_Normalize = "Normalize",
							   TypeName_Length = "Length",
							   TypeName_Distance = "Distance",
							   TypeName_Dot = "Dot",
							   TypeName_Sqrt = "Sqrt",
							   TypeName_Sin = "Sin",
							   TypeName_Cos = "Cos",
							   TypeName_Tan = "Tan",
							   TypeName_Asin = "Asin",
							   TypeName_Acos = "Acos",
							   TypeName_Atan = "Atan",
							   TypeName_Atan2 = "Atan2",
							   TypeName_Step = "Step",
							   TypeName_Lerp = "Lerp",
							   TypeName_Smoothstep = "Smoothstep",
							   TypeName_Smootherstep = "Smootherstep",
							   TypeName_Clamp = "Clamp",
							   TypeName_Floor = "Floor",
							   TypeName_Ceil = "Ceil",
							   TypeName_Abs = "Abs",
							   TypeName_Min = "Min",
							   TypeName_Max = "Max",
							   TypeName_Swizzle = "Swizzle",
							   TypeName_PureNoise = "PureNoise",
							   TypeName_SurfUV = "SurfUV",
							   TypeName_SurfPos = "SurfPos",
							   TypeName_SurfNormal = "SurfNormal",
							   TypeName_SurfTangent = "SurfTangent",
							   TypeName_SurfBitangent = "SurfBitangent",
							   TypeName_RayStartPos = "RayStartPos",
							   TypeName_RayPos = "RayPos",
							   TypeName_RayDir = "RayDir",
							   TypeName_ShapePos = "ShapePos",
							   TypeName_ShapeScale = "ShapeScale",
							   TypeName_ShapeRot = "ShapeRot";
		#endregion

		public static readonly uint INVALID_GUID = 0;

		
		[SerializeField]
		private uint guid;

		[SerializeField]
		private Rect pos;

		[SerializeField]
		private List<MV_Base> inputs = new List<MV_Base>();

		
		public uint GUID { get { return guid; }
						   set { guidToValue.Remove(guid); guid = value; guidToValue.Add(guid, this); } }

		public IEnumerable<MV_Base> Inputs { get { return inputs; } }
		public IEnumerable<MV_Base> Hierarchy
		{
			get
			{
				return GetHierarchy(new HashSet<MV_Base>());
			}
		}

		private IEnumerable<MV_Base> GetHierarchy(HashSet<MV_Base> usedSoFar)
		{
			yield return this;

			foreach (MV_Base input in inputs)
				if (!usedSoFar.Contains(input))
				{
					usedSoFar.Add(input);
					foreach (MV_Base input2 in input.GetHierarchy(usedSoFar))
						yield return input2;
				}
		}
		

		public virtual bool HasVariableNumberOfChildren { get { return false; } }

		public abstract string TypeName { get; }
		public abstract OutputSizes OutputSize { get; }

		public virtual string ShaderValueName { get { return "out" + GUID; } }
		public virtual bool IsUsableInSkyMaterial { get { return true; } }

		public abstract string PrettyName { get; }
		public virtual Color GUIColor { get { return Color.white; } }


		public MV_Base()
		{
			guid = (guidToValue.Count == 0 ?
						1 :
						(guidToValue.Max((kvp) => kvp.Key) + 1));
			guidToValue.Add(guid, this);

			//Double-check that this sub-class is serializable.
			if ((GetType().Attributes & System.Reflection.TypeAttributes.Serializable) !=
					System.Reflection.TypeAttributes.Serializable)
			{
				EditorUtility.DisplayDialog("Not marked serializable",
											"The class " + GetType().Name + " isn't marked [Serializable]",
											"OK");
			}
		}


		/// <summary>
		/// Cleans up the GUID allocated for this instance, plus the GUIDs of every input instance.
		/// </summary>
		public void Delete()
		{
			foreach (MV_Base b in Hierarchy)
				guidToValue.Remove(b.guid);
		}

		/// <summary>
		/// Gets the shader expression for this MaterialValue's value,
		/// scaled up/down to have the given number of dimensions.
		/// </summary>
		/// <param name="alsoAllow1D">
		/// If true, a size of One is acceptable along with the target size.
		/// </param>
		public string GetShaderValue(OutputSizes targetSize, bool alsoAllow1D = false)
		{
			string val = ShaderValueName;
			OutputSizes valSize = OutputSize;

			if (valSize == targetSize || (alsoAllow1D && valSize == OutputSizes.One))
				return val;
			
			switch (targetSize)
			{
				case OutputSizes.One:
					return "(" + val + ".x" + ")";

				case OutputSizes.Two:
					if (valSize == OutputSizes.One)
						return "float2(" + val + ", " + val + ")";
					else
						return "(" + val + ".xy" + ")";

				case OutputSizes.Three:
					if (valSize == OutputSizes.One)
						return "float3(" + val + ", " + val + ", " + val + ")";
					else if (valSize == OutputSizes.Two)
						return "float3(" + val + ".xy, 0.0)";
					else
						return "(" + val + ".xyz" + ")";

				case OutputSizes.Four:
					if (valSize == OutputSizes.One)
						return "float4(" + val + ", " + val + ", " + val + ", " + val + ")";
					else if (valSize == OutputSizes.Two)
						return "float4(" + val + ", " + val + ", 0.0, 1.0)";
					else
						return "float4(" + val + ".xyz, 1.0)";

				default:
					throw new NotImplementedException(valSize.ToString());
			}
		}

		public int GetNInputs() { return inputs.Count; }
		public MV_Base GetInput(int i) { return inputs[i]; }
		public void ChangeInput(int i, MV_Base newChild) { inputs[i] = newChild; }

		protected void AddInput(MV_Base v) { inputs.Add(v); }
		protected void RemoveInput(MV_Base v) { inputs.Remove(v); }
		protected void RemoveInput(int i) { inputs.RemoveAt(i); }
		protected void InsertInput(MV_Base v, int i) { inputs.Insert(i, v); }
		protected void ClearInput() { inputs.Clear(); }
		

		public abstract void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody);
		public virtual void SetParams(Transform shapeTr, Material unityMat) { }

		/// <summary>
		/// Gets a valid default input for the given input index.
		/// Default behavior: returns a constant float input of 0.0.
		/// </summary>
		public virtual MV_Base GetDefaultInput(int inputIndex) { return MV_Constant.MakeFloat(0.0f); }
		/// <summary>
		/// Returns the name for the given input.
		/// Note that this is the name used to serialize the inputs as well.
		/// Default behavior: Throws ArgumentOutOfRangeException().
		/// </summary>
		public virtual string GetInputName(int index) { throw new ArgumentOutOfRangeException("This node has no inputs!"); }


		public virtual void WriteData(Serialization.DataWriter writer, string namePrefix,
									  Dictionary<MV_Base, uint> idLookup)
		{
			writer.UInt(guid, namePrefix + "GUID");
			writer.Rect(pos, namePrefix + "Pos");

			//Write children nodes as a list of their IDs.
			List<uint> childIDs = new List<uint>(inputs.Count);
			for (int i = 0; i < inputs.Count; ++i)
				childIDs.Add(idLookup[inputs[i]]);
			writer.List(childIDs, namePrefix + "childrenIDs",
						(Serialization.DataWriter wr, uint val, string name) =>
						{
							wr.UInt(val, name);
						});
		}
		public virtual void ReadData(Serialization.DataReader reader, string namePrefix,
									 Dictionary<MV_Base, List<uint>> childIDsLookup)
		{
			GUID = reader.UInt(namePrefix + "GUID");
			pos = reader.Rect(namePrefix + "Pos");

			//Read children nodes as a list of their IDs.
			List<uint> childIDs = reader.List(namePrefix + "childrenIDs",
											  (Serialization.DataReader rd, ref uint outVal, string name) =>
											  {
												  outVal = rd.UInt(name);
											  });
			childIDsLookup.Add(this, childIDs);
		}
		public virtual void OnDoneReadingData(Dictionary<uint, MV_Base> mvLookup,
											  Dictionary<MV_Base, List<uint>> childIDsLookup)
		{
			//Add the children.
			var childIDs = childIDsLookup[this];
			inputs.Clear();
			for (int i = 0; i < childIDs.Count; ++i)
				inputs.Add(mvLookup[childIDs[i]]);
		}


		public enum GUIResults
		{
			/// <summary>
			/// Nothing happened.
			/// </summary>
			Nothing,
			/// <summary>
			/// The "Duplicate" button was clicked.
			/// </summary>
			Duplicate,
			/// <summary>
			/// The "Delete" button was clicked.
			/// </summary>
			Delete,
			/// <summary>
			/// This node was changed in some other way.
			/// </summary>
			Other,
		}
		/// <summary>
		/// Returns what happend to this node during this GUI frame.
		/// </summary>
		/// <param name="inputFrom">
		/// If a MaterialValue's output was previously selected,
		///		this is the MaterialValue in question.
		/// Otherwise, it should be "null".
		/// </param>
		/// <param name="outputTo">
		/// If a MaterialValue's input was previously selected,
		///     this is the MaterialValue in question.
		/// Otherwise, it should be "null".
		/// </param>
		/// <param name="outputTo_Index">
		/// If a MaterialValue's input was previously selected,
		///     this is the index of the input in question.
		/// </param>
		public GUIResults DoGUI(ref MV_Base inputFrom,
								ref MV_Base outputTo, ref int outputTo_Index)
		{
			GUIResults result = GUIResults.Nothing;

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			for (int i = 0; i < inputs.Count; ++i)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(GetInputName(i));

				//Button to select input.
				string buttStr = "O";
				if (outputTo != null && outputTo_Index == i)
					buttStr = "o";
				if (GUILayout.Button(buttStr))
				{
					if (inputFrom != null)
					{
						result = GUIResults.Other;
						ChangeInput(i, inputFrom);
						inputFrom = null;
					}
					else
					{
						outputTo = this;
						outputTo_Index = i;
					} 
				}

				//If this input is a constant, expose a little inline GUI to edit it more easily.
				//TODO: If we're going to do this, we should consider immediately deleting the node from the graph once a new node is connected.
				/*
				if (inputs[i] is MV_Constant)
				{
					MV_Constant inp = (MV_Constant)inputs[i];
					if (inp.ValueEditor.DoGUI())
						result = GUIResults.Other;
				}
				//Otherwise, draw a line to it and expose a button to release the connection.
				else
				*/
				{
					const float OutputHeight = 30.0f,
								TitleBarHeight = 30.0f,
								InputSpacing = 20.0f;
					Rect otherPos = inputs[i].pos;
					Vector2 endPos = new Vector2(otherPos.xMax, otherPos.yMin + OutputHeight) - pos.min;
					MyGUI.DrawLine(new Vector2(0.0f, TitleBarHeight + ((float)i * InputSpacing)),
								   endPos, 2.0f, Color.white);

					if (GUILayout.Button("Disconnect"))
					{
						result = GUIResults.Other;
						ChangeInput(i, GetDefaultInput(i));
					}
				}

				//A button to remove this input.
				if (HasVariableNumberOfChildren)
				{
					if (GUILayout.Button("X"))
					{
						result = GUIResults.Other;
						RemoveInput(i);
						i -= 1;
					}
				}

				GUILayout.EndHorizontal();
			}

			//A button to add a new input.
			if (HasVariableNumberOfChildren)
			{
				if (GUILayout.Button("Add input"))
					AddInput(GetDefaultInput(inputs.Count));
			}

			GUIResults subResult = DoCustomGUI();
			if (subResult != GUIResults.Nothing)
				result = subResult;
			
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();


			//"Duplicate" and "Delete" buttons.
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Duplicate"))
				result = GUIResults.Duplicate;
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Delete"))
				result = GUIResults.Delete;
			GUILayout.EndHorizontal();

			return result;
		}
		protected virtual GUIResults DoCustomGUI() { return GUIResults.Nothing; }
	}
}