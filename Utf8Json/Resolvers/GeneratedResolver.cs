// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.GeneratedResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Resolvers
{
  public class GeneratedResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new GeneratedResolver();

    private GeneratedResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return GeneratedResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter;

      static FormatterCache()
      {
        object formatter = GeneratedResolverGetFormatterHelper.GetFormatter(typeof (T));
        if (formatter == null)
          return;
        GeneratedResolver.FormatterCache<T>.formatter = (IJsonFormatter<T>) formatter;
      }
    }
  }
}
