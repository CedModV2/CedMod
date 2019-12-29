// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.BinaryUtil
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Internal
{
  public static class BinaryUtil
  {
    private const int ArrayMaxSize = 2147483591;

    public static void EnsureCapacity(ref byte[] bytes, int offset, int appendLength)
    {
      int length1 = offset + appendLength;
      if (bytes == null)
      {
        bytes = new byte[length1];
      }
      else
      {
        int length2 = bytes.Length;
        if (length1 <= length2)
          return;
        int newSize1 = length1;
        if (newSize1 < 256)
        {
          int newSize2 = 256;
          BinaryUtil.FastResize(ref bytes, newSize2);
        }
        else
        {
          if (length2 == 2147483591)
            throw new InvalidOperationException("byte[] size reached maximum size of array(0x7FFFFFC7), can not write to single byte[]. Details: https://msdn.microsoft.com/en-us/library/system.array");
          int num = length2 * 2;
          if (num < 0)
            newSize1 = 2147483591;
          else if (newSize1 < num)
            newSize1 = num;
          BinaryUtil.FastResize(ref bytes, newSize1);
        }
      }
    }

    public static void FastResize(ref byte[] array, int newSize)
    {
      if (newSize < 0)
        throw new ArgumentOutOfRangeException(nameof (newSize));
      byte[] numArray1 = array;
      if (numArray1 == null)
      {
        array = new byte[newSize];
      }
      else
      {
        if (numArray1.Length == newSize)
          return;
        byte[] numArray2 = new byte[newSize];
        Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray2, 0, numArray1.Length > newSize ? newSize : numArray1.Length);
        array = numArray2;
      }
    }

    public static byte[] FastCloneWithResize(byte[] src, int newSize)
    {
      if (newSize < 0)
        throw new ArgumentOutOfRangeException(nameof (newSize));
      if (src.Length < newSize)
        throw new ArgumentException("length < newSize");
      if (src == null)
        return new byte[newSize];
      byte[] numArray = new byte[newSize];
      Buffer.BlockCopy((Array) src, 0, (Array) numArray, 0, newSize);
      return numArray;
    }
  }
}
