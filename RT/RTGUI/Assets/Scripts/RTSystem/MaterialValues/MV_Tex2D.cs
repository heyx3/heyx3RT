﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using RT.Serialization;

namespace RT.MaterialValue
{
	public class MV_Tex2D : MV_Base
	{
		public override string TypeName { get { return TypeName_Tex2D; } }
		public override OutputSizes OutputSize { get { return OutputSizes.Four; } }

		public override string PrettyName { get { return "2D Texture"; } }
		public override Color GUIColor { get { return new Color(0.85f, 1.0f, 0.85f); } }


		public Texture2D Tex;


		public MV_Tex2D(MV_Base uv, Texture2D tex = null)
		{
			AddInput(uv);
			Tex = tex;
		}


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			string texName = "_tex" + idLookup[this];

			shaderlabProperties.Append("\t\t\t");
			shaderlabProperties.Append(texName);
			shaderlabProperties.Append(" (\"");
			shaderlabProperties.Append(texName);
			shaderlabProperties.AppendLine("\", 2D) = \"\" {}");

			cgDefinitions.Append("\t\t\t\tsampler2D ");
			cgDefinitions.Append(texName);
			cgDefinitions.AppendLine(";");

			cgFunctionBody.Append("float4 ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = tex2D(");
			cgFunctionBody.Append(texName);
			cgFunctionBody.Append(", ");
			cgFunctionBody.Append(GetInput(0).GetShaderValue(OutputSizes.Two, idLookup));
			cgFunctionBody.AppendLine(");");
		}
		public override void SetParams(Transform shapeTr, Material unityMat,
									   Dictionary<MV_Base, uint> idLookup)
		{
			unityMat.SetTexture("_tex" + idLookup[this], Tex);
		}

		public override MV_Base GetDefaultInput(int inputIndex) { return MV_Constant.MakeVec2(0.0f, 0.0f, true, 0.0f, 1.0f, OutputSizes.Two, true); }
		public override string GetInputName(int index) { return "UV"; }

		public override void WriteData(DataWriter writer, string namePrefix,
									   Dictionary<MV_Base, uint> idLookup)
		{
			base.WriteData(writer, namePrefix, idLookup);
			
			string assetsFolder = Application.dataPath,
				   texturePath = Path.Combine(Path.Combine(assetsFolder, "..\\"),
											  AssetDatabase.GetAssetPath(Tex)),
				   extension = Path.GetExtension(texturePath);

			if (extension == ".bmp")
				writer.String("BMP", namePrefix + "FileType");
			else if (extension == ".png")
				writer.String("PNG", namePrefix + "FileType");
			else
				throw new NotImplementedException("Only BMP and PNG textures are supported: " + texturePath);

			writer.String(Path.GetFullPath(texturePath).Replace('\\', '/'),
						  namePrefix + "FilePath");
		}
		public override void ReadData(DataReader reader, string namePrefix,
									  Dictionary<MV_Base, List<uint>> childIDsLookup)
		{
			base.ReadData(reader, namePrefix, childIDsLookup);

			//Get the texture path relative to the "Assets" folder.
			string texPath = Path.GetFullPath(reader.String(namePrefix + "FilePath"));
			string[] texPathDirs = texPath.Split(Path.DirectorySeparatorChar,
												 Path.AltDirectorySeparatorChar);
			int index = texPathDirs.IndexOf("Assets");
			if (index == -1)
				throw new Serialization.DataReader.ReadException("Can't find 'Assets' folder in path " + texPath);
			texPath = texPath.Substring(index);

			Tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath.MakePathRelative("Assets"));
		}

		public override GUIResults DoCustomGUI()
		{
			GUIResults results = GUIResults.Nothing;

			Texture2D newTex = (Texture2D)EditorGUILayout.ObjectField(Tex, typeof(Texture2D), false,
																	  GUILayout.MinWidth(200.0f));
			if (newTex != Tex)
			{
				Tex = newTex;
				results = GUIResults.Other;
			}

			return results;
		}
	}
}