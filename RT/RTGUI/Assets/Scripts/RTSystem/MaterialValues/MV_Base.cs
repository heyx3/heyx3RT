﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

using Assert = UnityEngine.Assertions.Assert;


namespace RT.MaterialValue
{
	public abstract class MV_Base
	{
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
				case TypeName_Tex2D: mv = new MV_Tex2D(null); break;
				case TypeName_Add: mv = MV_Arithmetic.Add(null, null); break;
				case TypeName_Subtract: mv = MV_Arithmetic.Subtract(null, null); break;
				case TypeName_Multiply: mv = MV_Arithmetic.Multiply(null, null); break;
				case TypeName_Divide: mv = MV_Arithmetic.Divide(null, null); break;
				case TypeName_Normalize: mv = MV_Simple1.Normalize(null); break;
				case TypeName_Length: mv = MV_Simple1.Length(null); break;
				case TypeName_Distance: mv = MV_Simple2.Distance(null, null); break;
				case TypeName_Dot: mv = MV_Simple2.Dot(null, null); break;
				case TypeName_Cross: mv = MV_Simple2.Cross(null, null); break;
				case TypeName_Reflect: mv = MV_Simple2.Reflect(null, null); break;
				case TypeName_Refract: mv = MV_Simple3.Refract(null, null, null); break;
				case TypeName_Sqrt: mv = MV_Simple1.Sqrt(null); break;
				case TypeName_Ln: mv = MV_Simple1.Ln(null); break;
				case TypeName_Pow: mv = MV_Simple2.Pow(null, null); break;
				case TypeName_Sin: mv = MV_Simple1.Sin(null); break;
				case TypeName_Cos: mv = MV_Simple1.Cos(null); break;
				case TypeName_Tan: mv = MV_Simple1.Tan(null); break;
				case TypeName_Asin: mv = MV_Simple1.Asin(null); break;
				case TypeName_Acos: mv = MV_Simple1.Acos(null); break;
				case TypeName_Atan: mv = MV_Simple1.Atan(null); break;
				case TypeName_Atan2: mv = MV_Simple2.Atan2(null, null); break;
				case TypeName_Step: mv = MV_Simple2.Step(null, null); break;
				case TypeName_Lerp: mv = MV_Simple3.Lerp(null, null, null); break;
				case TypeName_Map: mv = new MV_Map(null, null, null, null, null); break;
				case TypeName_Smoothstep: mv = MV_Simple1.Smoothstep(null); break;
				case TypeName_Smootherstep: mv = MV_Simple1.Smootherstep(null); break;
				case TypeName_Clamp: mv = MV_Simple3.Clamp(null, null, null); break;
				case TypeName_Floor: mv = MV_Simple1.Floor(null); break;
				case TypeName_Ceil: mv = MV_Simple1.Ceil(null); break;
				case TypeName_Abs: mv = MV_Simple1.Abs(null); break;
				case TypeName_Min: mv = new MV_MinMax(null, null, true); break;
				case TypeName_Max: mv = new MV_MinMax(null, null, false); break;
				case TypeName_Average: mv = new MV_Average(null, null); break;
				case TypeName_Append: mv = new MV_Append(null, null); break;
				case TypeName_Swizzle: mv = new MV_Swizzle(null, MV_Swizzle.Components.X); break;
				case TypeName_PureNoise: mv = new MV_PureNoise(1); break;
				case TypeName_PerlinNoise: mv = new MV_Perlin(null); break;
				case TypeName_WorleyNoise: mv = new MV_Worley(null, null); break;
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


		/// <summary>
		/// Returns a sorted version of the given collection
		///     so that every node comes before any of its parents.
		/// </summary>
		public static IEnumerable<MV_Base> SortDeepestFirst(IEnumerable<MV_Base> nodes)
		{
			List<MV_Base> toCheck = new List<MV_Base>(nodes);
			HashSet<MV_Base> alreadyChecked = new HashSet<MV_Base>();
			while (toCheck.Count > 0)
			{
				MV_Base node = toCheck[toCheck.Count - 1];

				//Make sure we return its children first.
				if (alreadyChecked.Contains(node))
				{
					yield return node;
					toCheck.RemoveAt(toCheck.Count - 1);
				}
				else
				{
					alreadyChecked.Add(node);
					
					for (int i = 0; i < node.inputs.Count; ++i)
					{
						//If this node is still queued up to be checked, move it to the front.
						if (toCheck.Contains(node.inputs[i]))
						{
							toCheck.Remove(node.inputs[i]);
							toCheck.Add(node.inputs[i]);
						}
						//Otherwise, if it hasn't been checked yet, queue it up to be checked.
						else if (!alreadyChecked.Contains(node.inputs[i]))
						{
							toCheck.Add(node.inputs[i]);
						}
					}
				}
			}
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
							   TypeName_Cross = "Cross",
							   TypeName_Reflect = "Reflect",
							   TypeName_Refract = "Refract",
							   TypeName_Sqrt = "Sqrt",
						       TypeName_Ln = "Ln",
							   TypeName_Pow = "Pow",
							   TypeName_Sin = "Sin",
							   TypeName_Cos = "Cos",
							   TypeName_Tan = "Tan",
							   TypeName_Asin = "Asin",
							   TypeName_Acos = "Acos",
							   TypeName_Atan = "Atan",
							   TypeName_Atan2 = "Atan2",
							   TypeName_Step = "Step",
							   TypeName_Lerp = "Lerp",
							   TypeName_Map = "Map",
							   TypeName_Smoothstep = "Smoothstep",
							   TypeName_Smootherstep = "Smootherstep",
							   TypeName_Clamp = "Clamp",
							   TypeName_Floor = "Floor",
							   TypeName_Ceil = "Ceil",
							   TypeName_Abs = "Abs",
							   TypeName_Min = "Min",
							   TypeName_Max = "Max",
							   TypeName_Average = "Average",
							   TypeName_Append = "Append",
							   TypeName_Swizzle = "Swizzle",
							   TypeName_PureNoise = "PureNoise",
							   TypeName_PerlinNoise = "PerlinNoise",
							   TypeName_WorleyNoise = "WorleyNoise",
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
        
		private Rect pos;
        
		private List<MV_Base> inputs = new List<MV_Base>();

		
		public Rect Pos { get { return pos; } set { pos = value; } }

		public IEnumerable<MV_Base> Inputs { get { return inputs; } }

		public IEnumerable<MV_Base> HierarchyRootFirst { get { return GetHierarchyRootFirst(new HashSet<MV_Base>()); } }
		private IEnumerable<MV_Base> GetHierarchyRootFirst(HashSet<MV_Base> usedSoFar)
		{
			yield return this;

			foreach (MV_Base input in inputs)
				if (!usedSoFar.Contains(input))
				{
					usedSoFar.Add(input);
					foreach (MV_Base input2 in input.GetHierarchyRootFirst(usedSoFar))
						yield return input2;
				}
		}

		public IEnumerable<MV_Base> HierarchyDeepestFirst { get { return GetDeepestFirstHierarchy(new HashSet<MV_Base>()); } }
		private IEnumerable<MV_Base> GetDeepestFirstHierarchy(HashSet<MV_Base> usedSoFar)
		{
			foreach (MV_Base input in inputs)
				if (!usedSoFar.Contains(input))
				{
					foreach (MV_Base childHierarchy in input.GetDeepestFirstHierarchy(usedSoFar))
						yield return childHierarchy;
				}

			yield return this;
			usedSoFar.Add(this);
		}
		

		public virtual bool HasVariableNumberOfChildren { get { return false; } }

		public abstract string TypeName { get; }
		public abstract OutputSizes OutputSize { get; }

		public virtual string ShaderValueName(Dictionary<MV_Base, uint> idLookup)
		{
			return "out" + idLookup[this];
		}
		public virtual bool IsUsableInSkyMaterial { get { return true; } }

		public abstract string PrettyName { get; }
		public virtual Color GUIColor { get { return Color.white; } }


		public MV_Base()
		{
			pos = new Rect(0.0f, 0.0f, 0.1f, 0.1f);
		}
		

		/// <summary>
		/// Gets the shader expression for this MaterialValue's value,
		/// scaled up/down to have the given number of dimensions.
		/// </summary>
		/// <param name="alsoAllow1D">
		/// If true, a size of One is acceptable along with the target size.
		/// </param>
		public string GetShaderValue(OutputSizes targetSize, Dictionary<MV_Base, uint> idLookup,
									 bool alsoAllow1D = false)
		{
			string val = ShaderValueName(idLookup);
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
        
		public void AddInput(MV_Base v) { inputs.Add(v); }
		public void RemoveInput(MV_Base v) { inputs.Remove(v); }
		public void RemoveInput(int i) { inputs.RemoveAt(i); }
		public void InsertInput(MV_Base v, int i) { inputs.Insert(i, v); }
		public void ClearInput() { inputs.Clear(); }
		

		public abstract void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup);
		public virtual void SetParams(Transform shapeTr, Material unityMat,
									  Dictionary<MV_Base, uint> idLookup) { }

		/// <summary>
		/// Gets a valid default input for the given input index.
		/// Default behavior: returns an inline MV_Constant input of 0.0.
		/// </summary>
		public virtual MV_Base GetDefaultInput(int inputIndex) { return MV_Constant.MakeFloat(0.0f, false, 0.0f, 1.0f, OutputSizes.One, true); }
		/// <summary>
		/// Returns the name for the given input.
		/// Note that this is the name used to serialize the inputs as well.
		/// Default behavior: Throws ArgumentOutOfRangeException().
		/// </summary>
		public virtual string GetInputName(int index) { throw new ArgumentOutOfRangeException("This node has no inputs!"); }


		public virtual void WriteData(Serialization.DataWriter writer, string namePrefix,
									  Dictionary<MV_Base, uint> idLookup)
		{
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
		public virtual GUIResults DoCustomGUI() { return GUIResults.Nothing; }
	}
}