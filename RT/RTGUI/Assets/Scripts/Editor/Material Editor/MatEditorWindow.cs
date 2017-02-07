using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using RT.MaterialValue;


namespace RT.MatEditor
{
	public class MatEditorWindow : EditorWindow
	{
		public static MatEditorWindow Create(MonoBehaviour owner, RT.MaterialValue.Graph graph)
		{
			var wnd = GetWindow<MatEditorWindow>();
			wnd.Init(owner, graph);
			return wnd;
		}


		private static readonly float LeftPanesWidth = 200.0f,
									  OptionsPaneHeight = 400.0f;


		public MonoBehaviour Owner { get; private set; }
		public RT.MaterialValue.Graph Graph { get; private set; }
		
		public Pane_ChooseMV ChooseMVPane { get; private set; }
		public Pane_Controls ControlsPane { get; private set; }
		public Pane_Graph GraphPane { get; private set; }

		private bool initYet = false;

		
		public void Init(MonoBehaviour owner, RT.MaterialValue.Graph graph)
		{
			initYet = true;

			minSize = new Vector2(700.0f, 500.0f);
			titleContent = new GUIContent("RTMaterial Graph");

			Owner = owner;
			Graph = graph;

			ChooseMVPane = new Pane_ChooseMV(this);
			ControlsPane = new Pane_Controls(this);
			GraphPane = new Pane_Graph(this);
			
			ControlsPane.OnSave += Save;
			ControlsPane.OnDiscardChanges += GraphPane.DiscardChanges;
			ControlsPane.OnClear += GraphPane.Clear;

			OnFocus();
		}

		public void Save()
		{
			//Set the original graph to have the same layout as the editor pane's copy.
			GraphPane.Graph.Clone(Graph);
		}
		
		private void OnFocus()
		{
			//If the material we were editing is gone, close the editor.
			if (initYet && Owner == null)
				Close();
		}
		private void OnGUI()
		{
			if (!initYet)
				return;

			Rect area_chooseMV = new Rect(0.0f, 0.0f, LeftPanesWidth, OptionsPaneHeight),
				 area_controls = new Rect(0.0f, OptionsPaneHeight,
										  LeftPanesWidth,
										  position.height - OptionsPaneHeight),
				 area_graph = new Rect(LeftPanesWidth, 0.0f,
									   position.width - LeftPanesWidth,
									   position.height);
			ChooseMVPane.DoGUI(area_chooseMV);
			ControlsPane.DoGUI(area_controls);
			GraphPane.DoGUI(area_graph);
		}
	}
}