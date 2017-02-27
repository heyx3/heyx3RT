using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RT.MaterialValue
{
	public class MV_Perlin : MV_Base
	{
		#region Definition of perlin functions

		
		public static readonly string HashingFuncs = @"
			float hashValue1(float x)
			{
				return frac(sin(x * 78.233) * 43758.5453);
			}
			float hashValue2(float2 x)
			{
				return frac(sin(dot(x, float2(12.9898, 78.233))) * 43758.5453);
			}
			float hashValue3(float3 x)
			{
				return frac(sin(dot(x, float3(12.9898, 78.233, 36.34621))) * 43758.5453);
			}
			float hashValue4(float4 x)
			{
				return frac(sin(dot(x, float4(55.234123, 12.9898, 78.233, 36.34621))) * 43758.5453);
			}
";
		public static readonly string PerlinFuncs = @"
			float PerlinNoise1(float f)
			{
				float minX = floor(f),
					  maxX = minX + 1.0,
					  lerpVal = f - minX;

				float minX_V = -1.0 + (2.0 * hashValue1(minX));
				float toMin = -lerpVal;

				float maxX_V = -1.0 + (2.0 * hashValue1(maxX));
				float toMax = maxX - f;

				float outVal = lerp(dot(minX_V, toMin),
									dot(maxX_V, toMax),
									smoothstep(0.0, 1.0, lerpVal));
				return 0.5 + (0.5 * outVal);
			}
			float PerlinNoise2(float2 f)
			{
				float2 minXY = floor(f),
					   maxXY = minXY + float2(1.0, 1.0),
					   minXmaxY = float2(minXY.x, maxXY.y),
					   maxXminY = float2(maxXY.x, minXY.y),
					   lerpVal = f - minXY;

				float temp = hashValue2(minXY);
				float2 minXY_V = -1.0 + (2.0 * float2(temp, hashValue1(temp)));
				float2 toMinXY = -lerpVal;

				temp = hashValue2(maxXY);
				float2 maxXY_V = -1.0 + (2.0 * float2(temp, hashValue1(temp)));
				float2 toMaxXY = maxXY - f;

				temp = hashValue2(minXmaxY);
				float2 minXmaxY_V = -1.0 + (2.0 * float2(temp, hashValue1(temp)));
				float2 toMinXmaxY = minXmaxY - f;

				temp = hashValue2(maxXminY);
				float2 maxXminY_V = -1.0 + (2.0 * float2(temp, hashValue1(temp)));
				float2 toMaxXminY = maxXminY - f;

				lerpVal = smoothstep(0.0, 1.0, lerpVal);
				float outVal = lerp(lerp(dot(minXY_V, toMinXY),
										 dot(maxXminY_V, toMaxXminY),
										 lerpVal.x),
									lerp(dot(minXmaxY_V, toMinXmaxY),
										 dot(maxXY_V, toMaxXY),
										 lerpVal.x),
									lerpVal.y);
				return 0.5 + (0.5 * outVal);
			}
			float PerlinNoise3(float3 f)
			{
				float3 minXYZ = floor(f),
					   maxXYZ = minXYZ + float3(1.0, 1.0, 1.0),
					   minXYmaxZ =    float3(minXYZ.xy, maxXYZ.z),
					   minXmaxYminZ = float3(minXYZ.x, maxXYZ.y, minXYZ.z),
					   minXmaxYZ =    float3(minXYZ.x, maxXYZ.y, maxXYZ.z),
					   maxXminYZ =    float3(maxXYZ.x, minXYZ.y, minXYZ.z),
					   maxXminYmaxZ = float3(maxXYZ.x, minXYZ.y, maxXYZ.z),
					   maxXYminZ =    float3(maxXYZ.xy, minXYZ.z),
					   lerpVal = f - minXYZ;

				float temp = hashValue3(minXYZ),
					  temp2 = hashValue1(temp);
				float3 minXYZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMinXYZ = -lerpVal;

				temp = hashValue3(maxXYZ);
				temp2 = hashValue1(temp);
				float3 maxXYZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMaxXYZ = maxXYZ - f;

				temp = hashValue3(minXYmaxZ);
				temp2 = hashValue1(temp);
				float3 minXYmaxZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMinXYmaxZ = minXYmaxZ - f;

				temp = hashValue3(minXmaxYminZ);
				temp2 = hashValue1(temp);
				float3 minXmaxYminZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMinXmaxYminZ = minXmaxYminZ - f;

				temp = hashValue3(minXmaxYZ);
				temp2 = hashValue1(temp);
				float3 minXmaxYZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMinXmaxYZ = minXmaxYZ - f;

				temp = hashValue3(maxXminYZ);
				temp2 = hashValue1(temp);
				float3 maxXminYZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMaxXminYZ = maxXminYZ - f;

				temp = hashValue3(maxXminYmaxZ);
				temp2 = hashValue1(temp);
				float3 maxXminYmaxZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMaxXminYmaxZ = maxXminYmaxZ - f;

				temp = hashValue3(maxXYminZ);
				temp2 = hashValue1(temp);
				float3 maxXYminZ_V = -1.0 + (2.0 * float3(temp, temp2, hashValue1(temp2)));
				float3 toMaxXYminZ = maxXYminZ - f;

				lerpVal = smoothstep(0.0, 1.0, lerpVal);
				float outVal = lerp(lerp(lerp(dot(minXYZ_V, toMinXYZ),
											  dot(maxXminYZ_V, toMaxXminYZ),
											  lerpVal.x),
										 lerp(dot(minXmaxYminZ_V, toMinXmaxYminZ),
											  dot(maxXYminZ_V, toMaxXYminZ),
											  lerpVal.x),
										 lerpVal.y),
									lerp(lerp(dot(minXYmaxZ_V, toMinXYmaxZ),
											  dot(maxXminYmaxZ_V, toMaxXminYmaxZ),
											  lerpVal.x),
										 lerp(dot(minXmaxYZ_V, toMinXmaxYZ),
											  dot(maxXYZ_V, toMaxXYZ),
											  lerpVal.x),
										 lerpVal.y),
									lerpVal.z);
				return 0.5 + (0.5 * outVal);
			}
			float PerlinNoise4(float4 f)
			{
				float4 min = floor(f),
					   max = min + 1.0,
					   minXminYminZminW = min,
					   minXminYminZmaxW = float4(min.x, min.y, min.z, max.w),
					   minXminYmaxZminW = float4(min.x, min.y, max.z, min.w),
					   minXminYmaxZmaxW = float4(min.x, min.y, max.z, max.w),
					   minXmaxYminZminW = float4(min.x, max.y, min.z, min.w),
					   minXmaxYminZmaxW = float4(min.x, max.y, min.z, max.w),
					   minXmaxYmaxZminW = float4(min.x, max.y, max.z, min.w),
					   minXmaxYmaxZmaxW = float4(min.x, max.y, max.z, max.w),
					   maxXminYminZminW = float4(max.x, min.y, min.z, min.w),
					   maxXminYminZmaxW = float4(max.x, min.y, min.z, max.w),
					   maxXminYmaxZminW = float4(max.x, min.y, max.z, min.w),
					   maxXminYmaxZmaxW = float4(max.x, min.y, max.z, max.w),
					   maxXmaxYminZminW = float4(max.x, max.y, min.z, min.w),
					   maxXmaxYminZmaxW = float4(max.x, max.y, min.z, max.w),
					   maxXmaxYmaxZminW = float4(max.x, max.y, max.z, min.w),
					   maxXmaxYmaxZmaxW = max;

				float4 t = f - min;

				float4 temp;
			#define DO(toDo) \
				temp.x = hashValue4(f); \
				temp.y = hashValue1(temp.x); \
				temp.z = hashValue1(temp.y); \
				temp.w = hashValue1(temp.z); \
				float4 toDo##_V = (temp * 2.0f) - 1.0f, \
					   to_##toDo = toDo - f;

				DO(minXminYminZminW);
				DO(minXminYminZmaxW);
				DO(minXminYmaxZminW);
				DO(minXminYmaxZmaxW);
				DO(minXmaxYminZminW);
				DO(minXmaxYminZmaxW);
				DO(minXmaxYmaxZminW);
				DO(minXmaxYmaxZmaxW);
				DO(maxXminYminZminW);
				DO(maxXminYminZmaxW);
				DO(maxXminYmaxZminW);
				DO(maxXminYmaxZmaxW);
				DO(maxXmaxYminZminW);
				DO(maxXmaxYminZmaxW);
				DO(maxXmaxYmaxZminW);
				DO(maxXmaxYmaxZmaxW);
			#undef DO

				t = smoothstep(0.0, 1.0, t);

			#define DOT(a) dot(a##_V, to_##a)
				float outVal = lerp(lerp(lerp(lerp(DOT(minXminYminZminW),
                                                   DOT(maxXminYminZminW),
												   t.x),
                                              lerp(DOT(minXmaxYminZminW),
												   DOT(maxXmaxYminZminW),
												   t.x),
											  t.y),
										 lerp(lerp(DOT(minXminYmaxZminW),
												   DOT(maxXminYmaxZminW),
												   t.x),
                                              lerp(DOT(minXmaxYmaxZminW),
												   DOT(maxXmaxYmaxZminW),
												   t.x),
											  t.y),
										 t.z),
									lerp(lerp(lerp(DOT(minXminYminZmaxW),
												   DOT(maxXminYminZmaxW),
												   t.x),
                                              lerp(DOT(minXmaxYminZmaxW),
												   DOT(maxXmaxYminZmaxW),
												   t.x),
											  t.y),
										 lerp(lerp(DOT(minXminYmaxZmaxW),
												   DOT(maxXminYmaxZmaxW),
												   t.x),
                                              lerp(DOT(minXmaxYmaxZmaxW),
												   DOT(maxXmaxYmaxZmaxW),
												   t.x),
											  t.y),
										 t.z),
									t.w);
			#undef DOT
				return 0.5 + (0.5 * outVal);
			}
";

		#endregion


		public override string TypeName { get { return TypeName_PerlinNoise; } }
		public override OutputSizes OutputSize {  get { return OutputSizes.One; } }

		public override string PrettyName { get { return "Perlin Noise"; } }
		public override Color GUIColor { get { return new Color(0.7f, 0.7f, 1.0f); } }


		public MV_Perlin(MV_Base x) { AddInput(x); }


		public override void Emit(StringBuilder shaderlabProperties,
								  StringBuilder cgDefinitions,
								  StringBuilder cgFunctionBody,
								  Dictionary<MV_Base, uint> idLookup)
		{
			//Define necessary functions if they haven't been already.
			string cgDefs = cgDefinitions.ToString();
			if (!cgDefs.Contains("float hashValue1("))
				cgDefinitions.AppendLine(HashingFuncs);
			if (!cgDefs.Contains("float PerlinNoise1("))
				cgDefinitions.AppendLine(PerlinFuncs);

			cgFunctionBody.Append("float ");
			cgFunctionBody.Append(ShaderValueName(idLookup));
			cgFunctionBody.Append(" = PerlinNoise");
			cgFunctionBody.Append(GetInput(0).OutputSize.ToNumber());
			cgFunctionBody.Append("(");
			cgFunctionBody.Append(GetInput(0).ShaderValueName(idLookup));
			cgFunctionBody.AppendLine(");");
		}

		public override string GetInputName(int index)
		{
			return "X";
		}
	}
}
