// Utf8Json.JsonSerializer
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Utf8Json;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;
using Utf8Json.Resolvers;

public static class JsonSerializer
{
	public static class NonGeneric
	{
		private delegate void SerializeJsonWriter(ref JsonWriter writer, object value, IJsonFormatterResolver resolver);

		private delegate object DeserializeJsonReader(ref JsonReader reader, IJsonFormatterResolver resolver);

		private class CompiledMethods
		{
			public readonly Func<object, IJsonFormatterResolver, byte[]> serialize1;

			public readonly Action<Stream, object, IJsonFormatterResolver> serialize2;

			public readonly SerializeJsonWriter serialize3;

			public readonly Func<object, IJsonFormatterResolver, ArraySegment<byte>> serializeUnsafe;

			public readonly Func<object, IJsonFormatterResolver, string> toJsonString;

			public readonly Func<string, IJsonFormatterResolver, object> deserialize1;

			public readonly Func<byte[], int, IJsonFormatterResolver, object> deserialize2;

			public readonly Func<Stream, IJsonFormatterResolver, object> deserialize3;

			public readonly DeserializeJsonReader deserialize4;

			public CompiledMethods(Type type)
			{
				DynamicMethod dynamicMethod = new DynamicMethod("serialize1", typeof(byte[]), new Type[2]
				{
					typeof(object),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.EmitLdarg(0);
				iLGenerator.EmitUnboxOrCast(type);
				iLGenerator.EmitLdarg(1);
				iLGenerator.EmitCall(GetMethod(type, "Serialize", new Type[2]
				{
					null,
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator.Emit(OpCodes.Ret);
				serialize1 = CreateDelegate<Func<object, IJsonFormatterResolver, byte[]>>(dynamicMethod);
				DynamicMethod dynamicMethod2 = new DynamicMethod("serialize2", null, new Type[3]
				{
					typeof(Stream),
					typeof(object),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
				iLGenerator2.EmitLdarg(0);
				iLGenerator2.EmitLdarg(1);
				iLGenerator2.EmitUnboxOrCast(type);
				iLGenerator2.EmitLdarg(2);
				iLGenerator2.EmitCall(GetMethod(type, "Serialize", new Type[3]
				{
					typeof(Stream),
					null,
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator2.Emit(OpCodes.Ret);
				serialize2 = CreateDelegate<Action<Stream, object, IJsonFormatterResolver>>(dynamicMethod2);
				DynamicMethod dynamicMethod3 = new DynamicMethod("serialize3", null, new Type[3]
				{
					typeof(JsonWriter).MakeByRefType(),
					typeof(object),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator3 = dynamicMethod3.GetILGenerator();
				iLGenerator3.EmitLdarg(0);
				iLGenerator3.EmitLdarg(1);
				iLGenerator3.EmitUnboxOrCast(type);
				iLGenerator3.EmitLdarg(2);
				iLGenerator3.EmitCall(GetMethod(type, "Serialize", new Type[3]
				{
					typeof(JsonWriter).MakeByRefType(),
					null,
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator3.Emit(OpCodes.Ret);
				serialize3 = CreateDelegate<SerializeJsonWriter>(dynamicMethod3);
				DynamicMethod dynamicMethod4 = new DynamicMethod("serializeUnsafe", typeof(ArraySegment<byte>), new Type[2]
				{
					typeof(object),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator4 = dynamicMethod4.GetILGenerator();
				iLGenerator4.EmitLdarg(0);
				iLGenerator4.EmitUnboxOrCast(type);
				iLGenerator4.EmitLdarg(1);
				iLGenerator4.EmitCall(GetMethod(type, "SerializeUnsafe", new Type[2]
				{
					null,
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator4.Emit(OpCodes.Ret);
				serializeUnsafe = CreateDelegate<Func<object, IJsonFormatterResolver, ArraySegment<byte>>>(dynamicMethod4);
				DynamicMethod dynamicMethod5 = new DynamicMethod("toJsonString", typeof(string), new Type[2]
				{
					typeof(object),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator5 = dynamicMethod5.GetILGenerator();
				iLGenerator5.EmitLdarg(0);
				iLGenerator5.EmitUnboxOrCast(type);
				iLGenerator5.EmitLdarg(1);
				iLGenerator5.EmitCall(GetMethod(type, "ToJsonString", new Type[2]
				{
					null,
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator5.Emit(OpCodes.Ret);
				toJsonString = CreateDelegate<Func<object, IJsonFormatterResolver, string>>(dynamicMethod5);
				DynamicMethod dynamicMethod6 = new DynamicMethod("Deserialize", typeof(object), new Type[2]
				{
					typeof(string),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator6 = dynamicMethod6.GetILGenerator();
				iLGenerator6.EmitLdarg(0);
				iLGenerator6.EmitLdarg(1);
				iLGenerator6.EmitCall(GetMethod(type, "Deserialize", new Type[2]
				{
					typeof(string),
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator6.EmitBoxOrDoNothing(type);
				iLGenerator6.Emit(OpCodes.Ret);
				deserialize1 = CreateDelegate<Func<string, IJsonFormatterResolver, object>>(dynamicMethod6);
				DynamicMethod dynamicMethod7 = new DynamicMethod("Deserialize", typeof(object), new Type[3]
				{
					typeof(byte[]),
					typeof(int),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator7 = dynamicMethod7.GetILGenerator();
				iLGenerator7.EmitLdarg(0);
				iLGenerator7.EmitLdarg(1);
				iLGenerator7.EmitLdarg(2);
				iLGenerator7.EmitCall(GetMethod(type, "Deserialize", new Type[3]
				{
					typeof(byte[]),
					typeof(int),
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator7.EmitBoxOrDoNothing(type);
				iLGenerator7.Emit(OpCodes.Ret);
				deserialize2 = CreateDelegate<Func<byte[], int, IJsonFormatterResolver, object>>(dynamicMethod7);
				DynamicMethod dynamicMethod8 = new DynamicMethod("Deserialize", typeof(object), new Type[2]
				{
					typeof(Stream),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator8 = dynamicMethod8.GetILGenerator();
				iLGenerator8.EmitLdarg(0);
				iLGenerator8.EmitLdarg(1);
				iLGenerator8.EmitCall(GetMethod(type, "Deserialize", new Type[2]
				{
					typeof(Stream),
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator8.EmitBoxOrDoNothing(type);
				iLGenerator8.Emit(OpCodes.Ret);
				deserialize3 = CreateDelegate<Func<Stream, IJsonFormatterResolver, object>>(dynamicMethod8);
				DynamicMethod dynamicMethod9 = new DynamicMethod("Deserialize", typeof(object), new Type[2]
				{
					typeof(JsonReader).MakeByRefType(),
					typeof(IJsonFormatterResolver)
				}, type.Module, skipVisibility: true);
				ILGenerator iLGenerator9 = dynamicMethod9.GetILGenerator();
				iLGenerator9.EmitLdarg(0);
				iLGenerator9.EmitLdarg(1);
				iLGenerator9.EmitCall(GetMethod(type, "Deserialize", new Type[2]
				{
					typeof(JsonReader).MakeByRefType(),
					typeof(IJsonFormatterResolver)
				}));
				iLGenerator9.EmitBoxOrDoNothing(type);
				iLGenerator9.Emit(OpCodes.Ret);
				deserialize4 = CreateDelegate<DeserializeJsonReader>(dynamicMethod9);
			}

			private static T CreateDelegate<T>(DynamicMethod dm)
			{
				return (T)(object)dm.CreateDelegate(typeof(T));
			}

			private static MethodInfo GetMethod(Type type, string name, Type[] arguments)
			{
				return (from x in typeof(JsonSerializer).GetMethods(BindingFlags.Static | BindingFlags.Public)
						where x.Name == name
						select x).Single(delegate (MethodInfo x)
						{
							ParameterInfo[] parameters = x.GetParameters();
							if (parameters.Length != arguments.Length)
							{
								return false;
							}
							for (int i = 0; i < parameters.Length; i++)
							{
								if ((!(arguments[i] == null) || !parameters[i].ParameterType.IsGenericParameter) && parameters[i].ParameterType != arguments[i])
								{
									return false;
								}
							}
							return true;
						}).MakeGenericMethod(type);
			}
		}

		private static readonly Func<Type, CompiledMethods> CreateCompiledMethods;

		private static readonly ThreadsafeTypeKeyHashTable<CompiledMethods> serializes;

		static NonGeneric()
		{
			serializes = new ThreadsafeTypeKeyHashTable<CompiledMethods>(64);
			CreateCompiledMethods = ((Type t) => new CompiledMethods(t));
		}

		private static CompiledMethods GetOrAdd(Type type)
		{
			return serializes.GetOrAdd(type, CreateCompiledMethods);
		}

		public static byte[] Serialize(object value)
		{
			if (value == null)
			{
				return JsonSerializer.Serialize(value);
			}
			return Serialize(value.GetType(), value, defaultResolver);
		}

		public static byte[] Serialize(Type type, object value)
		{
			return Serialize(type, value, defaultResolver);
		}

		public static byte[] Serialize(object value, IJsonFormatterResolver resolver)
		{
			if (value == null)
			{
				return JsonSerializer.Serialize(value, resolver);
			}
			return Serialize(value.GetType(), value, resolver);
		}

		public static byte[] Serialize(Type type, object value, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).serialize1(value, resolver);
		}

		public static void Serialize(Stream stream, object value)
		{
			if (value == null)
			{
				JsonSerializer.Serialize(stream, value);
			}
			else
			{
				Serialize(value.GetType(), stream, value, defaultResolver);
			}
		}

		public static void Serialize(Type type, Stream stream, object value)
		{
			Serialize(type, stream, value, defaultResolver);
		}

		public static void Serialize(Stream stream, object value, IJsonFormatterResolver resolver)
		{
			if (value == null)
			{
				JsonSerializer.Serialize(stream, value, resolver);
			}
			else
			{
				Serialize(value.GetType(), stream, value, resolver);
			}
		}

		public static void Serialize(Type type, Stream stream, object value, IJsonFormatterResolver resolver)
		{
			GetOrAdd(type).serialize2(stream, value, resolver);
		}

		public static void Serialize(ref JsonWriter writer, object value, IJsonFormatterResolver resolver)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				Serialize(value.GetType(), ref writer, value, resolver);
			}
		}

		public static void Serialize(Type type, ref JsonWriter writer, object value)
		{
			Serialize(type, ref writer, value, defaultResolver);
		}

		public static void Serialize(Type type, ref JsonWriter writer, object value, IJsonFormatterResolver resolver)
		{
			GetOrAdd(type).serialize3(ref writer, value, resolver);
		}

		public static ArraySegment<byte> SerializeUnsafe(object value)
		{
			if (value == null)
			{
				return JsonSerializer.SerializeUnsafe(value);
			}
			return SerializeUnsafe(value.GetType(), value);
		}

		public static ArraySegment<byte> SerializeUnsafe(Type type, object value)
		{
			return SerializeUnsafe(type, value, defaultResolver);
		}

		public static ArraySegment<byte> SerializeUnsafe(object value, IJsonFormatterResolver resolver)
		{
			if (value == null)
			{
				return JsonSerializer.SerializeUnsafe(value);
			}
			return SerializeUnsafe(value.GetType(), value, resolver);
		}

		public static ArraySegment<byte> SerializeUnsafe(Type type, object value, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).serializeUnsafe(value, resolver);
		}

		public static string ToJsonString(object value)
		{
			if (value == null)
			{
				return "null";
			}
			return ToJsonString(value.GetType(), value);
		}

		public static string ToJsonString(Type type, object value)
		{
			return ToJsonString(type, value, defaultResolver);
		}

		public static string ToJsonString(object value, IJsonFormatterResolver resolver)
		{
			if (value == null)
			{
				return "null";
			}
			return ToJsonString(value.GetType(), value, resolver);
		}

		public static string ToJsonString(Type type, object value, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).toJsonString(value, resolver);
		}

		public static object Deserialize(Type type, string json)
		{
			return Deserialize(type, json, defaultResolver);
		}

		public static object Deserialize(Type type, string json, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).deserialize1(json, resolver);
		}

		public static object Deserialize(Type type, byte[] bytes)
		{
			return Deserialize(type, bytes, defaultResolver);
		}

		public static object Deserialize(Type type, byte[] bytes, IJsonFormatterResolver resolver)
		{
			return Deserialize(type, bytes, 0, defaultResolver);
		}

		public static object Deserialize(Type type, byte[] bytes, int offset)
		{
			return Deserialize(type, bytes, offset, defaultResolver);
		}

		public static object Deserialize(Type type, byte[] bytes, int offset, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).deserialize2(bytes, offset, resolver);
		}

		public static object Deserialize(Type type, Stream stream)
		{
			return Deserialize(type, stream, defaultResolver);
		}

		public static object Deserialize(Type type, Stream stream, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).deserialize3(stream, resolver);
		}

		public static object Deserialize(Type type, ref JsonReader reader)
		{
			return Deserialize(type, ref reader, defaultResolver);
		}

		public static object Deserialize(Type type, ref JsonReader reader, IJsonFormatterResolver resolver)
		{
			return GetOrAdd(type).deserialize4(ref reader, resolver);
		}
	}

	private static class MemoryPool
	{
		[ThreadStatic]
		private static byte[] buffer;

		public static byte[] GetBuffer()
		{
			if (buffer == null)
			{
				buffer = new byte[65536];
			}
			return buffer;
		}
	}

	private static IJsonFormatterResolver defaultResolver;

	private static readonly byte[][] indent = (from x in Enumerable.Range(0, 100)
											   select Encoding.UTF8.GetBytes(new string(' ', x * 2))).ToArray();

	private static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

	public static IJsonFormatterResolver DefaultResolver
	{
		get
		{
			if (defaultResolver == null)
			{
				defaultResolver = StandardResolver.Default;
			}
			return defaultResolver;
		}
	}

	public static bool IsInitialized => defaultResolver != null;

	public static void SetDefaultResolver(IJsonFormatterResolver resolver)
	{
		defaultResolver = resolver;
	}

	public static byte[] Serialize<T>(T obj)
	{
		return Serialize(obj, defaultResolver);
	}

	public static byte[] Serialize<T>(T value, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
		return writer.ToUtf8ByteArray();
	}

	public static void Serialize<T>(ref JsonWriter writer, T value)
	{
		Serialize(ref writer, value, defaultResolver);
	}

	public static void Serialize<T>(ref JsonWriter writer, T value, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
	}

	public static void Serialize<T>(Stream stream, T value)
	{
		Serialize(stream, value, defaultResolver);
	}

	public static void Serialize<T>(Stream stream, T value, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		ArraySegment<byte> arraySegment = SerializeUnsafe(value, resolver);
		stream.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
	}

	public static ArraySegment<byte> SerializeUnsafe<T>(T obj)
	{
		return SerializeUnsafe(obj, defaultResolver);
	}

	public static ArraySegment<byte> SerializeUnsafe<T>(T value, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
		return writer.GetBuffer();
	}

	public static string ToJsonString<T>(T value)
	{
		return ToJsonString(value, defaultResolver);
	}

	public static string ToJsonString<T>(T value, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, resolver);
		return writer.ToString();
	}

	public static T Deserialize<T>(string json)
	{
		return Deserialize<T>(json, defaultResolver);
	}

	public static T Deserialize<T>(string json, IJsonFormatterResolver resolver)
	{
		return Deserialize<T>(StringEncoding.UTF8.GetBytes(json), resolver);
	}

	public static T Deserialize<T>(byte[] bytes)
	{
		return Deserialize<T>(bytes, defaultResolver);
	}

	public static T Deserialize<T>(byte[] bytes, IJsonFormatterResolver resolver)
	{
		return Deserialize<T>(bytes, 0, resolver);
	}

	public static T Deserialize<T>(byte[] bytes, int offset)
	{
		return Deserialize<T>(bytes, offset, defaultResolver);
	}

	public static T Deserialize<T>(byte[] bytes, int offset, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		JsonReader reader = new JsonReader(bytes, offset);
		return resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, resolver);
	}

	public static T Deserialize<T>(ref JsonReader reader)
	{
		return Deserialize<T>(ref reader, defaultResolver);
	}

	public static T Deserialize<T>(ref JsonReader reader, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		return resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, resolver);
	}

	public static T Deserialize<T>(Stream stream)
	{
		return Deserialize<T>(stream, defaultResolver);
	}

	public static T Deserialize<T>(Stream stream, IJsonFormatterResolver resolver)
	{
		if (resolver == null)
		{
			resolver = DefaultResolver;
		}
		byte[] buffer = MemoryPool.GetBuffer();
		int newSize = FillFromStream(stream, ref buffer);
		if (new JsonReader(buffer).GetCurrentJsonToken() == JsonToken.Number)
		{
			buffer = BinaryUtil.FastCloneWithResize(buffer, newSize);
		}
		return Deserialize<T>(buffer, resolver);
	}

	public static string PrettyPrint(byte[] json)
	{
		return PrettyPrint(json, 0);
	}

	public static string PrettyPrint(byte[] json, int offset)
	{
		JsonReader reader = new JsonReader(json, offset);
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		WritePrittyPrint(ref reader, ref writer, 0);
		return writer.ToString();
	}

	public static string PrettyPrint(string json)
	{
		JsonReader reader = new JsonReader(Encoding.UTF8.GetBytes(json));
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		WritePrittyPrint(ref reader, ref writer, 0);
		return writer.ToString();
	}

	public static byte[] PrettyPrintByteArray(byte[] json)
	{
		return PrettyPrintByteArray(json, 0);
	}

	public static byte[] PrettyPrintByteArray(byte[] json, int offset)
	{
		JsonReader reader = new JsonReader(json, offset);
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		WritePrittyPrint(ref reader, ref writer, 0);
		return writer.ToUtf8ByteArray();
	}

	public static byte[] PrettyPrintByteArray(string json)
	{
		JsonReader reader = new JsonReader(Encoding.UTF8.GetBytes(json));
		JsonWriter writer = new JsonWriter(MemoryPool.GetBuffer());
		WritePrittyPrint(ref reader, ref writer, 0);
		return writer.ToUtf8ByteArray();
	}

	private static void WritePrittyPrint(ref JsonReader reader, ref JsonWriter writer, int depth)
	{
		switch (reader.GetCurrentJsonToken())
		{
			case JsonToken.EndObject:
			case JsonToken.EndArray:
				break;
			case JsonToken.BeginObject:
				{
					writer.WriteBeginObject();
					writer.WriteRaw(newLine);
					int count2 = 0;
					while (reader.ReadIsInObject(ref count2))
					{
						if (count2 != 1)
						{
							writer.WriteRaw(44);
							writer.WriteRaw(newLine);
						}
						writer.WriteRaw(indent[depth + 1]);
						writer.WritePropertyName(reader.ReadPropertyName());
						writer.WriteRaw(32);
						WritePrittyPrint(ref reader, ref writer, depth + 1);
					}
					writer.WriteRaw(newLine);
					writer.WriteRaw(indent[depth]);
					writer.WriteEndObject();
					break;
				}
			case JsonToken.BeginArray:
				{
					writer.WriteBeginArray();
					writer.WriteRaw(newLine);
					int count = 0;
					while (reader.ReadIsInArray(ref count))
					{
						if (count != 1)
						{
							writer.WriteRaw(44);
							writer.WriteRaw(newLine);
						}
						writer.WriteRaw(indent[depth + 1]);
						WritePrittyPrint(ref reader, ref writer, depth + 1);
					}
					writer.WriteRaw(newLine);
					writer.WriteRaw(indent[depth]);
					writer.WriteEndArray();
					break;
				}
			case JsonToken.Number:
				{
					double value3 = reader.ReadDouble();
					writer.WriteDouble(value3);
					break;
				}
			case JsonToken.String:
				{
					string value2 = reader.ReadString();
					writer.WriteString(value2);
					break;
				}
			case JsonToken.True:
			case JsonToken.False:
				{
					bool value = reader.ReadBoolean();
					writer.WriteBoolean(value);
					break;
				}
			case JsonToken.Null:
				reader.ReadIsNull();
				writer.WriteNull();
				break;
		}
	}

	private static int FillFromStream(Stream input, ref byte[] buffer)
	{
		int num = 0;
		int num2;
		while ((num2 = input.Read(buffer, num, buffer.Length - num)) > 0)
		{
			num += num2;
			if (num == buffer.Length)
			{
				BinaryUtil.FastResize(ref buffer, num * 2);
			}
		}
		return num;
	}
}
