// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.DissonanceNetworkMessage
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Datastructures;
using Dissonance.Extensions;
using Mirror;
using System;

namespace Dissonance.Integrations.MirrorIgnorance
{
  internal struct DissonanceNetworkMessage : IMessageBase, IDisposable
  {
    private static readonly ConcurrentPool<byte[]> SerializationBuffers = new ConcurrentPool<byte[]>(8, (Func<byte[]>) (() => new byte[1024]));
    private const int BufferLength = 1024;
    public ArraySegment<byte> Data;

    public DissonanceNetworkMessage(ArraySegment<byte> packet)
    {
      this.Data = packet.CopyTo<byte>(DissonanceNetworkMessage.SerializationBuffers.Get(), 0);
    }

    public void Deserialize([NotNull] NetworkReader reader)
    {
      byte[] array = DissonanceNetworkMessage.SerializationBuffers.Get();
      ushort num = reader.ReadUInt16();
      for (int index = 0; index < (int) num; ++index)
        array[index] = reader.ReadByte();
      this.Data = new ArraySegment<byte>(array, 0, (int) num);
    }

    public void Serialize([NotNull] NetworkWriter writer)
    {
      writer.Write((ushort) this.Data.Count);
      writer.Write(this.Data.Array, this.Data.Offset, this.Data.Count);
      DissonanceNetworkMessage.SerializationBuffers.Put(this.Data.Array);
    }

    public void Dispose()
    {
      byte[] array = this.Data.Array;
      if (array == null || array.Length != 1024)
        return;
      DissonanceNetworkMessage.SerializationBuffers.Put(array);
      this.Data = new ArraySegment<byte>((byte[]) Array.Empty<byte>(), 0, 0);
    }
  }
}
