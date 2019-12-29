// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.MetaMember
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Utf8Json.Internal.Emit
{
  internal class MetaMember
  {
    private MethodInfo getMethod;
    private MethodInfo setMethod;

    public string Name { get; private set; }

    public string MemberName { get; private set; }

    public bool IsProperty
    {
      get
      {
        return this.PropertyInfo != (PropertyInfo) null;
      }
    }

    public bool IsField
    {
      get
      {
        return this.FieldInfo != (FieldInfo) null;
      }
    }

    public bool IsWritable { get; private set; }

    public bool IsReadable { get; private set; }

    public Type Type { get; private set; }

    public FieldInfo FieldInfo { get; private set; }

    public PropertyInfo PropertyInfo { get; private set; }

    public MethodInfo ShouldSerializeMethodInfo { get; private set; }

    protected MetaMember(
      Type type,
      string name,
      string memberName,
      bool isWritable,
      bool isReadable)
    {
      this.Name = name;
      this.MemberName = memberName;
      this.Type = type;
      this.IsWritable = isWritable;
      this.IsReadable = isReadable;
    }

    public MetaMember(FieldInfo info, string name, bool allowPrivate)
    {
      this.Name = name;
      this.MemberName = info.Name;
      this.FieldInfo = info;
      this.Type = info.FieldType;
      this.IsReadable = allowPrivate || info.IsPublic;
      this.IsWritable = allowPrivate || info.IsPublic && !info.IsInitOnly;
      this.ShouldSerializeMethodInfo = MetaMember.GetShouldSerialize((MemberInfo) info);
    }

    public MetaMember(PropertyInfo info, string name, bool allowPrivate)
    {
      this.getMethod = info.GetGetMethod(true);
      this.setMethod = info.GetSetMethod(true);
      this.Name = name;
      this.MemberName = info.Name;
      this.PropertyInfo = info;
      this.Type = info.PropertyType;
      this.IsReadable = this.getMethod != (MethodInfo) null && (allowPrivate || this.getMethod.IsPublic) && !this.getMethod.IsStatic;
      this.IsWritable = this.setMethod != (MethodInfo) null && (allowPrivate || this.setMethod.IsPublic) && !this.setMethod.IsStatic;
      this.ShouldSerializeMethodInfo = MetaMember.GetShouldSerialize((MemberInfo) info);
    }

    private static MethodInfo GetShouldSerialize(MemberInfo info)
    {
      string shouldSerialize = "ShouldSerialize" + info.Name;
      return ((IEnumerable<MethodInfo>) info.DeclaringType.GetMethods(BindingFlags.Instance | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (x => x.Name == shouldSerialize && x.ReturnType == typeof (bool) && x.GetParameters().Length == 0)).FirstOrDefault<MethodInfo>();
    }

    public T GetCustomAttribute<T>(bool inherit) where T : Attribute
    {
      if (this.IsProperty)
        return CustomAttributeExtensions.GetCustomAttribute<T>((MemberInfo) this.PropertyInfo, inherit);
      return this.FieldInfo != (FieldInfo) null ? CustomAttributeExtensions.GetCustomAttribute<T>((MemberInfo) this.FieldInfo, inherit) : default (T);
    }

    public virtual void EmitLoadValue(ILGenerator il)
    {
      if (this.IsProperty)
        il.EmitCall(this.getMethod);
      else
        il.Emit(OpCodes.Ldfld, this.FieldInfo);
    }

    public virtual void EmitStoreValue(ILGenerator il)
    {
      if (this.IsProperty)
        il.EmitCall(this.setMethod);
      else
        il.Emit(OpCodes.Stfld, this.FieldInfo);
    }
  }
}
