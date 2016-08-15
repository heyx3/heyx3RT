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
}