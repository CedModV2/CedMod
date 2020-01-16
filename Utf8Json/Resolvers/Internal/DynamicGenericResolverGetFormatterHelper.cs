// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicGenericResolverGetFormatterHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers.Internal
{
  internal static class DynamicGenericResolverGetFormatterHelper
  {
    private static readonly Dictionary<Type, Type> formatterMap = new Dictionary<Type, Type>()
    {
      {
        typeof (List<>),
        typeof (ListFormatter<>)
      },
      {
        typeof (LinkedList<>),
        typeof (LinkedListFormatter<>)
      },
      {
        typeof (Queue<>),
        typeof (QeueueFormatter<>)
      },
      {
        typeof (Stack<>),
        typeof (StackFormatter<>)
      },
      {
        typeof (HashSet<>),
        typeof (HashSetFormatter<>)
      },
      {
        typeof (ReadOnlyCollection<>),
        typeof (ReadOnlyCollectionFormatter<>)
      },
      {
        typeof (IList<>),
        typeof (InterfaceListFormatter<>)
      },
      {
        typeof (ICollection<>),
        typeof (InterfaceCollectionFormatter<>)
      },
      {
        typeof (IEnumerable<>),
        typeof (InterfaceEnumerableFormatter<>)
      },
      {
        typeof (Dictionary<,>),
        typeof (DictionaryFormatter<,>)
      },
      {
        typeof (IDictionary<,>),
        typeof (InterfaceDictionaryFormatter<,>)
      },
      {
        typeof (SortedDictionary<,>),
        typeof (SortedDictionaryFormatter<,>)
      },
      {
        typeof (SortedList<,>),
        typeof (SortedListFormatter<,>)
      },
      {
        typeof (ILookup<,>),
        typeof (InterfaceLookupFormatter<,>)
      },
      {
        typeof (IGrouping<,>),
        typeof (InterfaceGroupingFormatter<,>)
      }
    };

    internal static object GetFormatter(Type t)
    {
      TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(t);
      if (t.IsArray)
      {
        switch (t.GetArrayRank())
        {
          case 1:
            if (t.GetElementType() == typeof (byte))
              return (object) ByteArrayFormatter.Default;
            return Activator.CreateInstance(typeof (ArrayFormatter<>).MakeGenericType(t.GetElementType()));
          case 2:
            return Activator.CreateInstance(typeof (TwoDimentionalArrayFormatter<>).MakeGenericType(t.GetElementType()));
          case 3:
            return Activator.CreateInstance(typeof (ThreeDimentionalArrayFormatter<>).MakeGenericType(t.GetElementType()));
          case 4:
            return Activator.CreateInstance(typeof (FourDimentionalArrayFormatter<>).MakeGenericType(t.GetElementType()));
          default:
            return (object) null;
        }
      }
      else
      {
        if (((Type) typeInfo).IsGenericType)
        {
          Type genericTypeDefinition = ((Type) typeInfo).GetGenericTypeDefinition();
          bool flag = IntrospectionExtensions.GetTypeInfo(genericTypeDefinition).IsNullable();
          Type type = flag ? ((Type) typeInfo).GenericTypeArguments[0] : (Type) null; 
          if (genericTypeDefinition == typeof (KeyValuePair<,>))
            return DynamicGenericResolverGetFormatterHelper.CreateInstance(typeof (KeyValuePairFormatter<,>), ((Type) typeInfo).GenericTypeArguments, (object[]) Array.Empty<object>());
          if (flag && IntrospectionExtensions.GetTypeInfo(type).IsConstructedGenericType() && type.GetGenericTypeDefinition() == typeof (KeyValuePair<,>))
            return DynamicGenericResolverGetFormatterHelper.CreateInstance(typeof (NullableFormatter<>), new Type[1]
            {
              type
            }, (object[]) Array.Empty<object>());
          if (genericTypeDefinition == typeof (ArraySegment<>))
            return ((Type) typeInfo).GenericTypeArguments[0] == typeof (byte) ? (object) ByteArraySegmentFormatter.Default : DynamicGenericResolverGetFormatterHelper.CreateInstance(typeof (ArraySegmentFormatter<>), ((Type) typeInfo).get_GenericTypeArguments(), (object[]) Array.Empty<object>());
          if (flag && IntrospectionExtensions.GetTypeInfo(type).IsConstructedGenericType() && type.GetGenericTypeDefinition() == typeof (ArraySegment<>))
          {
            if (type == typeof (ArraySegment<byte>))
              return (object) new StaticNullableFormatter<ArraySegment<byte>>(ByteArraySegmentFormatter.Default);
            return DynamicGenericResolverGetFormatterHelper.CreateInstance(typeof (NullableFormatter<>), new Type[1]
            {
              type
            }, (object[]) Array.Empty<object>());
          }
          Type genericType;
          if (DynamicGenericResolverGetFormatterHelper.formatterMap.TryGetValue(genericTypeDefinition, out genericType))
            return DynamicGenericResolverGetFormatterHelper.CreateInstance(genericType, ((Type) typeInfo).get_GenericTypeArguments(), (object[]) Array.Empty<object>());
          if (((Type) typeInfo).get_GenericTypeArguments().Length == 1 && typeInfo.get_ImplementedInterfaces().Any<Type>((Func<Type, bool>) (x => IntrospectionExtensions.GetTypeInfo(x).IsConstructedGenericType() && x.GetGenericTypeDefinition() == typeof (ICollection<>))) && typeInfo.get_DeclaredConstructors().Any<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.GetParameters().Length == 0)))
            return DynamicGenericResolverGetFormatterHelper.CreateInstance(typeof (GenericCollectionFormatter<,>), new Type[2]
            {
              ((Type) typeInfo).get_GenericTypeArguments()[0],
              t
            }, (object[]) Array.Empty<object>());
          if (((Type) typeInfo).get_GenericTypeArguments().Length == 2 && typeInfo.get_ImplementedInterfaces().Any<Type>((Func<Type, bool>) (x => IntrospectionExtensions.GetTypeInfo(x).IsConstructedGenericType() && x.GetGenericTypeDefinition() == typeof (IDictionary<,>))) && typeInfo.get_DeclaredConstructors().Any<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.GetParameters().Length == 0)))
            return DynamicGenericResolverGetFormatterHelper.CreateInstance(typeof (GenericDictionaryFormatter<,,>), new Type[3]
            {
              ((Type) typeInfo).get_GenericTypeArguments()[0],
              ((Type) typeInfo).get_GenericTypeArguments()[1],
              t
            }, (object[]) Array.Empty<object>());
        }
        else
        {
          if (t == typeof (IEnumerable))
            return (object) NonGenericInterfaceEnumerableFormatter.Default;
          if (t == typeof (ICollection))
            return (object) NonGenericInterfaceCollectionFormatter.Default;
          if (t == typeof (IList))
            return (object) NonGenericInterfaceListFormatter.Default;
          if (t == typeof (IDictionary))
            return (object) NonGenericInterfaceDictionaryFormatter.Default;
          if (IntrospectionExtensions.GetTypeInfo(typeof (IList)).IsAssignableFrom(typeInfo) && typeInfo.get_DeclaredConstructors().Any<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.GetParameters().Length == 0)))
            return Activator.CreateInstance(typeof (NonGenericListFormatter<>).MakeGenericType(t));
          if (IntrospectionExtensions.GetTypeInfo(typeof (IDictionary)).IsAssignableFrom(typeInfo) && typeInfo.get_DeclaredConstructors().Any<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.GetParameters().Length == 0)))
            return Activator.CreateInstance(typeof (NonGenericDictionaryFormatter<>).MakeGenericType(t));
        }
        return (object) null;
      }
    }

    private static object CreateInstance(
      Type genericType,
      Type[] genericTypeArguments,
      params object[] arguments)
    {
      return Activator.CreateInstance(genericType.MakeGenericType(genericTypeArguments), arguments);
    }
  }
}
