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
				//TODO: Fill in once all classes are made.

				default:
					Debug.LogError("Unexpected MaterialValue type name \"" +
								   typeName + "\"");
					return null;
			}

			reader.ReadDataStructure(ref mv, name + "Value");
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


		private List<MaterialValue> children;
		private Dictionary<MaterialValue, bool> isExpanded;


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
					AddChild(new MV_Constant(Vector3.one));
				}
				GUILayout.EndHorizontal();
			}
		}

		protected virtual void DoGUI(float tabLevel) { }
		protected virtual string GetInputName(int index) { return (index + 1).ToString(); }

		public virtual void WriteData(RTSerializer.Writer writer) { }
		public virtual void ReadData(RTSerializer.Reader reader) { }
		

		protected void AddChild(MaterialValue mv) { children.Add(mv); isExpanded.Add(mv, false); }
		protected void RemoveChild(MaterialValue mv) { RemoveChild(children.IndexOf(mv)); }
		protected void RemoveChild(int index) { isExpanded.Remove(children[index]); children.RemoveAt(index); }
	}



	public class MV_Constant : MaterialValue
	{
		public override string TypeName { get { return TypeName_Constant; } }
		
		public Vector3 Value;
		
		public MV_Constant(Vector3 value) { Value = value; }
		
		protected override void DoGUI(float tabLevel)
		{
			Value = GUIUtil.RGBEditor(Value, tabLevel, Gui.Style_MaterialValue_Text,
									  Gui.Style_RGB_Slider, Gui.Style_RGB_SliderThumb);
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteVector3(Value, "Value");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
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
		
		public string TexturePath;

		private Texture2D loadedTex = null;
		private GUIUtil.FileBrowserData fileBrowser = null;


		public MV_Tex2D(string texPath, MaterialValue uv = new MV_SurfUV())
		{
			AddChild(uv);
			TexturePath = texPath;

			loadedTex = Load(TexturePath);
		}

		protected override void DoGUI(float tabLevel)
		{
			if (fileBrowser == null)
			{
				if (loadedTex != null)
				{
					GUILayout.Box(loadedTex, Gui.Style_MaterialValue_Texture,
								  GUILayout.MaxWidth(Gui.MaxTexPreviewSize.x),
								  GUILayout.MaxHeight(Gui.MaxTexPreviewSize.y));

					//TODO: Start file browser.
				}
			}
			else
			{
				GUILayout.Label("Waiting for file browser...");
			}
		}
		protected override string GetInputName(int index) { return "UV"; }
	}


	//TODO: Rest of MV classes.
}