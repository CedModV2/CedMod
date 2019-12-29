// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;

namespace Utf8Json.Resolvers.Internal
{
  internal sealed class DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase();
    private static readonly Func<string, string> nameMutator = new Func<string, string>(StringMutator.ToCamelCase);
    private static readonly bool excludeNull = true;
    private static readonly DynamicAssembly assembly = new DynamicAssembly("Utf8Json.Resolvers.DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase");
    private const string ModuleName = "Utf8Json.Resolvers.DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase";

    private DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter = (IJsonFormatter<T>) DynamicObjectTypeBuilder.BuildFormatterToAssembly<T>(DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase.assembly, DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase.Instance, DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase.nameMutator, DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase.excludeNull);
    }
  }
}
