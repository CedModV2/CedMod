// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.GuidFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class GuidFormatter : IJsonFormatter<Guid>, IJsonFormatter, IObjectPropertyNameFormatter<Guid>
  {
    public static readonly IJsonFormatter<Guid> Default = (IJsonFormatter<Guid>) new GuidFormatter();

    public void Serialize(
      ref JsonWriter writer,
      Guid value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.EnsureCapacity(38);
      writer.WriteRawUnsafe((byte) 34);
      ArraySegment<byte> buffer = writer.GetBuffer();
      new GuidBits(ref value).Write(buffer.Array, writer.CurrentOffset);
      writer.AdvanceOffset(36);
      writer.WriteRawUnsafe((byte) 34);
    }

    public Guid Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> utf8string = reader.ReadStringSegmentUnsafe();
      return new GuidBits(ref utf8string).Value;
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      Guid value,
      IJsonFormatterResolver formatterResolver)
    {
      this.Serialize(ref writer, value, formatterResolver);
    }

    public Guid DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return this.Deserialize(ref reader, formatterResolver);
    }
  }
}
