using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using MV_Base = RT.MaterialValue.MV_Base;
using MV_Constant = RT.MaterialValue.MV_Constant;


namespace RT.MatEditor
{
	//TODO: Bug when deleting input: parent node doesn't disconnect (maybe just graph output?)
	//TODO: Whenever something happens (i.e. Undo stack is modified), set all the nodes' sizes to 0.1 to force them to be recomputed.

	public class Pane_Graph
	{
		public MatEditorWindow Owner { get; private set; }
		public RT.MaterialValue.Graph Graph { get; private set; }
		public Func<int, string> RootIndexToDisplayName { get; private set; }


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
		/// A value of "uint.MaxValue" means no window is being dragged.
		/// A value of "uint.MaxValue - 1" means the graph's output window is being dragged.
		/// </summary>
		private uint draggingWindowID = uint.MaxValue;

		/// <summary>
		/// The window most recently clicked on.
		/// A value of "uint.MaxValue" means no window was recently clicked on.
		/// A value of "uint.MaxValue - 1" represents the graph's output window.
		/// </summary>
		private uint activeWindowID = uint.MaxValue - 1;

		/// <summary>
		/// The ID of the node whose output is currently being connected to something.
		/// Set to "uint.MaxValue" if the user isn't connecting a node's output to something.
		/// </summary>
		private uint reconnectingOutputID = uint.MaxValue;
		/// <summary>
		/// The ID of the node whose input is currently being connected to something.
		/// Set to "uint.MaxValue" if the user isn't connecting a node's input to something.
		/// Set to "uint.MaxValue - 1" if the user is connecting a graph output.
		/// </summary>
		private uint reconnectingInputID = uint.MaxValue;
		/// <summary>
		/// The index of the input being reconnected in the node with ID "reconnectingInputID".
		/// Set to -1 if the user isn't connecting a node's input to something.
		/// </summary>
		private int reconnectingInputIndex = -1;


		public Pane_Graph(MatEditorWindow owner)
		{
			Owner = owner;
			Graph = Owner.Owner.Graph.Clone();

			CamOffset = -Owner.position.center - Graph.OutputNodePos.position;

			RootIndexToDisplayName = Owner.Owner.GetRootNodeDisplayName;
		}


		public void DiscardChanges()
		{
			Graph = Owner.Owner.Graph.Clone();
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
				//Don't give inline constants their own window.
				if (node is MV_Constant && ((MV_Constant)node).IsInline)
					continue;

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

                            Graph.AddNode(node);

                            //TODO: "Undo" here.

                            Owner.Repaint();

                            Owner.ChooseMVPane.CurrentlyPlacing = null;
                        }
                        //Otherwise, see whether we're clicking a node or empty space.
                        else
                        {
                            activeWindowID = uint.MaxValue;
                            foreach (MV_Base node in Graph.AllNodes)
                                if (node.Pos.Contains(mousePos))//TODO: Shouldn't we use localMousePos?
                                    activeWindowID = Graph.UniqueNodeIDs[node];

                            if (activeWindowID == uint.MaxValue)
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
							draggingWindowID = uint.MaxValue;

							foreach (MV_Base node in Graph.AllNodes)
							{
								if (node.Pos.Contains(mousePos))
								{
									activeWindowID = Graph.UniqueNodeIDs[node];
									draggingWindowID = activeWindowID;
								}
							}
							if (Graph.OutputNodePos.Contains(mousePos))
							{
								draggingWindowID = uint.MaxValue - 1;
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
				if (!Graph.UniqueNodeIDs.ContainsKey(node))
					return nodeRect;

				GUI.color = node.GUIColor;
				if (Graph.UniqueNodeIDs[node] > uint.MaxValue)
				{
					Debug.LogError("node GUID is too big! " +
								   "If you get this message, restart the editor immediately.");
				}
				nodeID = unchecked((int)Graph.UniqueNodeIDs[node]);
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
			
			GUILayout.BeginVertical();
			for (int i = 0; i < Graph.NRoots; ++i)
				GUIWindowInput(null, i, false);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUI.DragWindow();
		}
		private void GUIWindow_Node(int windowID)
		{
			MV_Base node = Graph.NodesByUniqueID[unchecked((uint)windowID)];

			//The overall layout of the window is vertical.
			GUILayout.BeginVertical();

			//Inputs should be across from the output.
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();

			//Show the inputs.
			for (int i = 0; i < node.GetNInputs(); ++i)
			{
				if (GUIWindowInput(node, i, node.HasVariableNumberOfChildren))
					i -= 1;
			}
			//A button to add a new input.
			GUILayout.BeginHorizontal();
			if (node.HasVariableNumberOfChildren && GUILayout.Button("Add input"))
			{
				//TODO: "Undo" here.
				Graph.ConnectInput(node, node.GetNInputs(), node.GetDefaultInput(node.GetNInputs()));
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			//Show a button for connecting the node's output.
			string buttonStr = (reconnectingOutputID == Graph.UniqueNodeIDs[node] ? "o" : "O");
			if (GUILayout.Button(buttonStr))
			{
				if (reconnectingInputID == uint.MaxValue)
				{
					reconnectingOutputID = Graph.UniqueNodeIDs[node];
				}
				else
				{
					MV_Base rootNode = (reconnectingInputID == uint.MaxValue - 1 ?
											null :
											Graph.NodesByUniqueID[reconnectingInputID]);
					Graph.ConnectInput(rootNode, reconnectingInputIndex, node);
				}
			}

			GUILayout.EndHorizontal();
			
			//Custom GUI.
			MV_Base.GUIResults subResult = node.DoCustomGUI();
			if (subResult != MaterialValue.MV_Base.GUIResults.Nothing)
			{
				//TODO: "Undo" here.
			}
			
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
			
			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		/// <summary>
		/// Displays a node's input for a GUI window.
		/// Returns whether the input was deleted.
		/// </summary>
		private bool GUIWindowInput(MV_Base node, int inputIndex, bool allowDeletion)
		{
			GUILayout.BeginHorizontal();

			//Get some data about this node.
			MV_Base inputNode = (node == null ?
									 Graph.GetRootNode(inputIndex) :
									 node.GetInput(inputIndex));
			uint nodeID = (node == null ?
							   uint.MaxValue - 1 :
							   Graph.UniqueNodeIDs[node]);
			Rect nodePos = (node == null ?
								Graph.OutputNodePos :
								node.Pos);

			//The input's name.
			GUILayout.Label(node == null ?
								RootIndexToDisplayName(inputIndex) :
								node.GetInputName(inputIndex));

			//Button to select input.
			string buttStr = "O";
			if (reconnectingInputID == nodeID && reconnectingInputIndex == inputIndex)
			{
				buttStr = "o";
			}
			if (GUILayout.Button(buttStr))
			{
				if (reconnectingOutputID != uint.MaxValue)
				{
					//TODO: "Undo" here.
					Graph.ConnectInput(node, inputIndex,
									   Graph.NodesByUniqueID[reconnectingOutputID]);
					reconnectingOutputID = uint.MaxValue;
				}
				else
				{
					reconnectingInputID = nodeID;
					reconnectingInputIndex = inputIndex;
				}
			}

			GUILayout.FlexibleSpace();

			//If this input is a constant, expose a little inline GUI to edit it more easily.
			var constInput = inputNode as MV_Constant;
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
				Rect otherPos = inputNode.Pos;
				Vector2 endPos = new Vector2(otherPos.xMax, otherPos.yMin + OutputHeight) -
								   nodePos.min;
				MyGUI.DrawLine(new Vector2(0.0f, TitleBarHeight + (inputIndex * InputSpacing)),
							   endPos, 2.0f, Color.white);

				if (GUILayout.Button("Disconnect"))
				{
					//TODO: "Undo" here.
					var defaultInput = (node == null ?
											MV_Constant.MakeFloat(1.0f, true, 0.0f, 1.0f,
																  MaterialValue.OutputSizes.All,
																  true) :
											node.GetDefaultInput(inputIndex));
					Graph.ConnectInput(node, inputIndex, defaultInput);
				}
			}

			if (allowDeletion)
			{
				if (GUILayout.Button("X"))
				{
					//TODO: "Undo" here.
					Graph.DisconnectInput(node, inputIndex, true);
					return true;
				}
			}

			GUILayout.EndHorizontal();

			return false;
		}
	}
}
