// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.DynamicAssembly
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Utf8Json.Internal.Emit
{
  internal class DynamicAssembly
  {
    private readonly object gate = new object();
    private readonly AssemblyBuilder assemblyBuilder;
    private readonly ModuleBuilder moduleBuilder;

    public TypeBuilder DefineType(string name, TypeAttributes attr)
    {
      lock (this.gate)
        return this.moduleBuilder.DefineType(name, attr);
    }

    public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
    {
      lock (this.gate)
        return this.moduleBuilder.DefineType(name, attr, parent);
    }

    public TypeBuilder DefineType(
      string name,
      TypeAttributes attr,
      Type parent,
      Type[] interfaces)
    {
      lock (this.gate)
        return this.moduleBuilder.DefineType(name, attr, parent, interfaces);
    }

    public DynamicAssembly(string moduleName)
    {
      this.assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(moduleName), AssemblyBuilderAccess.Run);
      this.moduleBuilder = this.assemblyBuilder.DefineDynamicModule(moduleName);
    }
  }
}
