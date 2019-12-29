// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase();
    private static readonly Func<string, string> nameMutator = new Func<string, string>(StringMutator.ToSnakeCase);
    private static readonly bool excludeNull = false;
    private static readonly DynamicAssembly assembly = new DynamicAssembly("Utf8Json.Resolvers.DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase");
    private const string ModuleName = "Utf8Json.Resolvers.DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase";

    private DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter = (IJsonFormatter<T>) DynamicObjectTypeBuilder.BuildFormatterToAssembly<T>(DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase.assembly, DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase.Instance, DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase.nameMutator, DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase.excludeNull);
    }
  }
}
