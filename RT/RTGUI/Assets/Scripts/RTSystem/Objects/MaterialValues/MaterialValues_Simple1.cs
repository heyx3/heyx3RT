using System;


namespace RT
{
	/// <summary>
	/// A MaterialValue that only takes one parameter.
	/// </summary>
	public abstract class MV_Simple1 : MaterialValue
	{
		public MV_Simple1(MaterialValue x) { AddChild(x); }

		protected override string GetInputName(int index) { return "Input"; }
		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			Write(GetChild(0), writer, GetInputName(0));
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			ClearChildren();
			AddChild(Read(reader, GetInputName(0)));
		}
	}


	public class MV_Sin : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Sin; } }
		public MV_Sin(MaterialValue x) : base(x) { }
	}
	public class MV_Cos : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Cos; } }
		public MV_Cos(MaterialValue x) : base(x) { }
	}
	public class MV_Tan : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Tan; } }
		public MV_Tan(MaterialValue x) : base(x) { }
	}
	public class MV_Asin: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Asin; } }
		public MV_Asin(MaterialValue x) : base(x) { }
	}
	public class MV_Acos: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Acos; } }
		public MV_Acos(MaterialValue x) : base(x) { }
	}
	public class MV_Atan: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Atan; } }
		public MV_Atan(MaterialValue x) : base(x) { }
	}
	public class MV_Smoothstep : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Smoothstep; } }
		public MV_Smoothstep(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "T"; }
	}
	public class MV_Smootherstep : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Smootherstep; } }
		public MV_Smootherstep(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "T"; }
	}
	public class MV_Floor : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Floor; } }
		public MV_Floor(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Ceil : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Ceil; } }
		public MV_Ceil(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Abs : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Abs; } }
		public MV_Abs(MaterialValue x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_RayPos : MV_Simple1
	{
		public override string TypeName { get { return TypeName_RayPos; } }
		public MV_RayPos(MaterialValue t) : base(t) { }
		protected override string GetInputName(int index) { return "T"; }
	}
}