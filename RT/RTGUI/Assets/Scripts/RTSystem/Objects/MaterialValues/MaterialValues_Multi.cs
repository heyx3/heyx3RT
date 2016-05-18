using System;
using System.Collections.Generic;


namespace RT
{
	/// <summary>
	/// A MaterialValue that has a variable number of children.
	/// </summary>
	public abstract class MV_MultiType : MaterialValue
	{
		public override bool HasVariableNumberOfChildren { get { return true; } }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteList(GetChildrenCopy(), (mv, n, wr) => Write(mv, wr, n), "Items");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);

			List<MaterialValue> vals = new List<MaterialValue>();
			reader.ReadList(vals, (n, rd) => Read(rd, n), "Items");

			ClearChildren();
			for (int i = 0; i < vals.Count; ++i)
				AddChild(vals[i]);
		}
	}


	public class MV_Add : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Add; } }
		public MV_Add(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Add(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 0.0f); }
	}
	public class MV_Subtract: MV_MultiType
	{
		public override string TypeName { get { return TypeName_Subtract; } }
		public MV_Subtract(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Subtract(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 0.0f); }
	}
	public class MV_Multiply : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Multiply; } }
		public MV_Multiply(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Multiply(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 1.0f); }
	}
	public class MV_Divide : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Divide; } }
		public MV_Divide(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
		public MV_Divide(MaterialValue a, MaterialValue b, MaterialValue c) { AddChild(a); AddChild(b); AddChild(c); }
		protected override MaterialValue MakeDefaultChild() { return new MV_Constant(false, new uint[] { 1 }, 1.0f); }
	}
	public class MV_Min : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Min; } }
		public MV_Min(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
	}
	public class MV_Max : MV_MultiType
	{
		public override string TypeName { get { return TypeName_Max; } }
		public MV_Max(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }
	}
}
