// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.AllowPrivateExcludeNullSnakeCaseStandardResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class AllowPrivateExcludeNullSnakeCaseStandardResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new AllowPrivateExcludeNullSnakeCaseStandardResolver();
    private static readonly IJsonFormatter<object> fallbackFormatter = (IJsonFormatter<object>) new DynamicObjectTypeFallbackFormatter(new IJsonFormatterResolver[1]
    {
      AllowPrivateExcludeNullSnakeCaseStandardResolver.InnerResolver.Instance
    });

    private AllowPrivateExcludeNullSnakeCaseStandardResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return AllowPrivateExcludeNullSnakeCaseStandardResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        if (typeof (T) == typeof (object))
          AllowPrivateExcludeNullSnakeCaseStandardResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) AllowPrivateExcludeNullSnakeCaseStandardResolver.fallbackFormatter;
        else
          AllowPrivateExcludeNullSnakeCaseStandardResolver.FormatterCache<T>.formatter = AllowPrivateExcludeNullSnakeCaseStandardResolver.InnerResolver.Instance.GetFormatter<T>();
      }
    }

    private sealed class InnerResolver : IJsonFormatterResolver
    {
      public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new AllowPrivateExcludeNullSnakeCaseStandardResolver.InnerResolver();
      private static readonly IJsonFormatterResolver[] resolvers = ((IEnumerable<IJsonFormatterResolver>) StandardResolverHelper.CompositeResolverBase).Concat<IJsonFormatterResolver>((IEnumerable<IJsonFormatterResolver>) new IJsonFormatterResolver[1]
      {
        DynamicObjectResolver.AllowPrivateExcludeNullSnakeCase
      }).ToArray<IJsonFormatterResolver>();

      private InnerResolver()
      {
      }

      public IJsonFormatter<T> GetFormatter<T>()
      {
        return AllowPrivateExcludeNullSnakeCaseStandardResolver.InnerResolver.FormatterCache<T>.formatter;
      }

      private static class FormatterCache<T>
      {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
          foreach (IJsonFormatterResolver resolver in AllowPrivateExcludeNullSnakeCaseStandardResolver.InnerResolver.resolvers)
          {
            IJsonFormatter<T> formatter = resolver.GetFormatter<T>();
            if (formatter != null)
            {
              AllowPrivateExcludeNullSnakeCaseStandardResolver.InnerResolver.FormatterCache<T>.formatter = formatter;
              break;
            }
          }
        }
      }
    }
  }
}
