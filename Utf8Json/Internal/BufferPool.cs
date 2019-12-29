// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.BufferPool
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal
{
  internal sealed class BufferPool : ArrayPool<byte>
  {
    public static readonly BufferPool Default = new BufferPool((int) ushort.MaxValue);

    public BufferPool(int bufferLength)
      : base(bufferLength)
    {
    }
  }
}
