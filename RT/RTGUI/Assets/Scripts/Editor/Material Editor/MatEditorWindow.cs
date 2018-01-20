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
		public static MatEditorWindow Create(RTBaseMaterial owner)
		{
			var wnd = GetWindow<MatEditorWindow>();
			wnd.Init(owner);
			return wnd;
		}


		private static readonly float LeftPanesBorder = 5.0f,
									  LeftPanesWidth = 200.0f,
									  ControlsPaneHeight = 200.0f;
		private static readonly Vector2 MinSize = new Vector2(700.0f, 500.0f);


		public RTBaseMaterial Owner { get; private set; }

		public Pane_ChooseMV ChooseMVPane { get; private set; }
		public Pane_Controls ControlsPane { get; private set; }
		public Pane_Graph GraphPane { get; private set; }

		private bool initYet = false;


		public void Init(RTBaseMaterial owner)
		{
			initYet = true;

			minSize = MinSize;
			titleContent = new GUIContent("RTMaterial Graph");

			Owner = owner;

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
			GraphPane.Graph.Clone(Owner.Graph);

			Owner.RegenerateMaterial();
		}

		private void OnFocus()
		{
			//If the material we were editing is gone, close the editor.
			if (initYet && Owner == null)
				Close();
		}
		private void OnGUI()
		{
            //If Unity just recompiled, the non-serializable stuff will be left "null".
            if (ChooseMVPane == null)
                Close();

			if (!initYet)
				return;

			Rect area_chooseMV = new Rect(LeftPanesBorder, LeftPanesBorder,
										  LeftPanesWidth - LeftPanesBorder,
										  position.height - ControlsPaneHeight - (LeftPanesBorder * 2.0f)),
				 area_controls = new Rect(LeftPanesBorder,
										  position.height - ControlsPaneHeight + LeftPanesBorder,
										  LeftPanesWidth - LeftPanesBorder,
										  ControlsPaneHeight - (LeftPanesBorder * 2.0f)),
				 area_graph = new Rect(LeftPanesWidth, 0.0f,
									   position.width - LeftPanesWidth,
									   position.height);
			ChooseMVPane.DoGUI(area_chooseMV);
			GraphPane.DoGUI(area_graph);
			ControlsPane.DoGUI(area_controls);
		}
	}
}