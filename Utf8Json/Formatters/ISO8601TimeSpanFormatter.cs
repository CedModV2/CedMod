// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ISO8601TimeSpanFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ISO8601TimeSpanFormatter : IJsonFormatter<TimeSpan>, IJsonFormatter
  {
    public static readonly IJsonFormatter<TimeSpan> Default = (IJsonFormatter<TimeSpan>) new ISO8601TimeSpanFormatter();
    private static byte[] minValue = StringEncoding.UTF8.GetBytes("\"" + TimeSpan.MinValue.ToString() + "\"");

    public void Serialize(
      ref JsonWriter writer,
      TimeSpan value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == TimeSpan.MinValue)
      {
        writer.WriteRaw(ISO8601TimeSpanFormatter.minValue);
      }
      else
      {
        int num1 = value < TimeSpan.Zero ? 1 : 0;
        if (num1 != 0)
          value = value.Negate();
        int days = value.Days;
        int hours = value.Hours;
        int minutes = value.Minutes;
        int seconds = value.Seconds;
        long num2 = value.Ticks % 10000000L;
        writer.EnsureCapacity(19 + (num2 == 0L ? 0 : 8) + 6);
        writer.WriteRawUnsafe((byte) 34);
        if (num1 != 0)
          writer.WriteRawUnsafe((byte) 45);
        if (days != 0)
        {
          writer.WriteInt32(days);
          writer.WriteRawUnsafe((byte) 46);
        }
        if (hours < 10)
          writer.WriteRawUnsafe((byte) 48);
        writer.WriteInt32(hours);
        writer.WriteRawUnsafe((byte) 58);
        if (minutes < 10)
          writer.WriteRawUnsafe((byte) 48);
        writer.WriteInt32(minutes);
        writer.WriteRawUnsafe((byte) 58);
        if (seconds < 10)
          writer.WriteRawUnsafe((byte) 48);
        writer.WriteInt32(seconds);
        if (num2 != 0L)
        {
          writer.WriteRawUnsafe((byte) 46);
          writer.WriteInt64(num2);
        }
        writer.WriteRawUnsafe((byte) 34);
      }
    }

    public TimeSpan Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentUnsafe();
      byte[] array = arraySegment.Array;
      int offset = arraySegment.Offset;
      int count = arraySegment.Count;
      int num1 = arraySegment.Offset + arraySegment.Count;
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      for (int index = offset; index < arraySegment.Count; ++index)
      {
        if (array[index] == (byte) 46)
        {
          if (!flag3)
            flag2 = true;
          else
            break;
        }
        else if (array[index] == (byte) 58)
        {
          if (flag2)
            flag1 = true;
          flag3 = true;
        }
      }
      bool flag4 = false;
      if (array[offset] == (byte) 45)
      {
        flag4 = true;
        ++offset;
      }
      int days = 0;
      if (flag1)
      {
        byte[] numArray = BufferPool.Default.Rent();
        try
        {
          for (; array[offset] != (byte) 46; ++offset)
            numArray[days++] = array[offset];
          days = new JsonReader(numArray).ReadInt32();
          ++offset;
        }
        finally
        {
          BufferPool.Default.Return(numArray);
        }
      }
      byte[] numArray1 = array;
      int index1 = offset;
      int num2 = index1 + 1;
      int num3 = ((int) numArray1[index1] - 48) * 10;
      byte[] numArray2 = array;
      int index2 = num2;
      int num4 = index2 + 1;
      int num5 = (int) numArray2[index2] - 48;
      int hours = num3 + num5;
      byte[] numArray3 = array;
      int index3 = num4;
      int num6 = index3 + 1;
      if (numArray3[index3] == (byte) 58)
      {
        byte[] numArray4 = array;
        int index4 = num6;
        int num7 = index4 + 1;
        int num8 = ((int) numArray4[index4] - 48) * 10;
        byte[] numArray5 = array;
        int index5 = num7;
        int num9 = index5 + 1;
        int num10 = (int) numArray5[index5] - 48;
        int minutes = num8 + num10;
        byte[] numArray6 = array;
        int index6 = num9;
        int num11 = index6 + 1;
        if (numArray6[index6] == (byte) 58)
        {
          byte[] numArray7 = array;
          int index7 = num11;
          int num12 = index7 + 1;
          int num13 = ((int) numArray7[index7] - 48) * 10;
          byte[] numArray8 = array;
          int index8 = num12;
          int index9 = index8 + 1;
          int num14 = (int) numArray8[index8] - 48;
          int seconds = num13 + num14;
          int num15 = 0;
          if (index9 < num1 && array[index9] == (byte) 46)
          {
            int index10 = index9 + 1;
            if (index10 < num1 && NumberConverter.IsNumber(array[index10]))
            {
              num15 += ((int) array[index10] - 48) * 1000000;
              int index11 = index10 + 1;
              if (index11 < num1 && NumberConverter.IsNumber(array[index11]))
              {
                num15 += ((int) array[index11] - 48) * 100000;
                int index12 = index11 + 1;
                if (index12 < num1 && NumberConverter.IsNumber(array[index12]))
                {
                  num15 += ((int) array[index12] - 48) * 10000;
                  int index13 = index12 + 1;
                  if (index13 < num1 && NumberConverter.IsNumber(array[index13]))
                  {
                    num15 += ((int) array[index13] - 48) * 1000;
                    int index14 = index13 + 1;
                    if (index14 < num1 && NumberConverter.IsNumber(array[index14]))
                    {
                      num15 += ((int) array[index14] - 48) * 100;
                      int index15 = index14 + 1;
                      if (index15 < num1 && NumberConverter.IsNumber(array[index15]))
                      {
                        num15 += ((int) array[index15] - 48) * 10;
                        int index16 = index15 + 1;
                        if (index16 < num1 && NumberConverter.IsNumber(array[index16]))
                        {
                          num15 += (int) array[index16] - 48;
                          int index17 = index16 + 1;
                          while (index17 < num1 && NumberConverter.IsNumber(array[index17]))
                            ++index17;
                        }
                      }
                    }
                  }
                }
              }
            }
          }
          TimeSpan timeSpan = new TimeSpan(days, hours, minutes, seconds);
          TimeSpan ts = TimeSpan.FromTicks((long) num15);
          return !flag4 ? timeSpan.Add(ts) : timeSpan.Negate().Subtract(ts);
        }
      }
      throw new InvalidOperationException("invalid datetime format. value:" + StringEncoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count));
    }
  }
}
