// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.DecimalFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class DecimalFormatter : IJsonFormatter<Decimal>, IJsonFormatter
  {
    public static readonly IJsonFormatter<Decimal> Default = (IJsonFormatter<Decimal>) new DecimalFormatter();
    private readonly bool serializeAsString;

    public DecimalFormatter()
      : this(false)
    {
    }

    public DecimalFormatter(bool serializeAsString)
    {
      this.serializeAsString = serializeAsString;
    }

    public void Serialize(
      ref JsonWriter writer,
      Decimal value,
      IJsonFormatterResolver formatterResolver)
    {
      if (this.serializeAsString)
        writer.WriteString(value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      else
        writer.WriteRaw(StringEncoding.UTF8.GetBytes(value.ToString((IFormatProvider) CultureInfo.InvariantCulture)));
    }

    public Decimal Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      JsonToken currentJsonToken = reader.GetCurrentJsonToken();
      switch (currentJsonToken)
      {
        case JsonToken.Number:
          ArraySegment<byte> arraySegment = reader.ReadNumberSegment();
          return Decimal.Parse(StringEncoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count), NumberStyles.Float, (IFormatProvider) CultureInfo.InvariantCulture);
        case JsonToken.String:
          return Decimal.Parse(reader.ReadString(), NumberStyles.Float, (IFormatProvider) CultureInfo.InvariantCulture);
        default:
          throw new InvalidOperationException("Invalid Json Token for DecimalFormatter:" + (object) currentJsonToken);
      }
    }
  }
}
