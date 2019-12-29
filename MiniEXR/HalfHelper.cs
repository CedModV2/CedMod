// Decompiled with JetBrains decompiler
// Type: MiniEXR.HalfHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace MiniEXR
{
  internal static class HalfHelper
  {
    private static uint[] mantissaTable = HalfHelper.GenerateMantissaTable();
    private static uint[] exponentTable = HalfHelper.GenerateExponentTable();
    private static ushort[] offsetTable = HalfHelper.GenerateOffsetTable();
    private static ushort[] baseTable = HalfHelper.GenerateBaseTable();
    private static sbyte[] shiftTable = HalfHelper.GenerateShiftTable();

    private static uint ConvertMantissa(int i)
    {
      uint num1 = (uint) (i << 13);
      uint num2 = 0;
      for (; ((int) num1 & 8388608) == 0; num1 <<= 1)
        num2 -= 8388608U;
      return num1 & 4286578687U | num2 + 947912704U;
    }

    private static uint[] GenerateMantissaTable()
    {
      uint[] numArray = new uint[2048];
      numArray[0] = 0U;
      for (int i = 1; i < 1024; ++i)
        numArray[i] = HalfHelper.ConvertMantissa(i);
      for (int index = 1024; index < 2048; ++index)
        numArray[index] = (uint) (939524096 + (index - 1024 << 13));
      return numArray;
    }

    private static uint[] GenerateExponentTable()
    {
      uint[] numArray = new uint[64];
      numArray[0] = 0U;
      for (int index = 1; index < 31; ++index)
        numArray[index] = (uint) (index << 23);
      numArray[31] = 1199570944U;
      numArray[32] = 2147483648U;
      for (int index = 33; index < 63; ++index)
        numArray[index] = (uint) (2147483648UL + (ulong) (index - 32 << 23));
      numArray[63] = 3347054592U;
      return numArray;
    }

    private static ushort[] GenerateOffsetTable()
    {
      ushort[] numArray = new ushort[64];
      numArray[0] = (ushort) 0;
      for (int index = 1; index < 32; ++index)
        numArray[index] = (ushort) 1024;
      numArray[32] = (ushort) 0;
      for (int index = 33; index < 64; ++index)
        numArray[index] = (ushort) 1024;
      return numArray;
    }

    private static ushort[] GenerateBaseTable()
    {
      ushort[] numArray = new ushort[512];
      for (int index = 0; index < 256; ++index)
      {
        sbyte num = (sbyte) ((int) sbyte.MaxValue - index);
        if (num > (sbyte) 24)
        {
          numArray[index | 0] = (ushort) 0;
          numArray[index | 256] = (ushort) 32768;
        }
        else if (num > (sbyte) 14)
        {
          numArray[index | 0] = (ushort) (1024 >> 18 + (int) num);
          numArray[index | 256] = (ushort) (1024 >> 18 + (int) num | 32768);
        }
        else if (num >= (sbyte) -15)
        {
          numArray[index | 0] = (ushort) (15 - (int) num << 10);
          numArray[index | 256] = (ushort) (15 - (int) num << 10 | 32768);
        }
        else if (num > sbyte.MinValue)
        {
          numArray[index | 0] = (ushort) 31744;
          numArray[index | 256] = (ushort) 64512;
        }
        else
        {
          numArray[index | 0] = (ushort) 31744;
          numArray[index | 256] = (ushort) 64512;
        }
      }
      return numArray;
    }

    private static sbyte[] GenerateShiftTable()
    {
      sbyte[] numArray = new sbyte[512];
      for (int index = 0; index < 256; ++index)
      {
        sbyte num = (sbyte) ((int) sbyte.MaxValue - index);
        if (num > (sbyte) 24)
        {
          numArray[index | 0] = (sbyte) 24;
          numArray[index | 256] = (sbyte) 24;
        }
        else if (num > (sbyte) 14)
        {
          numArray[index | 0] = (sbyte) ((int) num - 1);
          numArray[index | 256] = (sbyte) ((int) num - 1);
        }
        else if (num >= (sbyte) -15)
        {
          numArray[index | 0] = (sbyte) 13;
          numArray[index | 256] = (sbyte) 13;
        }
        else if (num > sbyte.MinValue)
        {
          numArray[index | 0] = (sbyte) 24;
          numArray[index | 256] = (sbyte) 24;
        }
        else
        {
          numArray[index | 0] = (sbyte) 13;
          numArray[index | 256] = (sbyte) 13;
        }
      }
      return numArray;
    }

    public static float HalfToSingle(ushort half)
    {
      return BitConverter.ToSingle(BitConverter.GetBytes(HalfHelper.mantissaTable[(int) HalfHelper.offsetTable[(int) half >> 10] + ((int) half & 1023)] + HalfHelper.exponentTable[(int) half >> 10]), 0);
    }

    public static ushort SingleToHalf(float single)
    {
      uint uint32 = BitConverter.ToUInt32(BitConverter.GetBytes(single), 0);
      return (ushort) ((uint) HalfHelper.baseTable[(int) (uint32 >> 23) & 511] + ((uint32 & 8388607U) >> (int) HalfHelper.shiftTable[(int) (uint32 >> 23)]));
    }
  }
}
