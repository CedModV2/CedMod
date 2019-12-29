// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.VersionFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class VersionFormatter : IJsonFormatter<Version>, IJsonFormatter
  {
    public static readonly IJsonFormatter<Version> Default = (IJsonFormatter<Version>) new VersionFormatter();

    public void Serialize(
      ref JsonWriter writer,
      Version value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == (Version) null)
        writer.WriteNull();
      else
        writer.WriteString(value.ToString());
    }

    public Version Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? (Version) null : new Version(reader.ReadString());
    }
  }
}
