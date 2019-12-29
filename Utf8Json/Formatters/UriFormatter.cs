// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UriFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class UriFormatter : IJsonFormatter<Uri>, IJsonFormatter
  {
    public static readonly IJsonFormatter<Uri> Default = (IJsonFormatter<Uri>) new UriFormatter();

    public void Serialize(
      ref JsonWriter writer,
      Uri value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == (Uri) null)
        writer.WriteNull();
      else
        writer.WriteString(value.ToString());
    }

    public Uri Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? (Uri) null : new Uri(reader.ReadString(), UriKind.RelativeOrAbsolute);
    }
  }
}
