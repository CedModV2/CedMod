// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.AttributeFormatterResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;

namespace Utf8Json.Resolvers
{
  public sealed class AttributeFormatterResolver : IJsonFormatterResolver
  {
    public static IJsonFormatterResolver Instance = (IJsonFormatterResolver) new AttributeFormatterResolver();

    private AttributeFormatterResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return AttributeFormatterResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        JsonFormatterAttribute customAttribute = (JsonFormatterAttribute) CustomAttributeExtensions.GetCustomAttribute<JsonFormatterAttribute>((MemberInfo) IntrospectionExtensions.GetTypeInfo(typeof (T)));
        if (customAttribute == null)
          return;
        try
        {
          if (customAttribute.FormatterType.IsGenericType && !IntrospectionExtensions.GetTypeInfo(customAttribute.FormatterType).IsConstructedGenericType())
            AttributeFormatterResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) Activator.CreateInstance(customAttribute.FormatterType.MakeGenericType(typeof (T)), customAttribute.Arguments);
          else
            AttributeFormatterResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) Activator.CreateInstance(customAttribute.FormatterType, customAttribute.Arguments);
        }
        catch (Exception ex)
        {
          throw new InvalidOperationException("Can not create formatter from JsonFormatterAttribute, check the target formatter is public and has constructor with right argument. FormatterType:" + customAttribute.FormatterType.Name, ex);
        }
      }
    }
  }
}
