using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;

namespace AVXPerlinNoise
{
	public partial class Perlin
	{

		public static Vector256<double> perlinAVX(Vector256<double> x, Vector256<double> y, Vector256<double> z)
		{
			var xi =
				ConvertToVector256Int64(And(Vector256.Create(ConvertToVector128Int32WithTruncation(x), Vector128<int>.Zero),
				                            Vector256.Create(255)).GetLower());
			var yi =
				ConvertToVector256Int64(And(Vector256.Create(ConvertToVector128Int32WithTruncation(y), Vector128<int>.Zero),
				                            Vector256.Create(255)).GetLower());
			var zi =
				ConvertToVector256Int64(And(Vector256.Create(ConvertToVector128Int32WithTruncation(z), Vector128<int>.Zero),
				                            Vector256.Create(255)).GetLower());

			var xf = Subtract(x, ConvertToVector256Double(ConvertToVector128Int32WithTruncation(x)));
			var yf = Subtract(y, ConvertToVector256Double(ConvertToVector128Int32WithTruncation(y)));
			var zf = Subtract(z, ConvertToVector256Double(ConvertToVector128Int32WithTruncation(z)));

			var u = fadeAVX(xf);
			var v = fadeAVX(yf);
			var w = fadeAVX(zf);

			var a  = UnpackPermutationArrayAndAdd(xi,                            yi);
			var aa = UnpackPermutationArrayAndAdd(a,                             zi);
			var ab = UnpackPermutationArrayAndAdd(Add(a,  Vector256.Create(1L)), zi);
			var b  = UnpackPermutationArrayAndAdd(Add(xi, Vector256.Create(1L)), yi);
			var ba = UnpackPermutationArrayAndAdd(b,                             zi);
			var bb = UnpackPermutationArrayAndAdd(Add(b, Vector256.Create(1L)),  zi);

			var x1 = lerpAVX(gradAVX(UnpackPermutationArray(aa), xf,                                 yf, zf),
			                 gradAVX(UnpackPermutationArray(ba), Subtract(xf, Vector256.Create(1D)), yf, zf),
			                 u);
			var x2 = lerpAVX(gradAVX(UnpackPermutationArray(ab), xf, Subtract(yf, Vector256.Create(1D)), zf),
			                 gradAVX(UnpackPermutationArray(bb), Subtract(xf, Vector256.Create(1D)),
			                         Subtract(yf,                             Vector256.Create(1D)), zf),
			                 u);
			var y1 = lerpAVX(x1, x2, v);

			x1 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(aa, Vector256.Create(1L))), xf, yf, Subtract(zf, Vector256.Create(1D))),
				        gradAVX(UnpackPermutationArray(ba), Subtract(xf, Vector256.Create(1D)), yf,
				                Subtract(zf,                             Vector256.Create(1D))),
				        u);

			x2 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(ab, Vector256.Create(1L))), xf, Subtract(yf, Vector256.Create(1D)), Subtract(zf, Vector256.Create(1D))),
				        gradAVX(UnpackPermutationArray(Add(bb, Vector256.Create(1L))),
				                Subtract(xf, Vector256.Create(1D)), Subtract(yf, Vector256.Create(1D)),
				                Subtract(zf, Vector256.Create(1D))),
				        u);


			var y2 = lerpAVX(x1, x2, v);

			return Divide(Add(lerpAVX(y1, y2, w), Vector256.Create(1D)), Vector256.Create(2D));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> lerpAVX(Vector256<double> a, Vector256<double> b, Vector256<double> x)
			=> Add(a, Multiply(x, Subtract(b, a)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> fadeAVX(Vector256<double> t)
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
			                                           Vector256.Create(6D)
			                                          ),
			                                  Vector256.Create(15D)
			                                 )
			                        ),
			                Vector256.Create(10D)
			               )
			           );

		private static Vector256<long> UnpackPermutationArrayAndAdd(Vector256<long> xi, Vector256<long> yi)
			=> Add(UnpackPermutationArray(xi), yi);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe Vector256<long> UnpackPermutationArray(Vector256<long> xi)
		{
			fixed (long* pData = &pL[0])
			{
				var arr = stackalloc long[4];
				Store(arr, xi);

				for (var i = 0; i < 4; i++)
				{
					arr[i] = *(pData + arr[i]);
				}

				return LoadVector256(arr);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> gradAVXUVector(Vector256<long>   hashs, Vector256<double> xs,
		                                               Vector256<double> ys)
		{
			// If the most signifigant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.
			var MSB = Vector256.Create(0b1000L);
			var h   = CompareGreaterThan(MSB, hashs);

			var usX = And(h.AsDouble(), xs);
			var usY = AndNot(h.AsDouble(), ys);

			return Add(usX, usY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> gradAVXVYVector(Vector256<long> hashs, Vector256<double> ys)
		{
			// If the first and second signifigant bits are 0 set v = y
			var SSB  = Vector256.Create(0b0100L);
			var ssbh = CompareGreaterThan(SSB, hashs); // "< 4" == "4 >" 

			return And(ssbh.AsDouble(), ys);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> gradAVXVXVector(Vector256<long> hashs, Vector256<double> xs)
		{
			// If the first and second signifigant bits are 0 set v = y
			var TSB1  = Vector256.Create(0b1100L);
			var TSB2  = Vector256.Create(0b1110L);
			var TSB1h = CompareEqual(hashs, TSB1);
			var TSB2h = CompareEqual(hashs, TSB2);

			var vsX1 = And(TSB1h.AsDouble(), xs);
			var vsX2 = And(TSB2h.AsDouble(), xs);

			return Add(vsX1, vsX2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> gradAVXVZVector(Vector256<long> hashs, Vector256<double> zs)
		{
			// If the first and second signifigant bits are 0 set v = y
			var SSB  = Vector256.Create(4L);
			var TSB1 = Vector256.Create(12L);
			var TSB2 = Vector256.Create(13L);

			var bigger4   = CompareGreaterThan(hashs, SSB);
			var smaller12 = CompareGreaterThan(TSB1,  hashs);

			var range1 = And(bigger4, smaller12);

			var eq13 = CompareEqual(hashs, TSB2);

			var range = Or(range1, eq13);

			return And(range.AsDouble(), zs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> gradAVXVVector(Vector256<long>   hashs, Vector256<double> xs,
		                                               Vector256<double> ys,
		                                               Vector256<double> zs)
		{
			var VX = gradAVXVXVector(hashs, xs);
			var VY = gradAVXVYVector(hashs, ys);
			var VZ = gradAVXVZVector(hashs, zs);

			return Add(Add(VX, VY), VZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Vector256<double> gradAVXPartial(Vector256<long> hashs, Vector256<double> u, long toAndWith)
		{
			var hAnd1  = And(hashs, Vector256.Create(toAndWith));
			var hZeros = CompareEqual(hAnd1, Vector256<long>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<long>.Zero);

			var ucomp    = And(hZeros.AsDouble(), u);
			var ucompneg = Subtract(Vector256<double>.Zero, And(hOnes.AsDouble(), u));
			return Add(ucomp, ucompneg);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector256<double> gradAVX(Vector256<long>   hashs, Vector256<double> xs, Vector256<double> ys,
		                                        Vector256<double> zs)
		{
			var d15 = Vector256.Create(0b1111L);
			var hs  = And(hashs, d15);

			var u = gradAVXUVector(hs, xs, ys);
			var v = gradAVXVVector(hs, xs, ys, zs);

			return Add(gradAVXPartial(hs, u, 1L), gradAVXPartial(hs, v, 2L));
		}
	}
}