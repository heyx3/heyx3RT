using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.CustomInspectors
{
	public class Editor_RTShape : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var shape = (RTShape)target;

			MyGUI.BeginCompact();
			if (GUILayout.Button("Save to JSON"))
			{
				string path = EditorUtility.SaveFilePanel("Choose location", Application.dataPath + "\\..\\", "derp", "json");
				if (path != "")
				{
					var json = new System.Text.StringBuilder();
					using (var writer = new Serialization.JSONWriter(new System.IO.StringWriter(json)))
						writer.Structure(shape, "shape");

					System.IO.File.WriteAllText(path, json.ToString());
					EditorUtility.RevealInFinder(path);
				}
			}
			MyGUI.EndCompact();
		}
	}

	
	[CustomEditor(typeof(RTShape_Plane))]
	public class Editor_RTShape_Plane : Editor_RTShape { }
	
	[CustomEditor(typeof(RTShape_Sphere))]
	public class Editor_RTShape_Sphere : Editor_RTShape { }
	
	[CustomEditor(typeof(RTShape_Sphere))]
	public class Editor_RTShape_Mesh : Editor_RTShape { }
}
