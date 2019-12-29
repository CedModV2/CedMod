// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Formatters
{
  public sealed class NullableFormatter<T> : IJsonFormatter<T?>, IJsonFormatter
    where T : struct
  {
    public void Serialize(
      ref JsonWriter writer,
      T? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        formatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.Value, formatterResolver);
    }

    public T? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new T?() : new T?(formatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader, formatterResolver));
    }
  }
}
