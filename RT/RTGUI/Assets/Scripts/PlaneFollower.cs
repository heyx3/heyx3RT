using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Should be parented to an RTShape_Plane.
/// Matches the parent's transform, mesh, and material, but flips the faces backwards.
/// </summary>
[ExecuteInEditMode]
public class PlaneFollower : MonoBehaviour
{
	private Transform myTr;
	private MeshFilter myMF;
	private MeshRenderer myMR;


	private void Awake()
	{
		myTr = transform;

		myMF = GetComponent<MeshFilter>();
		if (myMF == null)
			myMF = gameObject.AddComponent<MeshFilter>();

		myMR = GetComponent<MeshRenderer>();
		if (myMR == null)
			myMR = gameObject.AddComponent<MeshRenderer>();

		Update();
	}
	private void Update()
	{
		//If no parent, hide.
		if (myTr.parent == null)
		{
			myMR.enabled = false;
			return;
		}
		
		//If parent isn't a plane, or parent is one-sided, hide.
		var plane = myTr.parent.GetComponent<RT.RTShape_Plane>();
		if (plane == null || plane.IsOneSided)
		{
			myMR.enabled = false;
			return;
		}

		myMR.enabled = true;

		myTr.localPosition = Vector3.zero;
		myTr.localScale = new Vector3(1.0f, 1.0f, -1.0f);
		myTr.localEulerAngles = new Vector3(180.0f, 0.0f, 0.0f);

		myMF.sharedMesh = plane.UnityMesh;
		myMR.sharedMaterial = plane.GetComponent<MeshRenderer>().sharedMaterial;
	}
}