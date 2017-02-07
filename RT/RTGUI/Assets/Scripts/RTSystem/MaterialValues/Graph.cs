using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace RT.MaterialValue
{
	//TODO: If AddNode/DeleteNode/ConnectInput/DisconnectInput get too hard to debug, try changing "ExtraNodes" to become "AllNodes", and filter out any nodes connected to the root when serializing.

	public class Graph : Serialization.ISerializableRT
	{
		public Rect OutputNodePos = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		
		public event Action<Graph, MV_Base> OnNodeAdded, OnNodeDeleted;

		public int NRoots { get { return rootValues.Count; } }
		
		/// <summary>
		/// All nodes in this graph not connected to the root.
		/// Not returned in any particular order.
		/// </summary>
		public IEnumerable<MV_Base> AllExtraNodes { get { return extraNodes; } }
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
				tempNodeCollection.Clear();
				foreach (MV_Base rootValue in rootValues)
				{
					if (rootValue == null)
						continue;

					foreach (MV_Base partOfRoot in rootValue.HierarchyDeepestFirst)
					{
						if (!tempNodeCollection.Contains(partOfRoot))
						{
							tempNodeCollection.Add(partOfRoot);
							yield return partOfRoot;
						}
					}
				}
			}
		}
		/// <summary>
		/// All nodes in this graph, starting with the ones connected to the root.
		/// You cannot assume anything about the order of these nodes except that all root-connected nodes come first.
		/// </summary>
		public IEnumerable<MV_Base> AllNodes
		{
			get
			{
				tempNodeCollection.Clear();

				foreach (MV_Base node in rootValues.Concat(extraNodes))
				{
					if (node == null)
						continue;

					foreach (MV_Base hierarchyNode in node.HierarchyRootFirst)
					{
						if (!tempNodeCollection.Contains(hierarchyNode))
						{
							tempNodeCollection.Add(hierarchyNode);
							yield return hierarchyNode;
						}
					}
				}
			}
		}

		public Dictionary<MV_Base, uint> UniqueNodeIDs { get { return nodeToID; } }
		public Dictionary<uint, MV_Base> NodesByUniqueID {  get { return idToNode; } }


		private List<MV_Base> rootValues = new List<MV_Base>();
		private HashSet<MV_Base> extraNodes = new HashSet<MV_Base>();

		private HashSet<MV_Base> tempNodeCollection = new HashSet<MV_Base>();

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
				if (nextID == nodeToID[node] + 1)
					nextID -= 1;

				idToNode.Remove(nodeToID[node]);
				nodeToID.Remove(node);
			};
		}
		public Graph(List<MV_Base> rootValues)
			: this()
		{
			rootValues = new List<MV_Base>(rootValues);
			extraNodes = new HashSet<MV_Base>();
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
			if (extraNodes.Contains(node))
			{
				UnityEngine.Assertions.Assert.IsFalse(AllConnectedNodes.Contains(node));
				return true;
			}
			else
			{
				UnityEngine.Assertions.Assert.IsTrue(AllConnectedNodes.Contains(node));
				return true;
			}
		}
		/// <summary>
		/// Gets whether the given node exists in this graph at all.
		/// </summary>
		public bool ContainsNode(MV_Base node)
		{
			return extraNodes.Contains(node) ||
				   AllConnectedNodes.Contains(node);
		}

		/// <summary>
		/// Adds a new unconnected node (plus any children) to the graph.
		/// </summary>
		public void AddNode(MV_Base node)
		{
			if (ContainsNode(node))
				return;

			//Add the node.
			extraNodes.Add(node);
			
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
			foreach (MV_Base graphNode in AllNodes)
				for (int i = 0; i < graphNode.GetNInputs(); ++i)
					if (graphNode.GetInput(i) == node)
						DisconnectInput(graphNode, i, false);

			//Disconnect the node's children.
			for (int i = 0; i < node.GetNInputs(); ++i)
				DisconnectInput(node, i, false, false);

			//Finally, delete the node.
			extraNodes.Remove(node);
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

			//If the node getting a new input is actually the root of the graph...
			if (node == null)
			{
				//If the new input was disconnected from the graph's roots, it obviously isn't now.
				foreach (MV_Base hierarchyNode in newInput.HierarchyRootFirst)
					extraNodes.Remove(hierarchyNode);

				//Set the new input.
				rootValues[inputIndex] = newInput;
			}
			//Otherwise, this is just a regular node getting a new input.
			else
			{
				//If the parent node is connected to the root, the input will be now as well.
				if (IsConnected(node))
					foreach (MV_Base hierarchyNode in newInput.HierarchyRootFirst)
						extraNodes.Remove(hierarchyNode);
			}
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

			//Check whether any nodes in the input's graph are now no longer connected to a root.
			foreach (MV_Base hierarchyNode in oldInput.HierarchyRootFirst)
				if (!extraNodes.Contains(hierarchyNode) && !IsConnected(hierarchyNode))
					extraNodes.Add(hierarchyNode);

			//If the input was an inline constant, delete it.
			if (oldInput is MV_Constant && ((MV_Constant)oldInput).IsInline)
			{
				extraNodes.Remove(oldInput);
				if (OnNodeDeleted != null)
					OnNodeDeleted(this, oldInput);
			}

			//Replace or remove the input.
			if (replaceInput)
			{
				MV_Base newInput = node.GetDefaultInput(inputIndex);
				AddNode(newInput);
				ConnectInput(node, inputIndex, newInput);
			}
			else if (deleteIndexEntirely)
			{
				node.RemoveInput(inputIndex);
			}
		}

		public void Clone(Graph destination)
		{
			//Serialize to a stream, then deserialize a new graph from that stream.

			string filePath = Path.Combine(Application.dataPath, "..\\temp.temp");

			const int maxAttempts = 10;

			for (int i = 0; i < maxAttempts; ++i)
			{
				try
				{
					if (File.Exists(filePath))
						File.Delete(filePath);

					using (var writer = new Serialization.JSONWriter(filePath))
					{
						writer.Structure(this, "data");
					}

					destination.Clear(true);
					var reader = new Serialization.JSONReader(filePath);
					reader.Structure(destination, "data");

					File.Delete(filePath);
				}
				catch (Exception e)
				{
					Debug.LogError("Unable to clone graph (will retry several times): |" + e.GetType().Name + "| " + e.Message +
									   "\n" + e.StackTrace);
				}
			}
		}
		public Graph Clone()
		{
			Graph g = new Graph();
			Clone(g);
			return g;
		}

		public void Clear(bool deleteNodes)
		{
			if (deleteNodes)
			{
				//Clear all nodes in order, starting with the deepest non-root-connected ones.
				var nodes = new List<MV_Base>(AllNodes);
				foreach (MV_Base node in MV_Base.SortDeepestFirst(nodes))
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

		public void WriteData(Serialization.DataWriter writer)
		{
			writer.Float(OutputNodePos.xMin, "outputNodePos_XMin");
			writer.Float(OutputNodePos.yMin, "outputNodePos_YMin");
			writer.Float(OutputNodePos.width, "outputNodePos_XSize");
			writer.Float(OutputNodePos.height, "outputNodePos_YSize");

			List<MV_Base> allNodes = new List<MV_Base>(AllNodes);

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
			writer.List(allNodes, "nodes",
						(wr, outVal, name) => MV_Base.Write(outVal, mvToID, name, wr));

			//Finally, write out the root values as IDs.
			List<uint> rootvalIDs = new List<uint>(rootValues.Select(mv => mvToID[mv]));
			writer.List(rootvalIDs, "rootVals", (wr, outVal, name) => wr.UInt(outVal, name));
		}
		public void ReadData(Serialization.DataReader reader)
		{
			nextID = 0;

			//Delete all nodes starting at the deepest inputs.
			foreach (MV_Base node in MV_Base.SortDeepestFirst(new List<MV_Base>(AllNodes)))
				DeleteNode(node);


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

			//Find the nodes that belong in "ExtraNodes".
			HashSet<MV_Base> connectedNodes = new HashSet<MV_Base>(AllConnectedNodes);
			extraNodes.Clear();
			foreach (MV_Base node in idLookup.Values.Where(node => !connectedNodes.Contains(node)))
				extraNodes.Add(node);

			//Raise the "add" event for all nodes starting at the deepest inputs.
			if (OnNodeAdded != null)
				foreach (MV_Base node in MV_Base.SortDeepestFirst(new List<MV_Base>(AllNodes)))
					OnNodeAdded(this, node);
		}
	}
}
