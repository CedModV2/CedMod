// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.DynamicCompositeResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;
using System.Reflection.Emit;
using Utf8Json.Internal.Emit;

namespace Utf8Json.Resolvers
{
  public abstract class DynamicCompositeResolver : IJsonFormatterResolver
  {
    private static readonly DynamicAssembly assembly = new DynamicAssembly("Utf8Json.Resolvers.DynamicCompositeResolver");
    private const string ModuleName = "Utf8Json.Resolvers.DynamicCompositeResolver";
    public readonly IJsonFormatter[] formatters;
    public readonly IJsonFormatterResolver[] resolvers;

    public static IJsonFormatterResolver Create(
      IJsonFormatter[] formatters,
      IJsonFormatterResolver[] resolvers)
    {
      string str = Guid.NewGuid().ToString().Replace("-", "");
      TypeBuilder typeBuilder1 = DynamicCompositeResolver.assembly.DefineType("DynamicCompositeResolver_" + str, TypeAttributes.Public | TypeAttributes.Sealed, typeof (DynamicCompositeResolver));
      TypeBuilder typeBuilder2 = DynamicCompositeResolver.assembly.DefineType("DynamicCompositeResolverCache_" + str, TypeAttributes.Public | TypeAttributes.Sealed, (Type) null);
      GenericTypeParameterBuilder genericParameter1 = typeBuilder2.DefineGenericParameters("T")[0];
      FieldBuilder fieldBuilder1 = typeBuilder1.DefineField("instance", (Type) typeBuilder1, FieldAttributes.Public | FieldAttributes.Static);
      FieldBuilder fieldBuilder2 = typeBuilder2.DefineField("formatter", typeof (IJsonFormatter<>).MakeGenericType((Type) genericParameter1), FieldAttributes.Public | FieldAttributes.Static);
      ILGenerator ilGenerator1 = typeBuilder2.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes).GetILGenerator();
      ilGenerator1.EmitLdsfld((FieldInfo) fieldBuilder1);
      ilGenerator1.EmitCall(typeof (DynamicCompositeResolver).GetMethod("GetFormatterLoop").MakeGenericMethod((Type) genericParameter1));
      ilGenerator1.Emit(OpCodes.Stsfld, (FieldInfo) fieldBuilder2);
      ilGenerator1.Emit(OpCodes.Ret);
      Type type = typeBuilder2.CreateTypeInfo().AsType();
      ILGenerator ilGenerator2 = typeBuilder1.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[2]
      {
        typeof (IJsonFormatter[]),
        typeof (IJsonFormatterResolver[])
      }).GetILGenerator();
      ilGenerator2.EmitLdarg(0);
      ilGenerator2.EmitLdarg(1);
      ilGenerator2.EmitLdarg(2);
      ilGenerator2.Emit(OpCodes.Call, typeof (DynamicCompositeResolver).GetConstructors()[0]);
      ilGenerator2.Emit(OpCodes.Ret);
      MethodBuilder methodBuilder = typeBuilder1.DefineMethod("GetFormatter", MethodAttributes.Public | MethodAttributes.Virtual);
      GenericTypeParameterBuilder genericParameter2 = methodBuilder.DefineGenericParameters("T")[0];
      methodBuilder.SetReturnType(typeof (IJsonFormatter<>).MakeGenericType((Type) genericParameter2));
      ILGenerator ilGenerator3 = methodBuilder.GetILGenerator();
      FieldInfo field = TypeBuilder.GetField(type.MakeGenericType((Type) genericParameter2), type.GetField("formatter", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField));
      ilGenerator3.EmitLdsfld(field);
      ilGenerator3.Emit(OpCodes.Ret);
      object instance = Activator.CreateInstance(typeBuilder1.CreateTypeInfo().AsType(), (object) formatters, (object) resolvers);
      instance.GetType().GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue((object) null, instance);
      return (IJsonFormatterResolver) instance;
    }

    public DynamicCompositeResolver(IJsonFormatter[] formatters, IJsonFormatterResolver[] resolvers)
    {
      this.formatters = formatters;
      this.resolvers = resolvers;
    }

    public IJsonFormatter<T> GetFormatterLoop<T>()
    {
      foreach (IJsonFormatter formatter in this.formatters)
      {
        foreach (Type implementedInterface in IntrospectionExtensions.GetTypeInfo(formatter.GetType()).get_ImplementedInterfaces())
        {
          TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(implementedInterface);
          if (((Type) typeInfo).IsGenericType && ((Type) typeInfo).get_GenericTypeArguments()[0] == typeof (T))
            return (IJsonFormatter<T>) formatter;
        }
      }
      foreach (IJsonFormatterResolver resolver in this.resolvers)
      {
        IJsonFormatter<T> formatter = resolver.GetFormatter<T>();
        if (formatter != null)
          return formatter;
      }
      return (IJsonFormatter<T>) null;
    }

    public abstract IJsonFormatter<T> GetFormatter<T>();
  }
}
