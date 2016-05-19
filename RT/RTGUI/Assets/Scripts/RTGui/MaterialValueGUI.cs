using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RT;


namespace RTGui
{
	/// <summary>
	/// Handles GUI for editing a root MaterialValue.
	/// </summary>
	public class MaterialValueGui
	{
		public MaterialValueSelector NewMVSelector = null;
		public bool IsExpanded = false;

		public string Name;

		public GUIStyle ButtonStyle;
		public GUIStyle TextStyle;

		private MaterialValue changeTo = null;


		public MaterialValueGui(string name, GUIStyle buttonStyle, GUIStyle textStyle)
		{
			Name = name;

			ButtonStyle = buttonStyle;
			TextStyle = textStyle;
		}


		public MaterialValue DoGUI(MaterialValue currentMV, bool canDelete)
		{
			MaterialValue toReturn = (changeTo == null ? currentMV : changeTo);
			changeTo = null;


			GUILayout.BeginHorizontal();

			//Expand/collapse the element with a button.
			if (GUILayout.Button(IsExpanded ? "^" : "V", ButtonStyle))
			{
				IsExpanded = !IsExpanded;
			}

			//Display the name/type.
			GUILayout.Label(Name + ": " + toReturn.TypeName, TextStyle);

			//Selector to change the type.
			if (NewMVSelector == null && GUILayout.Button("Change", ButtonStyle))
			{
				NewMVSelector = new MaterialValueSelector(new Rect(),
														  (mv) =>
														  {
															  changeTo = mv;
															  NewMVSelector = null;
														  },
														  ButtonStyle,
														  new GUIContent("Choose new type"),
														  toReturn.GetType());
			}
			if (NewMVSelector != null)
			{
				NewMVSelector.DoGUI();
			}

			//Delete the child with a button.
			if (canDelete)
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("-", ButtonStyle))
				{
					toReturn = null;
				}
			}

			GUILayout.EndHorizontal();

			//If the MaterialValue is expanded, show its gui.
			if (IsExpanded && toReturn != null)
			{
				GUIUtil.StartTab(RTGui.Gui.Instance.TabSize);
				toReturn.OnGUI();
				GUIUtil.EndTab();
			}


			return toReturn;
		}
	}
}