// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.Single
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal.DoubleConversion
{
  internal struct Single
  {
    private const int kExponentBias = 150;
    private const int kDenormalExponent = -149;
    private const int kMaxExponent = 105;
    private const uint kInfinity = 2139095040;
    private const uint kNaN = 2143289344;
    public const uint kSignMask = 2147483648;
    public const uint kExponentMask = 2139095040;
    public const uint kSignificandMask = 8388607;
    public const uint kHiddenBit = 8388608;
    public const int kPhysicalSignificandSize = 23;
    public const int kSignificandSize = 24;
    private uint d32_;

    public Single(float f)
    {
      this.d32_ = new UnionFloatUInt() { f = f }.u32;
    }

    public DiyFp AsDiyFp()
    {
      return new DiyFp((ulong) this.Significand(), this.Exponent());
    }

    public uint AsUint32()
    {
      return this.d32_;
    }

    public int Exponent()
    {
      return this.IsDenormal() ? -149 : (int) ((this.AsUint32() & 2139095040U) >> 23) - 150;
    }

    public uint Significand()
    {
      uint num = this.AsUint32() & 8388607U;
      return !this.IsDenormal() ? num + 8388608U : num;
    }

    public bool IsDenormal()
    {
      return ((int) this.AsUint32() & 2139095040) == 0;
    }

    public bool IsSpecial()
    {
      return ((int) this.AsUint32() & 2139095040) == 2139095040;
    }

    public bool IsNan()
    {
      uint num = this.AsUint32();
      return ((int) num & 2139095040) == 2139095040 && (num & 8388607U) > 0U;
    }

    public bool IsInfinite()
    {
      uint num = this.AsUint32();
      return ((int) num & 2139095040) == 2139095040 && ((int) num & 8388607) == 0;
    }

    public int Sign()
    {
      return ((int) this.AsUint32() & int.MinValue) != 0 ? -1 : 1;
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

    public DiyFp UpperBoundary()
    {
      return new DiyFp((ulong) (uint) ((int) this.Significand() * 2 + 1), this.Exponent() - 1);
    }

    public bool LowerBoundaryIsCloser()
    {
      return ((int) this.AsUint32() & 8388607) == 0 && this.Exponent() != -149;
    }

    public float value()
    {
      return new UnionFloatUInt() { u32 = this.d32_ }.f;
    }

    public static float Infinity()
    {
      return new Single(2.139095E+09f).value();
    }

    public static float NaN()
    {
      return new Single(2.143289E+09f).value();
    }
  }
}
