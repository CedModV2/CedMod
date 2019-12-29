// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase();
    private static readonly Func<string, string> nameMutator = new Func<string, string>(StringMutator.ToSnakeCase);
    private static readonly bool excludeNull = false;

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter = (IJsonFormatter<T>) DynamicObjectTypeBuilder.BuildFormatterToDynamicMethod<T>(DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase.Instance, DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase.nameMutator, DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase.excludeNull, true);
    }
  }
}
