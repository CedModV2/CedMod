// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicMethodAnonymousFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Resolvers.Internal
{
  internal class DynamicMethodAnonymousFormatter<T> : IJsonFormatter<T>, IJsonFormatter
  {
    private readonly byte[][] stringByteKeysField;
    private readonly object[] serializeCustomFormatters;
    private readonly object[] deserializeCustomFormatters;
    private readonly AnonymousJsonSerializeAction<T> serialize;
    private readonly AnonymousJsonDeserializeFunc<T> deserialize;

    public DynamicMethodAnonymousFormatter(
      byte[][] stringByteKeysField,
      object[] serializeCustomFormatters,
      object[] deserializeCustomFormatters,
      AnonymousJsonSerializeAction<T> serialize,
      AnonymousJsonDeserializeFunc<T> deserialize)
    {
      this.stringByteKeysField = stringByteKeysField;
      this.serializeCustomFormatters = serializeCustomFormatters;
      this.deserializeCustomFormatters = deserializeCustomFormatters;
      this.serialize = serialize;
      this.deserialize = deserialize;
    }

    public void Serialize(ref Utf8Json.JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
    {
      if (this.serialize == null)
        throw new InvalidOperationException(this.GetType().Name + " does not support Serialize.");
      this.serialize(this.stringByteKeysField, this.serializeCustomFormatters, ref writer, value, formatterResolver);
    }

    public T Deserialize(ref Utf8Json.JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (this.deserialize == null)
        throw new InvalidOperationException(this.GetType().Name + " does not support Deserialize.");
      return this.deserialize(this.deserializeCustomFormatters, ref reader, formatterResolver);
    }
  }
}
