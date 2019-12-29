// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ISO8601DateTimeOffsetFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ISO8601DateTimeOffsetFormatter : IJsonFormatter<DateTimeOffset>, IJsonFormatter
  {
    public static readonly IJsonFormatter<DateTimeOffset> Default = (IJsonFormatter<DateTimeOffset>) new ISO8601DateTimeOffsetFormatter();

    public void Serialize(
      ref JsonWriter writer,
      DateTimeOffset value,
      IJsonFormatterResolver formatterResolver)
    {
      int year = value.Year;
      int month = value.Month;
      int day = value.Day;
      int hour = value.Hour;
      int minute = value.Minute;
      int second = value.Second;
      long num = value.Ticks % 10000000L;
      writer.EnsureCapacity(21 + (num == 0L ? 0 : 8) + 6);
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
      TimeSpan timeSpan = value.Offset;
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
      writer.WriteRawUnsafe((byte) 34);
    }

    public DateTimeOffset Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentUnsafe();
      byte[] array = arraySegment.Array;
      int offset1 = arraySegment.Offset;
      int count = arraySegment.Count;
      int num1 = arraySegment.Offset + arraySegment.Count;
      int num2;
      switch (count)
      {
        case 4:
          byte[] numArray1 = array;
          int index1 = offset1;
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
          return new DateTimeOffset(num10 + num11, 1, 1, 0, 0, 0, TimeSpan.Zero);
        case 7:
          byte[] numArray5 = array;
          int index5 = offset1;
          int num12 = index5 + 1;
          int num13 = ((int) numArray5[index5] - 48) * 1000;
          byte[] numArray6 = array;
          int index6 = num12;
          int num14 = index6 + 1;
          int num15 = ((int) numArray6[index6] - 48) * 100;
          int num16 = num13 + num15;
          byte[] numArray7 = array;
          int index7 = num14;
          int num17 = index7 + 1;
          int num18 = ((int) numArray7[index7] - 48) * 10;
          int num19 = num16 + num18;
          byte[] numArray8 = array;
          int index8 = num17;
          int num20 = index8 + 1;
          int num21 = (int) numArray8[index8] - 48;
          int year1 = num19 + num21;
          byte[] numArray9 = array;
          int index9 = num20;
          int num22 = index9 + 1;
          if (numArray9[index9] == (byte) 45)
          {
            byte[] numArray10 = array;
            int index10 = num22;
            int num23 = index10 + 1;
            int num24 = ((int) numArray10[index10] - 48) * 10;
            byte[] numArray11 = array;
            int index11 = num23;
            num2 = index11 + 1;
            int num25 = (int) numArray11[index11] - 48;
            int month = num24 + num25;
            return new DateTimeOffset(year1, month, 1, 0, 0, 0, TimeSpan.Zero);
          }
          break;
        case 10:
          byte[] numArray12 = array;
          int index12 = offset1;
          int num26 = index12 + 1;
          int num27 = ((int) numArray12[index12] - 48) * 1000;
          byte[] numArray13 = array;
          int index13 = num26;
          int num28 = index13 + 1;
          int num29 = ((int) numArray13[index13] - 48) * 100;
          int num30 = num27 + num29;
          byte[] numArray14 = array;
          int index14 = num28;
          int num31 = index14 + 1;
          int num32 = ((int) numArray14[index14] - 48) * 10;
          int num33 = num30 + num32;
          byte[] numArray15 = array;
          int index15 = num31;
          int num34 = index15 + 1;
          int num35 = (int) numArray15[index15] - 48;
          int year2 = num33 + num35;
          byte[] numArray16 = array;
          int index16 = num34;
          int num36 = index16 + 1;
          if (numArray16[index16] == (byte) 45)
          {
            byte[] numArray10 = array;
            int index10 = num36;
            int num23 = index10 + 1;
            int num24 = ((int) numArray10[index10] - 48) * 10;
            byte[] numArray11 = array;
            int index11 = num23;
            int num25 = index11 + 1;
            int num37 = (int) numArray11[index11] - 48;
            int month = num24 + num37;
            byte[] numArray17 = array;
            int index17 = num25;
            int num38 = index17 + 1;
            if (numArray17[index17] == (byte) 45)
            {
              byte[] numArray18 = array;
              int index18 = num38;
              int num39 = index18 + 1;
              int num40 = ((int) numArray18[index18] - 48) * 10;
              byte[] numArray19 = array;
              int index19 = num39;
              num2 = index19 + 1;
              int num41 = (int) numArray19[index19] - 48;
              int day = num40 + num41;
              return new DateTimeOffset(year2, month, day, 0, 0, 0, TimeSpan.Zero);
            }
            break;
          }
          break;
        default:
          if (array.Length >= 19)
          {
            byte[] numArray10 = array;
            int index10 = offset1;
            int num23 = index10 + 1;
            int num24 = ((int) numArray10[index10] - 48) * 1000;
            byte[] numArray11 = array;
            int index11 = num23;
            int num25 = index11 + 1;
            int num37 = ((int) numArray11[index11] - 48) * 100;
            int num38 = num24 + num37;
            byte[] numArray17 = array;
            int index17 = num25;
            int num39 = index17 + 1;
            int num40 = ((int) numArray17[index17] - 48) * 10;
            int num41 = num38 + num40;
            byte[] numArray18 = array;
            int index18 = num39;
            int num42 = index18 + 1;
            int num43 = (int) numArray18[index18] - 48;
            int year3 = num41 + num43;
            byte[] numArray19 = array;
            int index19 = num42;
            int num44 = index19 + 1;
            if (numArray19[index19] == (byte) 45)
            {
              byte[] numArray20 = array;
              int index20 = num44;
              int num45 = index20 + 1;
              int num46 = ((int) numArray20[index20] - 48) * 10;
              byte[] numArray21 = array;
              int index21 = num45;
              int num47 = index21 + 1;
              int num48 = (int) numArray21[index21] - 48;
              int month = num46 + num48;
              byte[] numArray22 = array;
              int index22 = num47;
              int num49 = index22 + 1;
              if (numArray22[index22] == (byte) 45)
              {
                byte[] numArray23 = array;
                int index23 = num49;
                int num50 = index23 + 1;
                int num51 = ((int) numArray23[index23] - 48) * 10;
                byte[] numArray24 = array;
                int index24 = num50;
                int num52 = index24 + 1;
                int num53 = (int) numArray24[index24] - 48;
                int day = num51 + num53;
                byte[] numArray25 = array;
                int index25 = num52;
                int num54 = index25 + 1;
                if (numArray25[index25] == (byte) 84)
                {
                  byte[] numArray26 = array;
                  int index26 = num54;
                  int num55 = index26 + 1;
                  int num56 = ((int) numArray26[index26] - 48) * 10;
                  byte[] numArray27 = array;
                  int index27 = num55;
                  int num57 = index27 + 1;
                  int num58 = (int) numArray27[index27] - 48;
                  int hour = num56 + num58;
                  byte[] numArray28 = array;
                  int index28 = num57;
                  int num59 = index28 + 1;
                  if (numArray28[index28] == (byte) 58)
                  {
                    byte[] numArray29 = array;
                    int index29 = num59;
                    int num60 = index29 + 1;
                    int num61 = ((int) numArray29[index29] - 48) * 10;
                    byte[] numArray30 = array;
                    int index30 = num60;
                    int num62 = index30 + 1;
                    int num63 = (int) numArray30[index30] - 48;
                    int minute = num61 + num63;
                    byte[] numArray31 = array;
                    int index31 = num62;
                    int num64 = index31 + 1;
                    if (numArray31[index31] == (byte) 58)
                    {
                      byte[] numArray32 = array;
                      int index32 = num64;
                      int num65 = index32 + 1;
                      int num66 = ((int) numArray32[index32] - 48) * 10;
                      byte[] numArray33 = array;
                      int index33 = num65;
                      int index34 = index33 + 1;
                      int num67 = (int) numArray33[index33] - 48;
                      int second = num66 + num67;
                      int num68 = 0;
                      if (index34 < num1 && array[index34] == (byte) 46)
                      {
                        ++index34;
                        if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                        {
                          num68 += ((int) array[index34] - 48) * 1000000;
                          ++index34;
                          if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                          {
                            num68 += ((int) array[index34] - 48) * 100000;
                            ++index34;
                            if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                            {
                              num68 += ((int) array[index34] - 48) * 10000;
                              ++index34;
                              if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                              {
                                num68 += ((int) array[index34] - 48) * 1000;
                                ++index34;
                                if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                                {
                                  num68 += ((int) array[index34] - 48) * 100;
                                  ++index34;
                                  if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                                  {
                                    num68 += ((int) array[index34] - 48) * 10;
                                    ++index34;
                                    if (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                                    {
                                      num68 += (int) array[index34] - 48;
                                      ++index34;
                                      while (index34 < num1 && NumberConverter.IsNumber(array[index34]))
                                        ++index34;
                                    }
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                      if ((index34 >= num1 || array[index34] != (byte) 45) && array[index34] != (byte) 43)
                        return new DateTimeOffset(year3, month, day, hour, minute, second, TimeSpan.Zero).AddTicks((long) num68);
                      if (index34 + 5 < num1)
                      {
                        byte[] numArray34 = array;
                        int index35 = index34;
                        int num69 = index35 + 1;
                        int num70 = numArray34[index35] == (byte) 45 ? 1 : 0;
                        byte[] numArray35 = array;
                        int index36 = num69;
                        int num71 = index36 + 1;
                        int num72 = ((int) numArray35[index36] - 48) * 10;
                        byte[] numArray36 = array;
                        int index37 = num71;
                        int num73 = index37 + 1;
                        int num74 = (int) numArray36[index37] - 48;
                        int hours = num72 + num74;
                        int num75 = num73 + 1;
                        byte[] numArray37 = array;
                        int index38 = num75;
                        int num76 = index38 + 1;
                        int num77 = ((int) numArray37[index38] - 48) * 10;
                        byte[] numArray38 = array;
                        int index39 = num76;
                        num2 = index39 + 1;
                        int num78 = (int) numArray38[index39] - 48;
                        int minutes = num77 + num78;
                        TimeSpan offset2 = new TimeSpan(hours, minutes, 0);
                        if (num70 != 0)
                          offset2 = offset2.Negate();
                        return new DateTimeOffset(year3, month, day, hour, minute, second, offset2).AddTicks((long) num68);
                      }
                      break;
                    }
                    break;
                  }
                  break;
                }
                break;
              }
              break;
            }
            break;
          }
          break;
      }
      throw new InvalidOperationException("invalid datetime format. value:" + StringEncoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count));
    }
  }
}
