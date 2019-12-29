// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.EnumFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public class EnumFormatter<T> : IJsonFormatter<T>, IJsonFormatter, IObjectPropertyNameFormatter<T>
  {
    private static readonly ByteArrayStringHashTable<T> nameValueMapping;
    private static readonly Dictionary<T, string> valueNameMapping;
    private static readonly JsonSerializeAction<T> defaultSerializeByUnderlyingValue;
    private static readonly JsonDeserializeFunc<T> defaultDeserializeByUnderlyingValue;
    private readonly bool serializeByName;
    private readonly JsonSerializeAction<T> serializeByUnderlyingValue;
    private readonly JsonDeserializeFunc<T> deserializeByUnderlyingValue;

    static EnumFormatter()
    {
      List<string> stringList = new List<string>();
      List<object> objectList = new List<object>();
      Type type = typeof (T);
      foreach (FieldInfo fieldInfo in ((IEnumerable<FieldInfo>) type.GetFields()).Where<FieldInfo>((Func<FieldInfo, bool>) (fi => fi.FieldType == type)))
      {
        object obj = fieldInfo.GetValue((object) null);
        string name = Enum.GetName(type, obj);
        DataMemberAttribute dataMemberAttribute = fieldInfo.GetCustomAttributes(typeof (DataMemberAttribute), true).OfType<DataMemberAttribute>().FirstOrDefault<DataMemberAttribute>();
        EnumMemberAttribute enumMemberAttribute = fieldInfo.GetCustomAttributes(typeof (EnumMemberAttribute), true).OfType<EnumMemberAttribute>().FirstOrDefault<EnumMemberAttribute>();
        objectList.Add(obj);
        stringList.Add(enumMemberAttribute == null || enumMemberAttribute.Value == null ? (dataMemberAttribute == null || dataMemberAttribute.Name == null ? name : dataMemberAttribute.Name) : enumMemberAttribute.Value);
      }
      EnumFormatter<T>.nameValueMapping = new ByteArrayStringHashTable<T>(stringList.Count);
      EnumFormatter<T>.valueNameMapping = new Dictionary<T, string>(stringList.Count);
      for (int index = 0; index < stringList.Count; ++index)
      {
        EnumFormatter<T>.nameValueMapping.Add(JsonWriter.GetEncodedPropertyNameWithoutQuotation(stringList[index]), (T) objectList[index]);
        EnumFormatter<T>.valueNameMapping[(T) objectList[index]] = stringList[index];
      }
      bool isBoxed1;
      object serializeDelegate = EnumFormatterHelper.GetSerializeDelegate(typeof (T), out isBoxed1);
      if (isBoxed1)
      {
        JsonSerializeAction<object> boxSerialize = (JsonSerializeAction<object>) serializeDelegate;
        EnumFormatter<T>.defaultSerializeByUnderlyingValue = (JsonSerializeAction<T>) ((ref JsonWriter writer, T value, IJsonFormatterResolver _) => boxSerialize(ref writer, (object) value, _));
      }
      else
        EnumFormatter<T>.defaultSerializeByUnderlyingValue = (JsonSerializeAction<T>) serializeDelegate;
      bool isBoxed2;
      object deserializeDelegate = EnumFormatterHelper.GetDeserializeDelegate(typeof (T), out isBoxed2);
      if (isBoxed2)
      {
        JsonDeserializeFunc<object> boxDeserialize = (JsonDeserializeFunc<object>) deserializeDelegate;
        EnumFormatter<T>.defaultDeserializeByUnderlyingValue = (JsonDeserializeFunc<T>) ((ref JsonReader reader, IJsonFormatterResolver _) => (T) boxDeserialize(ref reader, _));
      }
      else
        EnumFormatter<T>.defaultDeserializeByUnderlyingValue = (JsonDeserializeFunc<T>) deserializeDelegate;
    }

    public EnumFormatter(bool serializeByName)
    {
      this.serializeByName = serializeByName;
      this.serializeByUnderlyingValue = EnumFormatter<T>.defaultSerializeByUnderlyingValue;
      this.deserializeByUnderlyingValue = EnumFormatter<T>.defaultDeserializeByUnderlyingValue;
    }

    public EnumFormatter(
      JsonSerializeAction<T> valueSerializeAction,
      JsonDeserializeFunc<T> valueDeserializeAction)
    {
      this.serializeByName = false;
      this.serializeByUnderlyingValue = valueSerializeAction;
      this.deserializeByUnderlyingValue = valueDeserializeAction;
    }

    public void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
    {
      if (this.serializeByName)
      {
        string str;
        if (!EnumFormatter<T>.valueNameMapping.TryGetValue(value, out str))
          str = value.ToString();
        writer.WriteString(str);
      }
      else
        this.serializeByUnderlyingValue(ref writer, value, formatterResolver);
    }

    public T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      switch (reader.GetCurrentJsonToken())
      {
        case JsonToken.Number:
          return this.deserializeByUnderlyingValue(ref reader, formatterResolver);
        case JsonToken.String:
          ArraySegment<byte> key = reader.ReadStringSegmentUnsafe();
          T obj;
          if (!EnumFormatter<T>.nameValueMapping.TryGetValue(key, out obj))
            obj = (T) Enum.Parse(typeof (T), StringEncoding.UTF8.GetString(key.Array, key.Offset, key.Count));
          return obj;
        default:
          throw new InvalidOperationException("Can't parse JSON to Enum format.");
      }
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      T value,
      IJsonFormatterResolver formatterResolver)
    {
      if (this.serializeByName)
      {
        this.Serialize(ref writer, value, formatterResolver);
      }
      else
      {
        writer.WriteQuotation();
        this.Serialize(ref writer, value, formatterResolver);
        writer.WriteQuotation();
      }
    }

    public T DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (this.serializeByName)
        return this.Deserialize(ref reader, formatterResolver);
      if (reader.GetCurrentJsonToken() != JsonToken.String)
        throw new InvalidOperationException("Can't parse JSON to Enum format.");
      reader.AdvanceOffset(1);
      T obj = this.Deserialize(ref reader, formatterResolver);
      reader.SkipWhiteSpace();
      reader.AdvanceOffset(1);
      return obj;
    }
  }
}
