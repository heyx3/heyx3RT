using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RT;


namespace RTGui
{
	/// <summary>
	/// A GUI window that allows the user to select a Material type.
	/// </summary>
	public class MaterialSelector : ManagedWindow
	{
		private static void GUIWindowCallback(int id)
		{
			MaterialSelector slc = Get<MaterialSelector>(id);

			slc.CurrentSelection = GUILayout.SelectionGrid(slc.CurrentSelection,
														   slc.OptionsDisplay, 6);

			//Select/cancel buttons.
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Select", slc.ButtonsStyle))
			{
				slc.OnRTMatChosen(slc.OptionsTypes[slc.CurrentSelection]);
			}
			if (GUILayout.Button("Cancel", slc.ButtonsStyle))
			{
				slc.OnRTMatChosen(null);
			}
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}


		#region Definition of Options
		
		private struct Option
		{
			public string GUIStr;
			public Type TypeOf;
			public Option(string guiStr, Type typeOf)
			{
				GUIStr = guiStr;
				TypeOf = typeOf;
			}
		}

		private static Option[] BaseOptions = new Option[] {
			new Option("Lambert", typeof(RT.RTMat_Lambert)),
			new Option("Metal", typeof(RT.RTMat_Metal)),
		};

		#endregion


		public GUIContent[] OptionsDisplay;
		public Type[] OptionsTypes;
		public int CurrentSelection = 0;

		/// <summary>
		/// If nothing was chosen, "null" is passed.
		/// </summary>
		public Action<Type> OnRTMatChosen;

		public GUIStyle ButtonsStyle;


		public MaterialSelector(Rect startWindowPos,
								Action<Type> onRTMatChosen,
								GUIStyle buttonsStyle,
								GUIContent windowTitle,
								params Type[] ignoreTypes)
			: base(windowTitle, startWindowPos, GUIWindowCallback, true)
		{
			OnRTMatChosen = onRTMatChosen;
			
			ButtonsStyle = buttonsStyle;

			//Build up the options.
			Option[] opts = BaseOptions.Where(opt => !ignoreTypes.Contains(opt.TypeOf)).ToArray();
			UnityEngine.Assertions.Assert.IsTrue(opts.Length > 0, "No available options");
			OptionsDisplay = new GUIContent[opts.Length];
			OptionsTypes = new Type[opts.Length];
			for (int i = 0; i < opts.Length; ++i)
			{
				OptionsDisplay[i] = new GUIContent(opts[i].GUIStr);
				OptionsTypes[i] = opts[i].TypeOf;
			}
		}
	}
}
