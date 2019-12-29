// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.DoubleToStringConverter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;

namespace Utf8Json.Internal.DoubleConversion
{
  internal static class DoubleToStringConverter
  {
    private static readonly byte[] infinity_symbol_ = StringEncoding.UTF8.GetBytes(double.PositiveInfinity.ToString());
    private static readonly byte[] nan_symbol_ = StringEncoding.UTF8.GetBytes(double.NaN.ToString());
    private static readonly DoubleToStringConverter.Flags flags_ = DoubleToStringConverter.Flags.EMIT_POSITIVE_EXPONENT_SIGN | DoubleToStringConverter.Flags.UNIQUE_ZERO;
    private static readonly char exponent_character_ = 'E';
    private static readonly int decimal_in_shortest_low_ = -4;
    private static readonly int decimal_in_shortest_high_ = 15;
    private static readonly uint[] kSmallPowersOfTen = new uint[11]
    {
      0U,
      1U,
      10U,
      100U,
      1000U,
      10000U,
      100000U,
      1000000U,
      10000000U,
      100000000U,
      1000000000U
    };
    [ThreadStatic]
    private static byte[] decimalRepBuffer;
    [ThreadStatic]
    private static byte[] exponentialRepBuffer;
    [ThreadStatic]
    private static byte[] toStringBuffer;
    private const int kBase10MaximalLength = 17;
    private const int kFastDtoaMaximalLength = 17;
    private const int kFastDtoaMaximalSingleLength = 9;
    private const int kMinimalTargetExponent = -60;
    private const int kMaximalTargetExponent = -32;

    private static byte[] GetDecimalRepBuffer(int size)
    {
      if (DoubleToStringConverter.decimalRepBuffer == null)
        DoubleToStringConverter.decimalRepBuffer = new byte[size];
      return DoubleToStringConverter.decimalRepBuffer;
    }

    private static byte[] GetExponentialRepBuffer(int size)
    {
      if (DoubleToStringConverter.exponentialRepBuffer == null)
        DoubleToStringConverter.exponentialRepBuffer = new byte[size];
      return DoubleToStringConverter.exponentialRepBuffer;
    }

    private static byte[] GetToStringBuffer()
    {
      if (DoubleToStringConverter.toStringBuffer == null)
        DoubleToStringConverter.toStringBuffer = new byte[24];
      return DoubleToStringConverter.toStringBuffer;
    }

    public static int GetBytes(ref byte[] buffer, int offset, float value)
    {
      StringBuilder result_builder = new StringBuilder(buffer, offset);
      if (!DoubleToStringConverter.ToShortestIeeeNumber((double) value, ref result_builder, DoubleToStringConverter.DtoaMode.SHORTEST_SINGLE))
        throw new InvalidOperationException("not support float value:" + (object) value);
      buffer = result_builder.buffer;
      return result_builder.offset - offset;
    }

    public static int GetBytes(ref byte[] buffer, int offset, double value)
    {
      StringBuilder result_builder = new StringBuilder(buffer, offset);
      if (!DoubleToStringConverter.ToShortestIeeeNumber(value, ref result_builder, DoubleToStringConverter.DtoaMode.SHORTEST))
        throw new InvalidOperationException("not support double value:" + (object) value);
      buffer = result_builder.buffer;
      return result_builder.offset - offset;
    }

    private static bool RoundWeed(
      byte[] buffer,
      int length,
      ulong distance_too_high_w,
      ulong unsafe_interval,
      ulong rest,
      ulong ten_kappa,
      ulong unit)
    {
      ulong num1 = distance_too_high_w - unit;
      ulong num2 = distance_too_high_w + unit;
      for (; rest < num1 && unsafe_interval - rest >= ten_kappa && (rest + ten_kappa < num1 || num1 - rest >= rest + ten_kappa - num1); rest += ten_kappa)
        --buffer[length - 1];
      return (rest >= num2 || unsafe_interval - rest < ten_kappa || rest + ten_kappa >= num2 && num2 - rest <= rest + ten_kappa - num2) && 2UL * unit <= rest && rest <= unsafe_interval - 4UL * unit;
    }

    private static void BiggestPowerTen(
      uint number,
      int number_bits,
      out uint power,
      out int exponent_plus_one)
    {
      int index = ((number_bits + 1) * 1233 >> 12) + 1;
      if (number < DoubleToStringConverter.kSmallPowersOfTen[index])
        --index;
      power = DoubleToStringConverter.kSmallPowersOfTen[index];
      exponent_plus_one = index;
    }

    private static bool DigitGen(
      DiyFp low,
      DiyFp w,
      DiyFp high,
      byte[] buffer,
      out int length,
      out int kappa)
    {
      ulong unit = 1;
      DiyFp b = new DiyFp(low.f - unit, low.e);
      DiyFp a = new DiyFp(high.f + unit, high.e);
      DiyFp diyFp1 = DiyFp.Minus(ref a, ref b);
      DiyFp diyFp2 = new DiyFp(1UL << -w.e, w.e);
      uint number = (uint) (a.f >> -diyFp2.e);
      ulong rest1 = a.f & diyFp2.f - 1UL;
      uint power;
      int exponent_plus_one;
      DoubleToStringConverter.BiggestPowerTen(number, 64 - -diyFp2.e, out power, out exponent_plus_one);
      kappa = exponent_plus_one;
      length = 0;
      while (kappa > 0)
      {
        int num = (int) (number / power);
        buffer[length] = (byte) (48 + num);
        ++length;
        number %= power;
        --kappa;
        ulong rest2 = ((ulong) number << -diyFp2.e) + rest1;
        if (rest2 < diyFp1.f)
          return DoubleToStringConverter.RoundWeed(buffer, length, DiyFp.Minus(ref a, ref w).f, diyFp1.f, rest2, (ulong) power << -diyFp2.e, unit);
        power /= 10U;
      }
      do
      {
        ulong num1 = rest1 * 10UL;
        unit *= 10UL;
        diyFp1.f *= 10UL;
        int num2 = (int) (num1 >> -diyFp2.e);
        buffer[length] = (byte) (48 + num2);
        ++length;
        rest1 = num1 & diyFp2.f - 1UL;
        --kappa;
      }
      while (rest1 >= diyFp1.f);
      return DoubleToStringConverter.RoundWeed(buffer, length, DiyFp.Minus(ref a, ref w).f * unit, diyFp1.f, rest1, diyFp2.f, unit);
    }

    private static bool Grisu3(
      double v,
      DoubleToStringConverter.FastDtoaMode mode,
      byte[] buffer,
      out int length,
      out int decimal_exponent)
    {
      DiyFp a = new Double(v).AsNormalizedDiyFp();
      DiyFp out_m_minus;
      DiyFp out_m_plus;
      if (mode == DoubleToStringConverter.FastDtoaMode.FAST_DTOA_SHORTEST)
      {
        new Double(v).NormalizedBoundaries(out out_m_minus, out out_m_plus);
      }
      else
      {
        if (mode != DoubleToStringConverter.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE)
          throw new Exception("Invalid Mode.");
        new Single((float) v).NormalizedBoundaries(out out_m_minus, out out_m_plus);
      }
      DiyFp power;
      int decimal_exponent1;
      PowersOfTenCache.GetCachedPowerForBinaryExponentRange(-60 - (a.e + 64), -32 - (a.e + 64), out power, out decimal_exponent1);
      DiyFp diyFp1 = DiyFp.Times(ref a, ref power);
      DiyFp low = DiyFp.Times(ref out_m_minus, ref power);
      DiyFp diyFp2 = DiyFp.Times(ref out_m_plus, ref power);
      DiyFp w = diyFp1;
      DiyFp high = diyFp2;
      byte[] buffer1 = buffer;
      ref int local1 = ref length;
      int num1;
      ref int local2 = ref num1;
      int num2 = DoubleToStringConverter.DigitGen(low, w, high, buffer1, out local1, out local2) ? 1 : 0;
      decimal_exponent = -decimal_exponent1 + num1;
      return num2 != 0;
    }

    private static bool FastDtoa(
      double v,
      DoubleToStringConverter.FastDtoaMode mode,
      byte[] buffer,
      out int length,
      out int decimal_point)
    {
      int decimal_exponent = 0;
      switch (mode)
      {
        case DoubleToStringConverter.FastDtoaMode.FAST_DTOA_SHORTEST:
        case DoubleToStringConverter.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE:
          bool flag = DoubleToStringConverter.Grisu3(v, mode, buffer, out length, out decimal_exponent);
          decimal_point = !flag ? -1 : length + decimal_exponent;
          return flag;
        default:
          throw new Exception("unreachable code.");
      }
    }

    private static bool HandleSpecialValues(double value, ref StringBuilder result_builder)
    {
      Double @double = new Double(value);
      if (@double.IsInfinite())
      {
        if (DoubleToStringConverter.infinity_symbol_ == null)
          return false;
        if (value < 0.0)
          result_builder.AddCharacter((byte) 45);
        result_builder.AddString(DoubleToStringConverter.infinity_symbol_);
        return true;
      }
      if (!@double.IsNan() || DoubleToStringConverter.nan_symbol_ == null)
        return false;
      result_builder.AddString(DoubleToStringConverter.nan_symbol_);
      return true;
    }

    private static bool ToShortestIeeeNumber(
      double value,
      ref StringBuilder result_builder,
      DoubleToStringConverter.DtoaMode mode)
    {
      if (new Double(value).IsSpecial())
        return DoubleToStringConverter.HandleSpecialValues(value, ref result_builder);
      byte[] decimalRepBuffer = DoubleToStringConverter.GetDecimalRepBuffer(18);
      bool sign;
      int length;
      int point;
      if (!DoubleToStringConverter.DoubleToAscii(value, mode, 0, decimalRepBuffer, out sign, out length, out point))
      {
        string str = value.ToString("G17", (IFormatProvider) CultureInfo.InvariantCulture);
        result_builder.AddStringSlow(str);
        return true;
      }
      bool flag = (uint) (DoubleToStringConverter.flags_ & DoubleToStringConverter.Flags.UNIQUE_ZERO) > 0U;
      if (sign && (value != 0.0 || !flag))
        result_builder.AddCharacter((byte) 45);
      int exponent = point - 1;
      if (DoubleToStringConverter.decimal_in_shortest_low_ <= exponent && exponent < DoubleToStringConverter.decimal_in_shortest_high_)
        DoubleToStringConverter.CreateDecimalRepresentation(decimalRepBuffer, length, point, Math.Max(0, length - point), ref result_builder);
      else
        DoubleToStringConverter.CreateExponentialRepresentation(decimalRepBuffer, length, exponent, ref result_builder);
      return true;
    }

    private static void CreateDecimalRepresentation(
      byte[] decimal_digits,
      int length,
      int decimal_point,
      int digits_after_point,
      ref StringBuilder result_builder)
    {
      if (decimal_point <= 0)
      {
        result_builder.AddCharacter((byte) 48);
        if (digits_after_point > 0)
        {
          result_builder.AddCharacter((byte) 46);
          result_builder.AddPadding((byte) 48, -decimal_point);
          result_builder.AddSubstring(decimal_digits, length);
          int count = digits_after_point - -decimal_point - length;
          result_builder.AddPadding((byte) 48, count);
        }
      }
      else if (decimal_point >= length)
      {
        result_builder.AddSubstring(decimal_digits, length);
        result_builder.AddPadding((byte) 48, decimal_point - length);
        if (digits_after_point > 0)
        {
          result_builder.AddCharacter((byte) 46);
          result_builder.AddPadding((byte) 48, digits_after_point);
        }
      }
      else
      {
        result_builder.AddSubstring(decimal_digits, decimal_point);
        result_builder.AddCharacter((byte) 46);
        result_builder.AddSubstring(decimal_digits, decimal_point, length - decimal_point);
        int count = digits_after_point - (length - decimal_point);
        result_builder.AddPadding((byte) 48, count);
      }
      if (digits_after_point != 0)
        return;
      if ((DoubleToStringConverter.flags_ & DoubleToStringConverter.Flags.EMIT_TRAILING_DECIMAL_POINT) != DoubleToStringConverter.Flags.NO_FLAGS)
        result_builder.AddCharacter((byte) 46);
      if ((DoubleToStringConverter.flags_ & DoubleToStringConverter.Flags.EMIT_TRAILING_ZERO_AFTER_POINT) == DoubleToStringConverter.Flags.NO_FLAGS)
        return;
      result_builder.AddCharacter((byte) 48);
    }

    private static void CreateExponentialRepresentation(
      byte[] decimal_digits,
      int length,
      int exponent,
      ref StringBuilder result_builder)
    {
      result_builder.AddCharacter(decimal_digits[0]);
      if (length != 1)
      {
        result_builder.AddCharacter((byte) 46);
        result_builder.AddSubstring(decimal_digits, 1, length - 1);
      }
      result_builder.AddCharacter((byte) DoubleToStringConverter.exponent_character_);
      if (exponent < 0)
      {
        result_builder.AddCharacter((byte) 45);
        exponent = -exponent;
      }
      else if ((DoubleToStringConverter.flags_ & DoubleToStringConverter.Flags.EMIT_POSITIVE_EXPONENT_SIGN) != DoubleToStringConverter.Flags.NO_FLAGS)
        result_builder.AddCharacter((byte) 43);
      if (exponent == 0)
      {
        result_builder.AddCharacter((byte) 48);
      }
      else
      {
        byte[] exponentialRepBuffer = DoubleToStringConverter.GetExponentialRepBuffer(6);
        exponentialRepBuffer[5] = (byte) 0;
        int start = 5;
        for (; exponent > 0; exponent /= 10)
          exponentialRepBuffer[--start] = (byte) (48 + exponent % 10);
        result_builder.AddSubstring(exponentialRepBuffer, start, 5 - start);
      }
    }

    private static bool DoubleToAscii(
      double v,
      DoubleToStringConverter.DtoaMode mode,
      int requested_digits,
      byte[] vector,
      out bool sign,
      out int length,
      out int point)
    {
      if (new Double(v).Sign() < 0)
      {
        sign = true;
        v = -v;
      }
      else
        sign = false;
      if (v == 0.0)
      {
        vector[0] = (byte) 48;
        length = 1;
        point = 1;
        return true;
      }
      if (mode == DoubleToStringConverter.DtoaMode.SHORTEST)
        return DoubleToStringConverter.FastDtoa(v, DoubleToStringConverter.FastDtoaMode.FAST_DTOA_SHORTEST, vector, out length, out point);
      if (mode == DoubleToStringConverter.DtoaMode.SHORTEST_SINGLE)
        return DoubleToStringConverter.FastDtoa(v, DoubleToStringConverter.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE, vector, out length, out point);
      throw new Exception("Unreachable code.");
    }

    private enum FastDtoaMode
    {
      FAST_DTOA_SHORTEST,
      FAST_DTOA_SHORTEST_SINGLE,
    }

    private enum DtoaMode
    {
      SHORTEST,
      SHORTEST_SINGLE,
    }

    private enum Flags
    {
      NO_FLAGS = 0,
      EMIT_POSITIVE_EXPONENT_SIGN = 1,
      EMIT_TRAILING_DECIMAL_POINT = 2,
      EMIT_TRAILING_ZERO_AFTER_POINT = 4,
      UNIQUE_ZERO = 8,
    }
  }
}
