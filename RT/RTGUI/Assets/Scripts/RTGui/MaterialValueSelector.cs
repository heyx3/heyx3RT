using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RT;


namespace RTGui
{
	/// <summary>
	/// A GUI window that allows the user to select a MaterialValue type.
	/// </summary>
	public class MaterialValueSelector : ManagedWindow
	{
		private static void GUIWindowCallback(int id)
		{
			MaterialValueSelector slc = Get<MaterialValueSelector>(id);

			slc.CurrentSelection = GUILayout.SelectionGrid(slc.CurrentSelection,
														   slc.OptionsDisplay, 6);

			//Select/cancel buttons.
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Select", slc.ButtonsStyle))
			{
				slc.OnNewMVChosen(slc.OptionsFactory[slc.CurrentSelection]());
			}
			if (GUILayout.Button("Cancel", slc.ButtonsStyle))
			{
				slc.OnNewMVChosen(null);
			}
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}


		#region Definition of Options
		
		private struct Option
		{
			public Func<MaterialValue> Factory;
			public string GUIStr;
			public Type TypeOf;
			public Option(string guiStr, Type typeOf, Func<MaterialValue> factory)
			{
				GUIStr = guiStr;
				TypeOf = typeOf;
				Factory = factory;
			}
		}

		private static MaterialValue DefaultMV { get { return new MV_Constant(true, new uint[] { 1, 2, 3, 4 }, 1.0f); } }
		private static Option[] BaseOptions = new Option[] {
			new Option("Constant", typeof(MV_Constant), () => DefaultMV),
			new Option("Tex2D", typeof(MV_Tex2D), () => new MV_Tex2D()),
			new Option("Add", typeof(MV_Add), () => new MV_Add(DefaultMV, DefaultMV)),
			new Option("Subtract", typeof(MV_Subtract), () => new MV_Subtract(DefaultMV, DefaultMV)),
			new Option("Multiply", typeof(MV_Multiply), () => new MV_Multiply(DefaultMV, DefaultMV)),
			new Option("Normalize", typeof(MV_Normalize), () => new MV_Normalize(DefaultMV)),
			new Option("Length", typeof(MV_Length), () => new MV_Length(DefaultMV)),
			new Option("Distance", typeof(MV_Distance), () => new MV_Distance(DefaultMV, DefaultMV)),
			new Option("Sqrt", typeof(MV_Sqrt), () => new MV_Sqrt(DefaultMV)),
			new Option("Sin", typeof(MV_Sin), () => new MV_Sin(DefaultMV)),
			new Option("Cos", typeof(MV_Cos), () => new MV_Cos(DefaultMV)),
			new Option("Tan", typeof(MV_Tan), () => new MV_Tan(DefaultMV)),
			new Option("Asin", typeof(MV_Asin), () => new MV_Asin(DefaultMV)),
			new Option("Acos", typeof(MV_Acos), () => new MV_Acos(DefaultMV)),
			new Option("Atan", typeof(MV_Atan), () => new MV_Atan(DefaultMV)),
			new Option("Atan2", typeof(MV_Atan2), () => new MV_Atan2(DefaultMV, DefaultMV)),
			new Option("Step", typeof(MV_Step), () => new MV_Step(DefaultMV, DefaultMV)),
			new Option("Lerp", typeof(MV_Lerp), () => new MV_Lerp(DefaultMV, DefaultMV, DefaultMV)),
			new Option("Smoothstep", typeof(MV_Smoothstep), () => new MV_Smoothstep(DefaultMV)),
			new Option("Smootherstep", typeof(MV_Smootherstep), () => new MV_Smootherstep(DefaultMV)),
			new Option("Clamp", typeof(MV_Clamp), () => new MV_Clamp(DefaultMV, DefaultMV, DefaultMV)),
			new Option("Floor", typeof(MV_Floor), () => new MV_Floor(DefaultMV)),
			new Option("Ceil", typeof(MV_Ceil), () => new MV_Ceil(DefaultMV)),
			new Option("Abs", typeof(MV_Abs), () => new MV_Abs(DefaultMV)),
			new Option("Min", typeof(MV_Min), () => new MV_Min(DefaultMV, DefaultMV)),
			new Option("Max", typeof(MV_Max), () => new MV_Max(DefaultMV, DefaultMV)),
			new Option("Surface Pos", typeof(MV_SurfPos), () => new MV_SurfPos()),
			new Option("Surface UV", typeof(MV_SurfUV), () => new MV_SurfUV()),
			new Option("Surface Normal", typeof(MV_SurfNormal), () => new MV_SurfNormal()),
			new Option("Surface Tangent", typeof(MV_SurfTangent), () => new MV_SurfTangent()),
			new Option("Surface Bitangent", typeof(MV_SurfBitangent), () => new MV_SurfBitangent()),
			new Option("Ray Start Position", typeof(MV_RayStartPos), () => new MV_RayStartPos()),
			new Option("Ray Direction", typeof(MV_RayDir), () => new MV_RayDir()),
			new Option("Position Along Ray", typeof(MV_RayPos), () => new MV_RayPos(DefaultMV)),
			new Option("Shape Position", typeof(MV_ShapePos), () => new MV_ShapePos()),
			new Option("Shape Rotation (XYZ axis, W radians)", typeof(MV_ShapeRot), () => new MV_ShapeRot()),
			new Option("Shape Scale", typeof(MV_ShapeScale), () => new MV_ShapeScale()),
			new Option("Pure Noise", typeof(MV_PureNoise), () => new MV_PureNoise(1)),
		};

		#endregion


		public GUIContent[] OptionsDisplay;
		public Func<MaterialValue>[] OptionsFactory;
		public int CurrentSelection = 0;

		/// <summary>
		/// If nothing was chosen, "null" is passed.
		/// </summary>
		public Action<MaterialValue> OnNewMVChosen;

		public GUIStyle ButtonsStyle;


		public MaterialValueSelector(Rect startWindowPos,
									 Action<MaterialValue> onNewMVChosen,
									 GUIStyle buttonsStyle,
									 GUIContent windowTitle,
									 params Type[] ignoreTypes)
			: base(windowTitle, startWindowPos, GUIWindowCallback, true)
		{
			OnNewMVChosen = onNewMVChosen;
			
			ButtonsStyle = buttonsStyle;

			//Build up the options.
			Option[] opts = BaseOptions.Where(opt => !ignoreTypes.Contains(opt.TypeOf)).ToArray();
			UnityEngine.Assertions.Assert.IsTrue(opts.Length > 0, "No available options");
			OptionsDisplay = new GUIContent[opts.Length];
			OptionsFactory = new Func<MaterialValue>[opts.Length];
			for (int i = 0; i < opts.Length; ++i)
			{
				OptionsDisplay[i] = new GUIContent(opts[i].GUIStr);
				OptionsFactory[i] = opts[i].Factory;
			}
		}
	}
}
