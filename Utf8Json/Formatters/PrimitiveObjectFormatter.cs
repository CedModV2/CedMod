// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.PrimitiveObjectFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class PrimitiveObjectFormatter : IJsonFormatter<object>, IJsonFormatter
  {
    public static readonly IJsonFormatter<object> Default = (IJsonFormatter<object>) new PrimitiveObjectFormatter();
    private static readonly Dictionary<Type, int> typeToJumpCode = new Dictionary<Type, int>()
    {
      {
        typeof (bool),
        0
      },
      {
        typeof (char),
        1
      },
      {
        typeof (sbyte),
        2
      },
      {
        typeof (byte),
        3
      },
      {
        typeof (short),
        4
      },
      {
        typeof (ushort),
        5
      },
      {
        typeof (int),
        6
      },
      {
        typeof (uint),
        7
      },
      {
        typeof (long),
        8
      },
      {
        typeof (ulong),
        9
      },
      {
        typeof (float),
        10
      },
      {
        typeof (double),
        11
      },
      {
        typeof (DateTime),
        12
      },
      {
        typeof (string),
        13
      },
      {
        typeof (byte[]),
        14
      }
    };

    public void Serialize(
      ref JsonWriter writer,
      object value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        Type type = value.GetType();
        int num1;
        if (PrimitiveObjectFormatter.typeToJumpCode.TryGetValue(type, out num1))
        {
          switch (num1)
          {
            case 0:
              writer.WriteBoolean((bool) value);
              return;
            case 1:
              CharFormatter.Default.Serialize(ref writer, (char) value, formatterResolver);
              return;
            case 2:
              writer.WriteSByte((sbyte) value);
              return;
            case 3:
              writer.WriteByte((byte) value);
              return;
            case 4:
              writer.WriteInt16((short) value);
              return;
            case 5:
              writer.WriteUInt16((ushort) value);
              return;
            case 6:
              writer.WriteInt32((int) value);
              return;
            case 7:
              writer.WriteUInt32((uint) value);
              return;
            case 8:
              writer.WriteInt64((long) value);
              return;
            case 9:
              writer.WriteUInt64((ulong) value);
              return;
            case 10:
              writer.WriteSingle((float) value);
              return;
            case 11:
              writer.WriteDouble((double) value);
              return;
            case 12:
              ISO8601DateTimeFormatter.Default.Serialize(ref writer, (DateTime) value, formatterResolver);
              return;
            case 13:
              writer.WriteString((string) value);
              return;
            case 14:
              ByteArrayFormatter.Default.Serialize(ref writer, (byte[]) value, formatterResolver);
              return;
          }
        }
        if (type.IsEnum)
        {
          writer.WriteString(type.ToString());
        }
        else
        {
          switch (value)
          {
            case IDictionary dictionary:
              int num2 = 0;
              writer.WriteBeginObject();
              foreach (DictionaryEntry dictionaryEntry in dictionary)
              {
                if (num2 != 0)
                  writer.WriteValueSeparator();
                writer.WritePropertyName((string) dictionaryEntry.Key);
                this.Serialize(ref writer, dictionaryEntry.Value, formatterResolver);
              }
              writer.WriteEndObject();
              break;
            case ICollection collection:
              int num3 = 0;
              writer.WriteBeginArray();
              foreach (object obj in (IEnumerable) collection)
              {
                if (num3 != 0)
                  writer.WriteValueSeparator();
                this.Serialize(ref writer, obj, formatterResolver);
              }
              writer.WriteEndArray();
              break;
            default:
              throw new InvalidOperationException("Not supported primitive object resolver. type:" + type.Name);
          }
        }
      }
    }

    public object Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      JsonToken currentJsonToken = reader.GetCurrentJsonToken();
      switch (currentJsonToken)
      {
        case JsonToken.BeginObject:
          Dictionary<string, object> dictionary = new Dictionary<string, object>();
          reader.ReadIsBeginObjectWithVerify();
          int count1 = 0;
          while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count1))
          {
            string key = reader.ReadPropertyName();
            object obj = this.Deserialize(ref reader, formatterResolver);
            dictionary.Add(key, obj);
          }
          return (object) dictionary;
        case JsonToken.EndObject:
        case JsonToken.EndArray:
        case JsonToken.ValueSeparator:
        case JsonToken.NameSeparator:
          throw new InvalidOperationException("Invalid Json Token:" + (object) currentJsonToken);
        case JsonToken.BeginArray:
          List<object> objectList = new List<object>(4);
          reader.ReadIsBeginArrayWithVerify();
          int count2 = 0;
          while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count2))
            objectList.Add(this.Deserialize(ref reader, formatterResolver));
          return (object) objectList;
        case JsonToken.Number:
          return (object) reader.ReadDouble();
        case JsonToken.String:
          return (object) reader.ReadString();
        case JsonToken.True:
          return (object) reader.ReadBoolean();
        case JsonToken.False:
          return (object) reader.ReadBoolean();
        case JsonToken.Null:
          reader.ReadIsNull();
          return (object) null;
        default:
          return (object) null;
      }
    }
  }
}
