// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.EnumDefaultResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Internal;
using Utf8Json.Resolvers.Internal;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class EnumDefaultResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new EnumDefaultResolver();

    private EnumDefaultResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return EnumDefaultResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
          TypeInfo typeInfo = typeof(T).GetTypeInfo();
          if (typeInfo.IsNullable())
          {
              typeInfo = typeInfo.GenericTypeArguments[0].GetTypeInfo();
              if (typeInfo.IsEnum)
              {
                  object formatterDynamic = Instance.GetFormatterDynamic(typeInfo.AsType());
                  if (formatterDynamic != null)
                  {
                      formatter = (IJsonFormatter<T>) Activator.CreateInstance(
                          typeof(StaticNullableFormatter<>).MakeGenericType(typeInfo.AsType()), formatterDynamic);
                  }
              }
          }
          else if (typeof(T).IsEnum)
          {
              formatter = new EnumFormatter<T>(serializeByName: true);
          }
      }
    }
  }
}
