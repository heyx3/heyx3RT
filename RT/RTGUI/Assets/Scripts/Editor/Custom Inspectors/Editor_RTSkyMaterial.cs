using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT.CustomInspectors
{
    public abstract class Editor_RTSkyMaterial : Editor
    {
        public override void OnInspectorGUI()
        {
            RTSkyMaterial mat = (RTSkyMaterial)target;

            MyGUI.BeginCompact();
            if (GUILayout.Button("Regenerate material"))
                mat.RegenerateMaterial(mat.GetComponent<MeshRenderer>());
            MyGUI.EndCompact();

            MyGUI.BeginCompact();
            if (GUILayout.Button("Edit material"))
            {
                MatEditor.MatEditorWindow.Create(mat, mat.Graph);
            }
            MyGUI.EndCompact();

            CustomInspectorGUI(mat);
        }

        protected virtual void CustomInspectorGUI(RTSkyMaterial mat) { }
    }


    [CustomEditor(typeof(RTSkyMaterial_SimpleColor))]
    public class Editor_RTSkyMaterial_SimpleColor : Editor_RTSkyMaterial { }

    [CustomEditor(typeof(RTSkyMaterial_VerticalGradient))]
    public class Editor_RTSkyMaterial_VerticalGradient : Editor_RTSkyMaterial { }
}