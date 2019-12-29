// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.DynamicGenericResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Resolvers.Internal;

namespace Utf8Json.Resolvers
{
  public sealed class DynamicGenericResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DynamicGenericResolver();

    private DynamicGenericResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return DynamicGenericResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter = (IJsonFormatter<T>) DynamicGenericResolverGetFormatterHelper.GetFormatter(typeof (T));
    }
  }
}
