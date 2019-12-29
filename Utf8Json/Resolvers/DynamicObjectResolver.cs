// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.DynamicObjectResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Resolvers.Internal;

namespace Utf8Json.Resolvers
{
  public static class DynamicObjectResolver
  {
    public static readonly IJsonFormatterResolver Default = DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateOriginal.Instance;
    public static readonly IJsonFormatterResolver CamelCase = DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateCamelCase.Instance;
    public static readonly IJsonFormatterResolver SnakeCase = DynamicObjectResolverAllowPrivateFalseExcludeNullFalseNameMutateSnakeCase.Instance;
    public static readonly IJsonFormatterResolver ExcludeNull = DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateOriginal.Instance;
    public static readonly IJsonFormatterResolver ExcludeNullCamelCase = DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateCamelCase.Instance;
    public static readonly IJsonFormatterResolver ExcludeNullSnakeCase = DynamicObjectResolverAllowPrivateFalseExcludeNullTrueNameMutateSnakeCase.Instance;
    public static readonly IJsonFormatterResolver AllowPrivate = DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateOriginal.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateCamelCase = DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateCamelCase.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateSnakeCase = DynamicObjectResolverAllowPrivateTrueExcludeNullFalseNameMutateSnakeCase.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateExcludeNull = DynamicObjectResolverAllowPrivateTrueExcludeNullTrueNameMutateOriginal.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateExcludeNullCamelCase = DynamicObjectResolverAllowPrivateTrueExcludeNullTrueNameMutateCamelCase.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateExcludeNullSnakeCase = DynamicObjectResolverAllowPrivateTrueExcludeNullTrueNameMutateSnakeCase.Instance;
  }
}
