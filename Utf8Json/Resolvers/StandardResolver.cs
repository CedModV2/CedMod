// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.StandardResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Resolvers.Internal;

namespace Utf8Json.Resolvers
{
  public static class StandardResolver
  {
    public static readonly IJsonFormatterResolver Default = DefaultStandardResolver.Instance;
    public static readonly IJsonFormatterResolver CamelCase = CamelCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver SnakeCase = SnakeCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver ExcludeNull = ExcludeNullStandardResolver.Instance;
    public static readonly IJsonFormatterResolver ExcludeNullCamelCase = ExcludeNullCamelCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver ExcludeNullSnakeCase = ExcludeNullSnakeCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver AllowPrivate = AllowPrivateStandardResolver.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateCamelCase = AllowPrivateCamelCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateSnakeCase = AllowPrivateSnakeCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateExcludeNull = AllowPrivateExcludeNullStandardResolver.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateExcludeNullCamelCase = AllowPrivateExcludeNullCamelCaseStandardResolver.Instance;
    public static readonly IJsonFormatterResolver AllowPrivateExcludeNullSnakeCase = AllowPrivateExcludeNullSnakeCaseStandardResolver.Instance;
  }
}
