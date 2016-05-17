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

		private const float TabIncrement = 10.0f;
		

		public abstract string TypeName { get; }
		public virtual bool HasVariableNumberOfChildren { get { return false; } }


		protected RTGui Gui { get { return RTGui.Instance; } }


		private List<MaterialValue> children = new List<MaterialValue>();
		private Dictionary<MaterialValue, bool> isExpanded = new Dictionary<MaterialValue,bool>();


		public void OnGUI(float tabLevel = 0.0f)
		{
			DoGUI(tabLevel);

			for (int i = 0; i < children.Count; ++i)
			{
				GUILayout.BeginHorizontal();

					GUILayout.Space(tabLevel);

					//Expand/collapse the child with a button.
					if (GUILayout.Button(isExpanded[children[i]] ? "^" : "V",
										 Gui.Style_MaterialValue_Button))
					{
						isExpanded[children[i]] = !isExpanded[children[i]];
					}

					//Display the child's name/type.
					GUILayout.Label(GetInputName(i) + ": " + children[i].TypeName,
									Gui.Style_MaterialValue_Text);

					//TODO: Option window for choosing a kind of MaterialValue. Use it here to change value type.

					//Delete the child with a button.
					if (HasVariableNumberOfChildren)
					{
						GUILayout.FlexibleSpace();

						if (GUILayout.Button("-", Gui.Style_MaterialValue_Button))
						{
							RemoveChild(i);
							i -= 1;
						}
					}

				GUILayout.EndHorizontal();

				//If the child is expanded, show its gui.
				if (isExpanded[children[i]])
					children[i].OnGUI(tabLevel + TabIncrement);
			}

			if (HasVariableNumberOfChildren)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(tabLevel);
				if (GUILayout.Button("+", Gui.Style_MaterialValue_Button))
				{
					AddChild(MakeDefaultChild());
				}
				GUILayout.EndHorizontal();
			}
		}

		protected virtual void DoGUI(float tabLevel) { }
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

		protected void ClearChildren() { children.Clear(); isExpanded.Clear(); }

		protected void AddChild(MaterialValue mv) { children.Add(mv); isExpanded.Add(mv, false); }
		protected void RemoveChild(MaterialValue mv) { RemoveChild(children.IndexOf(mv)); }
		protected void RemoveChild(int index) { isExpanded.Remove(children[index]); children.RemoveAt(index); }
	}



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
		

		protected override void DoGUI(float tabLevel)
		{
			base.DoGUI(tabLevel);

			Value.DoGUI(tabLevel, Gui.Style_MaterialValue_Text, AllowableDimensions);
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


	public class MV_Tex2D : MaterialValue
	{
		private static Texture2D Load(string filePath)
		{
			if (!File.Exists(filePath))
				return null;

			Texture2D tex = null;

			string ext = Path.GetExtension(filePath);
			if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
			{
				tex = new Texture2D(1, 1);
				tex.LoadImage(File.ReadAllBytes(filePath), true);
			}

			return tex;
		}


		public override string TypeName { get { return TypeName_Tex2D; } }
		
		public string TexturePath = null;

		private Texture2D loadedTex = null;
		private GUIUtil.FileBrowserData fileBrowser = null;


		public MV_Tex2D(string texPath = null) : this(new MV_SurfUV(), texPath) { }
		public MV_Tex2D(MaterialValue uv, string texPath = null)
		{
			AddChild(uv);
			TexturePath = texPath;

			loadedTex = Load(TexturePath);
		}


		protected override void DoGUI(float tabLevel)
		{
			base.DoGUI(tabLevel);

			if (fileBrowser == null)
			{
				if (loadedTex != null)
				{
					GUILayout.Box(loadedTex, Gui.Style_MaterialValue_Texture,
								  GUILayout.MaxWidth(Gui.MaxTexPreviewSize.x),
								  GUILayout.MaxHeight(Gui.MaxTexPreviewSize.y));

					if (GUILayout.Button("Reload", Gui.Style_MaterialValue_Button))
						loadedTex = Load(TexturePath);
				}

				if (GUILayout.Button("Change"))
				{
					fileBrowser = new GUIUtil.FileBrowserData((TexturePath == null ?
																Application.dataPath :
																TexturePath),
															  new Rect(),
															  (fle) =>
																  {
																	  loadedTex = Load(fle.FullName);
																	  fileBrowser = null;
																  },
															  Gui.Style_FileBrowser_Files,
															  Gui.Style_FileBrowser_Buttons,
															  ".png", ".jpg", ".jpeg");
				}
			}
			else
			{
				GUILayout.Label("Waiting for file browser...", Gui.Style_MaterialValue_Text);
				fileBrowser.CurrentWindowPos = GUILayout.Window(fileBrowser.ID,
																fileBrowser.CurrentWindowPos,
																GUIUtil.FileBrowserWindowCallback,
																"Choose new texture file");
			}
		}
		protected override string GetInputName(int index) { return "UV"; }

		public override void SetMaterialParams(Material mat,
											   string texParamName = null,
											   string colorParamName = null)
		{
			if (colorParamName != null)
				mat.SetColor(colorParamName, Color.white);
			if (texParamName != null && loadedTex != null)
				mat.SetTexture(texParamName, loadedTex);
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);

			writer.WriteString(TexturePath, "FilePath");
			writer.WriteString("Automatic", "FileType");

			Write(GetChild(0), writer, "UVs");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);

			TexturePath = reader.ReadString("FilePath");
			loadedTex = Load(TexturePath);

			ClearChildren();
			AddChild(Read(reader, "UVs"));
		}
	}


	public class MV_PureNoise : MaterialValue
	{
		public byte NChannels;


		public override string TypeName { get { return TypeName_PureNoise; } }


		public MV_PureNoise(byte nChannels) { NChannels = nChannels; }


		protected override void DoGUI(float tabLevel)
		{
			base.DoGUI(tabLevel);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Number of Channels:", Gui.Style_MaterialValue_Text);
			NChannels = (byte)GUILayout.HorizontalSlider((float)NChannels, 1.0f, 4.0f,
														 Gui.Style_MaterialValue_Slider,
														 Gui.Style_MaterialValue_SliderThumb);
			GUILayout.EndHorizontal();
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteByte(NChannels, "Dimensions");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			NChannels = reader.ReadByte("Dimensions");
		}
	}

	
	public abstract class MV_Simple1 : MaterialValue
	{
		public MV_Simple1(MaterialValue x) { AddChild(x); }

		protected override string GetInputName(int index) { return "Input"; }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			Write(GetChild(0), writer, GetInputName(0));
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			ClearChildren();
			AddChild(Read(reader, GetInputName(0)));
		}
	}
	public class MV_Sin : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Sin; } }
		public MV_Sin(MaterialValue x) : base(x) { }
	}
	public class MV_Cos : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Cos; } }
		public MV_Cos(MaterialValue x) : base(x) { }
	}
	public class MV_Tan : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Tan; } }
		public MV_Tan(MaterialValue x) : base(x) { }
	}
	public class MV_Asin: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Asin; } }
		public MV_Asin(MaterialValue x) : base(x) { }
	}
	public class MV_Acos: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Acos; } }
		public MV_Acos(MaterialValue x) : base(x) { }
	}
	public class MV_Atan: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Atan; } }
		public MV_Atan(MaterialValue x) : base(x) { }
	}
	public class MV_Smoothstep : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Smoothstep; } }
		public MV_Smoothstep(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "T"; }
	}
	public class MV_Smootherstep : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Smootherstep; } }
		public MV_Smootherstep(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "T"; }
	}
	public class MV_Floor : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Floor; } }
		public MV_Floor(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Ceil : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Ceil; } }
		public MV_Ceil(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Abs : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Abs; } }
		public MV_Abs(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_RayPos : MV_Simple1
	{
		public override string TypeName { get { return TypeName_RayPos; } }
		public MV_RayPos(MaterialValue t) : base(t) { }
		protected override string GetInputName(int index) { return "T"; }
	}


	public abstract class MV_Simple2 : MaterialValue
	{
		public MV_Simple2(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }

		protected override string GetInputName(int index) { return (index == 0 ? "A" : "B"); }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			Write(GetChild(0), writer, GetInputName(0));
			Write(GetChild(1), writer, GetInputName(1));
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			ClearChildren();
			AddChild(Read(reader, GetInputName(0)));
			AddChild(Read(reader, GetInputName(1)));
		}
	}
	public class MV_Atan2 : MV_Simple2
	{
		public override string TypeName { get { return TypeName_Atan2; } }
		public MV_Atan2(MaterialValue y, MaterialValue x) : base(y, x) { }
		protected override string GetInputName(int i) { return (i == 0 ? "Y" : "X"); }
	}
	public class MV_Step : MV_Simple2
	{
		public override string TypeName { get { return TypeName_Step; } }
		public MV_Step(MaterialValue edge, MaterialValue x) : base(edge, x) { }
		protected override string GetInputName(int i) { return (i == 0 ? "Edge" : "x"); }
	}
	

	public abstract class MV_Simple3 : MaterialValue
	{
		public MV_Simple3(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }

		protected override string GetInputName(int index) { return (index == 0 ? "A" : (index == 1 ? "B" : "C")); }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			Write(GetChild(0), writer, GetInputName(0));
			Write(GetChild(1), writer, GetInputName(1));
			Write(GetChild(2), writer, GetInputName(2));
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			ClearChildren();
			AddChild(Read(reader, GetInputName(0)));
			AddChild(Read(reader, GetInputName(1)));
			AddChild(Read(reader, GetInputName(2)));
		}
	}
	public class MV_Lerp : MV_Simple3
	{
		public override string TypeName { get { return TypeName_Lerp; } }
		public MV_Lerp(MaterialValue a, MaterialValue b, MaterialValue t) : base(a, b, t) { }
		protected override string GetInputName(int i) { return (i == 0 ? "A" : (i == 1 ? "B" : "T")); }
	}
	public class MV_Clamp : MV_Simple3
	{
		public override string TypeName { get { return TypeName_Clamp; } }
		public MV_Clamp(MaterialValue min, MaterialValue max, MaterialValue x) : base(min, max, x) { }
		protected override string GetInputName(int i) { return (i == 0 ? "Min" : (i == 1 ? "Max" : "X")); }
	}


	public class MV_SurfPos : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfPos; } }
	}
	public class MV_SurfNormal : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfNormal; } }
	}
	public class MV_SurfTangent : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfTangent; } }
	}
	public class MV_SurfBitangent : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfBitangent; } }
	}
	public class MV_SurfUV : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfUV; } }
	}
	public class MV_RayStartPos : MaterialValue
	{
		public override string TypeName { get { return TypeName_RayStartPos; } }
	}
	public class MV_RayDir : MaterialValue
	{
		public override string TypeName { get { return TypeName_RayDir; } }
	}
	public class MV_ShapePos : MaterialValue
	{
		public override string TypeName { get { return TypeName_ShapePos; } }
	}
	public class MV_ShapeScale : MaterialValue
	{
		public override string TypeName { get { return TypeName_ShapeScale; } }
	}
	public class MV_ShapeRot : MaterialValue
	{
		public override string TypeName { get { return TypeName_ShapeRot; } }
	}


	public abstract class MV_MultiType : MaterialValue
	{
		public override bool HasVariableNumberOfChildren { get { return true; } }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteList(GetChildrenCopy(), (mv, n, wr) => Write(mv, wr, n), "Items");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);

			List<MaterialValue> vals = new List<MaterialValue>();
			reader.ReadList(vals, (n, rd) => Read(rd, n), "Items");

			ClearChildren();
			for (int i = 0; i < vals.Count; ++i)
				AddChild(vals[i]);
		}
	}
	public class MV_Add : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Add; } }
		public MV_Add(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Add(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 0.0f); }
	}
	public class MV_Subtract: MV_MultiType
	{
		public override string TypeName { get { return TypeName_Subtract; } }
		public MV_Subtract(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Subtract(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 0.0f); }
	}
	public class MV_Multiply : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Multiply; } }
		public MV_Multiply(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Multiply(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 1.0f); }
	}
	public class MV_Divide : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Divide; } }
		public MV_Divide(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Divide(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 1.0f); }
	}
	public class MV_Min : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Min; } }
		public MV_Min(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
	}
	public class MV_Max : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Max; } }
		public MV_Max(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
	}
}