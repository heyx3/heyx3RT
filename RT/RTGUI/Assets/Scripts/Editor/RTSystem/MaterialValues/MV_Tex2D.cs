using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	[Serializable]
	public class MV_Tex2D : MV_Base
	{
		public override string TypeName { get { return TypeName_Tex2D; } }
		public override OutputSizes OutputSize { get { return OutputSizes.Four; } }

		public override string PrettyName { get { return "2D Texture"; } }
		public override Color GUIColor { get { return new Color(0.85f, 1.0f, 0.85f); } }


		public Texture2D Tex;


		public MV_Tex2D(Texture2D tex = null) { Tex = tex; }


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder fragmentShaderBody)
		{
			string texName = "_tex" + GUID;

			shaderlabProperties.Append("\t\t\t");
			shaderlabProperties.Append(texName);
			shaderlabProperties.Append(" (\"");
			shaderlabProperties.Append(texName);
			shaderlabProperties.AppendLine("\", 2D) = \"\" {}");

			cgDefinitions.Append("\t\t\t\tsampler2D ");
			cgDefinitions.Append(texName);
			cgDefinitions.AppendLine(";");

			fragmentShaderBody.Append("float4 ");
			fragmentShaderBody.Append(ShaderValueName);
			fragmentShaderBody.Append(" = tex2D(");
			fragmentShaderBody.Append(texName);
			fragmentShaderBody.Append(", ");
			fragmentShaderBody.Append(GetInputValue(0, OutputSizes.Two));
			fragmentShaderBody.AppendLine(");");
		}
		public override void SetParams(Transform shapeTr, Material unityMat)
		{
			unityMat.SetTexture("_tex" + GUID, Tex);
		}

		public override MV_Base GetDefaultInput(int inputIndex) { return MV_Constant.MakeVec2(0.0f, 0.0f); }
		public override string GetInputName(int index) { return "UV"; }

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);

			string assetsFolder = Application.dataPath,
				   texturePath = Path.Combine(Path.Combine(assetsFolder, "..\\"),
											  AssetDatabase.GetAssetPath(Tex)),
				   extension = Path.GetExtension(texturePath);
			
			//Write texture type.
			if (extension == ".bmp")
				writer.String("BMP", "FileType");
			else if (extension == ".png")
				writer.String("PNG", "FileType");
			else
				throw new NotImplementedException("Only BMP and PNG textures are supported: " + texturePath);

			//Write texture path.
			writer.String(texturePath, "FilePath");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);

			string texPath = Path.GetFullPath(reader.String("FilePath"));
			
			//Get the texture path relative to the "Assets" folder.
			string[] texPathDirs = texPath.Split(Path.DirectorySeparatorChar,
												 Path.AltDirectorySeparatorChar);
			int index = texPathDirs.IndexOf("Assets");
			if (index == -1)
				throw new Serialization.DataReader.ReadException("Can't find 'Assets' folder in path " + texPath);
			texPath = texPath.Substring(index);

			Tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
		}

		protected override GUIResults DoCustomGUI()
		{
			GUIResults results = GUIResults.Nothing;

			Texture2D newTex = (Texture2D)EditorGUILayout.ObjectField(Tex, typeof(Texture2D), false);
			if (newTex != Tex)
			{
				Tex = newTex;
				results = GUIResults.Other;
			}

			return results;
		}
	}
}