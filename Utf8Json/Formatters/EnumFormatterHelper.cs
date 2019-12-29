// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.EnumFormatterHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public static class EnumFormatterHelper
  {
    public static object GetSerializeDelegate(Type type, out bool isBoxed)
    {
      Type underlyingType = Enum.GetUnderlyingType(type);
      isBoxed = true;
      if (underlyingType == typeof (byte))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteByte((byte) value));
      if (underlyingType == typeof (sbyte))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteSByte((sbyte) value));
      if (underlyingType == typeof (short))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteInt16((short) value));
      if (underlyingType == typeof (ushort))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteUInt16((ushort) value));
      if (underlyingType == typeof (int))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteInt32((int) value));
      if (underlyingType == typeof (uint))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteUInt32((uint) value));
      if (underlyingType == typeof (long))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteInt64((long) value));
      if (underlyingType == typeof (ulong))
        return (object) (JsonSerializeAction<object>) ((ref JsonWriter writer, object value, IJsonFormatterResolver _) => writer.WriteUInt64((ulong) value));
      throw new InvalidOperationException("Type is not Enum. Type:" + (object) type);
    }

    public static object GetDeserializeDelegate(Type type, out bool isBoxed)
    {
      Type underlyingType = Enum.GetUnderlyingType(type);
      isBoxed = true;
      if (underlyingType == typeof (byte))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadByte());
      if (underlyingType == typeof (sbyte))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadSByte());
      if (underlyingType == typeof (short))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadInt16());
      if (underlyingType == typeof (ushort))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadUInt16());
      if (underlyingType == typeof (int))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadInt32());
      if (underlyingType == typeof (uint))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadUInt32());
      if (underlyingType == typeof (long))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadInt64());
      if (underlyingType == typeof (ulong))
        return (object) (JsonDeserializeFunc<object>) ((ref JsonReader reader, IJsonFormatterResolver _) => (object) reader.ReadUInt64());
      throw new InvalidOperationException("Type is not Enum. Type:" + (object) type);
    }
  }
}
