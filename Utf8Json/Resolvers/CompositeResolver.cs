// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.CompositeResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;

namespace Utf8Json.Resolvers
{
  public sealed class CompositeResolver : IJsonFormatterResolver
  {
    public static readonly CompositeResolver Instance = new CompositeResolver();
    private static bool isFreezed = false;
    private static IJsonFormatter[] formatters = new IJsonFormatter[0];
    private static IJsonFormatterResolver[] resolvers = new IJsonFormatterResolver[0];

    private CompositeResolver()
    {
    }

    public static void Register(params IJsonFormatterResolver[] resolvers)
    {
      if (CompositeResolver.isFreezed)
        throw new InvalidOperationException("Register must call on startup(before use GetFormatter<T>).");
      CompositeResolver.resolvers = resolvers;
    }

    public static void Register(params IJsonFormatter[] formatters)
    {
      if (CompositeResolver.isFreezed)
        throw new InvalidOperationException("Register must call on startup(before use GetFormatter<T>).");
      CompositeResolver.formatters = formatters;
    }

    public static void Register(IJsonFormatter[] formatters, IJsonFormatterResolver[] resolvers)
    {
      if (CompositeResolver.isFreezed)
        throw new InvalidOperationException("Register must call on startup(before use GetFormatter<T>).");
      CompositeResolver.resolvers = resolvers;
      CompositeResolver.formatters = formatters;
    }

    public static void RegisterAndSetAsDefault(params IJsonFormatterResolver[] resolvers)
    {
      CompositeResolver.Register(resolvers);
      JsonSerializer.SetDefaultResolver((IJsonFormatterResolver) CompositeResolver.Instance);
    }

    public static void RegisterAndSetAsDefault(params IJsonFormatter[] formatters)
    {
      CompositeResolver.Register(formatters);
      JsonSerializer.SetDefaultResolver((IJsonFormatterResolver) CompositeResolver.Instance);
    }

    public static void RegisterAndSetAsDefault(
      IJsonFormatter[] formatters,
      IJsonFormatterResolver[] resolvers)
    {
      CompositeResolver.Register(formatters);
      CompositeResolver.Register(resolvers);
      JsonSerializer.SetDefaultResolver((IJsonFormatterResolver) CompositeResolver.Instance);
    }

    public static IJsonFormatterResolver Create(
      params IJsonFormatter[] formatters)
    {
      return CompositeResolver.Create(formatters, new IJsonFormatterResolver[0]);
    }

    public static IJsonFormatterResolver Create(
      params IJsonFormatterResolver[] resolvers)
    {
      return CompositeResolver.Create(new IJsonFormatter[0], resolvers);
    }

    public static IJsonFormatterResolver Create(
      IJsonFormatter[] formatters,
      IJsonFormatterResolver[] resolvers)
    {
      return DynamicCompositeResolver.Create(formatters, resolvers);
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return CompositeResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        CompositeResolver.isFreezed = true;
        foreach (IJsonFormatter formatter in CompositeResolver.formatters)
        {
          foreach (Type implementedInterface in IntrospectionExtensions.GetTypeInfo(formatter.GetType()).get_ImplementedInterfaces())
          {
            TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(implementedInterface);
            if (((Type) typeInfo).IsGenericType && ((Type) typeInfo).get_GenericTypeArguments()[0] == typeof (T))
            {
              CompositeResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) formatter;
              return;
            }
          }
        }
        foreach (IJsonFormatterResolver resolver in CompositeResolver.resolvers)
        {
          IJsonFormatter<T> formatter = resolver.GetFormatter<T>();
          if (formatter != null)
          {
            CompositeResolver.FormatterCache<T>.formatter = formatter;
            break;
          }
        }
      }
    }
  }
}
