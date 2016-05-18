using System;


namespace RT
{
	/// <summary>
	/// A MaterialValue that takes three parameters.
	/// </summary>
	public abstract class MV_Simple3 : MaterialValue
	{
		public MV_Simple3(MaterialValue a, MaterialValue b, MaterialValue c)
		{
			AddChild(a);
			AddChild(b);
			AddChild(c);
		}

		protected override string GetInputName(int index)
		{
			return (index == 0 ? "A" : (index == 1 ? "B" : "C"));
		}
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			Write(GetChild(0), writer, GetInputName(0));
			Write(GetChild(1), writer, GetInputName(1));
			Write(GetChild(2), writer, GetInputName(2));
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			ClearChildren();
			AddChild(Read(reader, GetInputName(0)));
			AddChild(Read(reader, GetInputName(1)));
			AddChild(Read(reader, GetInputName(2)));
		}
	}


	public class MV_Lerp : MV_Simple3
	{
		public override string TypeName { get { return TypeName_Lerp; } }
		public MV_Lerp(MaterialValue a, MaterialValue b, MaterialValue t) : base(a, b, t) { }
		protected override string GetInputName(int i) { return (i == 0 ? "A" : (i == 1 ? "B" : "T")); }
	}
	public class MV_Clamp : MV_Simple3
	{
		public override string TypeName { get { return TypeName_Clamp; } }
		public MV_Clamp(MaterialValue min, MaterialValue max, MaterialValue x) : base(min, max, x) { }
		protected override string GetInputName(int i) { return (i == 0 ? "Min" : (i == 1 ? "Max" : "X")); }
	}
}