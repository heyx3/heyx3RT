using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	public class Graph : Serialization.ISerializableRT
	{
		public Rect OutputNodePos = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		
		public event Action<Graph, MV_Base> OnNodeAdded, OnNodeDeleted;

		public int NRoots { get { return rootValues.Count; } }
		
		/// <summary>
		/// All nodes in this graph not connected to the root.
		/// Not returned in any particular order.
		/// </summary>
		public IEnumerable<MV_Base> AllExtraNodes { get { var connectedNodes = new HashSet<MV_Base>(AllConnectedNodes); return allNodes.Where(n => !connectedNodes.Contains(n)); } }
		/// <summary>
		/// The roots of this graph, in the order they were added as a root.
		/// </summary>
		public IEnumerable<MV_Base> AllRootNodes { get { return rootValues; } }

		/// <summary>
		/// All nodes connected to this graph's roots, including the roots themselves.
		/// The order of the nodes is such that the deepest nodes in the graph always come first --
		///     you will never see a parent before any of its inputs.
		/// </summary>
		public IEnumerable<MV_Base> AllConnectedNodes
		{
			get
			{
                var nodes = new HashSet<MV_Base>();
				foreach (MV_Base rootValue in rootValues)
				{
					if (rootValue == null)
						continue;

					foreach (MV_Base partOfRoot in rootValue.HierarchyDeepestFirst)
					{
						if (!nodes.Contains(partOfRoot))
						{
							nodes.Add(partOfRoot);
							yield return partOfRoot;
						}
					}
				}
			}
		}
		/// <summary>
		/// All nodes in this graph, in the order they were added to the graph.
		/// </summary>
		public IEnumerable<MV_Base> AllNodes { get { List<MV_Base> _allNodes = new List<MV_Base>(allNodes.Count); _allNodes.AddRange(allNodes); return _allNodes; } }

		public Dictionary<MV_Base, uint> UniqueNodeIDs { get { return nodeToID; } }
		public Dictionary<uint, MV_Base> NodesByUniqueID {  get { return idToNode; } }
        
		private List<MV_Base> rootValues = new List<MV_Base>();
        private HashSet<MV_Base> allNodes = new HashSet<MV_Base>();
       
        private Dictionary<MV_Base, uint> nodeToID = new Dictionary<MV_Base, uint>();
        private Dictionary<uint, MV_Base> idToNode = new Dictionary<uint, MV_Base>();
        private uint nextID = 0;


		public Graph()
		{
			OnNodeAdded += (_this, node) =>
			{
				nodeToID.Add(node, nextID);
				idToNode.Add(nextID, node);
				nextID += 1;
			};
			OnNodeDeleted += (_this, node) =>
			{
				if (!nodeToID.ContainsKey(node))
					return;

				if (nextID == nodeToID[node] + 1)
					nextID -= 1;

				idToNode.Remove(nodeToID[node]);
				nodeToID.Remove(node);
			};
		}
		public Graph(List<MV_Base> _rootValues)
			: this()
		{
			for (int i = 0; i < _rootValues.Count; ++i)
			{
				AddNode(_rootValues[i]);
				ConnectInput(null, i, _rootValues[i]);
			}
		}


		/// <summary>
		/// Gets the root node at the given index.
		/// </summary>
		public MV_Base GetRootNode(int index) { return rootValues[index]; }

		/// <summary>
		/// Gets whether the given node is connected to one of the root nodes.
		/// </summary>
		public bool IsConnected(MV_Base node)
		{
			return AllConnectedNodes.Contains(node);
		}
		/// <summary>
		/// Gets whether the given node exists in this graph at all.
		/// </summary>
		public bool ContainsNode(MV_Base node)
		{
            return allNodes.Contains(node);
		}

		/// <summary>
		/// Adds a new node (plus any children) to this graph.
		/// </summary>
		public void AddNode(MV_Base node)
		{
			if (ContainsNode(node))
				return;

			//Add the node.
			allNodes.Add(node);
			
			//Run "AddNode()" on each of its children.
			for (int i = 0; i < node.GetNInputs(); ++i)
				AddNode(node.GetInput(i));

			if (OnNodeAdded != null)
				OnNodeAdded(this, node);
		}

		/// <summary>
		/// Properly removes the given node from the graph.
		/// </summary>
		public void DeleteNode(MV_Base node)
		{
			//Remove all references to the node by other nodes.
			for (int i = 0; i < rootValues.Count; ++i)
				if (rootValues[i] == node)
					DisconnectInput(null, i, false);
			foreach (MV_Base graphNode in AllNodes)
				for (int i = 0; i < graphNode.GetNInputs(); ++i)
					if (graphNode.GetInput(i) == node)
						DisconnectInput(graphNode, i, false);

			//Disconnect the node's children.
			for (int i = 0; i < node.GetNInputs(); ++i)
				DisconnectInput(node, i, false, false);

			//Finally, delete the node.
			allNodes.Remove(node);
			if (OnNodeDeleted != null)
				OnNodeDeleted(this, node);
		}
		/// <summary>
		/// Changes a node's input.
		/// </summary>
		/// <param name="node">Pass "null" if changing one of this graph's roots.</param>
		/// <param name="inputIndex">
		/// The index of the input on the node.
		/// If the index is equal to the current number of nodes,
		///     and the node has a variable number of children, then a new input index will be added.
		/// </param>
		/// <param name="newInput">The new input.</param>
		public void ConnectInput(MV_Base node, int inputIndex, MV_Base newInput)
		{
            //Make sure infinite loops can't happen.
            if (newInput.HierarchyRootFirst.Contains(node))
            {
                Debug.LogError("Can't create a loop in the graph!");
                return;
            }
             
			//If the node has variable numbers of children
			//    and this input index doesn't yet exist, add it.
			//Note that the graph outputs can always be added to.
			if (node == null && inputIndex == rootValues.Count)
			{
				rootValues.Add(newInput);
			}
			else if (node != null && inputIndex == node.GetNInputs())
			{
				UnityEngine.Assertions.Assert.IsTrue(node.HasVariableNumberOfChildren);
				node.AddInput(newInput);
			}
			//Otherwise, remove the current input at that index.
			else
			{
				DisconnectInput(node, inputIndex, false, false);
			}
			
			//Set the new input.
			if (node == null)
				rootValues[inputIndex] = newInput;
			else
                node.ChangeInput(inputIndex, newInput);
		}
		/// <summary>
		/// Removes a node's input.
		/// Either replaces it with an inline MV_Constant instance, or
		///     (assuming the node can have a variable number of children) deletes it entirely.
		/// </summary>
		/// <param name="deleteIndexEntirely">
		/// If true, assuming this node can have a variable number of inputs,
		///     the input index will be completely deleted from the node.
		/// If false, then the input will simply be replaced.
		/// </param>
		public void DisconnectInput(MV_Base node, int inputIndex, bool deleteIndexEntirely)
		{
			DisconnectInput(node, inputIndex, deleteIndexEntirely, true);
		}
		private void DisconnectInput(MV_Base node, int inputIndex, bool deleteIndexEntirely, bool replaceInput)
		{
			MV_Base oldInput = (node == null ?
							        rootValues[inputIndex] :
									node.GetInput(inputIndex));

			//If we're in the middle of a higher DisconnectInput() operation,
			//    "oldInput" could be null.
			if (oldInput == null)
				return;

			//Nullify the input reference.
			if (node == null)
				rootValues[inputIndex] = null;
			else
				node.ChangeInput(inputIndex, null);
			
			//If the input was an inline constant, delete it.
			if (oldInput is MV_Constant && ((MV_Constant)oldInput).IsInline)
			{
				allNodes.Remove(oldInput);
				if (OnNodeDeleted != null)
					OnNodeDeleted(this, oldInput);
			}

			//Replace or remove the input. 
			if (deleteIndexEntirely)
			{
				node.RemoveInput(inputIndex);
			}
			else if (replaceInput)
			{
				MV_Base newInput = (node == null ?
										MV_Constant.MakeFloat(1.0f, false, 0.0f, 1.0f,
															  OutputSizes.All, true) :
										node.GetDefaultInput(inputIndex));
				AddNode(newInput);
				ConnectInput(node, inputIndex, newInput);
			}
		}

		public void Clone(Graph destination)
		{
			//Serialize to a string, then deserialize a new graph from that string.

			try
			{
				StringBuilder json = new StringBuilder();
				using (var writer = new Serialization.JSONWriter(new StringWriter(json)))
				{
					writer.Structure(this, "data");
				}

				var reader = new Serialization.JSONReader(new StringReader(json.ToString()));
				reader.Structure(destination, "data");
			}
			catch (Serialization.DataReader.ReadException e)
			{
				Debug.LogError("Error serializing graph: " + e.Message + "\n" + e.StackTrace);
				destination.Clear(true);
			}
			catch (Serialization.DataWriter.WriteException e)
			{
				Debug.LogError("Error deserializing graph: " + e.Message + "\n" + e.StackTrace);
				destination.Clear(true);
			}
		}
		public Graph Clone()
		{
			Graph g = new Graph();
			Clone(g);
			return g;
		}

		/// <summary>
		/// Reset the root values of this graph, and optionally delete every node currently in it.
		/// </summary>
		public void Clear(bool deleteNodes)
		{
			if (deleteNodes)
			{
				//Clear all nodes in order, starting with the deepest non-root-connected ones.
				foreach (MV_Base node in MV_Base.SortDeepestFirst(new HashSet<MV_Base>(allNodes)))
					DeleteNode(node);
			}

			//Add some default inline constants to the output.
			for (int i = 0; i < rootValues.Count; ++i)
			{
				MV_Base val = MV_Constant.MakeFloat(1.0f, false, 0.0f, 1.0f,
													OutputSizes.One, true);
				AddNode(val);
				ConnectInput(null, i, val);
			}
		}
		/// <summary>
		/// A less "nice" version of "Clear()" for fixing bugs like null references in the graph.
		/// Note that this version doesn't raise the "OnNodeDeleted" event for any nodes.
		/// </summary>
		public void Wipe()
		{
			allNodes.Clear();

			//Add some default inline constants to the output.
			for (int i = 0; i < rootValues.Count; ++i)
			{
				MV_Base val = MV_Constant.MakeFloat(1.0f, false, 0.0f, 1.0f,
													OutputSizes.One, true);
				AddNode(val);
				ConnectInput(null, i, val);
			}
		}

		public void WriteData(Serialization.DataWriter writer)
		{
			writer.Float(OutputNodePos.xMin, "outputNodePos_XMin");
			writer.Float(OutputNodePos.yMin, "outputNodePos_YMin");
			writer.Float(OutputNodePos.width, "outputNodePos_XSize");
			writer.Float(OutputNodePos.height, "outputNodePos_YSize");
            
			//Give every node a unique ID.
			Dictionary<MV_Base, uint> mvToID = new Dictionary<MV_Base, uint>();
			uint nextID = uint.MinValue;
			uint nNodes = 0;
			foreach (MV_Base node in allNodes)
			{
				mvToID.Add(node, nextID);
				nextID += 1;
				nNodes += 1;
			}

			//Write the nodes.
			var allNodesList = new List<MV_Base>(allNodes);
			writer.List(allNodesList, "nodes",
						(wr, outVal, name) => MV_Base.Write(outVal, mvToID, name, wr));

			//Finally, write out the root values as IDs.
			List<uint> rootvalIDs = new List<uint>(rootValues.Select(mv => mvToID[mv]));
			writer.List(rootvalIDs, "rootVals", (wr, outVal, name) => wr.UInt(outVal, name));
		}
		public void ReadData(Serialization.DataReader reader)
		{
			//Delete all nodes starting at the deepest inputs.
			var allNodesList = AllNodes.ToList();
			foreach (MV_Base node in MV_Base.SortDeepestFirst(allNodesList))
				DeleteNode(node);

			allNodes.Clear();
			idToNode.Clear();
			nodeToID.Clear();
			nextID = 0;

			OutputNodePos = new Rect(reader.Float("outputNodePos_XMin"),
									 reader.Float("outputNodePos_YMin"),
									 reader.Float("outputNodePos_XSize"),
									 reader.Float("outputNodePos_YSize"));


			Dictionary<MV_Base, List<uint>> childIDs = new Dictionary<MV_Base, List<uint>>();
			Dictionary<uint, MV_Base> idLookup = new Dictionary<uint, MV_Base>();

			//Read each node.
			List<MV_Base> nodes =
				reader.List("nodes",
							(Serialization.DataReader rd, ref MV_Base val, string name) =>
							{
								uint id;
								val = MV_Base.Read(childIDs, name, rd, out id);
								idLookup.Add(id, val);
							});

			//Finalize each node.
			foreach (MV_Base node in nodes)
				node.OnDoneReadingData(idLookup, childIDs);

			//Read in the root values as IDs.
			List<uint> rootValIDs = reader.List("rootVals",
												(Serialization.DataReader rd, ref uint val, string name) =>
												{
													val = rd.UInt(name);
												});
			rootValues = new List<MV_Base>(rootValIDs.Select(id => idLookup[id]));

			//Raise the "add" event for all nodes starting at the deepest inputs.
			foreach (MV_Base node in MV_Base.SortDeepestFirst(nodes))
				AddNode(node);
		}
	}
}
