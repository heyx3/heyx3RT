using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace RT
{
	public abstract class MaterialValue : RTSerializer.ISerializable
	{
		public static void Write(MaterialValue val, RTSerializer.Writer writer, string name)
		{
			writer.WriteString(val.TypeName, name + "Type");
			writer.WriteDataStructure(val, name + "Value");
		}
		public static MaterialValue Read(RTSerializer.Reader reader, string name)
		{
			string typeName = reader.ReadString(name + "Type");
			MaterialValue mv = null;

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

			reader.ReadDataStructure(mv, name + "Value");
			return mv;
		}


		#region TypeNames
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


		public abstract string TypeName { get; }
		public virtual bool HasVariableNumberOfChildren { get { return false; } }


		protected RTGui.Gui Gui { get { return RTGui.Gui.Instance; } }


		private List<MaterialValue> children = new List<MaterialValue>();
		private List<RTGui.MaterialValueGui> childGUIs = new List<RTGui.MaterialValueGui>();


		public void OnGUI()
		{
			DoGUI();

			GUIUtil.StartTab(Gui.TabSize);

			for (int i = 0; i < children.Count; ++i)
			{
				MaterialValue newMV = childGUIs[i].DoGUI(children[i], HasVariableNumberOfChildren);
				if (newMV == null)
				{
					RemoveChild(i);
					i -= 1;
				}
			}

			if (HasVariableNumberOfChildren)
			{
				if (GUILayout.Button("+", Gui.Style_Button))
				{
					AddChild(MakeDefaultChild());
				}
			}

			GUIUtil.EndTab();
		}

		protected virtual void DoGUI() { }
		protected virtual string GetInputName(int index) { return (index + 1).ToString(); }

		public virtual void SetMaterialParams(Material mat,
											  string texParamName = null,
											  string colorParamName = null)
		{
			mat.SetTexture(texParamName, RTSystem.Instance.DefaultTex);
			mat.SetColor(colorParamName, Color.white);
		}
		protected virtual MaterialValue MakeDefaultChild() { return new MV_Constant(true, new uint[] { 3 }, new Vector3()); }

		public virtual void WriteData(RTSerializer.Writer writer) { }
		public virtual void ReadData(RTSerializer.Reader reader) { }


		protected List<MaterialValue> GetChildrenCopy() { return children.ToList(); }
		protected int GetNChildren() { return children.Count; }
		protected MaterialValue GetChild(int i) { return children[i]; }

		protected void ClearChildren() { children.Clear(); childGUIs.Clear(); }

		protected void AddChild(MaterialValue mv)
		{
			children.Add(mv);
			childGUIs.Add(new RTGui.MaterialValueGui(GetInputName(children.Count - 1),
													 Gui.Style_Button, Gui.Style_Text));
		}
		protected void RemoveChild(MaterialValue mv) { RemoveChild(children.IndexOf(mv)); }
		protected void RemoveChild(int index) { children.RemoveAt(index); childGUIs.RemoveAt(index); }
	}


	/// <summary>
	/// A MaterialValue that just outputs a constant value.
	/// </summary>
	public class MV_Constant : MaterialValue
	{
		public static MV_Constant MakeFloat(float f) { return new MV_Constant(false, new uint[] { 1 }, f); }
		public static MV_Constant MakeVec3(float x, float y, float z) { return MakeVec3(new Vector3(x, y, z)); }
		public static MV_Constant MakeVec3(Vector3 v)
		{
			return new MV_Constant(false, new uint[] { 3 }, v);
		}

		public static MV_Constant MakeRGB(float r, float g, float b) { return MakeRGB(new Vector3(r, g, b)); }
		public static MV_Constant MakeRGB(Vector3 col)
		{
			return new MV_Constant(true, new uint[] { 1, 3 }, col);
		}


		public override string TypeName { get { return TypeName_Constant; } }


		public bool UseSliders;
		public uint[] AllowableDimensions;
		public Vectorf Value;


		public MV_Constant(bool useSliders, uint[] allowableDimensions, Vectorf startValue)
		{
			UseSliders = useSliders;
			AllowableDimensions = allowableDimensions;
			Value = startValue;
		}
		

		protected override void DoGUI()
		{
			base.DoGUI();

			Value.DoGUI(Gui.Style_Text, Gui.Style_TextBox, Gui.Style_SelectionGrid, AllowableDimensions);
		}

		public override void SetMaterialParams(Material mat,
											   string texParamName = null,
											   string colorParamName = null)
		{
			if (colorParamName != null)
			{
				mat.SetColor(colorParamName,
							 (Value.NValues == 3 ?
								Value.ToRGB() :
								Value.ToRGBA()));
			}
			if (texParamName != null)
				mat.SetTexture(texParamName, RTSystem.Instance.WhiteTex);
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteDataStructure(Value, "Value");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);

			Value = new Vectorf();
			reader.ReadDataStructure(Value, "Value");
			Value = reader.ReadVector3("Value");
		}
	}
}