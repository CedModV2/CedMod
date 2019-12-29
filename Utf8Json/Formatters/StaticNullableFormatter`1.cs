// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.StaticNullableFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class StaticNullableFormatter<T> : IJsonFormatter<T?>, IJsonFormatter
    where T : struct
  {
    private readonly IJsonFormatter<T> underlyingFormatter;

    public StaticNullableFormatter(IJsonFormatter<T> underlyingFormatter)
    {
      this.underlyingFormatter = underlyingFormatter;
    }

    public StaticNullableFormatter(Type formatterType, object[] formatterArguments)
    {
      try
      {
        this.underlyingFormatter = (IJsonFormatter<T>) Activator.CreateInstance(formatterType, formatterArguments);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException("Can not create formatter from JsonFormatterAttribute, check the target formatter is public and has constructor with right argument. FormatterType:" + formatterType.Name, ex);
      }
    }

    public void Serialize(
      ref JsonWriter writer,
      T? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        this.underlyingFormatter.Serialize(ref writer, value.Value, formatterResolver);
    }

    public T? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new T?() : new T?(this.underlyingFormatter.Deserialize(ref reader, formatterResolver));
    }
  }
}
