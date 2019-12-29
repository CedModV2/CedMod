// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DefaultStandardResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class DefaultStandardResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DefaultStandardResolver();
    private static readonly IJsonFormatter<object> fallbackFormatter = (IJsonFormatter<object>) new DynamicObjectTypeFallbackFormatter(new IJsonFormatterResolver[1]
    {
      DefaultStandardResolver.InnerResolver.Instance
    });

    private DefaultStandardResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return DefaultStandardResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        if (typeof (T) == typeof (object))
          DefaultStandardResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) DefaultStandardResolver.fallbackFormatter;
        else
          DefaultStandardResolver.FormatterCache<T>.formatter = DefaultStandardResolver.InnerResolver.Instance.GetFormatter<T>();
      }
    }

    private sealed class InnerResolver : IJsonFormatterResolver
    {
      public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DefaultStandardResolver.InnerResolver();
      private static readonly IJsonFormatterResolver[] resolvers = ((IEnumerable<IJsonFormatterResolver>) StandardResolverHelper.CompositeResolverBase).Concat<IJsonFormatterResolver>((IEnumerable<IJsonFormatterResolver>) new IJsonFormatterResolver[1]
      {
        DynamicObjectResolver.Default
      }).ToArray<IJsonFormatterResolver>();

      private InnerResolver()
      {
      }

      public IJsonFormatter<T> GetFormatter<T>()
      {
        return DefaultStandardResolver.InnerResolver.FormatterCache<T>.formatter;
      }

      private static class FormatterCache<T>
      {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
          foreach (IJsonFormatterResolver resolver in DefaultStandardResolver.InnerResolver.resolvers)
          {
            IJsonFormatter<T> formatter = resolver.GetFormatter<T>();
            if (formatter != null)
            {
              DefaultStandardResolver.InnerResolver.FormatterCache<T>.formatter = formatter;
              break;
            }
          }
        }
      }
    }
  }
}
