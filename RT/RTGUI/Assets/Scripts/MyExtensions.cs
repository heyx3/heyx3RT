using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static partial class MyExtensions
{
	public static int IndexOf<T>(this IList<T> list, T toFind)
		where T : IEquatable<T>
	{
		for (int i = 0; i < list.Count; ++i)
			if (list[i].Equals(toFind))
				return i;
		return -1;
	}
	public static int IndexOf<T>(this IList<T> list, Predicate<T> isValid)
	{
		for (int i = 0; i < list.Count; ++i)
			if (isValid(list[i]))
				return i;
		return -1;
	}


	public static bool IsDirectorySeparator(this char c)
	{
		return c == System.IO.Path.DirectorySeparatorChar ||
			   c == System.IO.Path.AltDirectorySeparatorChar;
	}

	/// <summary>
	/// Assuming this string is a file path,
	///		gets the same path but relative to the given sub-folder.
	///	Example 1: "A/B/C".MakePathRelative("B") will return "B/C".
	///	Example 2: "A/B/C/A/B/C".MakePathRelative("B") will return "B/C/A/B/C".
	///	Example 3: "A/B/C".MakePathRelative("Fhqwhgads") will return null.
	/// </summary>
	public static string MakePathRelative(this string s, string newRootFolder)
	{
		//Find the beginning of the new root folder's name.
		//Make sure we don't accidentally find a folder whose name CONTAINS the target folder string!
		
		Func<int, bool> isActuallyFolder = (i) =>
			{
				//Is this sub-string preceded by a directory separator?
				if (i > 0 && !s[i - 1].IsDirectorySeparator())
					return false;

				//Does a directory separator immediately follow this sub-string?
				int afterI = i + newRootFolder.Length;
				if (afterI < s.Length && !s[afterI].IsDirectorySeparator())
					return false;

				return true;
			};

		int startI = s.IndexOf(newRootFolder);
		while (startI >= 0 && !isActuallyFolder(startI))
			startI = s.IndexOf(newRootFolder, startI + 1);

		if (startI == -1)
			return null;
		else
			return s.Substring(startI);
	}
}




//The following is needed for Visual Studio 2013 to be able to debug Unity C#.
//I think it has to do with the JSON plugin I'm using, which uses .NET 2.0.
namespace System.Runtime.CompilerServices
{
	public class ExtensionAttribute : Attribute { }
}