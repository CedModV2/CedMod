// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ISO8601DateTimeFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ISO8601DateTimeFormatter : IJsonFormatter<DateTime>, IJsonFormatter
  {
    public static readonly IJsonFormatter<DateTime> Default = (IJsonFormatter<DateTime>) new ISO8601DateTimeFormatter();

    public void Serialize(
      ref JsonWriter writer,
      DateTime value,
      IJsonFormatterResolver formatterResolver)
    {
      int year = value.Year;
      int month = value.Month;
      int day = value.Day;
      int hour = value.Hour;
      int minute = value.Minute;
      int second = value.Second;
      long num = value.Ticks % 10000000L;
      switch (value.Kind)
      {
        case DateTimeKind.Utc:
          writer.EnsureCapacity(21 + (num == 0L ? 0 : 8) + 1);
          break;
        case DateTimeKind.Local:
          writer.EnsureCapacity(21 + (num == 0L ? 0 : 8) + 6);
          break;
        default:
          writer.EnsureCapacity(21 + (num == 0L ? 0 : 8));
          break;
      }
      writer.WriteRawUnsafe((byte) 34);
      if (year < 10)
      {
        writer.WriteRawUnsafe((byte) 48);
        writer.WriteRawUnsafe((byte) 48);
        writer.WriteRawUnsafe((byte) 48);
      }
      else if (year < 100)
      {
        writer.WriteRawUnsafe((byte) 48);
        writer.WriteRawUnsafe((byte) 48);
      }
      else if (year < 1000)
        writer.WriteRawUnsafe((byte) 48);
      writer.WriteInt32(year);
      writer.WriteRawUnsafe((byte) 45);
      if (month < 10)
        writer.WriteRawUnsafe((byte) 48);
      writer.WriteInt32(month);
      writer.WriteRawUnsafe((byte) 45);
      if (day < 10)
        writer.WriteRawUnsafe((byte) 48);
      writer.WriteInt32(day);
      writer.WriteRawUnsafe((byte) 84);
      if (hour < 10)
        writer.WriteRawUnsafe((byte) 48);
      writer.WriteInt32(hour);
      writer.WriteRawUnsafe((byte) 58);
      if (minute < 10)
        writer.WriteRawUnsafe((byte) 48);
      writer.WriteInt32(minute);
      writer.WriteRawUnsafe((byte) 58);
      if (second < 10)
        writer.WriteRawUnsafe((byte) 48);
      writer.WriteInt32(second);
      if (num != 0L)
      {
        writer.WriteRawUnsafe((byte) 46);
        if (num < 10L)
        {
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
        }
        else if (num < 100L)
        {
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
        }
        else if (num < 1000L)
        {
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
        }
        else if (num < 10000L)
        {
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
        }
        else if (num < 100000L)
        {
          writer.WriteRawUnsafe((byte) 48);
          writer.WriteRawUnsafe((byte) 48);
        }
        else if (num < 1000000L)
          writer.WriteRawUnsafe((byte) 48);
        writer.WriteInt64(num);
      }
      switch (value.Kind)
      {
        case DateTimeKind.Utc:
          writer.WriteRawUnsafe((byte) 90);
          break;
        case DateTimeKind.Local:
          TimeSpan timeSpan = TimeZoneInfo.Local.GetUtcOffset(value);
          bool flag = timeSpan < TimeSpan.Zero;
          if (flag)
            timeSpan = timeSpan.Negate();
          int hours = timeSpan.Hours;
          int minutes = timeSpan.Minutes;
          writer.WriteRawUnsafe(flag ? (byte) 45 : (byte) 43);
          if (hours < 10)
            writer.WriteRawUnsafe((byte) 48);
          writer.WriteInt32(hours);
          writer.WriteRawUnsafe((byte) 58);
          if (minutes < 10)
            writer.WriteRawUnsafe((byte) 48);
          writer.WriteInt32(minutes);
          break;
      }
      writer.WriteRawUnsafe((byte) 34);
    }

    public DateTime Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentUnsafe();
      byte[] array = arraySegment.Array;
      int offset = arraySegment.Offset;
      int count = arraySegment.Count;
      int num1 = arraySegment.Offset + arraySegment.Count;
      int num2;
      if (count == 4)
      {
        byte[] numArray1 = array;
        int index1 = offset;
        int num3 = index1 + 1;
        int num4 = ((int) numArray1[index1] - 48) * 1000;
        byte[] numArray2 = array;
        int index2 = num3;
        int num5 = index2 + 1;
        int num6 = ((int) numArray2[index2] - 48) * 100;
        int num7 = num4 + num6;
        byte[] numArray3 = array;
        int index3 = num5;
        int num8 = index3 + 1;
        int num9 = ((int) numArray3[index3] - 48) * 10;
        int num10 = num7 + num9;
        byte[] numArray4 = array;
        int index4 = num8;
        num2 = index4 + 1;
        int num11 = (int) numArray4[index4] - 48;
        return new DateTime(num10 + num11, 1, 1);
      }
      if (count == 7)
      {
        byte[] numArray1 = array;
        int index1 = offset;
        int num3 = index1 + 1;
        int num4 = ((int) numArray1[index1] - 48) * 1000;
        byte[] numArray2 = array;
        int index2 = num3;
        int num5 = index2 + 1;
        int num6 = ((int) numArray2[index2] - 48) * 100;
        int num7 = num4 + num6;
        byte[] numArray3 = array;
        int index3 = num5;
        int num8 = index3 + 1;
        int num9 = ((int) numArray3[index3] - 48) * 10;
        int num10 = num7 + num9;
        byte[] numArray4 = array;
        int index4 = num8;
        int num11 = index4 + 1;
        int num12 = (int) numArray4[index4] - 48;
        int year = num10 + num12;
        byte[] numArray5 = array;
        int index5 = num11;
        int num13 = index5 + 1;
        if (numArray5[index5] == (byte) 45)
        {
          byte[] numArray6 = array;
          int index6 = num13;
          int num14 = index6 + 1;
          int num15 = ((int) numArray6[index6] - 48) * 10;
          byte[] numArray7 = array;
          int index7 = num14;
          num2 = index7 + 1;
          int num16 = (int) numArray7[index7] - 48;
          int month = num15 + num16;
          return new DateTime(year, month, 1);
        }
      }
      else if (count == 10)
      {
        byte[] numArray1 = array;
        int index1 = offset;
        int num3 = index1 + 1;
        int num4 = ((int) numArray1[index1] - 48) * 1000;
        byte[] numArray2 = array;
        int index2 = num3;
        int num5 = index2 + 1;
        int num6 = ((int) numArray2[index2] - 48) * 100;
        int num7 = num4 + num6;
        byte[] numArray3 = array;
        int index3 = num5;
        int num8 = index3 + 1;
        int num9 = ((int) numArray3[index3] - 48) * 10;
        int num10 = num7 + num9;
        byte[] numArray4 = array;
        int index4 = num8;
        int num11 = index4 + 1;
        int num12 = (int) numArray4[index4] - 48;
        int year = num10 + num12;
        byte[] numArray5 = array;
        int index5 = num11;
        int num13 = index5 + 1;
        if (numArray5[index5] == (byte) 45)
        {
          byte[] numArray6 = array;
          int index6 = num13;
          int num14 = index6 + 1;
          int num15 = ((int) numArray6[index6] - 48) * 10;
          byte[] numArray7 = array;
          int index7 = num14;
          int num16 = index7 + 1;
          int num17 = (int) numArray7[index7] - 48;
          int month = num15 + num17;
          byte[] numArray8 = array;
          int index8 = num16;
          int num18 = index8 + 1;
          if (numArray8[index8] == (byte) 45)
          {
            byte[] numArray9 = array;
            int index9 = num18;
            int num19 = index9 + 1;
            int num20 = ((int) numArray9[index9] - 48) * 10;
            byte[] numArray10 = array;
            int index10 = num19;
            num2 = index10 + 1;
            int num21 = (int) numArray10[index10] - 48;
            int day = num20 + num21;
            return new DateTime(year, month, day);
          }
        }
      }
      else if (count >= 19)
      {
        byte[] numArray1 = array;
        int index1 = offset;
        int num3 = index1 + 1;
        int num4 = ((int) numArray1[index1] - 48) * 1000;
        byte[] numArray2 = array;
        int index2 = num3;
        int num5 = index2 + 1;
        int num6 = ((int) numArray2[index2] - 48) * 100;
        int num7 = num4 + num6;
        byte[] numArray3 = array;
        int index3 = num5;
        int num8 = index3 + 1;
        int num9 = ((int) numArray3[index3] - 48) * 10;
        int num10 = num7 + num9;
        byte[] numArray4 = array;
        int index4 = num8;
        int num11 = index4 + 1;
        int num12 = (int) numArray4[index4] - 48;
        int year = num10 + num12;
        byte[] numArray5 = array;
        int index5 = num11;
        int num13 = index5 + 1;
        if (numArray5[index5] == (byte) 45)
        {
          byte[] numArray6 = array;
          int index6 = num13;
          int num14 = index6 + 1;
          int num15 = ((int) numArray6[index6] - 48) * 10;
          byte[] numArray7 = array;
          int index7 = num14;
          int num16 = index7 + 1;
          int num17 = (int) numArray7[index7] - 48;
          int month = num15 + num17;
          byte[] numArray8 = array;
          int index8 = num16;
          int num18 = index8 + 1;
          if (numArray8[index8] == (byte) 45)
          {
            byte[] numArray9 = array;
            int index9 = num18;
            int num19 = index9 + 1;
            int num20 = ((int) numArray9[index9] - 48) * 10;
            byte[] numArray10 = array;
            int index10 = num19;
            int num21 = index10 + 1;
            int num22 = (int) numArray10[index10] - 48;
            int day = num20 + num22;
            byte[] numArray11 = array;
            int index11 = num21;
            int num23 = index11 + 1;
            if (numArray11[index11] == (byte) 84)
            {
              byte[] numArray12 = array;
              int index12 = num23;
              int num24 = index12 + 1;
              int num25 = ((int) numArray12[index12] - 48) * 10;
              byte[] numArray13 = array;
              int index13 = num24;
              int num26 = index13 + 1;
              int num27 = (int) numArray13[index13] - 48;
              int hour = num25 + num27;
              byte[] numArray14 = array;
              int index14 = num26;
              int num28 = index14 + 1;
              if (numArray14[index14] == (byte) 58)
              {
                byte[] numArray15 = array;
                int index15 = num28;
                int num29 = index15 + 1;
                int num30 = ((int) numArray15[index15] - 48) * 10;
                byte[] numArray16 = array;
                int index16 = num29;
                int num31 = index16 + 1;
                int num32 = (int) numArray16[index16] - 48;
                int minute = num30 + num32;
                byte[] numArray17 = array;
                int index17 = num31;
                int num33 = index17 + 1;
                if (numArray17[index17] == (byte) 58)
                {
                  byte[] numArray18 = array;
                  int index18 = num33;
                  int num34 = index18 + 1;
                  int num35 = ((int) numArray18[index18] - 48) * 10;
                  byte[] numArray19 = array;
                  int index19 = num34;
                  int index20 = index19 + 1;
                  int num36 = (int) numArray19[index19] - 48;
                  int second = num35 + num36;
                  int num37 = 0;
                  if (index20 < num1 && array[index20] == (byte) 46)
                  {
                    ++index20;
                    if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                    {
                      num37 += ((int) array[index20] - 48) * 1000000;
                      ++index20;
                      if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                      {
                        num37 += ((int) array[index20] - 48) * 100000;
                        ++index20;
                        if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                        {
                          num37 += ((int) array[index20] - 48) * 10000;
                          ++index20;
                          if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                          {
                            num37 += ((int) array[index20] - 48) * 1000;
                            ++index20;
                            if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                            {
                              num37 += ((int) array[index20] - 48) * 100;
                              ++index20;
                              if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                              {
                                num37 += ((int) array[index20] - 48) * 10;
                                ++index20;
                                if (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                                {
                                  num37 += (int) array[index20] - 48;
                                  ++index20;
                                  while (index20 < num1 && NumberConverter.IsNumber(array[index20]))
                                    ++index20;
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                  DateTimeKind kind = DateTimeKind.Unspecified;
                  if (index20 < num1 && array[index20] == (byte) 90)
                    kind = DateTimeKind.Utc;
                  else if (index20 < num1 && array[index20] == (byte) 45 || array[index20] == (byte) 43)
                  {
                    if (index20 + 5 < num1)
                    {
                      byte[] numArray20 = array;
                      int index21 = index20;
                      int num38 = index21 + 1;
                      int num39 = numArray20[index21] == (byte) 45 ? 1 : 0;
                      byte[] numArray21 = array;
                      int index22 = num38;
                      int num40 = index22 + 1;
                      int num41 = ((int) numArray21[index22] - 48) * 10;
                      byte[] numArray22 = array;
                      int index23 = num40;
                      int num42 = index23 + 1;
                      int num43 = (int) numArray22[index23] - 48;
                      int hours = num41 + num43;
                      int num44 = num42 + 1;
                      byte[] numArray23 = array;
                      int index24 = num44;
                      int num45 = index24 + 1;
                      int num46 = ((int) numArray23[index24] - 48) * 10;
                      byte[] numArray24 = array;
                      int index25 = num45;
                      num2 = index25 + 1;
                      int num47 = (int) numArray24[index25] - 48;
                      int minutes = num46 + num47;
                      TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);
                      if (num39 != 0)
                        timeSpan = timeSpan.Negate();
                      return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks((long) num37).Subtract(timeSpan).ToLocalTime();
                    }
                    goto label_34;
                  }
                  return new DateTime(year, month, day, hour, minute, second, kind).AddTicks((long) num37);
                }
              }
            }
          }
        }
      }
label_34:
      throw new InvalidOperationException("invalid datetime format. value:" + StringEncoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count));
    }
  }
}
