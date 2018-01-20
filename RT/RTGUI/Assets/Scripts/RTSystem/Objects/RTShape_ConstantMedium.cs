using System;
using RT.Serialization;
using UnityEngine;


namespace RT
{
	[ExecuteInEditMode]
	public class RTShape_ConstantMedium : RTShape
	{
		public float Density;

		[SerializeField]
		private RTShape surface;


		public override string TypeName { get { return TypeName_ConstantMedium; } }
		public override Mesh UnityMesh { get { return (surface == null ? null : surface.UnityMesh); } }


		public override void WriteData(DataWriter writer)
		{
			base.WriteData(writer);

			writer.Float(Density, "Density");
			RTShape.Serialize(surface, "Surface", writer);
		}
		public override void ReadData(DataReader reader)
		{
			base.ReadData(reader);

			Density = reader.Float("Density");

			//Make sure a child exists to attach the shape to.
			if (transform.childCount == 0)
			{
				Transform surfaceTr = new GameObject("Surface").transform;
				surfaceTr.parent = transform;
				surfaceTr.localPosition = Vector3.zero;
				surfaceTr.localScale = Vector3.one;
				surfaceTr.localRotation = Quaternion.identity;
			}
			else
			{
				//Clear the child of any old components.
				foreach (Component c in transform.GetChild(0).GetComponents<Component>())
					if (!(c is Transform))
						Destroy(c);
			}

			surface = RTShape.Deserialize(transform.GetChild(0).gameObject,
										  reader, "Surface");
		}

		public override void Awake()
		{
			base.Awake();
			OnValidate();
		}
		public void OnValidate()
		{
			if (surface != null)
				GetComponent<MeshFilter>().sharedMesh = surface.UnityMesh;
		}
	}
}
