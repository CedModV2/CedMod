// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.UnionDoubleULong
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Runtime.InteropServices;

namespace Utf8Json.Internal.DoubleConversion
{
  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  internal struct UnionDoubleULong
  {
    [FieldOffset(0)]
    public double d;
    [FieldOffset(0)]
    public ulong u64;
  }
}
