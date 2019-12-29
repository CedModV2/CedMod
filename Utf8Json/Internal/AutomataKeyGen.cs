// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.AutomataKeyGen
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;

namespace Utf8Json.Internal
{
  public static class AutomataKeyGen
  {
    public static readonly MethodInfo GetKeyMethod = RuntimeReflectionExtensions.GetRuntimeMethod(typeof (AutomataKeyGen), "GetKey", new Type[2]
    {
      typeof (byte*).MakeByRefType(),
      typeof (int).MakeByRefType()
    });

    public static unsafe ulong GetKey(ref byte* p, ref int rest)
    {
      ulong num1;
      int num2;
      if (rest >= 8)
      {
        num1 = (ulong) *(long*) p;
        num2 = 8;
      }
      else
      {
        switch (rest)
        {
          case 1:
            num1 = (ulong) *p;
            num2 = 1;
            break;
          case 2:
            num1 = (ulong) *(ushort*) p;
            num2 = 2;
            break;
          case 3:
            num1 = (ulong) *p | (ulong) *(ushort*) (p + 1) << 8;
            num2 = 3;
            break;
          case 4:
            num1 = (ulong) *(uint*) p;
            num2 = 4;
            break;
          case 5:
            num1 = (ulong) *p | (ulong) *(uint*) (p + 1) << 8;
            num2 = 5;
            break;
          case 6:
            num1 = (ulong) *(ushort*) p | (ulong) *(uint*) (p + 2) << 16;
            num2 = 6;
            break;
          case 7:
            num1 = (ulong) ((long) *p | (long) *(ushort*) (p + 1) << 8 | (long) *(uint*) (p + 3) << 24);
            num2 = 7;
            break;
          default:
            throw new InvalidOperationException("Not Supported Length");
        }
      }
      p += num2;
      rest -= num2;
      return num1;
    }

    public static ulong GetKeySafe(byte[] bytes, ref int offset, ref int rest)
    {
      if (BitConverter.IsLittleEndian)
      {
        ulong num1;
        int num2;
        if (rest >= 8)
        {
          num1 = (ulong) ((long) bytes[offset] | (long) bytes[offset + 1] << 8 | (long) bytes[offset + 2] << 16 | (long) bytes[offset + 3] << 24 | (long) bytes[offset + 4] << 32 | (long) bytes[offset + 5] << 40 | (long) bytes[offset + 6] << 48 | (long) bytes[offset + 7] << 56);
          num2 = 8;
        }
        else
        {
          switch (rest)
          {
            case 1:
              num1 = (ulong) bytes[offset];
              num2 = 1;
              break;
            case 2:
              num1 = (ulong) bytes[offset] | (ulong) bytes[offset + 1] << 8;
              num2 = 2;
              break;
            case 3:
              num1 = (ulong) ((long) bytes[offset] | (long) bytes[offset + 1] << 8 | (long) bytes[offset + 2] << 16);
              num2 = 3;
              break;
            case 4:
              num1 = (ulong) ((long) bytes[offset] | (long) bytes[offset + 1] << 8 | (long) bytes[offset + 2] << 16 | (long) bytes[offset + 3] << 24);
              num2 = 4;
              break;
            case 5:
              num1 = (ulong) ((long) bytes[offset] | (long) bytes[offset + 1] << 8 | (long) bytes[offset + 2] << 16 | (long) bytes[offset + 3] << 24 | (long) bytes[offset + 4] << 32);
              num2 = 5;
              break;
            case 6:
              num1 = (ulong) ((long) bytes[offset] | (long) bytes[offset + 1] << 8 | (long) bytes[offset + 2] << 16 | (long) bytes[offset + 3] << 24 | (long) bytes[offset + 4] << 32 | (long) bytes[offset + 5] << 40);
              num2 = 6;
              break;
            case 7:
              num1 = (ulong) ((long) bytes[offset] | (long) bytes[offset + 1] << 8 | (long) bytes[offset + 2] << 16 | (long) bytes[offset + 3] << 24 | (long) bytes[offset + 4] << 32 | (long) bytes[offset + 5] << 40 | (long) bytes[offset + 6] << 48);
              num2 = 7;
              break;
            default:
              throw new InvalidOperationException("Not Supported Length");
          }
        }
        offset += num2;
        rest -= num2;
        return num1;
      }
      ulong num3;
      int num4;
      if (rest >= 8)
      {
        num3 = (ulong) ((long) bytes[offset] << 56 | (long) bytes[offset + 1] << 48 | (long) bytes[offset + 2] << 40 | (long) bytes[offset + 3] << 32 | (long) bytes[offset + 4] << 24 | (long) bytes[offset + 5] << 16 | (long) bytes[offset + 6] << 8) | (ulong) bytes[offset + 7];
        num4 = 8;
      }
      else
      {
        switch (rest)
        {
          case 1:
            num3 = (ulong) bytes[offset];
            num4 = 1;
            break;
          case 2:
            num3 = (ulong) bytes[offset] << 8 | (ulong) bytes[offset + 1];
            num4 = 2;
            break;
          case 3:
            num3 = (ulong) ((long) bytes[offset] << 16 | (long) bytes[offset + 1] << 8) | (ulong) bytes[offset + 2];
            num4 = 3;
            break;
          case 4:
            num3 = (ulong) ((long) bytes[offset] << 24 | (long) bytes[offset + 1] << 16 | (long) bytes[offset + 2] << 8) | (ulong) bytes[offset + 3];
            num4 = 4;
            break;
          case 5:
            num3 = (ulong) ((long) bytes[offset] << 32 | (long) bytes[offset + 1] << 24 | (long) bytes[offset + 2] << 16 | (long) bytes[offset + 3] << 8) | (ulong) bytes[offset + 4];
            num4 = 5;
            break;
          case 6:
            num3 = (ulong) ((long) bytes[offset] << 40 | (long) bytes[offset + 1] << 32 | (long) bytes[offset + 2] << 24 | (long) bytes[offset + 3] << 16 | (long) bytes[offset + 4] << 8) | (ulong) bytes[offset + 5];
            num4 = 6;
            break;
          case 7:
            num3 = (ulong) ((long) bytes[offset] << 48 | (long) bytes[offset + 1] << 40 | (long) bytes[offset + 2] << 32 | (long) bytes[offset + 3] << 24 | (long) bytes[offset + 4] << 16 | (long) bytes[offset + 5] << 8) | (ulong) bytes[offset + 6];
            num4 = 7;
            break;
          default:
            throw new InvalidOperationException("Not Supported Length");
        }
      }
      offset += num4;
      rest -= num4;
      return num3;
    }
  }
}
