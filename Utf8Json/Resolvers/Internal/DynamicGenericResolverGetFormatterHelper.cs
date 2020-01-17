// Utf8Json.Resolvers.Internal.DynamicGenericResolverGetFormatterHelper
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Utf8Json.Formatters;
using Utf8Json.Internal;

internal static class DynamicGenericResolverGetFormatterHelper
{
	private static readonly Dictionary<Type, Type> formatterMap = new Dictionary<Type, Type>
	{
		{
			typeof(List<>),
			typeof(ListFormatter<>)
		},
		{
			typeof(LinkedList<>),
			typeof(LinkedListFormatter<>)
		},
		{
			typeof(Queue<>),
			typeof(QeueueFormatter<>)
		},
		{
			typeof(Stack<>),
			typeof(StackFormatter<>)
		},
		{
			typeof(HashSet<>),
			typeof(HashSetFormatter<>)
		},
		{
			typeof(ReadOnlyCollection<>),
			typeof(ReadOnlyCollectionFormatter<>)
		},
		{
			typeof(IList<>),
			typeof(InterfaceListFormatter<>)
		},
		{
			typeof(ICollection<>),
			typeof(InterfaceCollectionFormatter<>)
		},
		{
			typeof(IEnumerable<>),
			typeof(InterfaceEnumerableFormatter<>)
		},
		{
			typeof(Dictionary<, >),
			typeof(DictionaryFormatter<, >)
		},
		{
			typeof(IDictionary<, >),
			typeof(InterfaceDictionaryFormatter<, >)
		},
		{
			typeof(SortedDictionary<, >),
			typeof(SortedDictionaryFormatter<, >)
		},
		{
			typeof(SortedList<, >),
			typeof(SortedListFormatter<, >)
		},
		{
			typeof(ILookup<, >),
			typeof(InterfaceLookupFormatter<, >)
		},
		{
			typeof(IGrouping<, >),
			typeof(InterfaceGroupingFormatter<, >)
		}
	};

	internal static object GetFormatter(Type t)
	{
		TypeInfo typeInfo = t.GetTypeInfo();
		if (t.IsArray)
		{
			switch (t.GetArrayRank())
			{
				case 1:
					if (t.GetElementType() == typeof(byte))
					{
						return ByteArrayFormatter.Default;
					}
					return Activator.CreateInstance(typeof(ArrayFormatter<>).MakeGenericType(t.GetElementType()));
				case 2:
					return Activator.CreateInstance(typeof(TwoDimentionalArrayFormatter<>).MakeGenericType(t.GetElementType()));
				case 3:
					return Activator.CreateInstance(typeof(ThreeDimentionalArrayFormatter<>).MakeGenericType(t.GetElementType()));
				case 4:
					return Activator.CreateInstance(typeof(FourDimentionalArrayFormatter<>).MakeGenericType(t.GetElementType()));
				default:
					return null;
			}
		}
		if (typeInfo.IsGenericType)
		{
			Type genericTypeDefinition = typeInfo.GetGenericTypeDefinition();
			bool flag = genericTypeDefinition.GetTypeInfo().IsNullable();
			Type type = flag ? typeInfo.GenericTypeArguments[0] : null;
			if (genericTypeDefinition == typeof(KeyValuePair<,>))
			{
				return CreateInstance(typeof(KeyValuePairFormatter<,>), typeInfo.GenericTypeArguments);
			}
			if (flag && type.GetTypeInfo().IsConstructedGenericType() && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
			{
				return CreateInstance(typeof(NullableFormatter<>), new Type[1]
				{
					type
				});
			}
			if (genericTypeDefinition == typeof(ArraySegment<>))
			{
				if (typeInfo.GenericTypeArguments[0] == typeof(byte))
				{
					return ByteArraySegmentFormatter.Default;
				}
				return CreateInstance(typeof(ArraySegmentFormatter<>), typeInfo.GenericTypeArguments);
			}
			if (flag && type.GetTypeInfo().IsConstructedGenericType() && type.GetGenericTypeDefinition() == typeof(ArraySegment<>))
			{
				if (type == typeof(ArraySegment<byte>))
				{
					return new StaticNullableFormatter<ArraySegment<byte>>(ByteArraySegmentFormatter.Default);
				}
				return CreateInstance(typeof(NullableFormatter<>), new Type[1]
				{
					type
				});
			}
			if (formatterMap.TryGetValue(genericTypeDefinition, out Type value))
			{
				return CreateInstance(value, typeInfo.GenericTypeArguments);
			}
			if (typeInfo.GenericTypeArguments.Length == 1 && typeInfo.ImplementedInterfaces.Any((Type x) => x.GetTypeInfo().IsConstructedGenericType() && x.GetGenericTypeDefinition() == typeof(ICollection<>)) && typeInfo.DeclaredConstructors.Any((ConstructorInfo x) => x.GetParameters().Length == 0))
			{
				Type type2 = typeInfo.GenericTypeArguments[0];
				return CreateInstance(typeof(GenericCollectionFormatter<,>), new Type[2]
				{
					type2,
					t
				});
			}
			if (typeInfo.GenericTypeArguments.Length == 2 && typeInfo.ImplementedInterfaces.Any((Type x) => x.GetTypeInfo().IsConstructedGenericType() && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)) && typeInfo.DeclaredConstructors.Any((ConstructorInfo x) => x.GetParameters().Length == 0))
			{
				Type type3 = typeInfo.GenericTypeArguments[0];
				Type type4 = typeInfo.GenericTypeArguments[1];
				return CreateInstance(typeof(GenericDictionaryFormatter<,,>), new Type[3]
				{
					type3,
					type4,
					t
				});
			}
		}
		else
		{
			if (t == typeof(IEnumerable))
			{
				return NonGenericInterfaceEnumerableFormatter.Default;
			}
			if (t == typeof(ICollection))
			{
				return NonGenericInterfaceCollectionFormatter.Default;
			}
			if (t == typeof(IList))
			{
				return NonGenericInterfaceListFormatter.Default;
			}
			if (t == typeof(IDictionary))
			{
				return NonGenericInterfaceDictionaryFormatter.Default;
			}
			if (typeof(IList).GetTypeInfo().IsAssignableFrom(typeInfo) && typeInfo.DeclaredConstructors.Any((ConstructorInfo x) => x.GetParameters().Length == 0))
			{
				return Activator.CreateInstance(typeof(NonGenericListFormatter<>).MakeGenericType(t));
			}
			if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeInfo) && typeInfo.DeclaredConstructors.Any((ConstructorInfo x) => x.GetParameters().Length == 0))
			{
				return Activator.CreateInstance(typeof(NonGenericDictionaryFormatter<>).MakeGenericType(t));
			}
		}
		return null;
	}

	private static object CreateInstance(Type genericType, Type[] genericTypeArguments, params object[] arguments)
	{
		return Activator.CreateInstance(genericType.MakeGenericType(genericTypeArguments), arguments);
	}
}
