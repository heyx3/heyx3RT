using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	public class MV_Swizzle : MV_Base
	{
		public enum Components { X = 0, Y, Z, W }
		private static readonly string[] ComponentStrings = { "x", "y", "z", "w" };
		private static readonly string[] ComponentGUIChoices = { "1D", "2D", "3D", "4D" };
		private static readonly int[] ComponentChoices = { 1, 2, 3, 4 };


		public byte NComponents;
		public Components NewX, NewY, NewZ, NewW;


		public override string TypeName { get { return TypeName_Swizzle; } }
		public override OutputSizes OutputSize { get { return OutputSizesExtensions.FromNumber(NComponents); } }

		public override string ShaderValueName
		{
			get
			{
				if (NComponents == 0 || NComponents > 4)
					Debug.LogError("NComponents is " + NComponents);

				string val = "(" + GetInput(0).ShaderValueName + ")" + "." + ComponentStrings[(int)NewX];
				if (NComponents > 1)
					val += ComponentStrings[(int)NewY];
				if (NComponents > 2)
					val += ComponentStrings[(int)NewZ];
				if (NComponents > 3)
					val += ComponentStrings[(int)NewW];

				return val;
			}
		}

		public override string PrettyName { get { return "Swizzle"; } }
		public override Color GUIColor { get { return new Color(0.85f, 0.75f, 0.95f); } }

		
		public MV_Swizzle(MV_Base val, Components newX)
			{ AddInput(val); NComponents = 1; NewX = newX; }
		public MV_Swizzle(MV_Base val, Components newX, Components newY)
			{ AddInput(val); NComponents = 2; NewX = newX; NewY = newY; }
		public MV_Swizzle(MV_Base val, Components newX, Components newY, Components newZ)
			{ AddInput(val); NComponents = 3; NewX = newX; NewY = newY; NewZ = newZ; }
		public MV_Swizzle(MV_Base val, Components newX, Components newY, Components newZ, Components newW)
			{ AddInput(val); NComponents = 4; NewX = newX; NewY = newY; NewZ = newZ; NewW = newW; }


		public override MV_Base GetDefaultInput(int inputIndex) { return MV_Constant.MakeVec2(0.0f, 0.0f); }
		public override string GetInputName(int index) { return "Val"; }

		public override void Emit(StringBuilder shaderlabProperties, StringBuilder cgDefinitions, StringBuilder cgFunctionBody) { }

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);

			writer.Byte(NComponents, "NValues");
			writer.Byte((byte)NewX, "Value0");
			if (NComponents > 1)
				writer.Byte((byte)NewY, "Value1");
			if (NComponents > 2)
				writer.Byte((byte)NewZ, "Value2");
			if (NComponents > 3)
				writer.Byte((byte)NewW, "Value3");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);

			NComponents = reader.Byte("NValues");
			NewX = (Components)reader.Byte("Value0");
			if (NComponents > 1)
				NewY = (Components)reader.Byte("Value1");
			if (NComponents > 2)
				NewZ = (Components)reader.Byte("Value2");
			if (NComponents > 3)
				NewW = (Components)reader.Byte("Value3");
		}

		protected override GUIResults DoCustomGUI()
		{
			GUIResults result = GUIResults.Nothing;

			//Store the original values for the fields.
			Components newX = NewX,
					   newY = NewY,
					   newZ = NewZ,
					   newW = NewW;
			int nComponents = NComponents;

			//Edit the different swizzled components.
			GUILayout.BeginHorizontal();
			newX = (Components)EditorGUILayout.EnumPopup(NewX);
			if (NComponents > 1)
				newY = (Components)EditorGUILayout.EnumPopup(NewY);
			if (NComponents > 2)
				newZ = (Components)EditorGUILayout.EnumPopup(NewZ);
			if (NComponents > 3)
				newW = (Components)EditorGUILayout.EnumPopup(NewW);
			GUILayout.EndHorizontal();

			//Edit the total number of components.
			NComponents = (byte)EditorGUILayout.IntPopup(NComponents,
									  					 ComponentGUIChoices,
														 ComponentChoices);

			if (newX != NewX || newY != NewY || newZ != NewZ || newW != NewW ||
				nComponents != NComponents)
			{
				result = GUIResults.Other;
				NewX = newX;
				NewY = newY;
				NewZ = newZ;
				NewW = newW;
			}

			return result;
		}
	}
}