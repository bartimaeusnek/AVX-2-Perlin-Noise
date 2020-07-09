using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;

namespace AVXPerlinNoise
{
	public partial class Perlin
	{
		private static readonly Vector256<int> v255 = LoadVectorCorrectly(255);
		private static readonly Vector256<int> v1 = LoadVectorCorrectly(1);
		private static readonly Vector256<int> v2 = LoadVectorCorrectly(2);
		private static readonly Vector256<float> v2f = LoadVectorCorrectly(2f);
		private static readonly Vector256<float> v1f = LoadVectorCorrectly(1f);
		private static readonly Vector256<int> v4 = LoadVectorCorrectly(4);
		private static readonly Vector256<float> v4f = LoadVectorCorrectly(4f);
		private static readonly Vector256<float> v6f = LoadVectorCorrectly(6f);
		private static readonly Vector256<int> v8 = LoadVectorCorrectly(8);
		private static readonly Vector256<float> v10f = LoadVectorCorrectly(10f);
		private static readonly Vector256<int> v12 = LoadVectorCorrectly(12);
		private static readonly Vector256<int> v13 = LoadVectorCorrectly(13);
		private static readonly Vector256<int> v14 = LoadVectorCorrectly(14);
		private static readonly Vector256<float> v15f = LoadVectorCorrectly(15f);
		private static readonly Vector256<int> v15 = LoadVectorCorrectly(15);
		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<float> LoadVectorCorrectly(float count)
		{
			var tobe = stackalloc float[1];
			tobe[0] = count;
			return BroadcastScalarToVector256(tobe);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<uint> LoadVectorCorrectly(uint count)
		{
			var tobe = stackalloc uint[1];
			tobe[0] = count;
			return BroadcastScalarToVector256(tobe);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<int> LoadVectorCorrectly(int count)
		{
			var tobe = stackalloc int[1];
			tobe[0] = count;
			return BroadcastScalarToVector256(tobe);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> perlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z)
		{
			var xi = And(ConvertToVector256Int32WithTruncation(x), v255);
			var yi = And(ConvertToVector256Int32WithTruncation(y), v255);
			var zi = And(ConvertToVector256Int32WithTruncation(z), v255);

			var xf = Subtract(x, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(x)));
			var yf = Subtract(y, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(y)));
			var zf = Subtract(z, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(z)));

			var u = fadeAVX(xf);
			var v = fadeAVX(yf);
			var w = fadeAVX(zf);

			var a  = UnpackPermutationArrayAndAdd(xi,                           yi);
			var aa = UnpackPermutationArrayAndAdd(a,                            zi);
			var ab = UnpackPermutationArrayAndAdd(Add(a,  v1), zi);
			var b  = UnpackPermutationArrayAndAdd(Add(xi, v1), yi);
			var ba = UnpackPermutationArrayAndAdd(b,                            zi);
			var bb = UnpackPermutationArrayAndAdd(Add(b, v1),  zi);

			var x1 = lerpAVX(gradAVX(UnpackPermutationArray(aa), xf,                                 yf, zf),
			                 gradAVX(UnpackPermutationArray(ba), Subtract(xf, v1f), yf, zf),
			                 u);
			var x2 = lerpAVX(gradAVX(UnpackPermutationArray(ab), xf, Subtract(yf, v1f), zf),
			                 gradAVX(UnpackPermutationArray(bb), Subtract(xf, v1f),
			                         Subtract(yf,                             v1f), zf),
			                 u);
			var y1 = lerpAVX(x1, x2, v);

			x1 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(aa, v1)), xf, yf, Subtract(zf, v1f)),
				        gradAVX(UnpackPermutationArray(ba), Subtract(xf, v1f), yf,
				                Subtract(zf,                             v1f)),
				        u);

			x2 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(ab, v1)), xf, Subtract(yf, v1f), Subtract(zf, v1f)),
				        gradAVX(UnpackPermutationArray(Add(bb, v1)),
				                Subtract(xf, v1f), Subtract(yf, v1f),
				                Subtract(zf, v1f)),
				        u);


			var y2 = lerpAVX(x1, x2, v);

			return Divide(Add(lerpAVX(y1, y2, w), v1f), v2f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> lerpAVX(Vector256<float> a, Vector256<float> b, Vector256<float> x)
			=> Add(a, Multiply(x, Subtract(b, a)));

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> fadeAVX(Vector256<float> t)
			=> Multiply(
			            Multiply(
			                     Multiply(
			                              t,
			                              t
			                             ),
			                     t
			                    ),
			            Add(
			                Multiply(
			                         t,
			                         Subtract(
			                                  Multiply(
			                                           t,
			                                           v6f
			                                          ),
			                                  v15f
			                                 )
			                        ),
			                v10f
			               )
			           );

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static Vector256<int> UnpackPermutationArrayAndAdd(Vector256<int> xi, Vector256<int> yi)
			=> Add(UnpackPermutationArray(xi), yi);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<int> UnpackPermutationArray(Vector256<int> xi)
		{
			fixed (int* pData = &p[0])
			{
				var arr = stackalloc int[8];
				Store(arr, xi);
				
				arr[0] = *(pData + arr[0]);
				arr[1] = *(pData + arr[1]);
				arr[2] = *(pData + arr[2]);
				arr[3] = *(pData + arr[3]);
				arr[4] = *(pData + arr[4]);
				arr[5] = *(pData + arr[5]);
				arr[6] = *(pData + arr[6]);
				arr[7] = *(pData + arr[7]);

				return LoadVector256(arr);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXUVector(Vector256<int> hashs, Vector256<float> xs, Vector256<float> ys)
		{
			// If the most signifigant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.
			var MSB = v8;
			var h   = CompareGreaterThan(MSB, hashs);

			var usX = And(h.AsSingle(), xs);
			var usY = AndNot(h.AsSingle(), ys);

			return Add(usX, usY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVYVector(Vector256<int> hashs, Vector256<float> ys)
		{
			// If the first and second signifigant bits are 0 set v = y
			var SSB  = v4;
			var ssbh = CompareGreaterThan(SSB, hashs); // "< 4" == "4 >" 

			return And(ssbh.AsSingle(), ys);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVXVector(Vector256<int> hashs, Vector256<float> xs)
		{
			// If the first and second signifigant bits are 0 set v = y
			var TSB1h = CompareEqual(hashs, v12);
			var TSB2h = CompareEqual(hashs, v14);

			var vsX1 = And(TSB1h.AsSingle(), xs);
			var vsX2 = And(TSB2h.AsSingle(), xs);

			return Add(vsX1, vsX2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVZVector(Vector256<int> hashs, Vector256<float> zs)
		{
			// If the first and second signifigant bits are 0 set v = y
			
			var bigger4   = CompareGreaterThan(hashs, v4);
			var smaller12 = CompareGreaterThan(v12,  hashs);

			var range1 = And(bigger4, smaller12);

			var eq13 = CompareEqual(hashs, v13);

			var range = Or(range1, eq13);

			return And(range.AsSingle(), zs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVVector(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
		                                              Vector256<float> zs)
		{
			var VX = gradAVXVXVector(hashs, xs);
			var VY = gradAVXVYVector(hashs, ys);
			var VZ = gradAVXVZVector(hashs, zs);

			return Add(Add(VX, VY), VZ);
		}

		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static Vector256<float> gradAVXPartialA(Vector256<int> hashs, Vector256<float> u)
		{
			var hAnd1  = And(hashs, v1);
			var hZeros = CompareEqual(hAnd1, Vector256<int>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<int>.Zero);

			var ucomp    = And(hZeros.AsSingle(), u);
			var ucompneg = Subtract(Vector256<float>.Zero, And(hOnes.AsSingle(), u));
			return Add(ucomp, ucompneg);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static Vector256<float> gradAVXPartialB(Vector256<int> hashs, Vector256<float> u)
		{
			var hAnd1  = And(hashs, v2);
			var hZeros = CompareEqual(hAnd1, Vector256<int>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<int>.Zero);

			var ucomp    = And(hZeros.AsSingle(), u);
			var ucompneg = Subtract(Vector256<float>.Zero, And(hOnes.AsSingle(), u));
			return Add(ucomp, ucompneg);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVX(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
		                                       Vector256<float> zs)
		{
			var hs  = And(hashs, v15);

			var u = gradAVXUVector(hs, xs, ys);
			var v = gradAVXVVector(hs, xs, ys, zs);

			return Add(gradAVXPartialA(hs, u), gradAVXPartialB(hs, v));
		}
		
		//------------------ EXPERIMENTAL METHODES --------------------
		
		private static unsafe float SumVector(Vector256<float> input)
		{
			var ret = 0f;
			var flt = stackalloc float[8];
			Store(flt, input);
			for (var i = 0; i < 8; i++)
			{
				ret += flt[i];
			}

			return ret;
		} 
		
		internal static float OctavePerlinAVX(float x, float y, float z, int octaves, float persistence, float frequency = 1, float amplitude = 1) {
			if (octaves % 8 != 0)
				throw new InvalidOperationException("octaves must be divideable by 8!");
			var total = 0f;
			var i     = 0;

			var ax = LoadVectorCorrectly(x); 
			var bx = LoadVectorCorrectly(y); 
			var cx = LoadVectorCorrectly(z); 
			
			do
			{
				var ampl    = LoadVectorWithMod(amplitude, persistence); 
				var fq      = LoadVectorWithMod(frequency, 2);
				var ay      = Multiply(ax, fq);
				var by      = Multiply(bx, fq);
				var cy      = Multiply(cx, fq);
				var pavx    = perlinAVX(ay, by, cy);
				var pavxAmp = Multiply(pavx, ampl);
				
				total += SumVector(pavx);
				++i;
			} while (i < octaves / 8);

			return total;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<float> LoadVectorWithMod(float count, float mod)
		{
			var tobe = stackalloc float[8];
			tobe[0] = count;
			tobe[1] = (count                                     * mod);
			tobe[2] = (count * mod                               * mod);
			tobe[3] = (count * mod * mod                         * mod);
			tobe[4] = (count * mod * mod * mod                   * mod);
			tobe[5] = (count * mod * mod * mod * mod             * mod);
			tobe[6] = (count * mod * mod * mod * mod * mod       * mod);
			tobe[7] = (count * mod * mod * mod * mod * mod * mod * mod);
			return LoadVector256(tobe);
		}
		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<int> UnpackPermutationArrayExperimental(Vector256<int> xi)
		{
			fixed (int* pData = &p[0])
			{
				
				var arrc = stackalloc uint[8];
				Store(arrc, xi.AsUInt32());
				
				arrc[0] = (uint)*(pData + arrc[0]);
				arrc[1] = (uint)*(pData + arrc[1]);
				arrc[2] = (uint)*(pData + arrc[2]);
				arrc[3] = (uint)*(pData + arrc[3]);
				arrc[4] = (uint)*(pData + arrc[4]);
				arrc[5] = (uint)*(pData + arrc[5]);
				arrc[6] = (uint)*(pData + arrc[6]);
				arrc[7] = (uint)*(pData + arrc[7]);
				
				var ptrData = (uint) pData;
				// Console.WriteLine((uint) arrc);
				// Console.WriteLine(ptrData);
				var offset = LoadVectorCorrectly(ptrData);

				var arr = stackalloc int[8];
				var OffsetVec = Add(xi, Multiply(xi.AsSingle(),LoadVectorCorrectly(3f)).AsInt32());
				var offsettedPTR = Add(offset.AsUInt32(), OffsetVec.AsUInt32());
				
				Store(arr, offsettedPTR.AsInt32());
				
				Console.WriteLine((uint) arrc);
				Console.WriteLine(ptrData);
				string cwElement0 = $"0 ARRC Value: {arrc[0]} OffsetVecValue: {OffsetVec.GetElement(0)}";
				string cwElement1 = $"1 ARRC Value: {arrc[1]} OffsetVecValue: {OffsetVec.GetElement(1)}";
				string cwElement2 = $"2 ARRC Value: {arrc[2]} OffsetVecValue: {OffsetVec.GetElement(2)}";
				string cwElement3 = $"3 ARRC Value: {arrc[3]} OffsetVecValue: {OffsetVec.GetElement(3)}";
				string cwElement4 = $"4 ARRC Value: {arrc[4]} OffsetVecValue: {OffsetVec.GetElement(4)}";
				string cwElement5 = $"5 ARRC Value: {arrc[5]} OffsetVecValue: {OffsetVec.GetElement(5)}";
				string cwElement6 = $"6 ARRC Value: {arrc[6]} OffsetVecValue: {OffsetVec.GetElement(6)}";
				string cwElement7 = $"7 ARRC Value: {arrc[7]} OffsetVecValue: {OffsetVec.GetElement(7)}";
				
				cwElement0 += $" Correct PTR: {(uint)(pData + arrc[0])};";
				cwElement1 += $" Correct PTR: {(uint)(pData + arrc[1])};";
				cwElement2 += $" Correct PTR: {(uint)(pData + arrc[2])};";
				cwElement3 += $" Correct PTR: {(uint)(pData + arrc[3])};";
				cwElement4 += $" Correct PTR: {(uint)(pData + arrc[4])};";
				cwElement5 += $" Correct PTR: {(uint)(pData + arrc[5])};";
				cwElement6 += $" Correct PTR: {(uint)(pData + arrc[6])};";
				cwElement7 += $" Correct PTR: {(uint)(pData + arrc[7])};";
				Store(arrc, offsettedPTR);
				Console.WriteLine((uint) arrc);
				Console.WriteLine(ptrData);
				cwElement0+=$" Actual PTR: {(uint)(int*)arrc[0]}";
				cwElement1+=$" Actual PTR: {(uint)(int*)arrc[1]}";
				cwElement2+=$" Actual PTR: {(uint)(int*)arrc[2]}";
				cwElement3+=$" Actual PTR: {(uint)(int*)arrc[3]}";
				cwElement4+=$" Actual PTR: {(uint)(int*)arrc[4]}";
				cwElement5+=$" Actual PTR: {(uint)(int*)arrc[5]}";
				cwElement6+=$" Actual PTR: {(uint)(int*)arrc[6]}";
				cwElement7+=$" Actual PTR: {(uint)(int*)arrc[7]}";
				Console.WriteLine((uint) arrc);
				Console.WriteLine(ptrData);
				Console.WriteLine(cwElement0);
				Console.WriteLine(cwElement1);
				Console.WriteLine(cwElement2);
				Console.WriteLine(cwElement3);
				Console.WriteLine(cwElement4);
				Console.WriteLine(cwElement5);
				Console.WriteLine(cwElement6);
				Console.WriteLine(cwElement7);
				
				// arr[0] = *(int*)arr[0];
				// arr[1] = *(int*)arr[1];
				// arr[2] = *(int*)arr[2];
				// arr[3] = *(int*)arr[3];
				// arr[4] = *(int*)arr[4];
				// arr[5] = *(int*)arr[5];
				// arr[6] = *(int*)arr[6];
				// arr[7] = *(int*)arr[7];

				return LoadVectorCorrectly(0); //LoadVector256(arr).AsInt32();
			}
		}
	}
}