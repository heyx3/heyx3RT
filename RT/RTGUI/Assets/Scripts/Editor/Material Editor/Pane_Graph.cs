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

		//public event Action<Vector2> OnClickEmptySpace;

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

			CamOffset = Owner.position.center - Graph.OutputNodePos.position;

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

            GUILayout.BeginArea(area);
            Owner.BeginWindows();
            

            //Do each node's GUI window.
            foreach (MV_Base node in Graph.AllNodes)
            {
                Rect oldPos = new Rect(node.Pos.position - CamOffset,
                                       node.Pos.size);
                Rect newPos = GUINode(oldPos, node);

                if (Mathf.Abs(oldPos.x - newPos.x) >= 2.0f ||
                        Mathf.Abs(oldPos.y - newPos.y) >= 2.0f)
                {
                    //TODO: "Undo" here.
                }

                newPos.position += CamOffset;
                node.Pos = newPos;
            }

            //Also do a GUI window for the graph roots.
            Rect oldRootPos = new Rect(Graph.OutputNodePos.position - CamOffset,
                                       Graph.OutputNodePos.size);
            Rect newRootPos = GUINode(oldRootPos, null);
            if (Mathf.Abs(oldRootPos.x - newRootPos.x) >= 2.0f ||
                    Mathf.Abs(oldRootPos.y - newRootPos.y) >= 2.0f)
            {
                //TODO: "Undo" here.
            }
            newRootPos.position += CamOffset;
            Graph.OutputNodePos = newRootPos;


            Owner.EndWindows();
            GUILayout.EndArea();
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
					if (draggingMouseDown && draggingWindowID == ulong.MaxValue)
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
					//TODO: "Undo" here.
					Graph.DisconnectInput(null, i, false);
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
			//TODO: I feel like these horizontal GUILayout sections are too heavily-used.

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();

			MV_Base node = MV_Base.GetValue(unchecked((uint)(ulong)windowID));
			
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			for (int i = 0; i < node.GetNInputs(); ++i)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(node.GetInputName(i));

				//Button to select input.
				string buttStr = "O";
				if (reconnectingInputID == node.GUID && reconnectingInputIndex == i)
					buttStr = "o";
				if (GUILayout.Button(buttStr))
				{
					if (reconnectingOutputID != ulong.MaxValue)
					{
						//TODO: "Undo" here.
						Graph.ConnectInput(node, i, MV_Base.GetValue(reconnectingOutputID));
						reconnectingOutputID = ulong.MaxValue;
					}
					else
					{
						reconnectingInputID = node.GUID;
						reconnectingInputIndex = i;
					} 
				}

				//If this input is a constant, expose a little inline GUI to edit it more easily.
				var constInput = node.GetInput(i) as MV_Constant;
				if (constInput != null && constInput.IsInline)
				{
					if (constInput.ValueEditor.DoGUI())
					{
						//TODO: "Undo" here.
					}
				}
				//Otherwise, draw a line to it and expose a button to release the connection.
				else
				{
					const float OutputHeight = 30.0f,
								TitleBarHeight = 30.0f,
								InputSpacing = 20.0f;
					Rect otherPos = node.GetInput(i).Pos;
					Vector2 endPos = new Vector2(otherPos.xMax, otherPos.yMin + OutputHeight) -
									   node.Pos.min;
					MyGUI.DrawLine(new Vector2(0.0f, TitleBarHeight + (i * InputSpacing)),
								   endPos, 2.0f, Color.white);

					if (GUILayout.Button("Disconnect"))
					{
						//TODO: "Undo" here.
						Graph.ConnectInput(node, i, node.GetDefaultInput(i));
					}
				}

				//A button to remove this input.
				if (node.HasVariableNumberOfChildren)
				{
					if (GUILayout.Button("X"))
					{
						//TODO: "Undo" here.
						Graph.DisconnectInput(node, i, true);
						i -= 1;
					}
				}

				GUILayout.EndHorizontal();
			}

			//A button to add a new input.
			if (node.HasVariableNumberOfChildren && GUILayout.Button("Add input"))
			{
				//TODO: "Undo" here.
				Graph.ConnectInput(node, node.GetNInputs(), node.GetDefaultInput(node.GetNInputs()));
			}
			
			//Custom GUI.
			MV_Base.GUIResults subResult = node.DoCustomGUI();
			if (subResult != MaterialValue.MV_Base.GUIResults.Nothing)
			{
				//TODO: "Undo" here.
			}
			
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();

			//A button for connecting the node's output.
			string buttonStr = (reconnectingOutputID == node.GUID ? "o" : "O");
			if (GUILayout.Button(buttonStr))
			{
				if (reconnectingInputID == ulong.MaxValue)
				{
					reconnectingOutputID = node.GUID;
				}
				else
				{
					Graph.ConnectInput(MV_Base.GetValue(reconnectingInputID),
									   reconnectingInputIndex,
									   node);
				}
			}

			GUILayout.EndHorizontal();


			//"Duplicate" and "Delete" buttons.
			//TODO: A "Duplicate" button basically requires an abstract "Clone()" method on MV_Base.
			GUILayout.BeginHorizontal();
			/*if (GUILayout.Button("Duplicate"))
			{
				Rect newPos = ;
				Graph.AddNode()
			}*/
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Delete"))
			{
				//TODO: "Undo" here.
				Graph.DeleteNode(node);
			}
			GUILayout.EndHorizontal();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}
}
