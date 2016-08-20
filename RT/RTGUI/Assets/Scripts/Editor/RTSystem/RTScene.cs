using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;


namespace RT
{
	public class RTScene : Serialization.ISerializableRT
	{
		public Transform Container;


		public RTScene(string containerName = null)
		{
			if (containerName != null)
			{
				Container = new GameObject(containerName).transform;
				Container.position = Vector3.zero;
				Container.rotation = Quaternion.identity;
				Container.localScale = Vector3.one;
			}
		}


		public void WriteData(Serialization.DataWriter writer)
		{
			//TODO: Implement.
		}
		public void ReadData(Serialization.DataReader reader)
		{
			//TODO: Implement.
		}
	}
}