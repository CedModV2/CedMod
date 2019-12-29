// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.Double
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal.DoubleConversion
{
  internal struct Double
  {
    public const ulong kSignMask = 9223372036854775808;
    public const ulong kExponentMask = 9218868437227405312;
    public const ulong kSignificandMask = 4503599627370495;
    public const ulong kHiddenBit = 4503599627370496;
    public const int kPhysicalSignificandSize = 52;
    public const int kSignificandSize = 53;
    private const int kExponentBias = 1075;
    private const int kDenormalExponent = -1074;
    private const int kMaxExponent = 972;
    private const ulong kInfinity = 9218868437227405312;
    private const ulong kNaN = 9221120237041090560;
    private ulong d64_;

    public Double(double d)
    {
      this.d64_ = new UnionDoubleULong() { d = d }.u64;
    }

    public Double(DiyFp d)
    {
      this.d64_ = Double.DiyFpToUint64(d);
    }

    public DiyFp AsDiyFp()
    {
      return new DiyFp(this.Significand(), this.Exponent());
    }

    public DiyFp AsNormalizedDiyFp()
    {
      ulong num1 = this.Significand();
      int num2 = this.Exponent();
      while (((long) num1 & 4503599627370496L) == 0L)
      {
        num1 <<= 1;
        --num2;
      }
      return new DiyFp(num1 << 11, num2 - 11);
    }

    public ulong AsUint64()
    {
      return this.d64_;
    }

    public double NextDouble()
    {
      if (this.d64_ == 9218868437227405312UL)
        return new Double(9.21886843722741E+18).value();
      if (this.Sign() < 0 && this.Significand() == 0UL)
        return 0.0;
      return this.Sign() < 0 ? new Double((double) (this.d64_ - 1UL)).value() : new Double((double) (this.d64_ + 1UL)).value();
    }

    public double PreviousDouble()
    {
      if (this.d64_ == 18442240474082181120UL)
        return -Double.Infinity();
      if (this.Sign() < 0)
        return new Double((double) (this.d64_ + 1UL)).value();
      return this.Significand() == 0UL ? -0.0 : new Double((double) (this.d64_ - 1UL)).value();
    }

    public int Exponent()
    {
      return this.IsDenormal() ? -1074 : (int) ((this.AsUint64() & 9218868437227405312UL) >> 52) - 1075;
    }

    public ulong Significand()
    {
      ulong num = this.AsUint64() & 4503599627370495UL;
      return !this.IsDenormal() ? num + 4503599627370496UL : num;
    }

    public bool IsDenormal()
    {
      return ((long) this.AsUint64() & 9218868437227405312L) == 0L;
    }

    public bool IsSpecial()
    {
      return ((long) this.AsUint64() & 9218868437227405312L) == 9218868437227405312L;
    }

    public bool IsNan()
    {
      ulong num = this.AsUint64();
      return ((long) num & 9218868437227405312L) == 9218868437227405312L && (num & 4503599627370495UL) > 0UL;
    }

    public bool IsInfinite()
    {
      ulong num = this.AsUint64();
      return ((long) num & 9218868437227405312L) == 9218868437227405312L && ((long) num & 4503599627370495L) == 0L;
    }

    public int Sign()
    {
      return ((long) this.AsUint64() & long.MinValue) != 0L ? -1 : 1;
    }

    public DiyFp UpperBoundary()
    {
      return new DiyFp(this.Significand() * 2UL + 1UL, this.Exponent() - 1);
    }

    public void NormalizedBoundaries(out DiyFp out_m_minus, out DiyFp out_m_plus)
    {
      DiyFp diyFp1 = this.AsDiyFp();
      DiyFp a = new DiyFp((diyFp1.f << 1) + 1UL, diyFp1.e - 1);
      DiyFp diyFp2 = DiyFp.Normalize(ref a);
      DiyFp diyFp3 = !this.LowerBoundaryIsCloser() ? new DiyFp((diyFp1.f << 1) - 1UL, diyFp1.e - 1) : new DiyFp((diyFp1.f << 2) - 1UL, diyFp1.e - 2);
      diyFp3.f <<= diyFp3.e - diyFp2.e;
      diyFp3.e = diyFp2.e;
      out_m_plus = diyFp2;
      out_m_minus = diyFp3;
    }

    public bool LowerBoundaryIsCloser()
    {
      return ((long) this.AsUint64() & 4503599627370495L) == 0L && this.Exponent() != -1074;
    }

    public double value()
    {
      return new UnionDoubleULong() { u64 = this.d64_ }.d;
    }

    public static int SignificandSizeForOrderOfMagnitude(int order)
    {
      if (order >= -1021)
        return 53;
      return order <= -1074 ? 0 : order - -1074;
    }

    public static double Infinity()
    {
      return new Double(9.21886843722741E+18).value();
    }

    public static double NaN()
    {
      return new Double(9.22112023704109E+18).value();
    }

    public static ulong DiyFpToUint64(DiyFp diy_fp)
    {
      ulong f = diy_fp.f;
      int e = diy_fp.e;
      while (f > 9007199254740991UL)
      {
        f >>= 1;
        ++e;
      }
      if (e >= 972)
        return 9218868437227405312;
      if (e < -1074)
        return 0;
      for (; e > -1074 && ((long) f & 4503599627370496L) == 0L; --e)
        f <<= 1;
      ulong num = e != -1074 || ((long) f & 4503599627370496L) != 0L ? (ulong) (e + 1075) : 0UL;
      return (ulong) ((long) f & 4503599627370495L | (long) num << 52);
    }
  }
}
