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
	public abstract class MV_Base : Serialization.ISerializableRT
	{
		private static Dictionary<uint, MV_Base> guidToValue = new Dictionary<uint, MV_Base>();

		public static MV_Base GetValue(uint guid)
			{ return (guidToValue.ContainsKey(guid) ? guidToValue[guid] : null); }


		public static void Serialize(MV_Base val, string name, Serialization.DataWriter writer)
		{
			writer.String(val.TypeName, name + "Type");
			writer.Structure(val, name + "Value");
		}
		public static MV_Base Deserialize(string name,Serialization.DataReader reader)
		{
			string typeName = reader.String(name + "Type");
			MV_Base mv = null;

			switch (typeName)
			{
				case TypeName_Constant: mv = new MV_Constant(false, null, new Vectorf()); break;
				case TypeName_Tex2D: mv = new MV_Tex2D(); break;
				case TypeName_Add: mv = new MV_Add(null, null); break;
				case TypeName_Subtract: mv = new MV_Subtract(null, null); break;
				case TypeName_Divide: mv = new MV_Divide(null, null); break;
				case TypeName_Normalize: mv = new MV_Normalize(null); break;
				case TypeName_Length: mv = new MV_Length(null); break;
				case TypeName_Distance: mv = new MV_Distance(null, null); break;
				case TypeName_Sqrt: mv = new MV_Sqrt(null); break;
				case TypeName_Sin: mv = new MV_Sin(null); break;
				case TypeName_Cos: mv = new MV_Cos(null); break;
				case TypeName_Tan: mv = new MV_Tan(null); break;
				case TypeName_Asin: mv = new MV_Asin(null); break;
				case TypeName_Acos: mv = new MV_Acos(null); break;
				case TypeName_Atan: mv = new MV_Atan(null); break;
				case TypeName_Atan2: mv = new MV_Atan2(null, null); break;
				case TypeName_Step: mv = new MV_Step(null, null); break;
				case TypeName_Lerp: mv = new MV_Lerp(null, null, null); break;
				case TypeName_Smoothstep: mv = new MV_Smoothstep(null); break;
				case TypeName_Smootherstep: mv = new MV_Smootherstep(null); break;
				case TypeName_Clamp: mv = new MV_Clamp(null, null, null); break;
				case TypeName_Floor: mv = new MV_Floor(null); break;
				case TypeName_Ceil: mv = new MV_Ceil(null); break;
				case TypeName_Abs: mv = new MV_Abs(null); break;
				case TypeName_Min: mv = new MV_Min(null, null); break;
				case TypeName_Max: mv = new MV_Max(null, null); break;
				case TypeName_SurfUV: mv = new MV_SurfUV(); break;
				case TypeName_SurfPos: mv = new MV_SurfPos(); break;
				case TypeName_SurfNormal: mv = new MV_SurfNormal(); break;
				case TypeName_SurfTangent: mv = new MV_SurfTangent(); break;
				case TypeName_SurfBitangent: mv = new MV_SurfBitangent(); break;
				case TypeName_RayStartPos: mv = new MV_RayStartPos(); break;
				case TypeName_RayDir: mv = new MV_RayDir(); break;
				case TypeName_RayPos: mv = new MV_RayPos(null); break;
				case TypeName_ShapePos: mv = new MV_ShapePos(); break;
				case TypeName_ShapeScale: mv = new MV_ShapeScale(); break;
				case TypeName_ShapeRot: mv = new MV_ShapeRot(); break;
				case TypeName_PureNoise: mv = new MV_PureNoise(1); break;

				default:
					Debug.LogError("Unexpected MaterialValue type name \"" +
								   typeName + "\"");
					return null;
			}

			reader.Structure(mv, name + "Value");
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
							   TypeName_ShapeRot = "ShapeRot",
							   TypeName_PureNoise = "PureNoise";
		#endregion

		public static readonly uint INVALID_GUID = 0;

		
		[SerializeField]
		private uint guid;

		[Serializable]
		private Rect pos;

		[SerializeField]
		private List<MV_Base> inputs = new List<MV_Base>();

		
		public uint GUID { get { return guid; }
						   set { guidToValue.Remove(guid); guid = value; guidToValue.Add(guid, this); } }

		public IEnumerable<MV_Base> Inputs { get { return inputs; } }
		
		public virtual bool HasVariableNumberOfChildren { get { return false; } }

		public abstract string TypeName { get; }
		public abstract OutputSizes OutputSize { get; }

		public virtual string ShaderValueName { get { return "out" + GUID; } }

		public abstract string PrettyName { get; }
		public virtual Color GUIColor { get { return Color.white; } }

		public MV_Base() : this(guidToValue.Max((kvp) => kvp.Key) + 1) { }
		public MV_Base(uint _guid)
		{
			guid = _guid;
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


		public void Delete() { guidToValue.Remove(guid); }

		public int GetNInputs() { return inputs.Count; }
		public MV_Base GetInput(int i) { return inputs[i]; }
		public void ChangeInput(int i, MV_Base newChild) { inputs[i] = newChild; }

		protected void AddInput(MV_Base v) { inputs.Add(v); }
		protected void RemoveInput(MV_Base v) { inputs.Remove(v); }
		protected void InsertInput(MV_Base v, int i) { inputs.Insert(i, v); }
		protected void ClearInput() { inputs.Clear(); }
		

		public abstract void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder fragmentShaderBody);
		public virtual void SetParams(Material unityMat) { }

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


		public virtual void WriteData(Serialization.DataWriter writer)
		{
			writer.UInt(guid, "GUID");
			writer.Rect(pos, "Pos");

			writer.Int(inputs.Count, "NChildren");
			for (int i = 0; i < inputs.Count; ++i)
				Serialize(inputs[i], GetInputName(i), writer);
		}
		public virtual void ReadData(Serialization.DataReader reader)
		{
			GUID = reader.UInt("GUID");
			pos = reader.Rect("Pos");
			
			int nChildren = reader.Int("NChildren");
			inputs.Clear();
			inputs.Capacity = nChildren;
			for (int i = 0; i < nChildren; ++i)
				inputs.Add(Deserialize(GetInputName(i), reader));
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
				string buttStr = "X";
				if (outputTo != null && outputTo_Index == i)
					buttStr = "x";
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
				if (false && inputs[i] is MV_Constant)
				{
					MV_Constant inp = (MV_Constant)inputs[i];
					if (inp.ValueEditor.DoGUI())
						result = GUIResults.Other;
				}
				//Otherwise, draw a line to it and expose a button to release the connection.
				else
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

				GUILayout.EndHorizontal();
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