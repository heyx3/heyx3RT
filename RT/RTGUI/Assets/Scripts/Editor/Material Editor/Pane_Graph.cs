using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using MV_Base = RT.MaterialValue.MV_Base;
using MV_Constant = RT.MaterialValue.MV_Constant;


namespace RT.MatEditor
{
	public class Pane_Graph
	{
		public MatEditorWindow Owner { get; private set; }
		public RT.MaterialValue.Graph Graph { get; private set; }
		public Func<int, string> RootIndexToDisplayName { get; private set; }

		public event Action<Vector2> OnClickEmptySpace;

		/// <summary>
		/// The position of the "camera" viewing the nodes.
		/// </summary>
		public Vector2 CamOffset;

		/// <summary>
		/// Whether the mouse is currently dragging.
		/// </summary>
		private bool draggingMouseDown = false;
		/// <summary>
		/// The ID of the window being dragged.
		/// A value of "ulong.MaxValue" means no window is being dragged.
		/// A value of "ulong.MaxValue - 1" means the graph's output window is being dragged.
		/// </summary>
		private ulong draggingWindowID = ulong.MaxValue;

		/// <summary>
		/// The window most recently clicked on.
		/// A value of "ulong.MaxValue" means no window was recently clicked on.
		/// A value of "ulong.MaxValue - 1" represents the graph's output window.
		/// </summary>
		private ulong activeWindowID = ulong.MaxValue - 1;

		/// <summary>
		/// The ID of the node whose output is currently being connected to something.
		/// Set to "ulong.MaxValue" if the user isn't connecting a node's output to something.
		/// </summary>
		private ulong reconnectingOutputID = ulong.MaxValue;
		/// <summary>
		/// The ID of the node whose input is currently being connected to something.
		/// Set to "ulong.MaxValue" if the user isn't connecting a node's input to something.
		/// Set to "ulong.MaxValue - 1" if the user is connecting a graph output.
		/// </summary>
		private ulong reconnectingInputID = ulong.MaxValue - 1;
		/// <summary>
		/// The index of the input being reconnected in the node with ID "reconnectingInputID".
		/// Set to -1 if the user isn't connecting a node's input to something.
		/// </summary>
		private int reconnectingInputIndex = -1;


		public Pane_Graph(MatEditorWindow owner)
		{
			Owner = owner;
			Graph = Owner.Graph.Clone();

			if (Owner.Owner is RT.RTMaterial)
				RootIndexToDisplayName = ((RT.RTMaterial)Owner.Owner).GetRootNodeDisplayName;
			else if (Owner.Owner is RT.RTSkyMaterial)
				RootIndexToDisplayName = ((RT.RTSkyMaterial)Owner.Owner).GetRootNodeDisplayName;
			else
				Debug.LogError("Owner of graph must be RTMaterial or RTSkyMaterial!");
		}


		public void DiscardChanges()
		{
			Graph = Owner.Graph.Clone();
		}
		public void Clear()
		{
			Graph.Clear(true);
		}

		public void DoGUI(Rect area)
		{
			HandleGUIEvents(area);

			//TODO: Implement.
		}

		/// <summary>
		/// Responds to various input events during DoGUI().
		/// </summary>
		private void HandleGUIEvents(Rect area)
		{
			Event currEvent = Event.current;
			Vector2 mousePos = currEvent.mousePosition - new Vector2(area.xMin, 0.0f),
					localMousePos = mousePos + CamOffset;

			switch (currEvent.type)
			{
				case EventType.MouseUp:
					draggingMouseDown = false;
					break;

				case EventType.MouseDrag:
					//If not dragging a window, pan the camera.
					if (draggingMouseDown && draggingWindowID == uint.MaxValue)
					{
						CamOffset -= currEvent.delta;
						Owner.Repaint();
					}
					break;

				case EventType.MouseDown:

					if (currEvent.button == 0)
					{
						//If the user is placing a node down, do that.
						if (Owner.ChooseMVPane.CurrentlyPlacing != null)
						{
							var node = Owner.ChooseMVPane.CurrentlyPlacing.NodeFactory();
							node.Pos = new Rect(localMousePos, Vector2.one);

							//Take the opportunity to verify that the node is serializable.
							if ((node.GetType().Attributes &
								 System.Reflection.TypeAttributes.Serializable) == 0)
							{
								EditorUtility.DisplayDialog("Not serializable!",
															"This node, type '" + node.GetType().Name +
																"', isn't marked with the 'Serializable' attribute! " +
																"Fix this problem in code before using this node.",
															"OK");
								node.Delete(true);
							}
							else
							{
								Graph.AddNode(node);
								//TODO: "Undo" here.
								Owner.Repaint();
							}

							Owner.ChooseMVPane.CurrentlyPlacing = null;
						}
						//Otherwise, see whether we're clicking a node or empty space.
						else
						{
							activeWindowID = ulong.MaxValue;
							foreach (MV_Base node in Graph.AllNodes)
								if (node.Pos.Contains(mousePos))//TODO: Shouldn't we use localMousePos?
									activeWindowID = node.GUID;

							if (activeWindowID == ulong.MaxValue)
							{
								EditorGUIUtility.editingTextField = false;
							}
						}
					}
					else if (currEvent.button == 1)
					{
						//If a node is currently being placed, cancel it.
						if (Owner.ChooseMVPane.CurrentlyPlacing != null)
						{
							Owner.ChooseMVPane.CurrentlyPlacing = null;
						}
						//Otherwise, see if we can drag the view.
						else
						{
							draggingMouseDown = true;
							draggingWindowID = ulong.MaxValue;

							foreach (MV_Base node in Graph.AllNodes)
							{
								if (node.Pos.Contains(mousePos))
								{
									activeWindowID = node.GUID;
									draggingWindowID = activeWindowID;
								}
							}
							if (Graph.OutputNodePos.Contains(mousePos))
							{
								draggingWindowID = ulong.MaxValue - 1;
							}
						}
					}
					break;

				case EventType.ContextClick:
					//If a node is currently being placed, cancel it.
					if (Owner.ChooseMVPane.CurrentlyPlacing != null)
						Owner.ChooseMVPane.CurrentlyPlacing = null;
					break;

				case EventType.ValidateCommand:
					//Keeping this here in case we want to react to special events later.
					if (!EditorGUIUtility.editingTextField)
					{
						//switch (evt.commandName)
						//{

						//}
					}
					break;

				case EventType.KeyDown:
					//TODO: Implement.
					break;
			}
		}
		/// <summary>
		/// Runs a node's window GUI code during DoGUI().
		/// Returns the node's new position.
		/// </summary>
		/// <param name="node">Pass "null" if doing the graph output node.</param>
		private Rect GUINode(Rect nodeRect, MV_Base node)
		{
			Color oldCol = GUI.color;
			if (node == null)
				GUI.color = new Color(0.65f, 0.65f, 0.65f);
			else
				GUI.color = node.GUIColor;

			int nodeID;
			GUI.WindowFunction nodeFunc;
			string nodeText;
			if (node == null)
			{
				GUI.color = new Color(0.65f, 0.65f, 0.65f);
				nodeID = int.MaxValue;
				nodeFunc = GUIWindow_Output;
				nodeText = "Output";
			}
			else
			{
				GUI.color = node.GUIColor;
				if (node.GUID > uint.MaxValue)
				{
					Debug.LogError("node GUID is too big! " +
								   "If you get this message, restart the editor immediately.");
				}
				nodeID = unchecked((int)(uint)node.GUID);
				nodeFunc = GUIWindow_Node;
				nodeText = node.PrettyName;
			}

			nodeRect = GUILayout.Window(nodeID, nodeRect, nodeFunc, nodeText,
										GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			GUI.color = oldCol;
			return nodeRect;
		}
		private void GUIWindow_Output(int windowID)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();

			//Buttons to connect inputs to an output.
			GUILayout.BeginVertical();
			for (int i = 0; i < Graph.RootValues.Count; ++i)
			{
				GUILayout.BeginHorizontal();

				string buttStr = "X";
				if (reconnectingInputID == ulong.MaxValue - 1 &&
					reconnectingInputIndex == i)
				{
					buttStr = "x";
				}

				if (GUILayout.Button(buttStr))
				{
					if (reconnectingOutputID != ulong.MaxValue)
					{
						Graph.ConnectInput(null, i, MV_Base.GetValue(reconnectingOutputID));

						//TODO: "Undo" here.

						reconnectingOutputID = ulong.MaxValue;
					}
					else
					{
						reconnectingInputID = ulong.MaxValue - 1;
						reconnectingInputIndex = i;
					}
				}

				if (GUILayout.Button("Disconnect"))
				{
					Graph.DisconnectInput(null, i);
				}

				GUILayout.Label(RootIndexToDisplayName(i));

				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		private void GUIWindow_Node(int windowID)
		{
			//TODO: Implement.
		}
	}
}
