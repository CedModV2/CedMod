// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.ReflectionExtensions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Utf8Json.Internal
{
  internal static class ReflectionExtensions
  {
    public static bool IsNullable(this TypeInfo type)
    {
      return ((Type) type).IsGenericType && ((Type) type).GetGenericTypeDefinition() == typeof (Nullable<>);
    }

    public static bool IsPublic(this TypeInfo type)
    {
      return ((Type) type).IsPublic;
    }

    public static bool IsAnonymous(this TypeInfo type)
    {
      return CustomAttributeExtensions.GetCustomAttribute<CompilerGeneratedAttribute>((MemberInfo) type) != null && ((MemberInfo) type).Name.Contains("AnonymousType") && (((MemberInfo) type).Name.StartsWith("<>") || ((MemberInfo) type).Name.StartsWith("VB$")) && (((Type) type).Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
    }

    public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
    {
      return ReflectionExtensions.GetAllPropertiesCore(type, new HashSet<string>());
    }

    private static IEnumerable<PropertyInfo> GetAllPropertiesCore(
      Type type,
      HashSet<string> nameCheck)
    {
      foreach (PropertyInfo runtimeProperty in RuntimeReflectionExtensions.GetRuntimeProperties(type))
      {
        if (nameCheck.Add(runtimeProperty.Name))
          yield return runtimeProperty;
      }
      if (type.BaseType != (Type) null)
      {
        foreach (PropertyInfo propertyInfo in ReflectionExtensions.GetAllPropertiesCore(type.BaseType, nameCheck))
          yield return propertyInfo;
      }
    }

    public static IEnumerable<FieldInfo> GetAllFields(this Type type)
    {
      return ReflectionExtensions.GetAllFieldsCore(type, new HashSet<string>());
    }

    private static IEnumerable<FieldInfo> GetAllFieldsCore(
      Type type,
      HashSet<string> nameCheck)
    {
      foreach (FieldInfo runtimeField in RuntimeReflectionExtensions.GetRuntimeFields(type))
      {
        if (nameCheck.Add(runtimeField.Name))
          yield return runtimeField;
      }
      if (type.BaseType != (Type) null)
      {
        foreach (FieldInfo fieldInfo in ReflectionExtensions.GetAllFieldsCore(type.BaseType, nameCheck))
          yield return fieldInfo;
      }
    }
  }
}
