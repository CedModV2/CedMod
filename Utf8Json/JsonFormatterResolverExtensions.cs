// Decompiled with JetBrains decompiler
// Type: Utf8Json.JsonFormatterResolverExtensions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;

namespace Utf8Json
{
  public static class JsonFormatterResolverExtensions
  {
    public static IJsonFormatter<T> GetFormatterWithVerify<T>(
      this IJsonFormatterResolver resolver)
    {
      IJsonFormatter<T> formatter;
      try
      {
        formatter = resolver.GetFormatter<T>();
      }
      catch (TypeInitializationException ex)
      {
        Exception exception = (Exception) ex;
        while (exception.InnerException != null)
          exception = exception.InnerException;
        throw exception;
      }
      if (formatter == null)
        throw new FormatterNotRegisteredException(typeof (T).FullName + " is not registered in this resolver. resolver:" + resolver.GetType().Name);
      return formatter;
    }

    public static object GetFormatterDynamic(this IJsonFormatterResolver resolver, Type type)
    {
      return RuntimeReflectionExtensions.GetRuntimeMethod(typeof (IJsonFormatterResolver), "GetFormatter", Type.EmptyTypes).MakeGenericMethod(type).Invoke((object) resolver, (object[]) null);
    }

    public static void DeserializeToWithFallbackReplace<T>(
      this IJsonFormatterResolver formatterResolver,
      ref T value,
      ref JsonReader reader)
    {
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      if (formatterWithVerify is IOverwriteJsonFormatter<T> overwriteJsonFormatter)
        overwriteJsonFormatter.DeserializeTo(ref value, ref reader, formatterResolver);
      else
        value = formatterWithVerify.Deserialize(ref reader, formatterResolver);
    }
  }
}
