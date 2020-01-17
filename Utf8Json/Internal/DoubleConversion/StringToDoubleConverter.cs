// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.StringToDoubleConverter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Text;

namespace Utf8Json.Internal.DoubleConversion
{
  internal static class StringToDoubleConverter
  {
    private static readonly byte[] infinity_symbol_ = StringEncoding.UTF8.GetBytes(double.PositiveInfinity.ToString());
    private static readonly byte[] nan_symbol_ = StringEncoding.UTF8.GetBytes(double.NaN.ToString());
    private static readonly byte[] kWhitespaceTable7 = new byte[6]
    {
      (byte) 32,
      (byte) 13,
      (byte) 10,
      (byte) 9,
      (byte) 11,
      (byte) 12
    };
    private static readonly int kWhitespaceTable7Length = StringToDoubleConverter.kWhitespaceTable7.Length;
    private static readonly ushort[] kWhitespaceTable16 = new ushort[20]
    {
      (ushort) 160,
      (ushort) 8232,
      (ushort) 8233,
      (ushort) 5760,
      (ushort) 6158,
      (ushort) 8192,
      (ushort) 8193,
      (ushort) 8194,
      (ushort) 8195,
      (ushort) 8196,
      (ushort) 8197,
      (ushort) 8198,
      (ushort) 8199,
      (ushort) 8200,
      (ushort) 8201,
      (ushort) 8202,
      (ushort) 8239,
      (ushort) 8287,
      (ushort) 12288,
      (ushort) 65279
    };
    private static readonly int kWhitespaceTable16Length = StringToDoubleConverter.kWhitespaceTable16.Length;
    [ThreadStatic]
    private static byte[] kBuffer;
    [ThreadStatic]
    private static byte[] fallbackBuffer;
    private const StringToDoubleConverter.Flags flags_ = StringToDoubleConverter.Flags.ALLOW_TRAILING_JUNK | StringToDoubleConverter.Flags.ALLOW_TRAILING_SPACES | StringToDoubleConverter.Flags.ALLOW_SPACES_AFTER_SIGN;
    private const double empty_string_value_ = 0.0;
    private const double junk_string_value_ = double.NaN;
    private const int kMaxSignificantDigits = 772;
    private const int kBufferSize = 782;

    private static byte[] GetBuffer()
    {
      if (StringToDoubleConverter.kBuffer == null)
        StringToDoubleConverter.kBuffer = new byte[782];
      return StringToDoubleConverter.kBuffer;
    }

    private static byte[] GetFallbackBuffer()
    {
      if (StringToDoubleConverter.fallbackBuffer == null)
        StringToDoubleConverter.fallbackBuffer = new byte[99];
      return StringToDoubleConverter.fallbackBuffer;
    }

    public static double ToDouble(byte[] buffer, int offset, out int readCount)
    {
      return StringToDoubleConverter.StringToIeee(new Iterator(buffer, offset), buffer.Length - offset, true, out readCount);
    }

    public static float ToSingle(byte[] buffer, int offset, out int readCount)
    {
      return (float) StringToDoubleConverter.StringToIeee(new Iterator(buffer, offset), buffer.Length - offset, false, out readCount);
    }

    private static bool isWhitespace(int x)
    {
      if (x < 128)
      {
        for (int index = 0; index < StringToDoubleConverter.kWhitespaceTable7Length; ++index)
        {
          if ((int) StringToDoubleConverter.kWhitespaceTable7[index] == x)
            return true;
        }
      }
      else
      {
        for (int index = 0; index < StringToDoubleConverter.kWhitespaceTable16Length; ++index)
        {
          if ((int) StringToDoubleConverter.kWhitespaceTable16[index] == x)
            return true;
        }
      }
      return false;
    }

    private static bool AdvanceToNonspace(ref Iterator current, Iterator end)
    {
      while (current != end)
      {
        if (!StringToDoubleConverter.isWhitespace((int) current.Value))
          return true;
        ++current;
      }
      return false;
    }

    private static bool ConsumeSubString(ref Iterator current, Iterator end, byte[] substring)
    {
      for (int index = 1; index < substring.Length; ++index)
      {
        ++current;
        if (current == end || current != substring[index])
          return false;
      }
      ++current;
      return true;
    }

    private static bool ConsumeFirstCharacter(ref Iterator iter, byte[] str, int offset)
    {
      return (int) iter.Value == (int) str[offset];
    }

    private static double SignedZero(bool sign)
    {
      return !sign ? 0.0 : -0.0;
    }

    private static double StringToIeee(
      Iterator input,
      int length,
      bool read_as_double,
      out int processed_characters_count)
    {
      Iterator self = input;
      Iterator end = input + length;
      processed_characters_count = 0;
      bool flag1 = true;
      bool flag2 = false;
      bool flag3 = true;
      bool flag4 = true;
      if (length == 0)
        return 0.0;
      if (flag2 | flag3)
      {
        if (!StringToDoubleConverter.AdvanceToNonspace(ref self, end))
        {
          processed_characters_count = self - input;
          return 0.0;
        }
        if (!flag2 && input != self)
          return double.NaN;
      }
      byte[] buffer = StringToDoubleConverter.GetBuffer();
      int length1 = 0;
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      bool flag5 = false;
      bool sign = false;
      if (self == '+' || self == '-')
      {
        sign = self == '-';
        Iterator iterator = self++;
        Iterator current = iterator;
        if (!StringToDoubleConverter.AdvanceToNonspace(ref current, end) || !flag4 && iterator != current)
          return double.NaN;
        self = current;
      }
      if (StringToDoubleConverter.infinity_symbol_ != null && StringToDoubleConverter.ConsumeFirstCharacter(ref self, StringToDoubleConverter.infinity_symbol_, 0))
      {
        if (!StringToDoubleConverter.ConsumeSubString(ref self, end, StringToDoubleConverter.infinity_symbol_) || !(flag3 | flag1) && self != end || !flag1 && StringToDoubleConverter.AdvanceToNonspace(ref self, end))
          return double.NaN;
        processed_characters_count = self - input;
        return !sign ? double.PositiveInfinity : double.NegativeInfinity;
      }
      if (StringToDoubleConverter.nan_symbol_ != null && StringToDoubleConverter.ConsumeFirstCharacter(ref self, StringToDoubleConverter.nan_symbol_, 0))
      {
        if (!StringToDoubleConverter.ConsumeSubString(ref self, end, StringToDoubleConverter.nan_symbol_) || !(flag3 | flag1) && self != end || !flag1 && StringToDoubleConverter.AdvanceToNonspace(ref self, end))
          return double.NaN;
        processed_characters_count = self - input;
        return !sign ? double.NaN : double.NaN;
      }
      bool flag6 = false;
      if (self == '0')
      {
        ++self;
        if (self == end)
        {
          processed_characters_count = self - input;
          return StringToDoubleConverter.SignedZero(sign);
        }
        flag6 = true;
        while (self == '0')
        {
          ++self;
          if (self == end)
          {
            processed_characters_count = self - input;
            return StringToDoubleConverter.SignedZero(sign);
          }
        }
      }
      bool flag7 = !flag6 && false;
      while (self >= '0' && self <= '9')
      {
        if (num2 < 772)
        {
          buffer[length1++] = self.Value;
          ++num2;
        }
        else
        {
          ++num3;
          flag5 = flag5 || self != '0';
        }
        ++self;
        if (self == end)
          goto label_78;
      }
      if (num2 == 0)
        flag7 = false;
      if (self == '.')
      {
        if (flag7 && !flag1)
          return double.NaN;
        if (!flag7)
        {
          ++self;
          if (self == end)
          {
            if (num2 == 0 && !flag6)
              return double.NaN;
            goto label_78;
          }
          else
          {
            if (num2 == 0)
            {
              while (self == '0')
              {
                ++self;
                if (self == end)
                {
                  processed_characters_count = self - input;
                  return StringToDoubleConverter.SignedZero(sign);
                }
                --num1;
              }
            }
            while (self >= '0' && self <= '9')
            {
              if (num2 < 772)
              {
                buffer[length1++] = self.Value;
                ++num2;
                --num1;
              }
              else
                flag5 = flag5 || self != '0';
              ++self;
              if (self == end)
                goto label_78;
            }
          }
        }
        else
          goto label_78;
      }
      if (!flag6 && num1 == 0 && num2 == 0)
        return double.NaN;
      if (self == 'e' || self == 'E')
      {
        if (flag7 && !flag1)
          return double.NaN;
        if (!flag7)
        {
          ++self;
          if (self == end)
          {
            if (!flag1)
              return double.NaN;
            goto label_78;
          }
          else
          {
            byte num4 = 43;
            if (self == '+' || self == '-')
            {
              num4 = self.Value;
              ++self;
              if (self == end)
              {
                if (!flag1)
                  return double.NaN;
                goto label_78;
              }
            }
            if (self == end || self < '0' || self > '9')
            {
              if (!flag1)
                return double.NaN;
              goto label_78;
            }
            else
            {
              int num5 = 0;
              do
              {
                int num6 = (int) self.Value - 48;
                num5 = num5 < 107374182 || num5 == 107374182 && num6 <= 3 ? num5 * 10 + num6 : 1073741823;
                ++self;
              }
              while (self != end && self >= '0' && self <= '9');
              num1 += num4 == (byte) 45 ? -num5 : num5;
            }
          }
        }
        else
          goto label_78;
      }
      if (!(flag3 | flag1) && self != end || !flag1 && StringToDoubleConverter.AdvanceToNonspace(ref self, end))
        return double.NaN;
      if (flag3)
        StringToDoubleConverter.AdvanceToNonspace(ref self, end);
label_78:
      int exponent = num1 + num3;
      if (flag5)
      {
        buffer[length1++] = (byte) 49;
        --exponent;
      }
      buffer[length1] = (byte) 0;
      double? nullable1;
      if (read_as_double)
      {
        nullable1 = StringToDouble.Strtod(new Vector(buffer, 0, length1), exponent);
      }
      else
      {
        float? nullable2 = StringToDouble.Strtof(new Vector(buffer, 0, length1), exponent);
        nullable1 = nullable2.HasValue ? new double?((double) nullable2.GetValueOrDefault()) : new double?();
      }
      if (!nullable1.HasValue)
      {
        processed_characters_count = self - input;
        byte[] fallbackBuffer = StringToDoubleConverter.GetFallbackBuffer();
        BinaryUtil.EnsureCapacity(ref StringToDoubleConverter.fallbackBuffer, 0, processed_characters_count);
        int count = 0;
        while (input != self)
        {
          fallbackBuffer[count++] = input.Value;
          ++input;
        }
        return double.Parse(Encoding.UTF8.GetString(fallbackBuffer, 0, count));
      }
      processed_characters_count = self - input;
      return !sign ? nullable1.Value : -nullable1.Value;
    }

    private enum Flags
    {
      NO_FLAGS = 0,
      ALLOW_HEX = 1,
      ALLOW_OCTALS = 2,
      ALLOW_TRAILING_JUNK = 4,
      ALLOW_LEADING_SPACES = 8,
      ALLOW_TRAILING_SPACES = 16, // 0x00000010
      ALLOW_SPACES_AFTER_SIGN = 32, // 0x00000020
      ALLOW_CASE_INSENSIBILITY = 64, // 0x00000040
    }
  }
}
