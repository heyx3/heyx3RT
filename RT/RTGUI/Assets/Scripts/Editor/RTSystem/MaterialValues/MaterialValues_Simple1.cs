using System;


namespace RT.MaterialValue
{
	/// <summary>
	/// A MaterialValue that only takes one parameter.
	/// </summary>
	public abstract class MV_Simple1 : MV_Base
	{
		public MV_Simple1(MV_Base x) { AddChild(x); }

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
	
	public class MV_Normalize : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Normalize; } }
		public MV_Normalize(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Length: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Length; } }
		public MV_Length(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Sqrt : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Sqrt; } }
		public MV_Sqrt(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Sin : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Sin; } }
		public MV_Sin(MV_Base x) : base(x) { }
	}
	public class MV_Cos : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Cos; } }
		public MV_Cos(MV_Base x) : base(x) { }
	}
	public class MV_Tan : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Tan; } }
		public MV_Tan(MV_Base x) : base(x) { }
	}
	public class MV_Asin: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Asin; } }
		public MV_Asin(MV_Base x) : base(x) { }
	}
	public class MV_Acos: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Acos; } }
		public MV_Acos(MV_Base x) : base(x) { }
	}
	public class MV_Atan: MV_Simple1
	{
		public override string TypeName { get { return TypeName_Atan; } }
		public MV_Atan(MV_Base x) : base(x) { }
	}
	public class MV_Smoothstep : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Smoothstep; } }
		public MV_Smoothstep(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "T"; }
	}
	public class MV_Smootherstep : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Smootherstep; } }
		public MV_Smootherstep(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "T"; }
	}
	public class MV_Floor : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Floor; } }
		public MV_Floor(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Ceil : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Ceil; } }
		public MV_Ceil(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_Abs : MV_Simple1
	{
		public override string TypeName { get { return TypeName_Abs; } }
		public MV_Abs(MV_Base x) : base(x) { }
		protected override string GetInputName(int index) { return "X"; }
	}
	public class MV_RayPos : MV_Simple1
	{
		public override string TypeName { get { return TypeName_RayPos; } }
		public MV_RayPos(MV_Base t) : base(t) { }
		protected override string GetInputName(int index) { return "T"; }
	}
}