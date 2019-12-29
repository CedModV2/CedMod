// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ByteArraySegmentFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class ByteArraySegmentFormatter : IJsonFormatter<ArraySegment<byte>>, IJsonFormatter
  {
    public static readonly IJsonFormatter<ArraySegment<byte>> Default = (IJsonFormatter<ArraySegment<byte>>) new ByteArraySegmentFormatter();

    public void Serialize(
      ref JsonWriter writer,
      ArraySegment<byte> value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value.Array == null)
        writer.WriteNull();
      else
        writer.WriteString(Convert.ToBase64String(value.Array, value.Offset, value.Count, Base64FormattingOptions.None));
    }

    public ArraySegment<byte> Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new ArraySegment<byte>();
      byte[] array = Convert.FromBase64String(reader.ReadString());
      return new ArraySegment<byte>(array, 0, array.Length);
    }
  }
}
