// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.ExcludeNullStandardResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class ExcludeNullStandardResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new ExcludeNullStandardResolver();
    private static readonly IJsonFormatter<object> fallbackFormatter = (IJsonFormatter<object>) new DynamicObjectTypeFallbackFormatter(new IJsonFormatterResolver[1]
    {
      ExcludeNullStandardResolver.InnerResolver.Instance
    });

    private ExcludeNullStandardResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return ExcludeNullStandardResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        if (typeof (T) == typeof (object))
          ExcludeNullStandardResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) ExcludeNullStandardResolver.fallbackFormatter;
        else
          ExcludeNullStandardResolver.FormatterCache<T>.formatter = ExcludeNullStandardResolver.InnerResolver.Instance.GetFormatter<T>();
      }
    }

    private sealed class InnerResolver : IJsonFormatterResolver
    {
      public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new ExcludeNullStandardResolver.InnerResolver();
      private static readonly IJsonFormatterResolver[] resolvers = ((IEnumerable<IJsonFormatterResolver>) StandardResolverHelper.CompositeResolverBase).Concat<IJsonFormatterResolver>((IEnumerable<IJsonFormatterResolver>) new IJsonFormatterResolver[1]
      {
        DynamicObjectResolver.ExcludeNull
      }).ToArray<IJsonFormatterResolver>();

      private InnerResolver()
      {
      }

      public IJsonFormatter<T> GetFormatter<T>()
      {
        return ExcludeNullStandardResolver.InnerResolver.FormatterCache<T>.formatter;
      }

      private static class FormatterCache<T>
      {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
          foreach (IJsonFormatterResolver resolver in ExcludeNullStandardResolver.InnerResolver.resolvers)
          {
            IJsonFormatter<T> formatter = resolver.GetFormatter<T>();
            if (formatter != null)
            {
              ExcludeNullStandardResolver.InnerResolver.FormatterCache<T>.formatter = formatter;
              break;
            }
          }
        }
      }
    }
  }
}
