// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UnixTimestampDateTimeFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class UnixTimestampDateTimeFormatter : IJsonFormatter<DateTime>, IJsonFormatter
  {
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Serialize(
      ref JsonWriter writer,
      DateTime value,
      IJsonFormatterResolver formatterResolver)
    {
      long totalSeconds = (long) (value.ToUniversalTime() - UnixTimestampDateTimeFormatter.UnixEpoch).TotalSeconds;
      writer.WriteQuotation();
      writer.WriteInt64(totalSeconds);
      writer.WriteQuotation();
    }

    public DateTime Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentUnsafe();
      ulong num = NumberConverter.ReadUInt64(arraySegment.Array, arraySegment.Offset, out int _);
      return UnixTimestampDateTimeFormatter.UnixEpoch.AddSeconds((double) num);
    }
  }
}
