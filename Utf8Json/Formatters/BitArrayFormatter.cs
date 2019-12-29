// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.BitArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class BitArrayFormatter : IJsonFormatter<BitArray>, IJsonFormatter
  {
    public static readonly IJsonFormatter<BitArray> Default = (IJsonFormatter<BitArray>) new BitArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      BitArray value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteBeginArray();
        for (int index = 0; index < value.Length; ++index)
        {
          if (index != 0)
            writer.WriteValueSeparator();
          writer.WriteBoolean(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public BitArray Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (BitArray) null;
      reader.ReadIsBeginArrayWithVerify();
      int count = 0;
      ArrayBuffer<bool> arrayBuffer = new ArrayBuffer<bool>(4);
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        arrayBuffer.Add(reader.ReadBoolean());
      return new BitArray(arrayBuffer.ToArray());
    }
  }
}
