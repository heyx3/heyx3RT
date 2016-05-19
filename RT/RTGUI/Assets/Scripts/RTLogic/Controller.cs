using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RTLogic
{
	public class Controller : MonoBehaviour
	{
		public static Controller Instance
		{
			get { if (cntrlr == null) cntrlr = FindObjectOfType<Controller>(); return cntrlr; }
		}
		private static Controller cntrlr = null;

		
		public RT.Tracer Scene { get; private set; }

		public float SpawnForwardDist = 5.0f;

		public Camera Cam;
		public UnityEngine.UI.Button Button_CreateSphere, Button_CreatePlane;


		private int shapeIndex = -1;
		private RT.RTShape selectedShape;
		private RT.RTMat selectedMat;
		private Rect shapeWindowPos;


		public void OnShapeClicked(RT.RTShape shpe)
		{
			Scene = GetComponent<RT.Tracer>();

			Button_CreateSphere.enabled = false;
			Button_CreatePlane.enabled = false;

			shapeIndex = Scene.Objects.IndexOf(shpe.gameObject);
			shapeWindowPos = new Rect();
			selectedShape = shpe;
			selectedMat = shpe.GetComponent<RT.RTMat>();
		}

		private RT.RTShape CreateObj(Type shapeType, Type matType)
		{
			Transform tr = Cam.transform;
			GameObject go = RT.Tracer.CreateObj(tr.position + (tr.forward * SpawnForwardDist),
												Vector3.one, Quaternion.identity);
			RT.RTShape shpe = (RT.RTShape)go.AddComponent(shapeType);
			go.AddComponent(matType);

			Scene.Objects.Add(go);

			return shpe;
		}

		private void Awake()
		{
			Scene = GetComponent<RT.Tracer>();

			Button_CreateSphere.onClick.AddListener(() =>
				{
					CreateObj(typeof(RT.RTShape_Sphere), typeof(RT.RTMat_Lambert));
				});
			Button_CreatePlane.onClick.AddListener(() =>
				{
					CreateObj(typeof(RT.RTShape_Plane), typeof(RT.RTMat_Lambert));
				});
		}
		private void OnGUI()
		{
			if (shapeIndex >= 0)
			{
				shapeWindowPos = GUILayout.Window(shapeIndex, shapeWindowPos,
												  ObjWindowCallback, "Selected Object");
			}
		}
		private void ObjWindowCallback(int id)
		{
			selectedShape.DoGUI();
			selectedMat.DoGUI();

			GUI.DragWindow();
		}
	}
}