using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;


namespace RT.CustomInspectors
{
	public abstract class Editor_RTBaseMaterial : Editor
	{
		public override void OnInspectorGUI()
		{
			RTBaseMaterial mat = (RTBaseMaterial)target;

			//TODO: Remove this when done testing.
			MyGUI.BeginCompact();
			if (GUILayout.Button("Regenerate material"))
				mat.RegenerateMaterial();
			MyGUI.EndCompact();

			MyGUI.BeginCompact();
			if (GUILayout.Button("Edit material"))
				MatEditor.MatEditorWindow.Create(mat);
			GUILayout.Space(25.0f);
			if (GUILayout.Button("Clear material") && EditorUtility.DisplayDialog("Confirm clear material", "Are you sure?", "OK"))
			{
				mat.Graph.Wipe();
				mat.RegenerateMaterial();
			}
			MyGUI.EndCompact();
			
			//TODO: Remove this when done testing.
			MyGUI.BeginCompact();
			if (GUILayout.Button("Save JSON"))
			{
				string path = EditorUtility.SaveFilePanel("Choose location", Application.dataPath + "\\..\\", "derp", "json");
				if (path != "")
				{
					System.IO.File.WriteAllText(path, mat.GraphJSON);
					EditorUtility.RevealInFinder(path);
				}
			}
			MyGUI.EndCompact();

			CustomInspectorGUI(mat);
		}

		protected virtual void CustomInspectorGUI(RTBaseMaterial mat) { }
	}


	public abstract class Editor_RTMaterial : Editor_RTBaseMaterial { }

	[CustomEditor(typeof(RTMaterial_Lambert))]
	public class Editor_RTMaterial_Lambert : Editor_RTMaterial { }

	[CustomEditor(typeof(RTMaterial_Metal))]
	public class Editor_RTMaterial_Metal : Editor_RTMaterial { }


	public abstract class Editor_RTSkyMaterial : Editor_RTBaseMaterial
	{
		protected override void CustomInspectorGUI(RTBaseMaterial mat)
		{
			base.CustomInspectorGUI(mat);

			RTSkyMaterial skyMat = (RTSkyMaterial)mat;

			EditorGUI.BeginChangeCheck();
			skyMat.Distance = EditorGUILayout.FloatField("Skybox Distance", skyMat.Distance);
			if (EditorGUI.EndChangeCheck())
			{
				skyMat.OnValidate();
				Undo.RecordObject(mat, "Change skybox distance");
			}
		}
	}

	[CustomEditor(typeof(RTSkyMaterial_SimpleColor))]
    public class Editor_RTSkyMaterial_SimpleColor : Editor_RTSkyMaterial { }

    [CustomEditor(typeof(RTSkyMaterial_VerticalGradient))]
    public class Editor_RTSkyMaterial_VerticalGradient : Editor_RTSkyMaterial { }
}
