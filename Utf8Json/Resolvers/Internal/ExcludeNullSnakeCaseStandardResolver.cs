// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.ExcludeNullSnakeCaseStandardResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class ExcludeNullSnakeCaseStandardResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new ExcludeNullSnakeCaseStandardResolver();
    private static readonly IJsonFormatter<object> fallbackFormatter = (IJsonFormatter<object>) new DynamicObjectTypeFallbackFormatter(new IJsonFormatterResolver[1]
    {
      ExcludeNullSnakeCaseStandardResolver.InnerResolver.Instance
    });

    private ExcludeNullSnakeCaseStandardResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return ExcludeNullSnakeCaseStandardResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        if (typeof (T) == typeof (object))
          ExcludeNullSnakeCaseStandardResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) ExcludeNullSnakeCaseStandardResolver.fallbackFormatter;
        else
          ExcludeNullSnakeCaseStandardResolver.FormatterCache<T>.formatter = ExcludeNullSnakeCaseStandardResolver.InnerResolver.Instance.GetFormatter<T>();
      }
    }

    private sealed class InnerResolver : IJsonFormatterResolver
    {
      public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new ExcludeNullSnakeCaseStandardResolver.InnerResolver();
      private static readonly IJsonFormatterResolver[] resolvers = ((IEnumerable<IJsonFormatterResolver>) StandardResolverHelper.CompositeResolverBase).Concat<IJsonFormatterResolver>((IEnumerable<IJsonFormatterResolver>) new IJsonFormatterResolver[1]
      {
        DynamicObjectResolver.ExcludeNullSnakeCase
      }).ToArray<IJsonFormatterResolver>();

      private InnerResolver()
      {
      }

      public IJsonFormatter<T> GetFormatter<T>()
      {
        return ExcludeNullSnakeCaseStandardResolver.InnerResolver.FormatterCache<T>.formatter;
      }

      private static class FormatterCache<T>
      {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
          foreach (IJsonFormatterResolver resolver in ExcludeNullSnakeCaseStandardResolver.InnerResolver.resolvers)
          {
            IJsonFormatter<T> formatter = resolver.GetFormatter<T>();
            if (formatter != null)
            {
              ExcludeNullSnakeCaseStandardResolver.InnerResolver.FormatterCache<T>.formatter = formatter;
              break;
            }
          }
        }
      }
    }
  }
}
