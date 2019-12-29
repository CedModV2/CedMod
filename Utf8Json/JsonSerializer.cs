// Decompiled with JetBrains decompiler
// Type: Utf8Json.JsonSerializer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;
using Utf8Json.Resolvers;

namespace Utf8Json
{
  public static class JsonSerializer
  {
    private static readonly byte[][] indent = Enumerable.Range(0, 100).Select<int, byte[]>((Func<int, byte[]>) (x => Encoding.UTF8.GetBytes(new string(' ', x * 2)))).ToArray<byte[]>();
    private static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
    private static IJsonFormatterResolver defaultResolver;

    public static IJsonFormatterResolver DefaultResolver
    {
      get
      {
        if (JsonSerializer.defaultResolver == null)
          JsonSerializer.defaultResolver = StandardResolver.Default;
        return JsonSerializer.defaultResolver;
      }
    }

    public static bool IsInitialized
    {
      get
      {
        return JsonSerializer.defaultResolver != null;
      }
    }

    public static void SetDefaultResolver(IJsonFormatterResolver resolver)
    {
      JsonSerializer.defaultResolver = resolver;
    }

    public static byte[] Serialize<T>(T obj)
    {
      return JsonSerializer.Serialize<T>(obj, JsonSerializer.defaultResolver);
    }

    public static byte[] Serialize<T>(T value, IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
      return writer.ToUtf8ByteArray();
    }

    public static void Serialize<T>(ref JsonWriter writer, T value)
    {
      JsonSerializer.Serialize<T>(ref writer, value, JsonSerializer.defaultResolver);
    }

    public static void Serialize<T>(
      ref JsonWriter writer,
      T value,
      IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
    }

    public static void Serialize<T>(Stream stream, T value)
    {
      JsonSerializer.Serialize<T>(stream, value, JsonSerializer.defaultResolver);
    }

    public static void Serialize<T>(Stream stream, T value, IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      ArraySegment<byte> arraySegment = JsonSerializer.SerializeUnsafe<T>(value, resolver);
      stream.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
    }

    public static ArraySegment<byte> SerializeUnsafe<T>(T obj)
    {
      return JsonSerializer.SerializeUnsafe<T>(obj, JsonSerializer.defaultResolver);
    }

    public static ArraySegment<byte> SerializeUnsafe<T>(
      T value,
      IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
      return writer.GetBuffer();
    }

    public static string ToJsonString<T>(T value)
    {
      return JsonSerializer.ToJsonString<T>(value, JsonSerializer.defaultResolver);
    }

    public static string ToJsonString<T>(T value, IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
      return writer.ToString();
    }

    public static T Deserialize<T>(string json)
    {
      return JsonSerializer.Deserialize<T>(json, JsonSerializer.defaultResolver);
    }

    public static T Deserialize<T>(string json, IJsonFormatterResolver resolver)
    {
      return JsonSerializer.Deserialize<T>(StringEncoding.UTF8.GetBytes(json), resolver);
    }

    public static T Deserialize<T>(byte[] bytes)
    {
      return JsonSerializer.Deserialize<T>(bytes, JsonSerializer.defaultResolver);
    }

    public static T Deserialize<T>(byte[] bytes, IJsonFormatterResolver resolver)
    {
      return JsonSerializer.Deserialize<T>(bytes, 0, resolver);
    }

    public static T Deserialize<T>(byte[] bytes, int offset)
    {
      return JsonSerializer.Deserialize<T>(bytes, offset, JsonSerializer.defaultResolver);
    }

    public static T Deserialize<T>(byte[] bytes, int offset, IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      JsonReader reader = new JsonReader(bytes, offset);
      return resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, resolver);
    }

    public static T Deserialize<T>(ref JsonReader reader)
    {
      return JsonSerializer.Deserialize<T>(ref reader, JsonSerializer.defaultResolver);
    }

    public static T Deserialize<T>(ref JsonReader reader, IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      return resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, resolver);
    }

    public static T Deserialize<T>(Stream stream)
    {
      return JsonSerializer.Deserialize<T>(stream, JsonSerializer.defaultResolver);
    }

    public static T Deserialize<T>(Stream stream, IJsonFormatterResolver resolver)
    {
      if (resolver == null)
        resolver = JsonSerializer.DefaultResolver;
      byte[] buffer = JsonSerializer.MemoryPool.GetBuffer();
      int newSize = JsonSerializer.FillFromStream(stream, ref buffer);
      if (new JsonReader(buffer).GetCurrentJsonToken() == JsonToken.Number)
        buffer = BinaryUtil.FastCloneWithResize(buffer, newSize);
      return JsonSerializer.Deserialize<T>(buffer, resolver);
    }

    public static string PrettyPrint(byte[] json)
    {
      return JsonSerializer.PrettyPrint(json, 0);
    }

    public static string PrettyPrint(byte[] json, int offset)
    {
      JsonReader reader = new JsonReader(json, offset);
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      JsonSerializer.WritePrittyPrint(ref reader, ref writer, 0);
      return writer.ToString();
    }

    public static string PrettyPrint(string json)
    {
      JsonReader reader = new JsonReader(Encoding.UTF8.GetBytes(json));
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      JsonSerializer.WritePrittyPrint(ref reader, ref writer, 0);
      return writer.ToString();
    }

    public static byte[] PrettyPrintByteArray(byte[] json)
    {
      return JsonSerializer.PrettyPrintByteArray(json, 0);
    }

    public static byte[] PrettyPrintByteArray(byte[] json, int offset)
    {
      JsonReader reader = new JsonReader(json, offset);
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      JsonSerializer.WritePrittyPrint(ref reader, ref writer, 0);
      return writer.ToUtf8ByteArray();
    }

    public static byte[] PrettyPrintByteArray(string json)
    {
      JsonReader reader = new JsonReader(Encoding.UTF8.GetBytes(json));
      JsonWriter writer = new JsonWriter(JsonSerializer.MemoryPool.GetBuffer());
      JsonSerializer.WritePrittyPrint(ref reader, ref writer, 0);
      return writer.ToUtf8ByteArray();
    }

    private static void WritePrittyPrint(ref JsonReader reader, ref JsonWriter writer, int depth)
    {
      switch (reader.GetCurrentJsonToken())
      {
        case JsonToken.BeginObject:
          writer.WriteBeginObject();
          writer.WriteRaw(JsonSerializer.newLine);
          int count1 = 0;
          while (reader.ReadIsInObject(ref count1))
          {
            if (count1 != 1)
            {
              writer.WriteRaw((byte) 44);
              writer.WriteRaw(JsonSerializer.newLine);
            }
            writer.WriteRaw(JsonSerializer.indent[depth + 1]);
            writer.WritePropertyName(reader.ReadPropertyName());
            writer.WriteRaw((byte) 32);
            JsonSerializer.WritePrittyPrint(ref reader, ref writer, depth + 1);
          }
          writer.WriteRaw(JsonSerializer.newLine);
          writer.WriteRaw(JsonSerializer.indent[depth]);
          writer.WriteEndObject();
          break;
        case JsonToken.BeginArray:
          writer.WriteBeginArray();
          writer.WriteRaw(JsonSerializer.newLine);
          int count2 = 0;
          while (reader.ReadIsInArray(ref count2))
          {
            if (count2 != 1)
            {
              writer.WriteRaw((byte) 44);
              writer.WriteRaw(JsonSerializer.newLine);
            }
            writer.WriteRaw(JsonSerializer.indent[depth + 1]);
            JsonSerializer.WritePrittyPrint(ref reader, ref writer, depth + 1);
          }
          writer.WriteRaw(JsonSerializer.newLine);
          writer.WriteRaw(JsonSerializer.indent[depth]);
          writer.WriteEndArray();
          break;
        case JsonToken.Number:
          double num = reader.ReadDouble();
          writer.WriteDouble(num);
          break;
        case JsonToken.String:
          string str = reader.ReadString();
          writer.WriteString(str);
          break;
        case JsonToken.True:
        case JsonToken.False:
          bool flag = reader.ReadBoolean();
          writer.WriteBoolean(flag);
          break;
        case JsonToken.Null:
          reader.ReadIsNull();
          writer.WriteNull();
          break;
      }
    }

    private static int FillFromStream(Stream input, ref byte[] buffer)
    {
      int offset = 0;
      int num;
      while ((num = input.Read(buffer, offset, buffer.Length - offset)) > 0)
      {
        offset += num;
        if (offset == buffer.Length)
          BinaryUtil.FastResize(ref buffer, offset * 2);
      }
      return offset;
    }

    public static class NonGeneric
    {
      private static readonly ThreadsafeTypeKeyHashTable<JsonSerializer.NonGeneric.CompiledMethods> serializes = new ThreadsafeTypeKeyHashTable<JsonSerializer.NonGeneric.CompiledMethods>(64, 0.75f);
      private static readonly Func<Type, JsonSerializer.NonGeneric.CompiledMethods> CreateCompiledMethods = (Func<Type, JsonSerializer.NonGeneric.CompiledMethods>) (t => new JsonSerializer.NonGeneric.CompiledMethods(t));

      private static JsonSerializer.NonGeneric.CompiledMethods GetOrAdd(Type type)
      {
        return JsonSerializer.NonGeneric.serializes.GetOrAdd(type, JsonSerializer.NonGeneric.CreateCompiledMethods);
      }

      public static byte[] Serialize(object value)
      {
        return value == null ? JsonSerializer.Serialize<object>(value) : JsonSerializer.NonGeneric.Serialize(value.GetType(), value, JsonSerializer.defaultResolver);
      }

      public static byte[] Serialize(Type type, object value)
      {
        return JsonSerializer.NonGeneric.Serialize(type, value, JsonSerializer.defaultResolver);
      }

      public static byte[] Serialize(object value, IJsonFormatterResolver resolver)
      {
        return value == null ? JsonSerializer.Serialize<object>(value, resolver) : JsonSerializer.NonGeneric.Serialize(value.GetType(), value, resolver);
      }

      public static byte[] Serialize(Type type, object value, IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).serialize1(value, resolver);
      }

      public static void Serialize(Stream stream, object value)
      {
        if (value == null)
          JsonSerializer.Serialize<object>(stream, value);
        else
          JsonSerializer.NonGeneric.Serialize(value.GetType(), stream, value, JsonSerializer.defaultResolver);
      }

      public static void Serialize(Type type, Stream stream, object value)
      {
        JsonSerializer.NonGeneric.Serialize(type, stream, value, JsonSerializer.defaultResolver);
      }

      public static void Serialize(Stream stream, object value, IJsonFormatterResolver resolver)
      {
        if (value == null)
          JsonSerializer.Serialize<object>(stream, value, resolver);
        else
          JsonSerializer.NonGeneric.Serialize(value.GetType(), stream, value, resolver);
      }

      public static void Serialize(
        Type type,
        Stream stream,
        object value,
        IJsonFormatterResolver resolver)
      {
        JsonSerializer.NonGeneric.GetOrAdd(type).serialize2(stream, value, resolver);
      }

      public static void Serialize(
        ref JsonWriter writer,
        object value,
        IJsonFormatterResolver resolver)
      {
        if (value == null)
          writer.WriteNull();
        else
          JsonSerializer.NonGeneric.Serialize(value.GetType(), ref writer, value, resolver);
      }

      public static void Serialize(Type type, ref JsonWriter writer, object value)
      {
        JsonSerializer.NonGeneric.Serialize(type, ref writer, value, JsonSerializer.defaultResolver);
      }

      public static void Serialize(
        Type type,
        ref JsonWriter writer,
        object value,
        IJsonFormatterResolver resolver)
      {
        JsonSerializer.NonGeneric.GetOrAdd(type).serialize3(ref writer, value, resolver);
      }

      public static ArraySegment<byte> SerializeUnsafe(object value)
      {
        return value == null ? JsonSerializer.SerializeUnsafe<object>(value) : JsonSerializer.NonGeneric.SerializeUnsafe(value.GetType(), value);
      }

      public static ArraySegment<byte> SerializeUnsafe(Type type, object value)
      {
        return JsonSerializer.NonGeneric.SerializeUnsafe(type, value, JsonSerializer.defaultResolver);
      }

      public static ArraySegment<byte> SerializeUnsafe(
        object value,
        IJsonFormatterResolver resolver)
      {
        return value == null ? JsonSerializer.SerializeUnsafe<object>(value) : JsonSerializer.NonGeneric.SerializeUnsafe(value.GetType(), value, resolver);
      }

      public static ArraySegment<byte> SerializeUnsafe(
        Type type,
        object value,
        IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).serializeUnsafe(value, resolver);
      }

      public static string ToJsonString(object value)
      {
        return value == null ? "null" : JsonSerializer.NonGeneric.ToJsonString(value.GetType(), value);
      }

      public static string ToJsonString(Type type, object value)
      {
        return JsonSerializer.NonGeneric.ToJsonString(type, value, JsonSerializer.defaultResolver);
      }

      public static string ToJsonString(object value, IJsonFormatterResolver resolver)
      {
        return value == null ? "null" : JsonSerializer.NonGeneric.ToJsonString(value.GetType(), value, resolver);
      }

      public static string ToJsonString(Type type, object value, IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).toJsonString(value, resolver);
      }

      public static object Deserialize(Type type, string json)
      {
        return JsonSerializer.NonGeneric.Deserialize(type, json, JsonSerializer.defaultResolver);
      }

      public static object Deserialize(Type type, string json, IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).deserialize1(json, resolver);
      }

      public static object Deserialize(Type type, byte[] bytes)
      {
        return JsonSerializer.NonGeneric.Deserialize(type, bytes, JsonSerializer.defaultResolver);
      }

      public static object Deserialize(Type type, byte[] bytes, IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.Deserialize(type, bytes, 0, JsonSerializer.defaultResolver);
      }

      public static object Deserialize(Type type, byte[] bytes, int offset)
      {
        return JsonSerializer.NonGeneric.Deserialize(type, bytes, offset, JsonSerializer.defaultResolver);
      }

      public static object Deserialize(
        Type type,
        byte[] bytes,
        int offset,
        IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).deserialize2(bytes, offset, resolver);
      }

      public static object Deserialize(Type type, Stream stream)
      {
        return JsonSerializer.NonGeneric.Deserialize(type, stream, JsonSerializer.defaultResolver);
      }

      public static object Deserialize(Type type, Stream stream, IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).deserialize3(stream, resolver);
      }

      public static object Deserialize(Type type, ref JsonReader reader)
      {
        return JsonSerializer.NonGeneric.Deserialize(type, ref reader, JsonSerializer.defaultResolver);
      }

      public static object Deserialize(
        Type type,
        ref JsonReader reader,
        IJsonFormatterResolver resolver)
      {
        return JsonSerializer.NonGeneric.GetOrAdd(type).deserialize4(ref reader, resolver);
      }

      private delegate void SerializeJsonWriter(
        ref JsonWriter writer,
        object value,
        IJsonFormatterResolver resolver);

      private delegate object DeserializeJsonReader(
        ref JsonReader reader,
        IJsonFormatterResolver resolver);

      private class CompiledMethods
      {
        public readonly Func<object, IJsonFormatterResolver, byte[]> serialize1;
        public readonly Action<Stream, object, IJsonFormatterResolver> serialize2;
        public readonly JsonSerializer.NonGeneric.SerializeJsonWriter serialize3;
        public readonly Func<object, IJsonFormatterResolver, ArraySegment<byte>> serializeUnsafe;
        public readonly Func<object, IJsonFormatterResolver, string> toJsonString;
        public readonly Func<string, IJsonFormatterResolver, object> deserialize1;
        public readonly Func<byte[], int, IJsonFormatterResolver, object> deserialize2;
        public readonly Func<Stream, IJsonFormatterResolver, object> deserialize3;
        public readonly JsonSerializer.NonGeneric.DeserializeJsonReader deserialize4;

        public CompiledMethods(Type type)
        {
          DynamicMethod dm1 = new DynamicMethod(nameof (serialize1), typeof (byte[]), new Type[2]
          {
            typeof (object),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator1 = dm1.GetILGenerator();
          ilGenerator1.EmitLdarg(0);
          ilGenerator1.EmitUnboxOrCast(type);
          ilGenerator1.EmitLdarg(1);
          ilGenerator1.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Serialize", new Type[2]
          {
            null,
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator1.Emit(OpCodes.Ret);
          this.serialize1 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Func<object, IJsonFormatterResolver, byte[]>>(dm1);
          DynamicMethod dm2 = new DynamicMethod(nameof (serialize2), (Type) null, new Type[3]
          {
            typeof (Stream),
            typeof (object),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator2 = dm2.GetILGenerator();
          ilGenerator2.EmitLdarg(0);
          ilGenerator2.EmitLdarg(1);
          ilGenerator2.EmitUnboxOrCast(type);
          ilGenerator2.EmitLdarg(2);
          ilGenerator2.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Serialize", new Type[3]
          {
            typeof (Stream),
            null,
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator2.Emit(OpCodes.Ret);
          this.serialize2 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Action<Stream, object, IJsonFormatterResolver>>(dm2);
          DynamicMethod dm3 = new DynamicMethod(nameof (serialize3), (Type) null, new Type[3]
          {
            typeof (JsonWriter).MakeByRefType(),
            typeof (object),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator3 = dm3.GetILGenerator();
          ilGenerator3.EmitLdarg(0);
          ilGenerator3.EmitLdarg(1);
          ilGenerator3.EmitUnboxOrCast(type);
          ilGenerator3.EmitLdarg(2);
          ilGenerator3.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Serialize", new Type[3]
          {
            typeof (JsonWriter).MakeByRefType(),
            null,
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator3.Emit(OpCodes.Ret);
          this.serialize3 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<JsonSerializer.NonGeneric.SerializeJsonWriter>(dm3);
          DynamicMethod dm4 = new DynamicMethod(nameof (serializeUnsafe), typeof (ArraySegment<byte>), new Type[2]
          {
            typeof (object),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator4 = dm4.GetILGenerator();
          ilGenerator4.EmitLdarg(0);
          ilGenerator4.EmitUnboxOrCast(type);
          ilGenerator4.EmitLdarg(1);
          ilGenerator4.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "SerializeUnsafe", new Type[2]
          {
            null,
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator4.Emit(OpCodes.Ret);
          this.serializeUnsafe = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Func<object, IJsonFormatterResolver, ArraySegment<byte>>>(dm4);
          DynamicMethod dm5 = new DynamicMethod(nameof (toJsonString), typeof (string), new Type[2]
          {
            typeof (object),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator5 = dm5.GetILGenerator();
          ilGenerator5.EmitLdarg(0);
          ilGenerator5.EmitUnboxOrCast(type);
          ilGenerator5.EmitLdarg(1);
          ilGenerator5.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "ToJsonString", new Type[2]
          {
            null,
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator5.Emit(OpCodes.Ret);
          this.toJsonString = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Func<object, IJsonFormatterResolver, string>>(dm5);
          DynamicMethod dm6 = new DynamicMethod("Deserialize", typeof (object), new Type[2]
          {
            typeof (string),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator6 = dm6.GetILGenerator();
          ilGenerator6.EmitLdarg(0);
          ilGenerator6.EmitLdarg(1);
          ilGenerator6.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Deserialize", new Type[2]
          {
            typeof (string),
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator6.EmitBoxOrDoNothing(type);
          ilGenerator6.Emit(OpCodes.Ret);
          this.deserialize1 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Func<string, IJsonFormatterResolver, object>>(dm6);
          DynamicMethod dm7 = new DynamicMethod("Deserialize", typeof (object), new Type[3]
          {
            typeof (byte[]),
            typeof (int),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator7 = dm7.GetILGenerator();
          ilGenerator7.EmitLdarg(0);
          ilGenerator7.EmitLdarg(1);
          ilGenerator7.EmitLdarg(2);
          ilGenerator7.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Deserialize", new Type[3]
          {
            typeof (byte[]),
            typeof (int),
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator7.EmitBoxOrDoNothing(type);
          ilGenerator7.Emit(OpCodes.Ret);
          this.deserialize2 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Func<byte[], int, IJsonFormatterResolver, object>>(dm7);
          DynamicMethod dm8 = new DynamicMethod("Deserialize", typeof (object), new Type[2]
          {
            typeof (Stream),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator8 = dm8.GetILGenerator();
          ilGenerator8.EmitLdarg(0);
          ilGenerator8.EmitLdarg(1);
          ilGenerator8.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Deserialize", new Type[2]
          {
            typeof (Stream),
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator8.EmitBoxOrDoNothing(type);
          ilGenerator8.Emit(OpCodes.Ret);
          this.deserialize3 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<Func<Stream, IJsonFormatterResolver, object>>(dm8);
          DynamicMethod dm9 = new DynamicMethod("Deserialize", typeof (object), new Type[2]
          {
            typeof (JsonReader).MakeByRefType(),
            typeof (IJsonFormatterResolver)
          }, type.Module, true);
          ILGenerator ilGenerator9 = dm9.GetILGenerator();
          ilGenerator9.EmitLdarg(0);
          ilGenerator9.EmitLdarg(1);
          ilGenerator9.EmitCall(JsonSerializer.NonGeneric.CompiledMethods.GetMethod(type, "Deserialize", new Type[2]
          {
            typeof (JsonReader).MakeByRefType(),
            typeof (IJsonFormatterResolver)
          }));
          ilGenerator9.EmitBoxOrDoNothing(type);
          ilGenerator9.Emit(OpCodes.Ret);
          this.deserialize4 = JsonSerializer.NonGeneric.CompiledMethods.CreateDelegate<JsonSerializer.NonGeneric.DeserializeJsonReader>(dm9);
        }

        private static T CreateDelegate<T>(DynamicMethod dm)
        {
          return (T) ((MethodInfo) dm).CreateDelegate(typeof (T));
        }

        private static MethodInfo GetMethod(Type type, string name, Type[] arguments)
        {
          return ((IEnumerable<MethodInfo>) typeof (JsonSerializer).GetMethods(BindingFlags.Static | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (x => x.Name == name)).Single<MethodInfo>((Func<MethodInfo, bool>) (x =>
          {
            ParameterInfo[] parameters = x.GetParameters();
            if (parameters.Length != arguments.Length)
              return false;
            for (int index = 0; index < parameters.Length; ++index)
            {
              if ((!(arguments[index] == (Type) null) || !parameters[index].ParameterType.IsGenericParameter) && parameters[index].ParameterType != arguments[index])
                return false;
            }
            return true;
          })).MakeGenericMethod(type);
        }
      }
    }

    private static class MemoryPool
    {
      [ThreadStatic]
      private static byte[] buffer;

      public static byte[] GetBuffer()
      {
        if (JsonSerializer.MemoryPool.buffer == null)
          JsonSerializer.MemoryPool.buffer = new byte[65536];
        return JsonSerializer.MemoryPool.buffer;
      }
    }
  }
}
