// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.DiyFp
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal.DoubleConversion
{
  internal struct DiyFp
  {
    public const int kSignificandSize = 64;
    public const ulong kUint64MSB = 9223372036854775808;
    public ulong f;
    public int e;

    public DiyFp(ulong significand, int exponent)
    {
      this.f = significand;
      this.e = exponent;
    }

    public void Subtract(ref DiyFp other)
    {
      this.f -= other.f;
    }

    public static DiyFp Minus(ref DiyFp a, ref DiyFp b)
    {
      DiyFp diyFp = a;
      diyFp.Subtract(ref b);
      return diyFp;
    }

    public static DiyFp operator -(DiyFp lhs, DiyFp rhs)
    {
      return DiyFp.Minus(ref lhs, ref rhs);
    }

    public void Multiply(ref DiyFp other)
    {
      long num1 = (long) (this.f >> 32);
      ulong num2 = this.f & (ulong) uint.MaxValue;
      ulong num3 = other.f >> 32;
      ulong num4 = other.f & (ulong) uint.MaxValue;
      ulong num5 = (ulong) num1 * num3;
      ulong num6 = num2 * num3;
      ulong num7 = (ulong) num1 * num4;
      ulong num8 = (ulong) ((long) (num2 * num4 >> 32) + ((long) num7 & (long) uint.MaxValue) + ((long) num6 & (long) uint.MaxValue)) + 2147483648UL;
      ulong num9 = num5 + (num7 >> 32) + (num6 >> 32) + (num8 >> 32);
      this.e += other.e + 64;
      this.f = num9;
    }

    public static DiyFp Times(ref DiyFp a, ref DiyFp b)
    {
      DiyFp diyFp = a;
      diyFp.Multiply(ref b);
      return diyFp;
    }

    public static DiyFp operator *(DiyFp lhs, DiyFp rhs)
    {
      return DiyFp.Times(ref lhs, ref rhs);
    }

    public void Normalize()
    {
      ulong f = this.f;
      int e = this.e;
      while (((long) f & -18014398509481984L) == 0L)
      {
        f <<= 10;
        e -= 10;
      }
      while (((long) f & long.MinValue) == 0L)
      {
        f <<= 1;
        --e;
      }
      this.f = f;
      this.e = e;
    }

    public static DiyFp Normalize(ref DiyFp a)
    {
      DiyFp diyFp = a;
      diyFp.Normalize();
      return diyFp;
    }
  }
}
