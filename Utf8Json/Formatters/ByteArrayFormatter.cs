// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ByteArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class ByteArrayFormatter : IJsonFormatter<byte[]>, IJsonFormatter
  {
    public static readonly IJsonFormatter<byte[]> Default = (IJsonFormatter<byte[]>) new ByteArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      byte[] value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
        writer.WriteNull();
      else
        writer.WriteString(Convert.ToBase64String(value, Base64FormattingOptions.None));
    }

    public byte[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? (byte[]) null : Convert.FromBase64String(reader.ReadString());
    }
  }
}
