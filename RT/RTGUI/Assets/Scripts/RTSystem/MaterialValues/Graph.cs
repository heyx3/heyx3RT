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

		public List<MV_Base> RootValues { get; private set; }
		public HashSet<MV_Base> ExtraNodes { get; private set; }

		public IEnumerable<MV_Base> AllConnectedNodes
		{
			get
			{
				tempNodeCollection.Clear();
				foreach (MV_Base rootValue in RootValues)
				{
					if (rootValue == null)
						continue;

					foreach (MV_Base partOfRoot in rootValue.Hierarchy)
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
		public IEnumerable<MV_Base> AllNodes
		{
			get
			{
				tempNodeCollection.Clear();

				foreach (MV_Base rootValue in RootValues)
				{
					foreach (MV_Base partOfRoot in rootValue.Hierarchy)
					{
						if (!tempNodeCollection.Contains(partOfRoot))
						{
							tempNodeCollection.Add(partOfRoot);
							yield return partOfRoot;
						}
					}
				}

				foreach (MV_Base extraNode in ExtraNodes)
				{
					foreach (MV_Base partOfNode in extraNode.Hierarchy)
					{
						if (!tempNodeCollection.Contains(partOfNode))
						{
							tempNodeCollection.Add(partOfNode);
							yield return partOfNode;
						}
					}
				}
			}
		}
		private HashSet<MV_Base> tempNodeCollection = new HashSet<MV_Base>();


		public Graph()
		{
			RootValues = new List<MV_Base>();
			ExtraNodes = new HashSet<MV_Base>();
		}
		public Graph(List<MV_Base> rootValues)
		{
			RootValues = new List<MV_Base>(rootValues);
			ExtraNodes = new HashSet<MV_Base>();
		}


		/// <summary>
		/// Gets whether the given node is connected to one of the root nodes.
		/// </summary>
		public bool IsConnected(MV_Base node)
		{
			if (ExtraNodes.Contains(node))
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
		/// Adds a new unconnected node (plus any children) to the graph.
		/// </summary>
		public void AddNode(MV_Base node)
		{
			//Add the node.
			ExtraNodes.Add(node);
			
			//Run "AddNode()" on each of its children.
			for (int i = 0; i < node.GetNInputs(); ++i)
				AddNode(node.GetInput(i));
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
			ExtraNodes.Remove(node);
			node.Delete(false);
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
			//If the node has variable numbers of children
			//    and this input index doesn't yet exist, add it.
			if (node != null && inputIndex == node.GetNInputs())
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
				foreach (MV_Base hierarchyNode in newInput.Hierarchy)
					ExtraNodes.Remove(hierarchyNode);

				//Set the new input.
				RootValues[inputIndex] = newInput;
			}
			//Otherwise, this is just a regular node getting a new input.
			else
			{
				//If the parent node is connected to the root, the input will be now as well.
				if (IsConnected(node))
					foreach (MV_Base hierarchyNode in newInput.Hierarchy)
						ExtraNodes.Remove(hierarchyNode);
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
							        RootValues[inputIndex] :
									node.GetInput(inputIndex));

			//If we're in the middle of a higher DisconnectInput() operation,
			//    "oldInput" could be null.
			if (oldInput == null)
				return;

			//Nullify the input reference.
			if (node == null)
				RootValues[inputIndex] = null;
			else
				node.ChangeInput(inputIndex, null);

			//Check whether any nodes in the input's graph are now no longer connected to a root.
			foreach (MV_Base hierarchyNode in oldInput.Hierarchy)
				if (!ExtraNodes.Contains(hierarchyNode) && !IsConnected(hierarchyNode))
					ExtraNodes.Add(hierarchyNode);

			//If the input was an inline constant, delete it.
			if (oldInput is MV_Constant && ((MV_Constant)oldInput).IsInline)
			{
				oldInput.Delete(false);
				ExtraNodes.Remove(oldInput);
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

		public Graph Clone()
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

					Graph g = new Graph();
					var reader = new Serialization.JSONReader(filePath);
					reader.Structure(this, "data");

					File.Delete(filePath);

					return g;
				}
				catch (Exception e)
				{
					Debug.LogError("Unable to clone graph (will retry several times): |" + e.GetType().Name + "| " + e.Message +
									   "\n" + e.StackTrace);
				}
			}

			return null;
		}
		public void Clear(bool deleteNodes)
		{
			if (deleteNodes)
			{
				foreach (MV_Base node in AllNodes)
					node.Delete(true);
			}

			ExtraNodes.Clear();
			for (int i = 0; i < RootValues.Count; ++i)
				RootValues[i] = MV_Constant.MakeFloat(1.0f);
		}

		public void WriteData(Serialization.DataWriter writer)
		{
			writer.Float(OutputNodePos.xMin, "OutputNodePos_XMin");
			writer.Float(OutputNodePos.yMin, "OutputNodePos_YMin");
			writer.Float(OutputNodePos.width, "OutputNodePos_XSize");
			writer.Float(OutputNodePos.height, "OutputNodePos_YSize");


			//Give every node a unique ID.

			Dictionary<MV_Base, uint> mvToID = new Dictionary<MV_Base, uint>();
			uint nextID = uint.MinValue;
			uint nNodes = 0;

			Stack<MV_Base> toInvestigate = new Stack<MV_Base>(RootValues.Concat(ExtraNodes));
			while (toInvestigate.Count > 0)
			{
				var mv = toInvestigate.Pop();
				nNodes += 1;

				if (mvToID.ContainsKey(mv))
					continue;

				mvToID.Add(mv, nextID);
				nextID += 1;

				for (int i = 0; i < mv.GetNInputs(); ++i)
					toInvestigate.Push(mv.GetInput(i));
			}


			//Write the nodes in order, from highest tree depth to lowest.

			writer.UInt(nNodes, "NumbNodes");
			
			List<MV_Base> toWrite = new List<MV_Base>();
			Dictionary<MV_Base, bool> processedChildrenYet = new Dictionary<MV_Base, bool>();
			foreach (MV_Base node in RootValues.Concat(ExtraNodes))
			{
				toWrite.Add(node);
				processedChildrenYet.Add(node, false);
			}

			uint count = 0;
			while (toWrite.Count > 0)
			{
				var mv = toWrite[toWrite.Count - 1];

				//If this node has already been processed, write it out and pop it off the stack.
				if (processedChildrenYet[mv])
				{
					toWrite.RemoveAt(toWrite.Count - 1);
					MV_Base.Write(mv, mvToID, count.ToString(), writer);
					count += 1;
				}
				//Otherwise, queue up its children to be processed first.
				else
				{
					processedChildrenYet[mv] = true;
					for (int i = 0; i < mv.GetNInputs(); ++i)
					{
						var childMV = mv.GetInput(i);

						//Add this child to the front of the stack.
						int stackPos = toWrite.IndexOf(childMV);
						bool isProcessedAlready = (processedChildrenYet.ContainsKey(childMV) &&
												   processedChildrenYet[childMV]);
						if (stackPos == -1)
						{
							if (isProcessedAlready)
							{
								//We already wrote the node out.
								continue;
							}
							else
							{
								//We haven't seen this node yet.
								toWrite.Add(childMV);
								processedChildrenYet.Add(childMV, false);
							}
						}
						else
						{
							//The node has already been seen, but not processed,
							//    so move it to the top of the stack.
							toWrite.RemoveAt(stackPos);
							toWrite.Add(childMV);
						}
					}
				}
			}


			//Finally, write out the root values as IDs.

			List<uint> rootvalIDs = new List<uint>(RootValues.Select(mv => mvToID[mv]));
			writer.List(rootvalIDs, "rootVals", (wr, outVal, name) => wr.UInt(outVal, name));
		}
		public void ReadData(Serialization.DataReader reader)
		{
			OutputNodePos = new Rect(reader.Float("OutputNodePos_XMin"),
									 reader.Float("OutputNodePos_YMin"),
									 reader.Float("OutputNodePos_XSize"),
									 reader.Float("OutputNodePos_YSize"));


			Dictionary<MV_Base, List<uint>> childIDs = new Dictionary<MV_Base, List<uint>>();
			Dictionary<uint, MV_Base> idLookup = new Dictionary<uint, MV_Base>();

			//Read each node.
			uint nNodes = reader.UInt("NumbNodes");
			for (uint i = 0; i < nNodes; ++i)
			{
				uint id;
				MV_Base mv = MV_Base.Read(childIDs, i.ToString(), reader, out id);
				idLookup.Add(id, mv);
			}

			//Finalize each node.
			foreach (MV_Base mv in childIDs.Keys)
				mv.OnDoneReadingData(idLookup, childIDs);

			//Read in the root values as IDs.
			List<uint> rootValIDs = reader.List("rootVals",
												(Serialization.DataReader rd, ref uint val, string name) =>
												{
													val = rd.UInt(name);
												});
			RootValues = new List<MV_Base>(rootValIDs.Select(id => idLookup[id]));


			//Figure out which nodes are extra/unused with a depth-first search.

			HashSet<uint> usedNodes = new HashSet<uint>();
			Stack<KeyValuePair<MV_Base, uint>> toProcess = new Stack<KeyValuePair<MV_Base, uint>>();
			HashSet<MV_Base> processedAlready = new HashSet<MV_Base>();
			foreach (uint rootValueID in rootValIDs)
			{
				usedNodes.Add(rootValueID);
				toProcess.Push(new KeyValuePair<MV_Base, uint>(idLookup[rootValueID], rootValueID));
			}

			while (toProcess.Count > 0)
			{
				var mvAndID = toProcess.Pop();
				for (int i = 0; i < mvAndID.Key.GetNInputs(); ++i)
				{
					var inputMV = mvAndID.Key.GetInput(i);
					if (!processedAlready.Contains(inputMV))
					{
						uint inputMVID = idLookup.First(kvp => kvp.Value == inputMV).Key;
						toProcess.Push(new KeyValuePair<MV_Base, uint>(inputMV, inputMVID));
						processedAlready.Add(inputMV);
						usedNodes.Add(inputMVID);
					}
				}
			}

			ExtraNodes.Clear();
			foreach (var idAndNode in idLookup)
			{
				if (!usedNodes.Contains(idAndNode.Key))
					ExtraNodes.Add(idAndNode.Value);
			}
		}
	}
}
