// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.NumberConverter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal.DoubleConversion;

namespace Utf8Json.Internal
{
  public static class NumberConverter
  {
    public static bool IsNumber(byte c)
    {
      return (byte) 48 <= c && c <= (byte) 57;
    }

    public static bool IsNumberRepresentation(byte c)
    {
      switch (c)
      {
        case 43:
        case 45:
        case 46:
        case 48:
        case 49:
        case 50:
        case 51:
        case 52:
        case 53:
        case 54:
        case 55:
        case 56:
        case 57:
          return true;
        default:
          return false;
      }
    }

    public static sbyte ReadSByte(byte[] bytes, int offset, out int readCount)
    {
      return checked ((sbyte) NumberConverter.ReadInt64(bytes, offset, out readCount));
    }

    public static short ReadInt16(byte[] bytes, int offset, out int readCount)
    {
      return checked ((short) NumberConverter.ReadInt64(bytes, offset, out readCount));
    }

    public static int ReadInt32(byte[] bytes, int offset, out int readCount)
    {
      return checked ((int) NumberConverter.ReadInt64(bytes, offset, out readCount));
    }

    public static long ReadInt64(byte[] bytes, int offset, out int readCount)
    {
      long num1 = 0;
      int num2 = 1;
      if (bytes[offset] == (byte) 45)
        num2 = -1;
      for (int index = num2 == -1 ? offset + 1 : offset; index < bytes.Length; ++index)
      {
        if (!NumberConverter.IsNumber(bytes[index]))
        {
          readCount = index - offset;
          goto label_8;
        }
        else
          num1 = num1 * 10L + (long) ((int) bytes[index] - 48);
      }
      readCount = bytes.Length - offset;
label_8:
      return num1 * (long) num2;
    }

    public static byte ReadByte(byte[] bytes, int offset, out int readCount)
    {
      return checked ((byte) NumberConverter.ReadUInt64(bytes, offset, out readCount));
    }

    public static ushort ReadUInt16(byte[] bytes, int offset, out int readCount)
    {
      return checked ((ushort) NumberConverter.ReadUInt64(bytes, offset, out readCount));
    }

    public static uint ReadUInt32(byte[] bytes, int offset, out int readCount)
    {
      return checked ((uint) NumberConverter.ReadUInt64(bytes, offset, out readCount));
    }

    public static ulong ReadUInt64(byte[] bytes, int offset, out int readCount)
    {
      ulong num = 0;
      for (int index = offset; index < bytes.Length; ++index)
      {
        if (!NumberConverter.IsNumber(bytes[index]))
        {
          readCount = index - offset;
          goto label_6;
        }
        else
          num = checked (num * 10UL + (ulong) ((int) bytes[index] - 48));
      }
      readCount = bytes.Length - offset;
label_6:
      return num;
    }

    public static float ReadSingle(byte[] bytes, int offset, out int readCount)
    {
      return StringToDoubleConverter.ToSingle(bytes, offset, out readCount);
    }

    public static double ReadDouble(byte[] bytes, int offset, out int readCount)
    {
      return StringToDoubleConverter.ToDouble(bytes, offset, out readCount);
    }

    public static int WriteByte(ref byte[] buffer, int offset, byte value)
    {
      return NumberConverter.WriteUInt64(ref buffer, offset, (ulong) value);
    }

    public static int WriteUInt16(ref byte[] buffer, int offset, ushort value)
    {
      return NumberConverter.WriteUInt64(ref buffer, offset, (ulong) value);
    }

    public static int WriteUInt32(ref byte[] buffer, int offset, uint value)
    {
      return NumberConverter.WriteUInt64(ref buffer, offset, (ulong) value);
    }

    public static int WriteUInt64(ref byte[] buffer, int offset, ulong value)
    {
      int num1 = offset;
      ulong num2 = value;
      if (num2 < 10000UL)
      {
        if (num2 < 10UL)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
          goto label_59;
        }
        else if (num2 < 100UL)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 2);
          goto label_58;
        }
        else if (num2 < 1000UL)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 3);
          goto label_57;
        }
        else
          BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
      }
      else
      {
        ulong num3 = num2 / 10000UL;
        num2 -= num3 * 10000UL;
        if (num3 < 10000UL)
        {
          if (num3 < 10UL)
          {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 5);
            goto label_55;
          }
          else if (num3 < 100UL)
          {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 6);
            goto label_54;
          }
          else if (num3 < 1000UL)
          {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 7);
            goto label_53;
          }
          else
            BinaryUtil.EnsureCapacity(ref buffer, offset, 8);
        }
        else
        {
          ulong num4 = num3 / 10000UL;
          num3 -= num4 * 10000UL;
          if (num4 < 10000UL)
          {
            if (num4 < 10UL)
            {
              BinaryUtil.EnsureCapacity(ref buffer, offset, 9);
              goto label_51;
            }
            else if (num4 < 100UL)
            {
              BinaryUtil.EnsureCapacity(ref buffer, offset, 10);
              goto label_50;
            }
            else if (num4 < 1000UL)
            {
              BinaryUtil.EnsureCapacity(ref buffer, offset, 11);
              goto label_49;
            }
            else
              BinaryUtil.EnsureCapacity(ref buffer, offset, 12);
          }
          else
          {
            ulong num5 = num4 / 10000UL;
            num4 -= num5 * 10000UL;
            if (num5 < 10000UL)
            {
              if (num5 < 10UL)
              {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 13);
                goto label_47;
              }
              else if (num5 < 100UL)
              {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 14);
                goto label_46;
              }
              else if (num5 < 1000UL)
              {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 15);
                goto label_45;
              }
              else
                BinaryUtil.EnsureCapacity(ref buffer, offset, 16);
            }
            else
            {
              ulong num6 = num5 / 10000UL;
              num5 -= num6 * 10000UL;
              if (num6 < 10000UL)
              {
                if (num6 < 10UL)
                {
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 17);
                  goto label_43;
                }
                else if (num6 < 100UL)
                {
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 18);
                  goto label_42;
                }
                else if (num6 < 1000UL)
                {
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 19);
                  goto label_41;
                }
                else
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 20);
              }
              ulong num7;
              buffer[offset++] = (byte) (48UL + (num7 = num6 * 8389UL >> 23));
              num6 -= num7 * 1000UL;
label_41:
              ulong num8;
              buffer[offset++] = (byte) (48UL + (num8 = num6 * 5243UL >> 19));
              num6 -= num8 * 100UL;
label_42:
              ulong num9;
              buffer[offset++] = (byte) (48UL + (num9 = num6 * 6554UL >> 16));
              num6 -= num9 * 10UL;
label_43:
              buffer[offset++] = (byte) (48UL + num6);
            }
            ulong num10;
            buffer[offset++] = (byte) (48UL + (num10 = num5 * 8389UL >> 23));
            num5 -= num10 * 1000UL;
label_45:
            ulong num11;
            buffer[offset++] = (byte) (48UL + (num11 = num5 * 5243UL >> 19));
            num5 -= num11 * 100UL;
label_46:
            ulong num12;
            buffer[offset++] = (byte) (48UL + (num12 = num5 * 6554UL >> 16));
            num5 -= num12 * 10UL;
label_47:
            buffer[offset++] = (byte) (48UL + num5);
          }
          ulong num13;
          buffer[offset++] = (byte) (48UL + (num13 = num4 * 8389UL >> 23));
          num4 -= num13 * 1000UL;
label_49:
          ulong num14;
          buffer[offset++] = (byte) (48UL + (num14 = num4 * 5243UL >> 19));
          num4 -= num14 * 100UL;
label_50:
          ulong num15;
          buffer[offset++] = (byte) (48UL + (num15 = num4 * 6554UL >> 16));
          num4 -= num15 * 10UL;
label_51:
          buffer[offset++] = (byte) (48UL + num4);
        }
        ulong num16;
        buffer[offset++] = (byte) (48UL + (num16 = num3 * 8389UL >> 23));
        num3 -= num16 * 1000UL;
label_53:
        ulong num17;
        buffer[offset++] = (byte) (48UL + (num17 = num3 * 5243UL >> 19));
        num3 -= num17 * 100UL;
label_54:
        ulong num18;
        buffer[offset++] = (byte) (48UL + (num18 = num3 * 6554UL >> 16));
        num3 -= num18 * 10UL;
label_55:
        buffer[offset++] = (byte) (48UL + num3);
      }
      ulong num19;
      buffer[offset++] = (byte) (48UL + (num19 = num2 * 8389UL >> 23));
      num2 -= num19 * 1000UL;
label_57:
      ulong num20;
      buffer[offset++] = (byte) (48UL + (num20 = num2 * 5243UL >> 19));
      num2 -= num20 * 100UL;
label_58:
      ulong num21;
      buffer[offset++] = (byte) (48UL + (num21 = num2 * 6554UL >> 16));
      num2 -= num21 * 10UL;
label_59:
      buffer[offset++] = (byte) (48UL + num2);
      return offset - num1;
    }

    public static int WriteSByte(ref byte[] buffer, int offset, sbyte value)
    {
      return NumberConverter.WriteInt64(ref buffer, offset, (long) value);
    }

    public static int WriteInt16(ref byte[] buffer, int offset, short value)
    {
      return NumberConverter.WriteInt64(ref buffer, offset, (long) value);
    }

    public static int WriteInt32(ref byte[] buffer, int offset, int value)
    {
      return NumberConverter.WriteInt64(ref buffer, offset, (long) value);
    }

    public static int WriteInt64(ref byte[] buffer, int offset, long value)
    {
      int num1 = offset;
      long num2 = value;
      if (value < 0L)
      {
        if (value == long.MinValue)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 20);
          buffer[offset++] = (byte) 45;
          buffer[offset++] = (byte) 57;
          buffer[offset++] = (byte) 50;
          buffer[offset++] = (byte) 50;
          buffer[offset++] = (byte) 51;
          buffer[offset++] = (byte) 51;
          buffer[offset++] = (byte) 55;
          buffer[offset++] = (byte) 50;
          buffer[offset++] = (byte) 48;
          buffer[offset++] = (byte) 51;
          buffer[offset++] = (byte) 54;
          buffer[offset++] = (byte) 56;
          buffer[offset++] = (byte) 53;
          buffer[offset++] = (byte) 52;
          buffer[offset++] = (byte) 55;
          buffer[offset++] = (byte) 55;
          buffer[offset++] = (byte) 53;
          buffer[offset++] = (byte) 56;
          buffer[offset++] = (byte) 48;
          buffer[offset++] = (byte) 56;
          return offset - num1;
        }
        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
        buffer[offset++] = (byte) 45;
        num2 = -value;
      }
      if (num2 < 10000L)
      {
        if (num2 < 10L)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
          goto label_63;
        }
        else if (num2 < 100L)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 2);
          goto label_62;
        }
        else if (num2 < 1000L)
        {
          BinaryUtil.EnsureCapacity(ref buffer, offset, 3);
          goto label_61;
        }
        else
          BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
      }
      else
      {
        long num3 = num2 / 10000L;
        num2 -= num3 * 10000L;
        if (num3 < 10000L)
        {
          if (num3 < 10L)
          {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 5);
            goto label_59;
          }
          else if (num3 < 100L)
          {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 6);
            goto label_58;
          }
          else if (num3 < 1000L)
          {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 7);
            goto label_57;
          }
          else
            BinaryUtil.EnsureCapacity(ref buffer, offset, 8);
        }
        else
        {
          long num4 = num3 / 10000L;
          num3 -= num4 * 10000L;
          if (num4 < 10000L)
          {
            if (num4 < 10L)
            {
              BinaryUtil.EnsureCapacity(ref buffer, offset, 9);
              goto label_55;
            }
            else if (num4 < 100L)
            {
              BinaryUtil.EnsureCapacity(ref buffer, offset, 10);
              goto label_54;
            }
            else if (num4 < 1000L)
            {
              BinaryUtil.EnsureCapacity(ref buffer, offset, 11);
              goto label_53;
            }
            else
              BinaryUtil.EnsureCapacity(ref buffer, offset, 12);
          }
          else
          {
            long num5 = num4 / 10000L;
            num4 -= num5 * 10000L;
            if (num5 < 10000L)
            {
              if (num5 < 10L)
              {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 13);
                goto label_51;
              }
              else if (num5 < 100L)
              {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 14);
                goto label_50;
              }
              else if (num5 < 1000L)
              {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 15);
                goto label_49;
              }
              else
                BinaryUtil.EnsureCapacity(ref buffer, offset, 16);
            }
            else
            {
              long num6 = num5 / 10000L;
              num5 -= num6 * 10000L;
              if (num6 < 10000L)
              {
                if (num6 < 10L)
                {
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 17);
                  goto label_47;
                }
                else if (num6 < 100L)
                {
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 18);
                  goto label_46;
                }
                else if (num6 < 1000L)
                {
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 19);
                  goto label_45;
                }
                else
                  BinaryUtil.EnsureCapacity(ref buffer, offset, 20);
              }
              long num7;
              buffer[offset++] = (byte) (48UL + (ulong) (num7 = num6 * 8389L >> 23));
              num6 -= num7 * 1000L;
label_45:
              long num8;
              buffer[offset++] = (byte) (48UL + (ulong) (num8 = num6 * 5243L >> 19));
              num6 -= num8 * 100L;
label_46:
              long num9;
              buffer[offset++] = (byte) (48UL + (ulong) (num9 = num6 * 6554L >> 16));
              num6 -= num9 * 10L;
label_47:
              buffer[offset++] = (byte) (48UL + (ulong) num6);
            }
            long num10;
            buffer[offset++] = (byte) (48UL + (ulong) (num10 = num5 * 8389L >> 23));
            num5 -= num10 * 1000L;
label_49:
            long num11;
            buffer[offset++] = (byte) (48UL + (ulong) (num11 = num5 * 5243L >> 19));
            num5 -= num11 * 100L;
label_50:
            long num12;
            buffer[offset++] = (byte) (48UL + (ulong) (num12 = num5 * 6554L >> 16));
            num5 -= num12 * 10L;
label_51:
            buffer[offset++] = (byte) (48UL + (ulong) num5);
          }
          long num13;
          buffer[offset++] = (byte) (48UL + (ulong) (num13 = num4 * 8389L >> 23));
          num4 -= num13 * 1000L;
label_53:
          long num14;
          buffer[offset++] = (byte) (48UL + (ulong) (num14 = num4 * 5243L >> 19));
          num4 -= num14 * 100L;
label_54:
          long num15;
          buffer[offset++] = (byte) (48UL + (ulong) (num15 = num4 * 6554L >> 16));
          num4 -= num15 * 10L;
label_55:
          buffer[offset++] = (byte) (48UL + (ulong) num4);
        }
        long num16;
        buffer[offset++] = (byte) (48UL + (ulong) (num16 = num3 * 8389L >> 23));
        num3 -= num16 * 1000L;
label_57:
        long num17;
        buffer[offset++] = (byte) (48UL + (ulong) (num17 = num3 * 5243L >> 19));
        num3 -= num17 * 100L;
label_58:
        long num18;
        buffer[offset++] = (byte) (48UL + (ulong) (num18 = num3 * 6554L >> 16));
        num3 -= num18 * 10L;
label_59:
        buffer[offset++] = (byte) (48UL + (ulong) num3);
      }
      long num19;
      buffer[offset++] = (byte) (48UL + (ulong) (num19 = num2 * 8389L >> 23));
      num2 -= num19 * 1000L;
label_61:
      long num20;
      buffer[offset++] = (byte) (48UL + (ulong) (num20 = num2 * 5243L >> 19));
      num2 -= num20 * 100L;
label_62:
      long num21;
      buffer[offset++] = (byte) (48UL + (ulong) (num21 = num2 * 6554L >> 16));
      num2 -= num21 * 10L;
label_63:
      buffer[offset++] = (byte) (48UL + (ulong) num2);
      return offset - num1;
    }

    public static int WriteSingle(ref byte[] bytes, int offset, float value)
    {
      return DoubleToStringConverter.GetBytes(ref bytes, offset, value);
    }

    public static int WriteDouble(ref byte[] bytes, int offset, double value)
    {
      return DoubleToStringConverter.GetBytes(ref bytes, offset, value);
    }

    public static bool ReadBoolean(byte[] bytes, int offset, out int readCount)
    {
      if (bytes[offset] == (byte) 116)
      {
        if (bytes[offset + 1] != (byte) 114 || bytes[offset + 2] != (byte) 117 || bytes[offset + 3] != (byte) 101)
          throw new InvalidOperationException("value is not boolean(true).");
        readCount = 4;
        return true;
      }
      if (bytes[offset] != (byte) 102)
        throw new InvalidOperationException("value is not boolean.");
      if (bytes[offset + 1] != (byte) 97 || bytes[offset + 2] != (byte) 108 || (bytes[offset + 3] != (byte) 115 || bytes[offset + 4] != (byte) 101))
        throw new InvalidOperationException("value is not boolean(false).");
      readCount = 5;
      return false;
    }
  }
}
