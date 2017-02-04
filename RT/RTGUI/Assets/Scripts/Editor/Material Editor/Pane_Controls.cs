using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RT.MatEditor
{
	public class Pane_Controls
	{
		public MatEditorWindow Owner { get; private set; }

		public event Action OnSave, OnDiscardChanges, OnClear;


		public Pane_Controls(MatEditorWindow owner)
		{
			Owner = owner;
		}


		public void DoGUI(Rect area)
		{
			GUILayout.BeginArea(area);

			if (GUILayout.Button("Save Graph"))
			{
				if (OnSave != null)
					OnSave();
			}

			GUILayout.Space(15.0f);

			if (GUILayout.Button("Discard Changes"))
			{
				if (OnDiscardChanges != null)
					OnDiscardChanges();
			}
			if (GUILayout.Button("Clear Graph") &&
				EditorUtility.DisplayDialog("Confirm", "Are you sure you want to clear the graph?",
											"OK"))
			{
				if (OnClear != null)
					OnClear();
			}

			//TODO: Preview of the result.

			GUILayout.EndArea();
		}
	}
}