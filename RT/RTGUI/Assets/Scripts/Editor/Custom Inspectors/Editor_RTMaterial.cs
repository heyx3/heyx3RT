using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.CustomInspectors
{
	[CustomEditor(typeof(RTMaterial))]
	public class Editor_RTMaterial : Editor
	{
		public override void OnInspectorGUI()
		{
			RTMaterial mat = (RTMaterial)target;
			
			MyGUI.BeginCompact();
			if (GUILayout.Button("Regenerate material"))
				mat.RegenerateMaterial(mat.GetComponent<MeshRenderer>());
			MyGUI.EndCompact();

			MyGUI.BeginCompact();
			if (GUILayout.Button("Edit material"))
				{ }//TODO: Show material editor.
			MyGUI.EndCompact();
		}
	}
}