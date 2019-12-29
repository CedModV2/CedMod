// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.StringToDouble
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Internal.DoubleConversion
{
  internal static class StringToDouble
  {
    private static readonly double[] exact_powers_of_ten = new double[23]
    {
      1.0,
      10.0,
      100.0,
      1000.0,
      10000.0,
      100000.0,
      1000000.0,
      10000000.0,
      100000000.0,
      1000000000.0,
      10000000000.0,
      100000000000.0,
      1000000000000.0,
      10000000000000.0,
      100000000000000.0,
      1E+15,
      1E+16,
      1E+17,
      1E+18,
      1E+19,
      1E+20,
      1E+21,
      1E+22
    };
    private static readonly int kExactPowersOfTenSize = StringToDouble.exact_powers_of_ten.Length;
    [ThreadStatic]
    private static byte[] copyBuffer;
    private const int kMaxExactDoubleIntegerDecimalDigits = 15;
    private const int kMaxUint64DecimalDigits = 19;
    private const int kMaxDecimalPower = 309;
    private const int kMinDecimalPower = -324;
    private const ulong kMaxUint64 = 18446744073709551615;
    private const int kMaxSignificantDecimalDigits = 780;

    private static byte[] GetCopyBuffer()
    {
      if (StringToDouble.copyBuffer == null)
        StringToDouble.copyBuffer = new byte[780];
      return StringToDouble.copyBuffer;
    }

    private static Vector TrimLeadingZeros(Vector buffer)
    {
      for (int from = 0; from < buffer.length(); ++from)
      {
        if (buffer[from] != (byte) 48)
          return buffer.SubVector(from, buffer.length());
      }
      return new Vector(buffer.bytes, buffer.start, 0);
    }

    private static Vector TrimTrailingZeros(Vector buffer)
    {
      for (int index = buffer.length() - 1; index >= 0; --index)
      {
        if (buffer[index] != (byte) 48)
          return buffer.SubVector(0, index + 1);
      }
      return new Vector(buffer.bytes, buffer.start, 0);
    }

    private static void CutToMaxSignificantDigits(
      Vector buffer,
      int exponent,
      byte[] significant_buffer,
      out int significant_exponent)
    {
      for (int index = 0; index < 779; ++index)
        significant_buffer[index] = buffer[index];
      significant_buffer[779] = (byte) 49;
      significant_exponent = exponent + (buffer.length() - 780);
    }

    private static void TrimAndCut(
      Vector buffer,
      int exponent,
      byte[] buffer_copy_space,
      int space_size,
      out Vector trimmed,
      out int updated_exponent)
    {
      Vector buffer1 = StringToDouble.TrimLeadingZeros(buffer);
      Vector buffer2 = StringToDouble.TrimTrailingZeros(buffer1);
      exponent += buffer1.length() - buffer2.length();
      if (buffer2.length() > 780)
      {
        StringToDouble.CutToMaxSignificantDigits(buffer2, exponent, buffer_copy_space, out updated_exponent);
        trimmed = new Vector(buffer_copy_space, 0, 780);
      }
      else
      {
        trimmed = buffer2;
        updated_exponent = exponent;
      }
    }

    private static ulong ReadUint64(Vector buffer, out int number_of_read_digits)
    {
      ulong num1 = 0;
      int num2;
      int num3;
      for (num2 = 0; num2 < buffer.length() && num1 <= 1844674407370955160UL; num1 = 10UL * num1 + (ulong) num3)
        num3 = (int) buffer[num2++] - 48;
      number_of_read_digits = num2;
      return num1;
    }

    private static void ReadDiyFp(Vector buffer, out DiyFp result, out int remaining_decimals)
    {
      int number_of_read_digits;
      ulong significand = StringToDouble.ReadUint64(buffer, out number_of_read_digits);
      if (buffer.length() == number_of_read_digits)
      {
        result = new DiyFp(significand, 0);
        remaining_decimals = 0;
      }
      else
      {
        if (buffer[number_of_read_digits] >= (byte) 53)
          ++significand;
        int exponent = 0;
        result = new DiyFp(significand, exponent);
        remaining_decimals = buffer.length() - number_of_read_digits;
      }
    }

    private static bool DoubleStrtod(Vector trimmed, int exponent, out double result)
    {
      if (trimmed.length() <= 15)
      {
        int number_of_read_digits;
        if (exponent < 0 && -exponent < StringToDouble.kExactPowersOfTenSize)
        {
          result = (double) StringToDouble.ReadUint64(trimmed, out number_of_read_digits);
          result /= StringToDouble.exact_powers_of_ten[-exponent];
          return true;
        }
        if (0 <= exponent && exponent < StringToDouble.kExactPowersOfTenSize)
        {
          result = (double) StringToDouble.ReadUint64(trimmed, out number_of_read_digits);
          result *= StringToDouble.exact_powers_of_ten[exponent];
          return true;
        }
        int index = 15 - trimmed.length();
        if (0 <= exponent && exponent - index < StringToDouble.kExactPowersOfTenSize)
        {
          result = (double) StringToDouble.ReadUint64(trimmed, out number_of_read_digits);
          result *= StringToDouble.exact_powers_of_ten[index];
          result *= StringToDouble.exact_powers_of_ten[exponent - index];
          return true;
        }
      }
      result = 0.0;
      return false;
    }

    private static DiyFp AdjustmentPowerOfTen(int exponent)
    {
      switch (exponent)
      {
        case 1:
          return new DiyFp(11529215046068469760UL, -60);
        case 2:
          return new DiyFp(14411518807585587200UL, -57);
        case 3:
          return new DiyFp(18014398509481984000UL, -54);
        case 4:
          return new DiyFp(11258999068426240000UL, -50);
        case 5:
          return new DiyFp(14073748835532800000UL, -47);
        case 6:
          return new DiyFp(17592186044416000000UL, -44);
        case 7:
          return new DiyFp(10995116277760000000UL, -40);
        default:
          throw new Exception("unreached code.");
      }
    }

    private static bool DiyFpStrtod(Vector buffer, int exponent, out double result)
    {
      DiyFp result1;
      int remaining_decimals;
      StringToDouble.ReadDiyFp(buffer, out result1, out remaining_decimals);
      exponent += remaining_decimals;
      ulong num1 = remaining_decimals == 0 ? 0UL : 4UL;
      int e1 = result1.e;
      result1.Normalize();
      ulong num2 = num1 << e1 - result1.e;
      if (exponent < -348)
      {
        result = 0.0;
        return true;
      }
      DiyFp power;
      int found_exponent;
      PowersOfTenCache.GetCachedPowerForDecimalExponent(exponent, out power, out found_exponent);
      if (found_exponent != exponent)
      {
        int exponent1 = exponent - found_exponent;
        DiyFp other = StringToDouble.AdjustmentPowerOfTen(exponent1);
        result1.Multiply(ref other);
        if (19 - buffer.length() < exponent1)
          num2 += 4UL;
      }
      result1.Multiply(ref power);
      int num3 = 4;
      int num4 = num2 == 0UL ? 0 : 1;
      int num5 = 4;
      ulong num6 = num2 + (ulong) (num3 + num4 + num5);
      int e2 = result1.e;
      result1.Normalize();
      ulong num7 = num6 << e2 - result1.e;
      int num8 = 64 - Double.SignificandSizeForOrderOfMagnitude(64 + result1.e);
      if (num8 + 3 >= 64)
      {
        int num9 = num8 + 3 - 64 + 1;
        result1.f >>= num9;
        result1.e += num9;
        num7 = (num7 >> num9) + 1UL + 8UL;
        num8 -= num9;
      }
      long num10 = 1;
      ulong num11 = (ulong) (num10 << num8) - 1UL;
      ulong num12 = result1.f & num11;
      ulong num13 = (ulong) (num10 << num8 - 1);
      ulong num14 = num12 * 8UL;
      ulong num15 = num13 * 8UL;
      DiyFp d = new DiyFp(result1.f >> num8, result1.e + num8);
      if (num14 >= num15 + num7)
        ++d.f;
      result = new Double(d).value();
      return num15 - num7 >= num14 || num14 >= num15 + num7;
    }

    private static bool ComputeGuess(Vector trimmed, int exponent, out double guess)
    {
      if (trimmed.length() == 0)
      {
        guess = 0.0;
        return true;
      }
      if (exponent + trimmed.length() - 1 >= 309)
      {
        guess = Double.Infinity();
        return true;
      }
      if (exponent + trimmed.length() <= -324)
      {
        guess = 0.0;
        return true;
      }
      return StringToDouble.DoubleStrtod(trimmed, exponent, out guess) || StringToDouble.DiyFpStrtod(trimmed, exponent, out guess) || guess == Double.Infinity();
    }

    public static double? Strtod(Vector buffer, int exponent)
    {
      byte[] copyBuffer = StringToDouble.GetCopyBuffer();
      Vector trimmed;
      int updated_exponent;
      StringToDouble.TrimAndCut(buffer, exponent, copyBuffer, 780, out trimmed, out updated_exponent);
      exponent = updated_exponent;
      double guess;
      return StringToDouble.ComputeGuess(trimmed, exponent, out guess) ? new double?(guess) : new double?();
    }

    public static float? Strtof(Vector buffer, int exponent)
    {
      byte[] copyBuffer = StringToDouble.GetCopyBuffer();
      Vector trimmed;
      int updated_exponent;
      StringToDouble.TrimAndCut(buffer, exponent, copyBuffer, 780, out trimmed, out updated_exponent);
      exponent = updated_exponent;
      double guess1;
      bool guess2 = StringToDouble.ComputeGuess(trimmed, exponent, out guess1);
      float num1 = (float) guess1;
      if ((double) num1 == guess1)
        return new float?(num1);
      Double @double = new Double(guess1);
      double d = @double.NextDouble();
      @double = new Double(guess1);
      float num2 = (float) @double.PreviousDouble();
      float num3 = (float) d;
      float num4 = !guess2 ? (float) new Double(d).NextDouble() : num3;
      return (double) num2 == (double) num4 ? new float?(num1) : new float?();
    }
  }
}
