using System;


namespace RT
{
	/// <summary>
	/// A MaterialValue that takes two parameters.
	/// </summary>
	public abstract class MV_Simple2 : MaterialValue
	{
		public MV_Simple2(MaterialValue a, MaterialValue b) { AddChild(a); AddChild(b); }

		protected override string GetInputName(int index) { return (index == 0 ? "A" : "B"); }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			Write(GetChild(0), writer, GetInputName(0));
			Write(GetChild(1), writer, GetInputName(1));
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			ClearChildren();
			AddChild(Read(reader, GetInputName(0)));
			AddChild(Read(reader, GetInputName(1)));
		}
	}


	public class MV_Distance : MV_Simple2
	{
		public override string TypeName { get { return TypeName_Distance; } }
		public MV_Distance(MaterialValue a, MaterialValue b) : base(a, b) { }
		protected override string GetInputName(int i) { return (i == 0 ? "A" : "B"); }
	}
	public class MV_Atan2 : MV_Simple2
	{
		public override string TypeName { get { return TypeName_Atan2; } }
		public MV_Atan2(MaterialValue y, MaterialValue x) : base(y, x) { }
		protected override string GetInputName(int i) { return (i == 0 ? "Y" : "X"); }
	}
	public class MV_Step : MV_Simple2
	{
		public override string TypeName { get { return TypeName_Step; } }
		public MV_Step(MaterialValue edge, MaterialValue x) : base(edge, x) { }
		protected override string GetInputName(int i) { return (i == 0 ? "Edge" : "x"); }
	}
}