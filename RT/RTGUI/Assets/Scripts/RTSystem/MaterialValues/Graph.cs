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
		public List<MV_Base> RootValues { get; private set; }

		public Graph() { RootValues = new List<MV_Base>(); }
		public Graph(List<MV_Base> rootValues)
		{
			RootValues = new List<MV_Base>(rootValues);
		}

		public void WriteData(Serialization.DataWriter writer)
		{
			//Give every node a unique ID.

			Dictionary<MV_Base, uint> mvToID = new Dictionary<MV_Base, uint>();
			uint nextID = uint.MinValue;
			uint nNodes = 0;

			Stack<MV_Base> toInvestigate = new Stack<MV_Base>(RootValues);
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
			for (int i = 0; i < RootValues.Count; ++i)
			{
				toWrite.Add(RootValues[i]);
				processedChildrenYet.Add(RootValues[i], false);
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
		}
	}
}
