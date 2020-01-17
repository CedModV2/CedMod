// Utf8Json.Resolvers.Internal.DynamicObjectTypeBuilder
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;
using Utf8Json.Resolvers.Internal;

internal static class DynamicObjectTypeBuilder
{
	private struct DeserializeInfo
	{
		public MetaMember MemberInfo;

		public LocalBuilder LocalField;

		public LocalBuilder IsDeserializedField;
	}

	internal static class EmitInfo
	{
		internal static class JsonWriter
		{
			public static readonly MethodInfo GetEncodedPropertyNameWithBeginObject;

			public static readonly MethodInfo GetEncodedPropertyNameWithPrefixValueSeparator;

			public static readonly MethodInfo GetEncodedPropertyNameWithoutQuotation;

			public static readonly MethodInfo GetEncodedPropertyName;

			public static readonly MethodInfo WriteNull;

			public static readonly MethodInfo WriteRaw;

			public static readonly MethodInfo WriteBeginObject;

			public static readonly MethodInfo WriteEndObject;

			public static readonly MethodInfo WriteValueSeparator;

			static JsonWriter()
			{
				GetEncodedPropertyNameWithBeginObject = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithBeginObject(null));
				GetEncodedPropertyNameWithPrefixValueSeparator = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(null));
				GetEncodedPropertyNameWithoutQuotation = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithoutQuotation(null));
				GetEncodedPropertyName = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyName(null));
				WriteNull = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteNull());
				WriteRaw = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteRaw(null));
				WriteBeginObject = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteBeginObject());
				WriteEndObject = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteEndObject());
				WriteValueSeparator = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteValueSeparator());
			}
		}

		internal static class JsonReader
		{
			public static readonly MethodInfo ReadIsNull;

			public static readonly MethodInfo ReadIsBeginObjectWithVerify;

			public static readonly MethodInfo ReadIsEndObjectWithSkipValueSeparator;

			public static readonly MethodInfo ReadPropertyNameSegmentUnsafe;

			public static readonly MethodInfo ReadNextBlock;

			public static readonly MethodInfo GetBufferUnsafe;

			public static readonly MethodInfo GetCurrentOffsetUnsafe;

			static JsonReader()
			{
				ReadIsNull = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadIsNull());
				ReadIsBeginObjectWithVerify = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadIsBeginObjectWithVerify());
				ReadIsEndObjectWithSkipValueSeparator = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader, int count) => reader.ReadIsEndObjectWithSkipValueSeparator(ref count));
				ReadPropertyNameSegmentUnsafe = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadPropertyNameSegmentRaw());
				ReadNextBlock = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadNextBlock());
				GetBufferUnsafe = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.GetBufferUnsafe());
				GetCurrentOffsetUnsafe = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.GetCurrentOffsetUnsafe());
			}
		}

		internal static class JsonFormatterAttr
		{
			internal static readonly MethodInfo FormatterType = ExpressionUtility.GetPropertyInfo((JsonFormatterAttribute attr) => attr.FormatterType).GetGetMethod();

			internal static readonly MethodInfo Arguments = ExpressionUtility.GetPropertyInfo((JsonFormatterAttribute attr) => attr.Arguments).GetGetMethod();
		}

		public static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.First((ConstructorInfo x) => x.GetParameters().Length == 0);

		public static readonly MethodInfo GetFormatterWithVerify = typeof(JsonFormatterResolverExtensions).GetRuntimeMethod("GetFormatterWithVerify", new Type[1]
		{
			typeof(IJsonFormatterResolver)
		});

		public static readonly ConstructorInfo InvalidOperationExceptionConstructor = typeof(InvalidOperationException).GetTypeInfo().DeclaredConstructors.First(delegate (ConstructorInfo x)
		{
			ParameterInfo[] parameters = x.GetParameters();
			return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
		});

		public static readonly MethodInfo GetTypeFromHandle = ExpressionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(default(RuntimeTypeHandle)));

		public static readonly MethodInfo TypeGetProperty = ExpressionUtility.GetMethodInfo((Type t) => t.GetProperty(null, BindingFlags.Default));

		public static readonly MethodInfo TypeGetField = ExpressionUtility.GetMethodInfo((Type t) => t.GetField(null, BindingFlags.Default));

		public static readonly MethodInfo GetCustomAttributeJsonFormatterAttribute = ExpressionUtility.GetMethodInfo(() => ((MemberInfo)null).GetCustomAttribute<JsonFormatterAttribute>(inherit: false));

		public static readonly MethodInfo ActivatorCreateInstance = ExpressionUtility.GetMethodInfo(() => Activator.CreateInstance((Type)null, (object[])null));

		public static readonly MethodInfo GetUninitializedObject = ExpressionUtility.GetMethodInfo(() => FormatterServices.GetUninitializedObject(null));

		public static readonly MethodInfo GetTypeMethod = ExpressionUtility.GetMethodInfo((object o) => o.GetType());

		public static readonly MethodInfo TypeEquals = ExpressionUtility.GetMethodInfo((Type t) => t.Equals(null));

		public static readonly MethodInfo NongenericSerialize = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => JsonSerializer.NonGeneric.Serialize(null, ref writer, null, null));

		public static MethodInfo Serialize(Type type)
		{
			return typeof(IJsonFormatter<>).MakeGenericType(type).GetRuntimeMethod("Serialize", new Type[3]
			{
				typeof(Utf8Json.JsonWriter).MakeByRefType(),
				type,
				typeof(IJsonFormatterResolver)
			});
		}

		public static MethodInfo Deserialize(Type type)
		{
			return typeof(IJsonFormatter<>).MakeGenericType(type).GetRuntimeMethod("Deserialize", new Type[2]
			{
				typeof(Utf8Json.JsonReader).MakeByRefType(),
				typeof(IJsonFormatterResolver)
			});
		}

		public static MethodInfo GetNullableHasValue(Type type)
		{
			return typeof(Nullable<>).MakeGenericType(type).GetRuntimeProperty("HasValue").GetGetMethod();
		}
	}

	internal class Utf8JsonDynamicObjectResolverException : Exception
	{
		public Utf8JsonDynamicObjectResolverException(string message)
			: base(message)
		{
		}
	}

	private static readonly Regex SubtractFullNameRegex = new Regex(", Version=\\d+.\\d+.\\d+.\\d+, Culture=\\w+, PublicKeyToken=\\w+");

	private static int nameSequence = 0;

	private static HashSet<Type> ignoreTypes = new HashSet<Type>
	{
		typeof(object),
		typeof(short),
		typeof(int),
		typeof(long),
		typeof(ushort),
		typeof(uint),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(bool),
		typeof(byte),
		typeof(sbyte),
		typeof(decimal),
		typeof(char),
		typeof(string),
		typeof(Guid),
		typeof(TimeSpan),
		typeof(DateTime),
		typeof(DateTimeOffset)
	};

	private static HashSet<Type> jsonPrimitiveTypes = new HashSet<Type>
	{
		typeof(short),
		typeof(int),
		typeof(long),
		typeof(ushort),
		typeof(uint),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(bool),
		typeof(byte),
		typeof(sbyte),
		typeof(string)
	};

	public static object BuildFormatterToAssembly<T>(DynamicAssembly assembly, IJsonFormatterResolver selfResolver, Func<string, string> nameMutator, bool excludeNull)
	{
		TypeInfo typeInfo = typeof(T).GetTypeInfo();
		if (typeInfo.IsNullable())
		{
			typeInfo = typeInfo.GenericTypeArguments[0].GetTypeInfo();
			object formatterDynamic = selfResolver.GetFormatterDynamic(typeInfo.AsType());
			if (formatterDynamic == null)
			{
				return null;
			}
			return (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(typeInfo.AsType()), formatterDynamic);
		}
		if (typeof(Exception).GetTypeInfo().IsAssignableFrom(typeInfo))
		{
			return BuildAnonymousFormatter(typeof(T), nameMutator, excludeNull, allowPrivate: false, isException: true);
		}
		if (typeInfo.IsAnonymous() || TryGetInterfaceEnumerableElementType(typeof(T), out Type _))
		{
			return BuildAnonymousFormatter(typeof(T), nameMutator, excludeNull, allowPrivate: false, isException: false);
		}
		TypeInfo typeInfo2 = BuildType(assembly, typeof(T), nameMutator, excludeNull);
		if (typeInfo2 == null)
		{
			return null;
		}
		return (IJsonFormatter<T>)Activator.CreateInstance(typeInfo2.AsType());
	}

	public static object BuildFormatterToDynamicMethod<T>(IJsonFormatterResolver selfResolver, Func<string, string> nameMutator, bool excludeNull, bool allowPrivate)
	{
		TypeInfo typeInfo = typeof(T).GetTypeInfo();
		if (typeInfo.IsNullable())
		{
			typeInfo = typeInfo.GenericTypeArguments[0].GetTypeInfo();
			object formatterDynamic = selfResolver.GetFormatterDynamic(typeInfo.AsType());
			if (formatterDynamic == null)
			{
				return null;
			}
			return (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(typeInfo.AsType()), formatterDynamic);
		}
		if (typeof(Exception).GetTypeInfo().IsAssignableFrom(typeInfo))
		{
			return BuildAnonymousFormatter(typeof(T), nameMutator, excludeNull, allowPrivate: false, isException: true);
		}
		return BuildAnonymousFormatter(typeof(T), nameMutator, excludeNull, allowPrivate, isException: false);
	}

	private static TypeInfo BuildType(DynamicAssembly assembly, Type type, Func<string, string> nameMutator, bool excludeNull)
	{
		if (ignoreTypes.Contains(type))
		{
			return null;
		}
		MetaType metaType = new MetaType(type, nameMutator, allowPrivate: false);
		bool hasShouldSerialize = metaType.Members.Any((MetaMember x) => x.ShouldSerializeMethodInfo != null);
		Type type2 = typeof(IJsonFormatter<>).MakeGenericType(type);
		TypeBuilder typeBuilder = assembly.DefineType("Utf8Json.Formatters." + SubtractFullNameRegex.Replace(type.FullName, "").Replace(".", "_") + "Formatter" + Interlocked.Increment(ref nameSequence), TypeAttributes.Public | TypeAttributes.Sealed, null, new Type[1]
		{
			type2
		});
		ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
		FieldBuilder stringByteKeysField = typeBuilder.DefineField("stringByteKeys", typeof(byte[][]), FieldAttributes.Private | FieldAttributes.InitOnly);
		ILGenerator iLGenerator = constructorBuilder.GetILGenerator();
		Dictionary<MetaMember, FieldInfo> customFormatterLookup = BuildConstructor(typeBuilder, metaType, constructorBuilder, stringByteKeysField, iLGenerator, excludeNull, hasShouldSerialize);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("Serialize", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual, null, new Type[3]
		{
			typeof(JsonWriter).MakeByRefType(),
			type,
			typeof(IJsonFormatterResolver)
		});
		ILGenerator il2 = methodBuilder.GetILGenerator();
		BuildSerialize(type, metaType, il2, delegate
		{
			il2.EmitLoadThis();
			il2.EmitLdfld(stringByteKeysField);
		}, delegate (int index, MetaMember member)
		{
			if (!customFormatterLookup.TryGetValue(member, out FieldInfo value2))
			{
				return false;
			}
			il2.EmitLoadThis();
			il2.EmitLdfld(value2);
			return true;
		}, excludeNull, hasShouldSerialize, 1);
		MethodBuilder methodBuilder2 = typeBuilder.DefineMethod("Deserialize", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual, type, new Type[2]
		{
			typeof(JsonReader).MakeByRefType(),
			typeof(IJsonFormatterResolver)
		});
		ILGenerator il = methodBuilder2.GetILGenerator();
		BuildDeserialize(type, metaType, il, delegate (int index, MetaMember member)
		{
			if (!customFormatterLookup.TryGetValue(member, out FieldInfo value))
			{
				return false;
			}
			il.EmitLoadThis();
			il.EmitLdfld(value);
			return true;
		}, useGetUninitializedObject: false, 1);
		return typeBuilder.CreateTypeInfo();
	}

	public static object BuildAnonymousFormatter(Type type, Func<string, string> nameMutator, bool excludeNull, bool allowPrivate, bool isException)
	{
		if (ignoreTypes.Contains(type))
		{
			return false;
		}
		MetaType metaType;
		if (isException)
		{
			HashSet<string> ignoreSet = new HashSet<string>(new string[6]
			{
				"HelpLink",
				"TargetSite",
				"HResult",
				"Data",
				"ClassName",
				"InnerException"
			}.Select((string x) => nameMutator(x)));
			metaType = new MetaType(type, nameMutator, allowPrivate: false);
			metaType.BestmatchConstructor = null;
			metaType.ConstructorParameters = new MetaMember[0];
			metaType.Members = new StringConstantValueMetaMember[1]
			{
				new StringConstantValueMetaMember(nameMutator("ClassName"), type.FullName)
			}.Concat(metaType.Members.Where((MetaMember x) => !ignoreSet.Contains(x.Name))).Concat(new InnerExceptionMetaMember[1]
			{
				new InnerExceptionMetaMember(nameMutator("InnerException"))
			}).ToArray();
		}
		else
		{
			metaType = new MetaType(type, nameMutator, allowPrivate);
		}
		bool flag = metaType.Members.Any((MetaMember x) => x.ShouldSerializeMethodInfo != null);
		List<byte[]> list = new List<byte[]>();
		int num = 0;
		foreach (MetaMember item3 in metaType.Members.Where((MetaMember x) => x.IsReadable))
		{
			if (excludeNull | flag)
			{
				list.Add(JsonWriter.GetEncodedPropertyName(item3.Name));
			}
			else if (num == 0)
			{
				list.Add(JsonWriter.GetEncodedPropertyNameWithBeginObject(item3.Name));
			}
			else
			{
				list.Add(JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(item3.Name));
			}
			num++;
		}
		List<object> serializeCustomFormatters = new List<object>();
		List<object> deserializeCustomFormatters = new List<object>();
		foreach (MetaMember item4 in metaType.Members.Where((MetaMember x) => x.IsReadable))
		{
			JsonFormatterAttribute customAttribute = item4.GetCustomAttribute<JsonFormatterAttribute>(inherit: true);
			if (customAttribute != null)
			{
				object item = Activator.CreateInstance(customAttribute.FormatterType, customAttribute.Arguments);
				serializeCustomFormatters.Add(item);
			}
			else
			{
				serializeCustomFormatters.Add(null);
			}
		}
		MetaMember[] members = metaType.Members;
		for (int i = 0; i < members.Length; i++)
		{
			JsonFormatterAttribute customAttribute2 = members[i].GetCustomAttribute<JsonFormatterAttribute>(inherit: true);
			if (customAttribute2 != null)
			{
				object item2 = Activator.CreateInstance(customAttribute2.FormatterType, customAttribute2.Arguments);
				deserializeCustomFormatters.Add(item2);
			}
			else
			{
				deserializeCustomFormatters.Add(null);
			}
		}
		DynamicMethod dynamicMethod = new DynamicMethod("Serialize", null, new Type[5]
		{
			typeof(byte[][]),
			typeof(object[]),
			typeof(JsonWriter).MakeByRefType(),
			type,
			typeof(IJsonFormatterResolver)
		}, type.Module, skipVisibility: true);
		ILGenerator il2 = dynamicMethod.GetILGenerator();
		BuildSerialize(type, metaType, il2, delegate
		{
			il2.EmitLdarg(0);
		}, delegate (int index, MetaMember member)
		{
			if (serializeCustomFormatters.Count == 0)
			{
				return false;
			}
			if (serializeCustomFormatters[index] == null)
			{
				return false;
			}
			il2.EmitLdarg(1);
			il2.EmitLdc_I4(index);
			il2.Emit(OpCodes.Ldelem_Ref);
			il2.Emit(OpCodes.Castclass, serializeCustomFormatters[index].GetType());
			return true;
		}, excludeNull, flag, 2);
		DynamicMethod dynamicMethod2 = new DynamicMethod("Deserialize", type, new Type[3]
		{
			typeof(object[]),
			typeof(JsonReader).MakeByRefType(),
			typeof(IJsonFormatterResolver)
		}, type.Module, skipVisibility: true);
		ILGenerator il = dynamicMethod2.GetILGenerator();
		BuildDeserialize(type, metaType, il, delegate (int index, MetaMember member)
		{
			if (deserializeCustomFormatters.Count == 0)
			{
				return false;
			}
			if (deserializeCustomFormatters[index] == null)
			{
				return false;
			}
			il.EmitLdarg(0);
			il.EmitLdc_I4(index);
			il.Emit(OpCodes.Ldelem_Ref);
			il.Emit(OpCodes.Castclass, deserializeCustomFormatters[index].GetType());
			return true;
		}, useGetUninitializedObject: true, 1);
		object obj = dynamicMethod.CreateDelegate(typeof(AnonymousJsonSerializeAction<>).MakeGenericType(type));
		object obj2 = dynamicMethod2.CreateDelegate(typeof(AnonymousJsonDeserializeFunc<>).MakeGenericType(type));
		return Activator.CreateInstance(typeof(DynamicMethodAnonymousFormatter<>).MakeGenericType(type), list.ToArray(), serializeCustomFormatters.ToArray(), deserializeCustomFormatters.ToArray(), obj, obj2);
	}

	private static Dictionary<MetaMember, FieldInfo> BuildConstructor(TypeBuilder builder, MetaType info, ConstructorInfo method, FieldBuilder stringByteKeysField, ILGenerator il, bool excludeNull, bool hasShouldSerialize)
	{
		il.EmitLdarg(0);
		il.Emit(OpCodes.Call, EmitInfo.ObjectCtor);
		int value = info.Members.Count((MetaMember x) => x.IsReadable);
		il.EmitLdarg(0);
		il.EmitLdc_I4(value);
		il.Emit(OpCodes.Newarr, typeof(byte[]));
		int num = 0;
		foreach (MetaMember item in info.Members.Where((MetaMember x) => x.IsReadable))
		{
			il.Emit(OpCodes.Dup);
			il.EmitLdc_I4(num);
			il.Emit(OpCodes.Ldstr, item.Name);
			if (excludeNull | hasShouldSerialize)
			{
				il.EmitCall(EmitInfo.JsonWriter.GetEncodedPropertyName);
			}
			else if (num == 0)
			{
				il.EmitCall(EmitInfo.JsonWriter.GetEncodedPropertyNameWithBeginObject);
			}
			else
			{
				il.EmitCall(EmitInfo.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator);
			}
			il.Emit(OpCodes.Stelem_Ref);
			num++;
		}
		il.Emit(OpCodes.Stfld, stringByteKeysField);
		Dictionary<MetaMember, FieldInfo> result = BuildCustomFormatterField(builder, info, il);
		il.Emit(OpCodes.Ret);
		return result;
	}

	private static Dictionary<MetaMember, FieldInfo> BuildCustomFormatterField(TypeBuilder builder, MetaType info, ILGenerator il)
	{
		Dictionary<MetaMember, FieldInfo> dictionary = new Dictionary<MetaMember, FieldInfo>();
		foreach (MetaMember item in info.Members.Where((MetaMember x) => x.IsReadable || x.IsWritable))
		{
			JsonFormatterAttribute customAttribute = item.GetCustomAttribute<JsonFormatterAttribute>(inherit: true);
			if (customAttribute != null)
			{
				FieldBuilder fieldBuilder = builder.DefineField(item.Name + "_formatter", customAttribute.FormatterType, FieldAttributes.Private | FieldAttributes.InitOnly);
				int value = 52;
				LocalBuilder local = il.DeclareLocal(typeof(JsonFormatterAttribute));
				il.Emit(OpCodes.Ldtoken, info.Type);
				il.EmitCall(EmitInfo.GetTypeFromHandle);
				il.Emit(OpCodes.Ldstr, item.MemberName);
				il.EmitLdc_I4(value);
				if (item.IsProperty)
				{
					il.EmitCall(EmitInfo.TypeGetProperty);
				}
				else
				{
					il.EmitCall(EmitInfo.TypeGetField);
				}
				il.EmitTrue();
				il.EmitCall(EmitInfo.GetCustomAttributeJsonFormatterAttribute);
				il.EmitStloc(local);
				il.EmitLoadThis();
				il.EmitLdloc(local);
				il.EmitCall(EmitInfo.JsonFormatterAttr.FormatterType);
				il.EmitLdloc(local);
				il.EmitCall(EmitInfo.JsonFormatterAttr.Arguments);
				il.EmitCall(EmitInfo.ActivatorCreateInstance);
				il.Emit(OpCodes.Castclass, customAttribute.FormatterType);
				il.Emit(OpCodes.Stfld, fieldBuilder);
				dictionary.Add(item, fieldBuilder);
			}
		}
		return dictionary;
	}

	private static void BuildSerialize(Type type, MetaType info, ILGenerator il, Action emitStringByteKeys, Func<int, MetaMember, bool> tryEmitLoadCustomFormatter, bool excludeNull, bool hasShouldSerialize, int firstArgIndex)
	{
		ArgumentField argumentField = new ArgumentField(il, firstArgIndex);
		ArgumentField argValue = new ArgumentField(il, firstArgIndex + 1, type);
		ArgumentField argResolver = new ArgumentField(il, firstArgIndex + 2);
		TypeInfo typeInfo = type.GetTypeInfo();
		InnerExceptionMetaMember innerExceptionMetaMember = info.Members.OfType<InnerExceptionMetaMember>().FirstOrDefault();
		if (innerExceptionMetaMember != null)
		{
			innerExceptionMetaMember.argWriter = argumentField;
			innerExceptionMetaMember.argValue = argValue;
			innerExceptionMetaMember.argResolver = argResolver;
		}
		if (info.IsClass && info.BestmatchConstructor == null && TryGetInterfaceEnumerableElementType(type, out Type elementType))
		{
			Type type2 = typeof(IEnumerable<>).MakeGenericType(elementType);
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.GetFormatterWithVerify.MakeGenericMethod(type2));
			argumentField.EmitLoad();
			argValue.EmitLoad();
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.Serialize(type2));
			il.Emit(OpCodes.Ret);
			return;
		}
		if (info.IsClass)
		{
			Label label = il.DefineLabel();
			argValue.EmitLoad();
			il.Emit(OpCodes.Brtrue_S, label);
			argumentField.EmitLoad();
			il.EmitCall(EmitInfo.JsonWriter.WriteNull);
			il.Emit(OpCodes.Ret);
			il.MarkLabel(label);
		}
		if (type == typeof(Exception))
		{
			Label label2 = il.DefineLabel();
			LocalBuilder local = il.DeclareLocal(typeof(Type));
			argValue.EmitLoad();
			il.EmitCall(EmitInfo.GetTypeMethod);
			il.EmitStloc(local);
			il.EmitLdloc(local);
			il.Emit(OpCodes.Ldtoken, typeof(Exception));
			il.EmitCall(EmitInfo.GetTypeFromHandle);
			il.EmitCall(EmitInfo.TypeEquals);
			il.Emit(OpCodes.Brtrue, label2);
			il.EmitLdloc(local);
			argumentField.EmitLoad();
			argValue.EmitLoad();
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.NongenericSerialize);
			il.Emit(OpCodes.Ret);
			il.MarkLabel(label2);
		}
		LocalBuilder local2 = null;
		Label label3 = il.DefineLabel();
		Label[] array = null;
		if (excludeNull | hasShouldSerialize)
		{
			local2 = il.DeclareLocal(typeof(bool));
			argumentField.EmitLoad();
			il.EmitCall(EmitInfo.JsonWriter.WriteBeginObject);
			array = (from x in info.Members
					 where x.IsReadable
					 select x into _
					 select il.DefineLabel()).ToArray();
		}
		int num = 0;
		foreach (MetaMember item in info.Members.Where((MetaMember x) => x.IsReadable))
		{
			if (excludeNull | hasShouldSerialize)
			{
				il.MarkLabel(array[num]);
				if (excludeNull)
				{
					if (item.Type.GetTypeInfo().IsNullable())
					{
						LocalBuilder local3 = il.DeclareLocal(item.Type);
						argValue.EmitLoad();
						item.EmitLoadValue(il);
						il.EmitStloc(local3);
						il.EmitLdloca(local3);
						il.EmitCall(EmitInfo.GetNullableHasValue(item.Type.GetGenericArguments()[0]));
						il.Emit(OpCodes.Brfalse_S, (num < array.Length - 1) ? array[num + 1] : label3);
					}
					else if (!item.Type.IsValueType && !(item is StringConstantValueMetaMember))
					{
						argValue.EmitLoad();
						item.EmitLoadValue(il);
						il.Emit(OpCodes.Brfalse_S, (num < array.Length - 1) ? array[num + 1] : label3);
					}
				}
				if (hasShouldSerialize && item.ShouldSerializeMethodInfo != null)
				{
					argValue.EmitLoad();
					il.EmitCall(item.ShouldSerializeMethodInfo);
					il.Emit(OpCodes.Brfalse_S, (num < array.Length - 1) ? array[num + 1] : label3);
				}
				Label label4 = il.DefineLabel();
				Label label5 = il.DefineLabel();
				il.EmitLdloc(local2);
				il.Emit(OpCodes.Brtrue_S, label5);
				il.EmitTrue();
				il.EmitStloc(local2);
				il.Emit(OpCodes.Br, label4);
				il.MarkLabel(label5);
				argumentField.EmitLoad();
				il.EmitCall(EmitInfo.JsonWriter.WriteValueSeparator);
				il.MarkLabel(label4);
			}
			argumentField.EmitLoad();
			emitStringByteKeys();
			il.EmitLdc_I4(num);
			il.Emit(OpCodes.Ldelem_Ref);
			il.EmitCall(EmitInfo.JsonWriter.WriteRaw);
			EmitSerializeValue(typeInfo, item, il, num, tryEmitLoadCustomFormatter, argumentField, argValue, argResolver);
			num++;
		}
		il.MarkLabel(label3);
		if (!excludeNull && num == 0)
		{
			argumentField.EmitLoad();
			il.EmitCall(EmitInfo.JsonWriter.WriteBeginObject);
		}
		argumentField.EmitLoad();
		il.EmitCall(EmitInfo.JsonWriter.WriteEndObject);
		il.Emit(OpCodes.Ret);
	}

	private static void EmitSerializeValue(TypeInfo type, MetaMember member, ILGenerator il, int index, Func<int, MetaMember, bool> tryEmitLoadCustomFormatter, ArgumentField writer, ArgumentField argValue, ArgumentField argResolver)
	{
		Type type2 = member.Type;
		if (member is InnerExceptionMetaMember)
		{
			(member as InnerExceptionMetaMember).EmitSerializeDirectly(il);
		}
		else if (tryEmitLoadCustomFormatter(index, member))
		{
			writer.EmitLoad();
			argValue.EmitLoad();
			member.EmitLoadValue(il);
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.Serialize(type2));
		}
		else if (jsonPrimitiveTypes.Contains(type2))
		{
			writer.EmitLoad();
			argValue.EmitLoad();
			member.EmitLoadValue(il);
			il.EmitCall((from x in typeof(JsonWriter).GetTypeInfo().GetDeclaredMethods("Write" + type2.Name)
						 orderby x.GetParameters().Length descending
						 select x).First());
		}
		else
		{
			argResolver.EmitLoad();
			il.Emit(OpCodes.Call, EmitInfo.GetFormatterWithVerify.MakeGenericMethod(type2));
			writer.EmitLoad();
			argValue.EmitLoad();
			member.EmitLoadValue(il);
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.Serialize(type2));
		}
	}

	private unsafe static void BuildDeserialize(Type type, MetaType info, ILGenerator il, Func<int, MetaMember, bool> tryEmitLoadCustomFormatter, bool useGetUninitializedObject, int firstArgIndex)
	{
		if (info.IsClass && info.BestmatchConstructor == null && (!useGetUninitializedObject || !info.IsConcreteClass))
		{
			il.Emit(OpCodes.Ldstr, "generated serializer for " + type.Name + " does not support deserialize.");
			il.Emit(OpCodes.Newobj, EmitInfo.InvalidOperationExceptionConstructor);
			il.Emit(OpCodes.Throw);
			return;
		}
		ArgumentField argReader = new ArgumentField(il, firstArgIndex);
		ArgumentField argResolver = new ArgumentField(il, firstArgIndex + 1);
		Label label = il.DefineLabel();
		argReader.EmitLoad();
		il.EmitCall(EmitInfo.JsonReader.ReadIsNull);
		il.Emit(OpCodes.Brfalse_S, label);
		if (info.IsClass)
		{
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ret);
		}
		else
		{
			il.Emit(OpCodes.Ldstr, "json value is null, struct is not supported");
			il.Emit(OpCodes.Newobj, EmitInfo.InvalidOperationExceptionConstructor);
			il.Emit(OpCodes.Throw);
		}
		il.MarkLabel(label);
		argReader.EmitLoad();
		il.EmitCall(EmitInfo.JsonReader.ReadIsBeginObjectWithVerify);
		bool isSideEffectFreeType = true;
		if (info.BestmatchConstructor != null)
		{
			isSideEffectFreeType = IsSideEffectFreeConstructorType(info.BestmatchConstructor);
			if (info.Members.Any((MetaMember x) => !x.IsReadable && x.IsWritable))
			{
				isSideEffectFreeType = false;
			}
		}
		DeserializeInfo[] infoList = info.Members.Select(delegate (MetaMember item)
		{
			DeserializeInfo result = default(DeserializeInfo);
			result.MemberInfo = item;
			result.LocalField = il.DeclareLocal(item.Type);
			result.IsDeserializedField = (isSideEffectFreeType ? null : il.DeclareLocal(typeof(bool)));
			return result;
		}).ToArray();
		LocalBuilder local = il.DeclareLocal(typeof(int));
		AutomataDictionary automataDictionary = new AutomataDictionary();
		for (int i = 0; i < info.Members.Length; i++)
		{
			automataDictionary.Add(JsonWriter.GetEncodedPropertyNameWithoutQuotation(info.Members[i].Name), i);
		}
		LocalBuilder local2 = il.DeclareLocal(typeof(byte[]));
		LocalBuilder local3 = il.DeclareLocal(typeof(byte).MakeByRefType(), pinned: true);
		LocalBuilder local4 = il.DeclareLocal(typeof(ArraySegment<byte>));
		LocalBuilder key = il.DeclareLocal(typeof(ulong));
		LocalBuilder localBuilder = il.DeclareLocal(typeof(byte*));
		LocalBuilder localBuilder2 = il.DeclareLocal(typeof(int));
		argReader.EmitLoad();
		il.EmitCall(EmitInfo.JsonReader.GetBufferUnsafe);
		il.EmitStloc(local2);
		il.EmitLdloc(local2);
		il.EmitLdc_I4(0);
		il.Emit(OpCodes.Ldelema, typeof(byte));
		il.EmitStloc(local3);
		Label continueWhile = il.DefineLabel();
		Label label2 = il.DefineLabel();
		Label readNext = il.DefineLabel();
		il.MarkLabel(continueWhile);
		argReader.EmitLoad();
		il.EmitLdloca(local);
		il.EmitCall(EmitInfo.JsonReader.ReadIsEndObjectWithSkipValueSeparator);
		il.Emit(OpCodes.Brtrue, label2);
		argReader.EmitLoad();
		il.EmitCall(EmitInfo.JsonReader.ReadPropertyNameSegmentUnsafe);
		il.EmitStloc(local4);
		il.EmitLdloc(local3);
		il.Emit(OpCodes.Conv_I);
		il.EmitLdloca(local4);
		il.EmitCall(typeof(ArraySegment<byte>).GetRuntimeProperty("Offset").GetGetMethod());
		il.Emit(OpCodes.Add);
		il.EmitStloc(localBuilder);
		il.EmitLdloca(local4);
		il.EmitCall(typeof(ArraySegment<byte>).GetRuntimeProperty("Count").GetGetMethod());
		il.EmitStloc(localBuilder2);
		il.EmitLdloc(localBuilder2);
		il.Emit(OpCodes.Brfalse, readNext);
		automataDictionary.EmitMatch(il, localBuilder, localBuilder2, key, delegate (KeyValuePair<string, int> x)
		{
			int value = x.Value;
			if (infoList[value].MemberInfo != null)
			{
				EmitDeserializeValue(il, infoList[value], value, tryEmitLoadCustomFormatter, argReader, argResolver);
				if (!isSideEffectFreeType)
				{
					il.EmitTrue();
					il.EmitStloc(infoList[value].IsDeserializedField);
				}
				il.Emit(OpCodes.Br, continueWhile);
			}
			else
			{
				il.Emit(OpCodes.Br, readNext);
			}
		}, delegate
		{
			il.Emit(OpCodes.Br, readNext);
		});
		il.MarkLabel(readNext);
		argReader.EmitLoad();
		il.EmitCall(EmitInfo.JsonReader.ReadNextBlock);
		il.Emit(OpCodes.Br, continueWhile);
		il.MarkLabel(label2);
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Conv_U);
		il.EmitStloc(local3);
		LocalBuilder localBuilder3 = EmitNewObject(il, type, info, infoList, isSideEffectFreeType);
		if (localBuilder3 != null)
		{
			il.Emit(OpCodes.Ldloc, localBuilder3);
		}
		il.Emit(OpCodes.Ret);
	}

	private static void EmitDeserializeValue(ILGenerator il, DeserializeInfo info, int index, Func<int, MetaMember, bool> tryEmitLoadCustomFormatter, ArgumentField reader, ArgumentField argResolver)
	{
		MetaMember memberInfo = info.MemberInfo;
		Type type = memberInfo.Type;
		if (tryEmitLoadCustomFormatter(index, memberInfo))
		{
			reader.EmitLoad();
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.Deserialize(type));
		}
		else if (jsonPrimitiveTypes.Contains(type))
		{
			reader.EmitLoad();
			il.EmitCall((from x in typeof(JsonReader).GetTypeInfo().GetDeclaredMethods("Read" + type.Name)
						 orderby x.GetParameters().Length descending
						 select x).First());
		}
		else
		{
			argResolver.EmitLoad();
			il.Emit(OpCodes.Call, EmitInfo.GetFormatterWithVerify.MakeGenericMethod(type));
			reader.EmitLoad();
			argResolver.EmitLoad();
			il.EmitCall(EmitInfo.Deserialize(type));
		}
		il.EmitStloc(info.LocalField);
	}

	private static LocalBuilder EmitNewObject(ILGenerator il, Type type, MetaType info, DeserializeInfo[] members, bool isSideEffectFreeType)
	{
		if (info.IsClass)
		{
			LocalBuilder localBuilder = null;
			if (!isSideEffectFreeType)
			{
				localBuilder = il.DeclareLocal(type);
			}
			if (info.BestmatchConstructor != null)
			{
				MetaMember[] constructorParameters = info.ConstructorParameters;
				MetaMember[] array = constructorParameters;
				foreach (MetaMember item2 in array)
				{
					DeserializeInfo deserializeInfo = members.First((DeserializeInfo x) => x.MemberInfo == item2);
					il.EmitLdloc(deserializeInfo.LocalField);
				}
				il.Emit(OpCodes.Newobj, info.BestmatchConstructor);
			}
			else
			{
				il.Emit(OpCodes.Ldtoken, type);
				il.EmitCall(EmitInfo.GetTypeFromHandle);
				il.EmitCall(EmitInfo.GetUninitializedObject);
			}
			if (!isSideEffectFreeType)
			{
				il.EmitStloc(localBuilder);
			}
			foreach (DeserializeInfo item3 in members.Where((DeserializeInfo x) => x.MemberInfo != null && x.MemberInfo.IsWritable))
			{
				if (isSideEffectFreeType)
				{
					il.Emit(OpCodes.Dup);
					il.EmitLdloc(item3.LocalField);
					item3.MemberInfo.EmitStoreValue(il);
				}
				else
				{
					Label label = il.DefineLabel();
					il.EmitLdloc(item3.IsDeserializedField);
					il.Emit(OpCodes.Brfalse, label);
					il.EmitLdloc(localBuilder);
					il.EmitLdloc(item3.LocalField);
					item3.MemberInfo.EmitStoreValue(il);
					il.MarkLabel(label);
				}
			}
			return localBuilder;
		}
		LocalBuilder localBuilder2 = il.DeclareLocal(type);
		if (info.BestmatchConstructor == null)
		{
			il.Emit(OpCodes.Ldloca, localBuilder2);
			il.Emit(OpCodes.Initobj, type);
		}
		else
		{
			MetaMember[] constructorParameters2 = info.ConstructorParameters;
			MetaMember[] array2 = constructorParameters2;
			foreach (MetaMember item in array2)
			{
				DeserializeInfo deserializeInfo2 = members.First((DeserializeInfo x) => x.MemberInfo == item);
				il.EmitLdloc(deserializeInfo2.LocalField);
			}
			il.Emit(OpCodes.Newobj, info.BestmatchConstructor);
			il.Emit(OpCodes.Stloc, localBuilder2);
		}
		foreach (DeserializeInfo item4 in members.Where((DeserializeInfo x) => x.MemberInfo != null && x.MemberInfo.IsWritable))
		{
			if (isSideEffectFreeType)
			{
				il.EmitLdloca(localBuilder2);
				il.EmitLdloc(item4.LocalField);
				item4.MemberInfo.EmitStoreValue(il);
			}
			else
			{
				Label label2 = il.DefineLabel();
				il.EmitLdloc(item4.IsDeserializedField);
				il.Emit(OpCodes.Brfalse, label2);
				il.EmitLdloca(localBuilder2);
				il.EmitLdloc(item4.LocalField);
				item4.MemberInfo.EmitStoreValue(il);
				il.MarkLabel(label2);
			}
		}
		return localBuilder2;
	}

	private static bool IsSideEffectFreeConstructorType(ConstructorInfo ctorInfo)
	{
		MethodBody methodBody = ctorInfo.GetMethodBody();
		if (methodBody == null)
		{
			return false;
		}
		byte[] iLAsByteArray = methodBody.GetILAsByteArray();
		if (iLAsByteArray == null)
		{
			return false;
		}
		List<OpCode> list = new List<OpCode>();
		using (ILStreamReader iLStreamReader = new ILStreamReader(iLAsByteArray))
		{
			while (!iLStreamReader.EndOfStream)
			{
				OpCode opCode = iLStreamReader.ReadOpCode();
				if (opCode != OpCodes.Nop && opCode != OpCodes.Ldloc_0 && opCode != OpCodes.Ldloc_S && opCode != OpCodes.Stloc_0 && opCode != OpCodes.Stloc_S && opCode != OpCodes.Blt && opCode != OpCodes.Blt_S && opCode != OpCodes.Bgt && opCode != OpCodes.Bgt_S)
				{
					list.Add(opCode);
					if (list.Count == 4)
					{
						break;
					}
				}
			}
		}
		if (list.Count == 3 && list[0] == OpCodes.Ldarg_0 && list[1] == OpCodes.Call && list[2] == OpCodes.Ret)
		{
			if (ctorInfo.DeclaringType.BaseType == typeof(object))
			{
				return true;
			}
			ConstructorInfo constructor = ctorInfo.DeclaringType.BaseType.GetConstructor(Type.EmptyTypes);
			if (constructor == null)
			{
				return false;
			}
			return IsSideEffectFreeConstructorType(constructor);
		}
		return false;
	}

	private static bool TryGetInterfaceEnumerableElementType(Type type, out Type elementType)
	{
		Type[] interfaces = type.GetInterfaces();
		Type[] array = interfaces;
		foreach (Type type2 in array)
		{
			if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				Type[] genericArguments = type2.GetGenericArguments();
				elementType = genericArguments[0];
				return true;
			}
		}
		elementType = null;
		return false;
	}
}
