using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT.CustomInspectors
{
	//TODO: Do this for SkyMaterial as well.

	public abstract class Editor_RTMaterial : Editor
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
			{
				Dictionary<string, MV_Base> matVals = new Dictionary<string, MV_Base>();
				mat.GetMVs(matVals);
				MatEditor.MatEditorWindow.Create(mat, matVals);
			}
			MyGUI.EndCompact();

			CustomInspectorGUI(mat);
		}

		protected virtual void CustomInspectorGUI(RTMaterial mat) { }
	}


	[CustomEditor(typeof(RTMaterial_Lambert))]
	public class Editor_RTMaterial_Lambert : Editor_RTMaterial { }

	[CustomEditor(typeof(RTMaterial_Metal))]
	public class Editor_RTMaterial_Metal : Editor_RTMaterial { }
}