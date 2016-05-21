using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RTGui
{
	/// <summary>
	/// Manages windows in the scene.
	/// </summary>
	public class WindowHandler : MonoBehaviour
	{
		public static WindowHandler Instance
		{
			get { if (instance == null) instance = FindObjectOfType<WindowHandler>(); return instance; }
		}
		private static WindowHandler instance = null;


		private List<ManagedWindow> openWindows = new List<ManagedWindow>();


		public void AddWindow(ManagedWindow wnd) { openWindows.Add(wnd); }

		public void RemoveWindow(ManagedWindow wnd) { RemoveWindow(wnd.ID); }
		public void RemoveWindow(int id)
		{
			int index = openWindows.IndexOf(mw => mw.ID == id);
			if (index >= 0)
			{
				openWindows.RemoveAt(index);
			}
		}


		private void OnGUI()
		{
			for (int i = 0; i < openWindows.Count; ++i)
				openWindows[i].DoGUI();
		}
	}
}