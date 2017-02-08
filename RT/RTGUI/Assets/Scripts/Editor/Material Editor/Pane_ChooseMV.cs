using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RT.MatEditor
{
	public class Pane_ChooseMV
	{
		public MatEditorWindow Owner { get; private set; }

		/// <summary>
		/// The option the user wishes to place into the graph.
		/// If set to "null", no option is currently chosen.
		/// </summary>
		public NodeTree_Element_Option CurrentlyPlacing
		{
			get { return currentlyPlacing; }
			set
			{
				currentlyPlacing = value;
				if (OnCurrentOptionChanged != null)
					OnCurrentOptionChanged();
			}
		}
		private NodeTree_Element_Option currentlyPlacing = null;

		public List<NodeTree_Element> NodeOptions;

		public event Action OnCurrentOptionChanged;

		private Vector2 scrollPos = Vector2.zero;


		public Pane_ChooseMV(MatEditorWindow owner)
		{
			CurrentlyPlacing = null;
			NodeOptions = NodeOptionsGenerator.GenerateList();
		}

		
		public void DoGUI(Rect area)
		{
			GUI.Box(area, GlobalTextures.WhitePixel);

			GUILayout.BeginArea(area);
			scrollPos = GUILayout.BeginScrollView(scrollPos);

			if (CurrentlyPlacing == null)
			{
				GUILayout.Label("Click on an option, then\nclick in the graph to place it.");
				GUILayout.Label("Mouse over an option to\nget more info about it.");
				GUILayout.Space(25.0f);

				for (int i = 0; i < NodeOptions.Count; ++i)
				{
					var chosenOption = NodeOptions[i].OnGUI();
					if (chosenOption != null)
					{
						CurrentlyPlacing = chosenOption;
						break;
					}
				}
			}
			else
			{
				GUILayout.Label("Left-click in the graph\nto place \"" + CurrentlyPlacing.Name + "\"");
				GUILayout.Label("Right-click in the graph\nto cancel its placement");
				if (GUILayout.Button("Cancel placement"))
					CurrentlyPlacing = null;
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}
