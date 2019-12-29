// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.AnonymousFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class AnonymousFormatter<T> : IJsonFormatter<T>, IJsonFormatter
  {
    private readonly JsonSerializeAction<T> serialize;
    private readonly JsonDeserializeFunc<T> deserialize;

    public AnonymousFormatter(JsonSerializeAction<T> serialize, JsonDeserializeFunc<T> deserialize)
    {
      this.serialize = serialize;
      this.deserialize = deserialize;
    }

    public void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
    {
      if (this.serialize == null)
        throw new InvalidOperationException(this.GetType().Name + " does not support Serialize.");
      this.serialize(ref writer, value, formatterResolver);
    }

    public T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (this.deserialize == null)
        throw new InvalidOperationException(this.GetType().Name + " does not support Deserialize.");
      return this.deserialize(ref reader, formatterResolver);
    }
  }
}
