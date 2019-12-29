// Decompiled with JetBrains decompiler
// Type: JsonSerialize
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json;
using Utf8Json.Resolvers;
using Utf8Json.Unity;

public static class JsonSerialize
{
  static JsonSerialize()
  {
    CompositeResolver.RegisterAndSetAsDefault(GeneratedResolver.Instance, BuiltinResolver.Instance, EnumResolver.Default, UnityResolver.Instance, StandardResolver.Default);
  }

  public static string ToJson<T>(T value) where T : IJsonSerializable
  {
    return JsonSerializer.ToJsonString<T>(value);
  }

  public static T FromJson<T>(string value) where T : IJsonSerializable
  {
    return JsonSerializer.Deserialize<T>(value);
  }
}
