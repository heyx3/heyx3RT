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
		public UnityEngine.UI.Button Button_GenerateImage;


		private int shapeIndex = -1;
		private RT.RTShape selectedShape;
		private RT.RTMat selectedMat;
		private Rect shapeWindowPos;

		private RTGui.ImageRenderWindow imgRenderer;


		public void OnShapeClicked(RT.RTShape shpe)
		{
			Scene = GetComponent<RT.Tracer>();

			Button_CreateSphere.enabled = false;
			Button_CreatePlane.enabled = false;
			Button_GenerateImage.enabled = false;

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
			Button_GenerateImage.onClick.AddListener(() =>
				{
					imgRenderer = new RTGui.ImageRenderWindow(new Rect(), new GUIContent("Render"),
															  () => { imgRenderer.Release(); imgRenderer = null; });
				});
		}
		private void OnGUI()
		{
			if (shapeIndex >= 0)
			{
				shapeWindowPos = GUILayout.Window(shapeIndex, shapeWindowPos,
												  ObjWindowCallback, "Selected Object",
												  RTGui.Gui.Instance.Style_Window);
			}
		}
		private void ObjWindowCallback(int id)
		{
			if (GUILayout.Button("Delete", RTGui.Gui.Instance.Style_Button))
			{
				Scene.Objects.RemoveAt(shapeIndex);
				Destroy(selectedShape.gameObject);
				selectedShape = null;
				selectedMat = null;
				shapeIndex = -1;
				return;
			}

			GUILayout.Space(10.0f);

			selectedShape.DoGUI();

			GUILayout.Space(10.0f);

			selectedMat.DoGUI();

			GUI.DragWindow();
		}
	}
}