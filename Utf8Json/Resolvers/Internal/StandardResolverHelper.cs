// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.StandardResolverHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Unity;

namespace Utf8Json.Resolvers.Internal
{
  internal static class StandardResolverHelper
  {
    internal static readonly IJsonFormatterResolver[] CompositeResolverBase = new IJsonFormatterResolver[5]
    {
      BuiltinResolver.Instance,
      UnityResolver.Instance,
      EnumResolver.Default,
      DynamicGenericResolver.Instance,
      AttributeFormatterResolver.Instance
    };
  }
}
