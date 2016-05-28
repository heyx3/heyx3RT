using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTGui
{
	/// <summary>
	/// A window that is managed by the WindowHandler.
	/// </summary>
	public abstract class ManagedWindow
	{
		private static int nextID = 0;
		private static Dictionary<int, ManagedWindow> windowLookup = new Dictionary<int, ManagedWindow>();


		public static T Get<T>(int id)
			where T : ManagedWindow
		{
			return (T)windowLookup[id];
		}


		public int ID { get; private set; }

		public GUIContent Title;
		public Rect WindowPos;
		public bool IsLayout;

		private GUI.WindowFunction windowFunc;

			
		public ManagedWindow(GUIContent title, Rect windowPos, GUI.WindowFunction _windowFunc, bool isLayout)
		{
			Title = title;
			WindowPos = windowPos;
			windowFunc = _windowFunc;
			IsLayout = isLayout;

			ID = nextID;
			unchecked { nextID += 1; }
			windowLookup.Add(ID, this);

			WindowHandler.Instance.AddWindow(this);
		}


		public void DoGUI()
		{
			if (IsLayout)
				WindowPos = GUILayout.Window(ID, WindowPos, windowFunc,
											 Title, RTGui.Gui.Instance.Style_Window);
			else
				WindowPos = GUI.Window(ID, WindowPos, windowFunc,
									   Title, RTGui.Gui.Instance.Style_Window);

			//Make sure the window is wide enough for the window content,
			float minWidth = RTGui.Gui.Instance.Style_Window.CalcSize(Title).x;
			WindowPos.width = Math.Max(minWidth, WindowPos.width);
		}
		public virtual void Release()
		{
			WindowHandler.Instance.RemoveWindow(ID);
			windowLookup.Remove(ID);
		}
	}
}