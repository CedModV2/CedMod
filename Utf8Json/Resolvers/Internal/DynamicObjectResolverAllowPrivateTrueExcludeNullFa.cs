// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal();
    private static readonly Func<string, string> nameMutator = new Func<string, string>(StringMutator.Original);
    private static readonly bool excludeNull = false;

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter = (IJsonFormatter<T>) DynamicObjectTypeBuilder.BuildFormatterToDynamicMethod<T>(DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal.Instance, DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal.nameMutator, DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal.excludeNull, true);
    }
  }
}
