using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using RT.MaterialValue;

using Category = RT.MatEditor.NodeTree_Element_Category;
using Option = RT.MatEditor.NodeTree_Element_Option;


namespace RT.MatEditor
{
	/// <summary>
	/// A tree of node types to choose from.
	/// Each element is either an option, or a branch leading to more elements of the same category.
	/// </summary>
	public abstract class NodeTree_Element
	{
		/// <summary>
		/// Draws this element as a selectable option in an editor window using GUILayout.
		/// Returns the element that was selected, or "null" if nothing was selected.
		/// </summary>
		public abstract NodeTree_Element_Option OnGUI();
	}

	
	public class NodeTree_Element_Category : NodeTree_Element
	{
		public string Title, Tooltip;
		public NodeTree_Element[] SubItems;

		public NodeTree_Element_Category(string title, params NodeTree_Element[] subItems)
		{
			Title = title;
			SubItems = subItems;
		}
		public NodeTree_Element_Category(string title, string tooltip, params NodeTree_Element[] subItems)
		{
			Title = title;
			Tooltip = tooltip;
			SubItems = subItems;
		}

		private bool foldout = false;
		public override NodeTree_Element_Option OnGUI()
		{
			NodeTree_Element_Option option = null;

			foldout = EditorGUILayout.Foldout(foldout, Title);
			if (foldout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(25.0f);
				GUILayout.BeginVertical();

				foreach (NodeTree_Element el in SubItems)
				{
					NodeTree_Element_Option temp = el.OnGUI();
					if (temp != null)
						option = temp;
				}

				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}

			return option;
		}
	}
	public class NodeTree_Element_Option : NodeTree_Element
	{
		public Func<MV_Base> NodeFactory;
		public string Name, Tooltip;

		public NodeTree_Element_Option(Func<MV_Base> nodeFactory, string name, string tooltip = "")
		{
			NodeFactory = nodeFactory;
			Name = name;
			Tooltip = tooltip;
		}

		public override NodeTree_Element_Option OnGUI()
		{
			GUILayout.BeginHorizontal();
			
			bool pressed = GUILayout.Button(new GUIContent(Name, Tooltip));
			GUILayout.FlexibleSpace();
			
			GUILayout.EndHorizontal();

			return (pressed ? this : null);
		}
	}


	public static class NodeOptionsGenerator
	{
		/// <summary>
		/// Returns the root of the options list.
		/// </summary>
		public static List<NodeTree_Element> GenerateList()
		{
			return new List<NodeTree_Element>()
			{
				new Option(() => new MV_Tex2D(MV_Inputs.SurfaceUV), "Texture2D"),
				new Option(() => new MV_Swizzle(F(), MV_Swizzle.Components.X), "Swizzle"),
				new Option(() => new MV_PureNoise(1), "Noise", "Generates up to 4 random values between 0 and 1"),
				new Option(() => new MV_Constant(new EditableVectorf(0.0f, false, OutputSizes.All,
																	 float.NegativeInfinity,
																	 float.PositiveInfinity)),
						   "Constant"),
				new Option(() => MV_Arithmetic.Add(F(), F()), "Add"),
				new Option(() => MV_Arithmetic.Subtract(F(), F()), "Subtract"),
				new Option(() => MV_Arithmetic.Multiply(F(), F()), "Multiply"),
				new Option(() => MV_Arithmetic.Divide(F(), F()), "Divide"),
				new Category("Input",
						new Option(() => MV_Inputs.SurfacePos, "Surface pos", "The position of the surface hit by the ray"),
						new Option(() => MV_Inputs.SurfaceNormal, "Surface normal", "The normal of the surface hit by the ray"),
						new Option(() => MV_Inputs.SurfaceTangent, "Surface tangent", "The tangent of the surface hit by the ray"),
						new Option(() => MV_Inputs.SurfaceBitangent, "Surface bitangent", "The bitangent of the surface hit by the ray"),
						new Option(() => MV_Inputs.SurfaceUV, "Surface UV", "The UV of the surface hit by the ray"),
						new Option(() => MV_Inputs.RayStart, "Ray start", "The initial position of the ray"),
						new Option(() => MV_Inputs.RayDir, "Ray dir", "The direction of the ray"),
						new Option(() => new MV_RayPos(F()), "Ray pos", "The ray's position at time t"),
						new Option(() => MV_Inputs.ShapePos, "Shape pos", "The center position of the shape hit by the ray"),
						new Option(() => MV_Inputs.ShapeRot, "Shape rot", "The rotation of the shape hit by the ray (xyz is axis, w is angle in radians)"),
						new Option(() => MV_Inputs.ShapeScale, "Shape scale", "The scale of the shape hit by the ray (xyz)")),
				new Category("Trig",
						new Option(() => MV_Simple1.Sin(F()), "sin"),
						new Option(() => MV_Simple1.Cos(F()), "cos"),
						new Option(() => MV_Simple1.Tan(F()), "tan"),
						new Option(() => MV_Simple1.Asin(F()), "asin", "Inverse sine"),
						new Option(() => MV_Simple1.Acos(F()), "acos", "Inverse cosine"),
						new Option(() => MV_Simple1.Atan(F()), "atan", "Inverse tangent"),
						new Option(() => MV_Simple2.Atan2(F(), F(1.0f)), "atan2", "Inverse tangent")),
				new Category("Spacial",
						new Option(() => MV_Simple1.Normalize(F(1.0f)), "normalize"),
						new Option(() => MV_Simple1.Length(F()), "length"),
						new Option(() => MV_Simple2.Distance(F(), F()), "distance"),
						new Option(() => MV_Simple2.Dot(F(), F()), "dot", "Dot Product"),
						new Option(() => MV_Simple2.Reflect(V3(0.0f, 1.0f), V3(1.0f)), "reflect", "Reflect vector along normal"),
						new Option(() => MV_Simple3.Refract(V3(1.0f), V3(0.0f, 1.0f), F()), "refract", "Refract vector along a normal with an index of refraction")),
				new Category("Interpolation",
						new Option(() => MV_Simple1.Smoothstep(F()), "smoothstep", "A smooth interp from 0 to 1"),
						new Option(() => MV_Simple1.Smootherstep(F()), "smootherstep", "A very smooth from 0 to 1"),
						new Option(() => MV_Simple2.Step(F(0.5f), F()), "step", "0 if x < step, 1 if x > step"),
						new Option(() => MV_Simple3.Lerp(F(), F(1.0f), F()), "lerp")),
				new Category("Numeric",
						new Option(() => MV_Simple1.Sqrt(F()), "sqrt", "Square Root"),
						new Option(() => MV_Simple2.Pow(F(), F()), "pow", "Raise a number to an exponent"),
						new Option(() => MV_Simple1.Ln(F()), "ln", "Natural logarithm (base e)"),
						new Option(() => MV_Simple1.Floor(F()), "floor", "Round towards -inf"),
						new Option(() => MV_Simple1.Ceil(F()), "ceil", "Round towards +inf"),
						new Option(() => MV_Simple1.Abs(F()), "abs", "Absolute Value"),
						new Option(() => MV_Simple3.Clamp(F(), F(1.0f), F()), "clamp"),
						new Option(() => new MV_MinMax(F(), F(), true), "min", "Gets the smallest value"),
						new Option(() => new MV_MinMax(F(), F(), false), "max", "Gets the largest value"),
						new Option(() => new MV_Average(F(), F()), "average", "Gets the average of a set of values"),
						new Option(() => new MV_Append(F(), F()), "append", "Combine multiple inputs into a single output")),
			};
		}

		private static MV_Base F(float f = 0.0f, bool inline = true) { return MV_Constant.MakeFloat(f, false, float.NegativeInfinity, float.PositiveInfinity, OutputSizes.One, inline); }
		private static MV_Base V3(float x = 0.0f, float y = 0.0f, float z = 0.0f) { return MV_Constant.MakeVec3(x, y, z, 0.0f, 1.0f, OutputSizes.Three, true); }
	}
}
