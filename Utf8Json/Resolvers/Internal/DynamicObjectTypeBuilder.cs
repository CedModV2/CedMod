// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.Internal.DynamicObjectTypeBuilder
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;
using Utf8Json.Resolvers.Internal;

namespace Utf8Json.Resolvers.Internal
{
  internal static class DynamicObjectTypeBuilder
  {
    private static readonly Regex SubtractFullNameRegex = new Regex(", Version=\\d+.\\d+.\\d+.\\d+, Culture=\\w+, PublicKeyToken=\\w+");
    private static int nameSequence = 0;
    private static HashSet<Type> ignoreTypes = new HashSet<Type>()
    {
      typeof (object),
      typeof (short),
      typeof (int),
      typeof (long),
      typeof (ushort),
      typeof (uint),
      typeof (ulong),
      typeof (float),
      typeof (double),
      typeof (bool),
      typeof (byte),
      typeof (sbyte),
      typeof (Decimal),
      typeof (char),
      typeof (string),
      typeof (Guid),
      typeof (TimeSpan),
      typeof (DateTime),
      typeof (DateTimeOffset)
    };
    private static HashSet<Type> jsonPrimitiveTypes = new HashSet<Type>()
    {
      typeof (short),
      typeof (int),
      typeof (long),
      typeof (ushort),
      typeof (uint),
      typeof (ulong),
      typeof (float),
      typeof (double),
      typeof (bool),
      typeof (byte),
      typeof (sbyte),
      typeof (string)
    };

    public static object BuildFormatterToAssembly<T>(
      DynamicAssembly assembly,
      IJsonFormatterResolver selfResolver,
      Func<string, string> nameMutator,
      bool excludeNull)
    {
      TypeInfo typeInfo1 = IntrospectionExtensions.GetTypeInfo(typeof (T));
      if (typeInfo1.IsNullable())
      {
        TypeInfo typeInfo2 = IntrospectionExtensions.GetTypeInfo(((Type) typeInfo1).get_GenericTypeArguments()[0]);
        object formatterDynamic = selfResolver.GetFormatterDynamic(typeInfo2.AsType());
        if (formatterDynamic == null)
          return (object) null;
        return (object) (IJsonFormatter<T>) Activator.CreateInstance(typeof (StaticNullableFormatter<>).MakeGenericType(typeInfo2.AsType()), formatterDynamic);
      }
      if (IntrospectionExtensions.GetTypeInfo(typeof (Exception)).IsAssignableFrom(typeInfo1))
        return DynamicObjectTypeBuilder.BuildAnonymousFormatter(typeof (T), nameMutator, excludeNull, false, true);
      if (typeInfo1.IsAnonymous() || DynamicObjectTypeBuilder.TryGetInterfaceEnumerableElementType(typeof (T), out Type _))
        return DynamicObjectTypeBuilder.BuildAnonymousFormatter(typeof (T), nameMutator, excludeNull, false, false);
      TypeInfo typeInfo3 = DynamicObjectTypeBuilder.BuildType(assembly, typeof (T), nameMutator, excludeNull);
      return (Type) typeInfo3 == (Type) null ? (object) null : (object) (IJsonFormatter<T>) Activator.CreateInstance(typeInfo3.AsType());
    }

    public static object BuildFormatterToDynamicMethod<T>(
      IJsonFormatterResolver selfResolver,
      Func<string, string> nameMutator,
      bool excludeNull,
      bool allowPrivate)
    {
      TypeInfo typeInfo1 = IntrospectionExtensions.GetTypeInfo(typeof (T));
      if (typeInfo1.IsNullable())
      {
        TypeInfo typeInfo2 = IntrospectionExtensions.GetTypeInfo(((Type) typeInfo1).get_GenericTypeArguments()[0]);
        object formatterDynamic = selfResolver.GetFormatterDynamic(typeInfo2.AsType());
        if (formatterDynamic == null)
          return (object) null;
        return (object) (IJsonFormatter<T>) Activator.CreateInstance(typeof (StaticNullableFormatter<>).MakeGenericType(typeInfo2.AsType()), formatterDynamic);
      }
      return IntrospectionExtensions.GetTypeInfo(typeof (Exception)).IsAssignableFrom(typeInfo1) ? DynamicObjectTypeBuilder.BuildAnonymousFormatter(typeof (T), nameMutator, excludeNull, false, true) : DynamicObjectTypeBuilder.BuildAnonymousFormatter(typeof (T), nameMutator, excludeNull, allowPrivate, false);
    }

    private static TypeInfo BuildType(
      DynamicAssembly assembly,
      Type type,
      Func<string, string> nameMutator,
      bool excludeNull)
    {
      if (DynamicObjectTypeBuilder.ignoreTypes.Contains(type))
        return (TypeInfo) null;
      MetaType info = new MetaType(type, nameMutator, false);
      bool hasShouldSerialize = ((IEnumerable<MetaMember>) info.Members).Any<MetaMember>((Func<MetaMember, bool>) (x => x.ShouldSerializeMethodInfo != (MethodInfo) null));
      Type type1 = typeof (IJsonFormatter<>).MakeGenericType(type);
      TypeBuilder builder = assembly.DefineType("Utf8Json.Formatters." + DynamicObjectTypeBuilder.SubtractFullNameRegex.Replace(type.FullName, "").Replace(".", "_") + "Formatter" + (object) Interlocked.Increment(ref DynamicObjectTypeBuilder.nameSequence), TypeAttributes.Public | TypeAttributes.Sealed, (Type) null, new Type[1]
      {
        type1
      });
      ConstructorBuilder constructorBuilder = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
      FieldBuilder stringByteKeysField = builder.DefineField("stringByteKeys", typeof (byte[][]), FieldAttributes.Private | FieldAttributes.InitOnly);
      ILGenerator ilGenerator = constructorBuilder.GetILGenerator();
      Dictionary<MetaMember, FieldInfo> customFormatterLookup = DynamicObjectTypeBuilder.BuildConstructor(builder, info, (ConstructorInfo) constructorBuilder, stringByteKeysField, ilGenerator, excludeNull, hasShouldSerialize);
      ILGenerator il1 = builder.DefineMethod("Serialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual, (Type) null, new Type[3]
      {
        typeof (Utf8Json.JsonWriter).MakeByRefType(),
        type,
        typeof (IJsonFormatterResolver)
      }).GetILGenerator();
      DynamicObjectTypeBuilder.BuildSerialize(type, info, il1, (Action) (() =>
      {
        il1.EmitLoadThis();
        il1.EmitLdfld((FieldInfo) stringByteKeysField);
      }), (Func<int, MetaMember, bool>) ((index, member) =>
      {
        FieldInfo fieldInfo;
        if (!customFormatterLookup.TryGetValue(member, out fieldInfo))
          return false;
        il1.EmitLoadThis();
        il1.EmitLdfld(fieldInfo);
        return true;
      }), excludeNull, hasShouldSerialize, 1);
      ILGenerator il2 = builder.DefineMethod("Deserialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual, type, new Type[2]
      {
        typeof (Utf8Json.JsonReader).MakeByRefType(),
        typeof (IJsonFormatterResolver)
      }).GetILGenerator();
      DynamicObjectTypeBuilder.BuildDeserialize(type, info, il2, (Func<int, MetaMember, bool>) ((index, member) =>
      {
        FieldInfo fieldInfo;
        if (!customFormatterLookup.TryGetValue(member, out fieldInfo))
          return false;
        il2.EmitLoadThis();
        il2.EmitLdfld(fieldInfo);
        return true;
      }), false, 1);
      return builder.CreateTypeInfo();
    }

    public static object BuildAnonymousFormatter(
      Type type,
      Func<string, string> nameMutator,
      bool excludeNull,
      bool allowPrivate,
      bool isException)
    {
      if (DynamicObjectTypeBuilder.ignoreTypes.Contains(type))
        return (object) false;
      MetaType info;
      if (isException)
      {
        HashSet<string> ignoreSet = new HashSet<string>(((IEnumerable<string>) new string[6]
        {
          "HelpLink",
          "TargetSite",
          "HResult",
          "Data",
          "ClassName",
          "InnerException"
        }).Select<string, string>((Func<string, string>) (x => nameMutator(x))));
        info = new MetaType(type, nameMutator, false)
        {
          BestmatchConstructor = (ConstructorInfo) null,
          ConstructorParameters = new MetaMember[0]
        };
        info.Members = ((IEnumerable<MetaMember>) new StringConstantValueMetaMember[1]
        {
          new StringConstantValueMetaMember(nameMutator("ClassName"), type.FullName)
        }).Concat<MetaMember>(((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => !ignoreSet.Contains(x.Name)))).Concat<MetaMember>((IEnumerable<MetaMember>) new InnerExceptionMetaMember[1]
        {
          new InnerExceptionMetaMember(nameMutator("InnerException"))
        }).ToArray<MetaMember>();
      }
      else
        info = new MetaType(type, nameMutator, allowPrivate);
      bool hasShouldSerialize = ((IEnumerable<MetaMember>) info.Members).Any<MetaMember>((Func<MetaMember, bool>) (x => x.ShouldSerializeMethodInfo != (MethodInfo) null));
      List<byte[]> numArrayList = new List<byte[]>();
      int num = 0;
      foreach (MetaMember metaMember in ((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable)))
      {
        if (excludeNull | hasShouldSerialize)
          numArrayList.Add(Utf8Json.JsonWriter.GetEncodedPropertyName(metaMember.Name));
        else if (num == 0)
          numArrayList.Add(Utf8Json.JsonWriter.GetEncodedPropertyNameWithBeginObject(metaMember.Name));
        else
          numArrayList.Add(Utf8Json.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(metaMember.Name));
        ++num;
      }
      List<object> serializeCustomFormatters = new List<object>();
      List<object> deserializeCustomFormatters = new List<object>();
      foreach (MetaMember metaMember in ((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable)))
      {
        JsonFormatterAttribute customAttribute = metaMember.GetCustomAttribute<JsonFormatterAttribute>(true);
        if (customAttribute != null)
        {
          object instance = Activator.CreateInstance(customAttribute.FormatterType, customAttribute.Arguments);
          serializeCustomFormatters.Add(instance);
        }
        else
          serializeCustomFormatters.Add((object) null);
      }
      foreach (MetaMember member in info.Members)
      {
        JsonFormatterAttribute customAttribute = member.GetCustomAttribute<JsonFormatterAttribute>(true);
        if (customAttribute != null)
        {
          object instance = Activator.CreateInstance(customAttribute.FormatterType, customAttribute.Arguments);
          deserializeCustomFormatters.Add(instance);
        }
        else
          deserializeCustomFormatters.Add((object) null);
      }
      DynamicMethod dynamicMethod1 = new DynamicMethod("Serialize", (Type) null, new Type[5]
      {
        typeof (byte[][]),
        typeof (object[]),
        typeof (Utf8Json.JsonWriter).MakeByRefType(),
        type,
        typeof (IJsonFormatterResolver)
      }, type.Module, true);
      ILGenerator il1 = dynamicMethod1.GetILGenerator();
      DynamicObjectTypeBuilder.BuildSerialize(type, info, il1, (Action) (() => il1.EmitLdarg(0)), (Func<int, MetaMember, bool>) ((index, member) =>
      {
        if (serializeCustomFormatters.Count == 0 || serializeCustomFormatters[index] == null)
          return false;
        il1.EmitLdarg(1);
        il1.EmitLdc_I4(index);
        il1.Emit(OpCodes.Ldelem_Ref);
        il1.Emit(OpCodes.Castclass, serializeCustomFormatters[index].GetType());
        return true;
      }), excludeNull, hasShouldSerialize, 2);
      DynamicMethod dynamicMethod2 = new DynamicMethod("Deserialize", type, new Type[3]
      {
        typeof (object[]),
        typeof (Utf8Json.JsonReader).MakeByRefType(),
        typeof (IJsonFormatterResolver)
      }, type.Module, true);
      ILGenerator il2 = dynamicMethod2.GetILGenerator();
      DynamicObjectTypeBuilder.BuildDeserialize(type, info, il2, (Func<int, MetaMember, bool>) ((index, member) =>
      {
        if (deserializeCustomFormatters.Count == 0 || deserializeCustomFormatters[index] == null)
          return false;
        il2.EmitLdarg(0);
        il2.EmitLdc_I4(index);
        il2.Emit(OpCodes.Ldelem_Ref);
        il2.Emit(OpCodes.Castclass, deserializeCustomFormatters[index].GetType());
        return true;
      }), true, 1);
      object obj1 = (object) ((MethodInfo) dynamicMethod1).CreateDelegate(typeof (AnonymousJsonSerializeAction<>).MakeGenericType(type));
      object obj2 = (object) ((MethodInfo) dynamicMethod2).CreateDelegate(typeof (AnonymousJsonDeserializeFunc<>).MakeGenericType(type));
      return Activator.CreateInstance(typeof (DynamicMethodAnonymousFormatter<>).MakeGenericType(type), (object) numArrayList.ToArray(), (object) serializeCustomFormatters.ToArray(), (object) deserializeCustomFormatters.ToArray(), obj1, obj2);
    }

    private static Dictionary<MetaMember, FieldInfo> BuildConstructor(
      TypeBuilder builder,
      MetaType info,
      ConstructorInfo method,
      FieldBuilder stringByteKeysField,
      ILGenerator il,
      bool excludeNull,
      bool hasShouldSerialize)
    {
      il.EmitLdarg(0);
      il.Emit(OpCodes.Call, DynamicObjectTypeBuilder.EmitInfo.ObjectCtor);
      int num1 = ((IEnumerable<MetaMember>) info.Members).Count<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable));
      il.EmitLdarg(0);
      il.EmitLdc_I4(num1);
      il.Emit(OpCodes.Newarr, typeof (byte[]));
      int num2 = 0;
      foreach (MetaMember metaMember in ((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable)))
      {
        il.Emit(OpCodes.Dup);
        il.EmitLdc_I4(num2);
        il.Emit(OpCodes.Ldstr, metaMember.Name);
        if (excludeNull | hasShouldSerialize)
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.GetEncodedPropertyName);
        else if (num2 == 0)
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.GetEncodedPropertyNameWithBeginObject);
        else
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator);
        il.Emit(OpCodes.Stelem_Ref);
        ++num2;
      }
      il.Emit(OpCodes.Stfld, (FieldInfo) stringByteKeysField);
      Dictionary<MetaMember, FieldInfo> dictionary = DynamicObjectTypeBuilder.BuildCustomFormatterField(builder, info, il);
      il.Emit(OpCodes.Ret);
      return dictionary;
    }

    private static Dictionary<MetaMember, FieldInfo> BuildCustomFormatterField(
      TypeBuilder builder,
      MetaType info,
      ILGenerator il)
    {
      Dictionary<MetaMember, FieldInfo> dictionary = new Dictionary<MetaMember, FieldInfo>();
      foreach (MetaMember key in ((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable || x.IsWritable)))
      {
        JsonFormatterAttribute customAttribute = key.GetCustomAttribute<JsonFormatterAttribute>(true);
        if (customAttribute != null)
        {
          FieldBuilder fieldBuilder = builder.DefineField(key.Name + "_formatter", customAttribute.FormatterType, FieldAttributes.Private | FieldAttributes.InitOnly);
          int num = 52;
          LocalBuilder local = il.DeclareLocal(typeof (JsonFormatterAttribute));
          il.Emit(OpCodes.Ldtoken, info.Type);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetTypeFromHandle);
          il.Emit(OpCodes.Ldstr, key.MemberName);
          il.EmitLdc_I4(num);
          if (key.IsProperty)
            il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.TypeGetProperty);
          else
            il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.TypeGetField);
          il.EmitTrue();
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetCustomAttributeJsonFormatterAttribute);
          il.EmitStloc(local);
          il.EmitLoadThis();
          il.EmitLdloc(local);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonFormatterAttr.FormatterType);
          il.EmitLdloc(local);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonFormatterAttr.Arguments);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.ActivatorCreateInstance);
          il.Emit(OpCodes.Castclass, customAttribute.FormatterType);
          il.Emit(OpCodes.Stfld, (FieldInfo) fieldBuilder);
          dictionary.Add(key, (FieldInfo) fieldBuilder);
        }
      }
      return dictionary;
    }

    private static void BuildSerialize(
      Type type,
      MetaType info,
      ILGenerator il,
      Action emitStringByteKeys,
      Func<int, MetaMember, bool> tryEmitLoadCustomFormatter,
      bool excludeNull,
      bool hasShouldSerialize,
      int firstArgIndex)
    {
      ArgumentField writer = new ArgumentField(il, firstArgIndex, false);
      ArgumentField argValue = new ArgumentField(il, firstArgIndex + 1, type);
      ArgumentField argResolver = new ArgumentField(il, firstArgIndex + 2, false);
      TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(type);
      InnerExceptionMetaMember exceptionMetaMember = info.Members.OfType<InnerExceptionMetaMember>().FirstOrDefault<InnerExceptionMetaMember>();
      if (exceptionMetaMember != null)
      {
        exceptionMetaMember.argWriter = writer;
        exceptionMetaMember.argValue = argValue;
        exceptionMetaMember.argResolver = argResolver;
      }
      Type elementType;
      if (info.IsClass && info.BestmatchConstructor == (ConstructorInfo) null && DynamicObjectTypeBuilder.TryGetInterfaceEnumerableElementType(type, out elementType))
      {
        Type type1 = typeof (IEnumerable<>).MakeGenericType(elementType);
        argResolver.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetFormatterWithVerify.MakeGenericMethod(type1));
        writer.EmitLoad();
        argValue.EmitLoad();
        argResolver.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Serialize(type1));
        il.Emit(OpCodes.Ret);
      }
      else
      {
        if (info.IsClass)
        {
          Label label = il.DefineLabel();
          argValue.EmitLoad();
          il.Emit(OpCodes.Brtrue_S, label);
          writer.EmitLoad();
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteNull);
          il.Emit(OpCodes.Ret);
          il.MarkLabel(label);
        }
        if (type == typeof (Exception))
        {
          Label label = il.DefineLabel();
          LocalBuilder local = il.DeclareLocal(typeof (Type));
          argValue.EmitLoad();
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetTypeMethod);
          il.EmitStloc(local);
          il.EmitLdloc(local);
          il.Emit(OpCodes.Ldtoken, typeof (Exception));
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetTypeFromHandle);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.TypeEquals);
          il.Emit(OpCodes.Brtrue, label);
          il.EmitLdloc(local);
          writer.EmitLoad();
          argValue.EmitLoad();
          argResolver.EmitLoad();
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.NongenericSerialize);
          il.Emit(OpCodes.Ret);
          il.MarkLabel(label);
        }
        LocalBuilder local1 = (LocalBuilder) null;
        Label loc = il.DefineLabel();
        Label[] labelArray = (Label[]) null;
        if (excludeNull | hasShouldSerialize)
        {
          local1 = il.DeclareLocal(typeof (bool));
          writer.EmitLoad();
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteBeginObject);
          labelArray = ((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable)).Select<MetaMember, Label>((Func<MetaMember, Label>) (_ => il.DefineLabel())).ToArray<Label>();
        }
        int index = 0;
        foreach (MetaMember member in ((IEnumerable<MetaMember>) info.Members).Where<MetaMember>((Func<MetaMember, bool>) (x => x.IsReadable)))
        {
          if (excludeNull | hasShouldSerialize)
          {
            il.MarkLabel(labelArray[index]);
            if (excludeNull)
            {
              if (IntrospectionExtensions.GetTypeInfo(member.Type).IsNullable())
              {
                LocalBuilder local2 = il.DeclareLocal(member.Type);
                argValue.EmitLoad();
                member.EmitLoadValue(il);
                il.EmitStloc(local2);
                il.EmitLdloca(local2);
                il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetNullableHasValue(member.Type.GetGenericArguments()[0]));
                il.Emit(OpCodes.Brfalse_S, index < labelArray.Length - 1 ? labelArray[index + 1] : loc);
              }
              else if (!member.Type.IsValueType && !(member is StringConstantValueMetaMember))
              {
                argValue.EmitLoad();
                member.EmitLoadValue(il);
                il.Emit(OpCodes.Brfalse_S, index < labelArray.Length - 1 ? labelArray[index + 1] : loc);
              }
            }
            if (hasShouldSerialize && member.ShouldSerializeMethodInfo != (MethodInfo) null)
            {
              argValue.EmitLoad();
              il.EmitCall(member.ShouldSerializeMethodInfo);
              il.Emit(OpCodes.Brfalse_S, index < labelArray.Length - 1 ? labelArray[index + 1] : loc);
            }
            Label label1 = il.DefineLabel();
            Label label2 = il.DefineLabel();
            il.EmitLdloc(local1);
            il.Emit(OpCodes.Brtrue_S, label2);
            il.EmitTrue();
            il.EmitStloc(local1);
            il.Emit(OpCodes.Br, label1);
            il.MarkLabel(label2);
            writer.EmitLoad();
            il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteValueSeparator);
            il.MarkLabel(label1);
          }
          writer.EmitLoad();
          emitStringByteKeys();
          il.EmitLdc_I4(index);
          il.Emit(OpCodes.Ldelem_Ref);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteRaw);
          DynamicObjectTypeBuilder.EmitSerializeValue(typeInfo, member, il, index, tryEmitLoadCustomFormatter, writer, argValue, argResolver);
          ++index;
        }
        il.MarkLabel(loc);
        if (!excludeNull && index == 0)
        {
          writer.EmitLoad();
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteBeginObject);
        }
        writer.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteEndObject);
        il.Emit(OpCodes.Ret);
      }
    }

    private static void EmitSerializeValue(
      TypeInfo type,
      MetaMember member,
      ILGenerator il,
      int index,
      Func<int, MetaMember, bool> tryEmitLoadCustomFormatter,
      ArgumentField writer,
      ArgumentField argValue,
      ArgumentField argResolver)
    {
      Type type1 = member.Type;
      if (member is InnerExceptionMetaMember)
        (member as InnerExceptionMetaMember).EmitSerializeDirectly(il);
      else if (tryEmitLoadCustomFormatter(index, member))
      {
        writer.EmitLoad();
        argValue.EmitLoad();
        member.EmitLoadValue(il);
        argResolver.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Serialize(type1));
      }
      else if (DynamicObjectTypeBuilder.jsonPrimitiveTypes.Contains(type1))
      {
        writer.EmitLoad();
        argValue.EmitLoad();
        member.EmitLoadValue(il);
        il.EmitCall(IntrospectionExtensions.GetTypeInfo(typeof (Utf8Json.JsonWriter)).GetDeclaredMethods("Write" + type1.Name).OrderByDescending<MethodInfo, int>((Func<MethodInfo, int>) (x => x.GetParameters().Length)).First<MethodInfo>());
      }
      else
      {
        argResolver.EmitLoad();
        il.Emit(OpCodes.Call, DynamicObjectTypeBuilder.EmitInfo.GetFormatterWithVerify.MakeGenericMethod(type1));
        writer.EmitLoad();
        argValue.EmitLoad();
        member.EmitLoadValue(il);
        argResolver.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Serialize(type1));
      }
    }

    private static void BuildDeserialize(
      Type type,
      MetaType info,
      ILGenerator il,
      Func<int, MetaMember, bool> tryEmitLoadCustomFormatter,
      bool useGetUninitializedObject,
      int firstArgIndex)
    {
      if (info.IsClass && info.BestmatchConstructor == (ConstructorInfo) null && (!useGetUninitializedObject || !info.IsConcreteClass))
      {
        il.Emit(OpCodes.Ldstr, "generated serializer for " + type.Name + " does not support deserialize.");
        il.Emit(OpCodes.Newobj, DynamicObjectTypeBuilder.EmitInfo.InvalidOperationExceptionConstructor);
        il.Emit(OpCodes.Throw);
      }
      else
      {
        ArgumentField argReader = new ArgumentField(il, firstArgIndex, false);
        ArgumentField argResolver = new ArgumentField(il, firstArgIndex + 1, false);
        Label label1 = il.DefineLabel();
        argReader.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadIsNull);
        il.Emit(OpCodes.Brfalse_S, label1);
        if (info.IsClass)
        {
          il.Emit(OpCodes.Ldnull);
          il.Emit(OpCodes.Ret);
        }
        else
        {
          il.Emit(OpCodes.Ldstr, "json value is null, struct is not supported");
          il.Emit(OpCodes.Newobj, DynamicObjectTypeBuilder.EmitInfo.InvalidOperationExceptionConstructor);
          il.Emit(OpCodes.Throw);
        }
        il.MarkLabel(label1);
        argReader.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadIsBeginObjectWithVerify);
        bool isSideEffectFreeType = true;
        if (info.BestmatchConstructor != (ConstructorInfo) null)
        {
          isSideEffectFreeType = DynamicObjectTypeBuilder.IsSideEffectFreeConstructorType(info.BestmatchConstructor);
          if (((IEnumerable<MetaMember>) info.Members).Any<MetaMember>((Func<MetaMember, bool>) (x => !x.IsReadable && x.IsWritable)))
            isSideEffectFreeType = false;
        }
        DynamicObjectTypeBuilder.DeserializeInfo[] infoList = ((IEnumerable<MetaMember>) info.Members).Select<MetaMember, DynamicObjectTypeBuilder.DeserializeInfo>((Func<MetaMember, DynamicObjectTypeBuilder.DeserializeInfo>) (item => new DynamicObjectTypeBuilder.DeserializeInfo()
        {
          MemberInfo = item,
          LocalField = il.DeclareLocal(item.Type),
          IsDeserializedField = isSideEffectFreeType ? (LocalBuilder) null : il.DeclareLocal(typeof (bool))
        })).ToArray<DynamicObjectTypeBuilder.DeserializeInfo>();
        LocalBuilder local1 = il.DeclareLocal(typeof (int));
        AutomataDictionary automataDictionary = new AutomataDictionary();
        for (int index = 0; index < info.Members.Length; ++index)
          automataDictionary.Add(Utf8Json.JsonWriter.GetEncodedPropertyNameWithoutQuotation(info.Members[index].Name), index);
        LocalBuilder local2 = il.DeclareLocal(typeof (byte[]));
        LocalBuilder local3 = il.DeclareLocal(typeof (byte).MakeByRefType(), true);
        LocalBuilder local4 = il.DeclareLocal(typeof (ArraySegment<byte>));
        LocalBuilder key = il.DeclareLocal(typeof (ulong));
        LocalBuilder localBuilder1 = il.DeclareLocal(typeof (byte*));
        LocalBuilder localBuilder2 = il.DeclareLocal(typeof (int));
        argReader.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonReader.GetBufferUnsafe);
        il.EmitStloc(local2);
        il.EmitLdloc(local2);
        il.EmitLdc_I4(0);
        il.Emit(OpCodes.Ldelema, typeof (byte));
        il.EmitStloc(local3);
        Label continueWhile = il.DefineLabel();
        Label label2 = il.DefineLabel();
        Label readNext = il.DefineLabel();
        il.MarkLabel(continueWhile);
        argReader.EmitLoad();
        il.EmitLdloca(local1);
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadIsEndObjectWithSkipValueSeparator);
        il.Emit(OpCodes.Brtrue, label2);
        argReader.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadPropertyNameSegmentUnsafe);
        il.EmitStloc(local4);
        il.EmitLdloc(local3);
        il.Emit(OpCodes.Conv_I);
        il.EmitLdloca(local4);
        il.EmitCall(RuntimeReflectionExtensions.GetRuntimeProperty(typeof (ArraySegment<byte>), "Offset").GetGetMethod());
        il.Emit(OpCodes.Add);
        il.EmitStloc(localBuilder1);
        il.EmitLdloca(local4);
        il.EmitCall(RuntimeReflectionExtensions.GetRuntimeProperty(typeof (ArraySegment<byte>), "Count").GetGetMethod());
        il.EmitStloc(localBuilder2);
        il.EmitLdloc(localBuilder2);
        il.Emit(OpCodes.Brfalse, readNext);
        automataDictionary.EmitMatch(il, localBuilder1, localBuilder2, key, (Action<KeyValuePair<string, int>>) (x =>
        {
          int index = x.Value;
          if (infoList[index].MemberInfo != null)
          {
            DynamicObjectTypeBuilder.EmitDeserializeValue(il, infoList[index], index, tryEmitLoadCustomFormatter, argReader, argResolver);
            if (!isSideEffectFreeType)
            {
              il.EmitTrue();
              il.EmitStloc(infoList[index].IsDeserializedField);
            }
            il.Emit(OpCodes.Br, continueWhile);
          }
          else
            il.Emit(OpCodes.Br, readNext);
        }), (Action) (() => il.Emit(OpCodes.Br, readNext)));
        il.MarkLabel(readNext);
        argReader.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadNextBlock);
        il.Emit(OpCodes.Br, continueWhile);
        il.MarkLabel(label2);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Conv_U);
        il.EmitStloc(local3);
        LocalBuilder local5 = DynamicObjectTypeBuilder.EmitNewObject(il, type, info, infoList, isSideEffectFreeType);
        if (local5 != null)
          il.Emit(OpCodes.Ldloc, local5);
        il.Emit(OpCodes.Ret);
      }
    }

    private static void EmitDeserializeValue(
      ILGenerator il,
      DynamicObjectTypeBuilder.DeserializeInfo info,
      int index,
      Func<int, MetaMember, bool> tryEmitLoadCustomFormatter,
      ArgumentField reader,
      ArgumentField argResolver)
    {
      MetaMember memberInfo = info.MemberInfo;
      Type type = memberInfo.Type;
      if (tryEmitLoadCustomFormatter(index, memberInfo))
      {
        reader.EmitLoad();
        argResolver.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Deserialize(type));
      }
      else if (DynamicObjectTypeBuilder.jsonPrimitiveTypes.Contains(type))
      {
        reader.EmitLoad();
        il.EmitCall(IntrospectionExtensions.GetTypeInfo(typeof (Utf8Json.JsonReader)).GetDeclaredMethods("Read" + type.Name).OrderByDescending<MethodInfo, int>((Func<MethodInfo, int>) (x => x.GetParameters().Length)).First<MethodInfo>());
      }
      else
      {
        argResolver.EmitLoad();
        il.Emit(OpCodes.Call, DynamicObjectTypeBuilder.EmitInfo.GetFormatterWithVerify.MakeGenericMethod(type));
        reader.EmitLoad();
        argResolver.EmitLoad();
        il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Deserialize(type));
      }
      il.EmitStloc(info.LocalField);
    }

    private static LocalBuilder EmitNewObject(
      ILGenerator il,
      Type type,
      MetaType info,
      DynamicObjectTypeBuilder.DeserializeInfo[] members,
      bool isSideEffectFreeType)
    {
      if (info.IsClass)
      {
        LocalBuilder local = (LocalBuilder) null;
        if (!isSideEffectFreeType)
          local = il.DeclareLocal(type);
        if (info.BestmatchConstructor != (ConstructorInfo) null)
        {
          foreach (MetaMember constructorParameter in info.ConstructorParameters)
          {
            MetaMember item = constructorParameter;
            DynamicObjectTypeBuilder.DeserializeInfo deserializeInfo = ((IEnumerable<DynamicObjectTypeBuilder.DeserializeInfo>) members).First<DynamicObjectTypeBuilder.DeserializeInfo>((Func<DynamicObjectTypeBuilder.DeserializeInfo, bool>) (x => x.MemberInfo == item));
            il.EmitLdloc(deserializeInfo.LocalField);
          }
          il.Emit(OpCodes.Newobj, info.BestmatchConstructor);
        }
        else
        {
          il.Emit(OpCodes.Ldtoken, type);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetTypeFromHandle);
          il.EmitCall(DynamicObjectTypeBuilder.EmitInfo.GetUninitializedObject);
        }
        if (!isSideEffectFreeType)
          il.EmitStloc(local);
        foreach (DynamicObjectTypeBuilder.DeserializeInfo deserializeInfo in ((IEnumerable<DynamicObjectTypeBuilder.DeserializeInfo>) members).Where<DynamicObjectTypeBuilder.DeserializeInfo>((Func<DynamicObjectTypeBuilder.DeserializeInfo, bool>) (x => x.MemberInfo != null && x.MemberInfo.IsWritable)))
        {
          if (isSideEffectFreeType)
          {
            il.Emit(OpCodes.Dup);
            il.EmitLdloc(deserializeInfo.LocalField);
            deserializeInfo.MemberInfo.EmitStoreValue(il);
          }
          else
          {
            Label label = il.DefineLabel();
            il.EmitLdloc(deserializeInfo.IsDeserializedField);
            il.Emit(OpCodes.Brfalse, label);
            il.EmitLdloc(local);
            il.EmitLdloc(deserializeInfo.LocalField);
            deserializeInfo.MemberInfo.EmitStoreValue(il);
            il.MarkLabel(label);
          }
        }
        return local;
      }
      LocalBuilder local1 = il.DeclareLocal(type);
      if (info.BestmatchConstructor == (ConstructorInfo) null)
      {
        il.Emit(OpCodes.Ldloca, local1);
        il.Emit(OpCodes.Initobj, type);
      }
      else
      {
        foreach (MetaMember constructorParameter in info.ConstructorParameters)
        {
          MetaMember item = constructorParameter;
          DynamicObjectTypeBuilder.DeserializeInfo deserializeInfo = ((IEnumerable<DynamicObjectTypeBuilder.DeserializeInfo>) members).First<DynamicObjectTypeBuilder.DeserializeInfo>((Func<DynamicObjectTypeBuilder.DeserializeInfo, bool>) (x => x.MemberInfo == item));
          il.EmitLdloc(deserializeInfo.LocalField);
        }
        il.Emit(OpCodes.Newobj, info.BestmatchConstructor);
        il.Emit(OpCodes.Stloc, local1);
      }
      foreach (DynamicObjectTypeBuilder.DeserializeInfo deserializeInfo in ((IEnumerable<DynamicObjectTypeBuilder.DeserializeInfo>) members).Where<DynamicObjectTypeBuilder.DeserializeInfo>((Func<DynamicObjectTypeBuilder.DeserializeInfo, bool>) (x => x.MemberInfo != null && x.MemberInfo.IsWritable)))
      {
        if (isSideEffectFreeType)
        {
          il.EmitLdloca(local1);
          il.EmitLdloc(deserializeInfo.LocalField);
          deserializeInfo.MemberInfo.EmitStoreValue(il);
        }
        else
        {
          Label label = il.DefineLabel();
          il.EmitLdloc(deserializeInfo.IsDeserializedField);
          il.Emit(OpCodes.Brfalse, label);
          il.EmitLdloca(local1);
          il.EmitLdloc(deserializeInfo.LocalField);
          deserializeInfo.MemberInfo.EmitStoreValue(il);
          il.MarkLabel(label);
        }
      }
      return local1;
    }

    private static bool IsSideEffectFreeConstructorType(ConstructorInfo ctorInfo)
    {
      MethodBody methodBody = ctorInfo.GetMethodBody();
      if (methodBody == null)
        return false;
      byte[] ilAsByteArray = methodBody.GetILAsByteArray();
      if (ilAsByteArray == null)
        return false;
      List<OpCode> opCodeList = new List<OpCode>();
      using (ILStreamReader ilStreamReader = new ILStreamReader(ilAsByteArray))
      {
        while (!ilStreamReader.EndOfStream)
        {
          OpCode opCode = ilStreamReader.ReadOpCode();
          if (opCode != OpCodes.Nop && opCode != OpCodes.Ldloc_0 && (opCode != OpCodes.Ldloc_S && opCode != OpCodes.Stloc_0) && (opCode != OpCodes.Stloc_S && opCode != OpCodes.Blt && (opCode != OpCodes.Blt_S && opCode != OpCodes.Bgt)) && opCode != OpCodes.Bgt_S)
          {
            opCodeList.Add(opCode);
            if (opCodeList.Count == 4)
              break;
          }
        }
      }
      if (opCodeList.Count != 3 || !(opCodeList[0] == OpCodes.Ldarg_0) || (!(opCodeList[1] == OpCodes.Call) || !(opCodeList[2] == OpCodes.Ret)))
        return false;
      if (ctorInfo.DeclaringType.BaseType == typeof (object))
        return true;
      ConstructorInfo constructor = ctorInfo.DeclaringType.BaseType.GetConstructor(Type.EmptyTypes);
      return !(constructor == (ConstructorInfo) null) && DynamicObjectTypeBuilder.IsSideEffectFreeConstructorType(constructor);
    }

    private static bool TryGetInterfaceEnumerableElementType(Type type, out Type elementType)
    {
      foreach (Type type1 in type.GetInterfaces())
      {
        if (type1.IsGenericType && type1.GetGenericTypeDefinition() == typeof (IEnumerable<>))
        {
          Type[] genericArguments = type1.GetGenericArguments();
          elementType = genericArguments[0];
          return true;
        }
      }
      elementType = (Type) null;
      return false;
    }

    private struct DeserializeInfo
    {
      public MetaMember MemberInfo;
      public LocalBuilder LocalField;
      public LocalBuilder IsDeserializedField;
    }

    internal static class EmitInfo
    {
      public static readonly ConstructorInfo ObjectCtor = IntrospectionExtensions.GetTypeInfo(typeof (object)).get_DeclaredConstructors().First<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.GetParameters().Length == 0));
      public static readonly MethodInfo GetFormatterWithVerify = RuntimeReflectionExtensions.GetRuntimeMethod(typeof (JsonFormatterResolverExtensions), nameof (GetFormatterWithVerify), new Type[1]
      {
        typeof (IJsonFormatterResolver)
      });
      public static readonly ConstructorInfo InvalidOperationExceptionConstructor = IntrospectionExtensions.GetTypeInfo(typeof (InvalidOperationException)).get_DeclaredConstructors().First<ConstructorInfo>((Func<ConstructorInfo, bool>) (x =>
      {
        ParameterInfo[] parameters = x.GetParameters();
        return parameters.Length == 1 && parameters[0].ParameterType == typeof (string);
      }));
      public static readonly MethodInfo GetTypeFromHandle = ExpressionUtility.GetMethodInfo<Type>((Expression<Func<Type>>) (() => Type.GetTypeFromHandle(new RuntimeTypeHandle())));
      public static readonly MethodInfo TypeGetProperty = ExpressionUtility.GetMethodInfo<Type, PropertyInfo>((Expression<Func<Type, PropertyInfo>>) (t => t.GetProperty(default (string), BindingFlags.Default)));
      public static readonly MethodInfo TypeGetField = ExpressionUtility.GetMethodInfo<Type, FieldInfo>((Expression<Func<Type, FieldInfo>>) (t => t.GetField(default (string), BindingFlags.Default)));
      public static readonly MethodInfo GetCustomAttributeJsonFormatterAttribute = ExpressionUtility.GetMethodInfo<JsonFormatterAttribute>((Expression<Func<JsonFormatterAttribute>>) (() => CustomAttributeExtensions.GetCustomAttribute<JsonFormatterAttribute>(default (MemberInfo), false)));
      public static readonly MethodInfo ActivatorCreateInstance = ExpressionUtility.GetMethodInfo<object>((Expression<Func<object>>) (() => Activator.CreateInstance(default (Type), default (object[]))));
      public static readonly MethodInfo GetUninitializedObject = ExpressionUtility.GetMethodInfo<object>((Expression<Func<object>>) (() => FormatterServices.GetUninitializedObject(default (Type))));
      public static readonly MethodInfo GetTypeMethod;
      public static readonly MethodInfo TypeEquals;
      public static readonly MethodInfo NongenericSerialize;

      public static MethodInfo Serialize(Type type)
      {
        return RuntimeReflectionExtensions.GetRuntimeMethod(typeof (IJsonFormatter<>).MakeGenericType(type), nameof (Serialize), new Type[3]
        {
          typeof (Utf8Json.JsonWriter).MakeByRefType(),
          type,
          typeof (IJsonFormatterResolver)
        });
      }

      public static MethodInfo Deserialize(Type type)
      {
        return RuntimeReflectionExtensions.GetRuntimeMethod(typeof (IJsonFormatter<>).MakeGenericType(type), nameof (Deserialize), new Type[2]
        {
          typeof (Utf8Json.JsonReader).MakeByRefType(),
          typeof (IJsonFormatterResolver)
        });
      }

      public static MethodInfo GetNullableHasValue(Type type)
      {
        return RuntimeReflectionExtensions.GetRuntimeProperty(typeof (Nullable<>).MakeGenericType(type), "HasValue").GetGetMethod();
      }

      static EmitInfo()
      {
        ParameterExpression parameterExpression;
        // ISSUE: method reference
        DynamicObjectTypeBuilder.EmitInfo.GetTypeMethod = ExpressionUtility.GetMethodInfo<object, Type>(Expression.Lambda<Func<object, Type>>((Expression) Expression.Call(o, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.GetType)), (Expression[]) Array.Empty<Expression>()), parameterExpression));
        DynamicObjectTypeBuilder.EmitInfo.TypeEquals = ExpressionUtility.GetMethodInfo<Type, bool>((Expression<Func<Type, bool>>) (t => t.Equals(default (Type))));
        DynamicObjectTypeBuilder.EmitInfo.NongenericSerialize = ExpressionUtility.GetMethodInfo<Utf8Json.JsonWriter>((Expression<Action<Utf8Json.JsonWriter>>) (writer => JsonSerializer.NonGeneric.Serialize(default (Type), writer, default (object), default (IJsonFormatterResolver))));
      }

      internal static class JsonWriter
      {
        public static readonly MethodInfo GetEncodedPropertyNameWithBeginObject = ExpressionUtility.GetMethodInfo<byte[]>((Expression<Func<byte[]>>) (() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithBeginObject(default (string))));
        public static readonly MethodInfo GetEncodedPropertyNameWithPrefixValueSeparator = ExpressionUtility.GetMethodInfo<byte[]>((Expression<Func<byte[]>>) (() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(default (string))));
        public static readonly MethodInfo GetEncodedPropertyNameWithoutQuotation = ExpressionUtility.GetMethodInfo<byte[]>((Expression<Func<byte[]>>) (() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithoutQuotation(default (string))));
        public static readonly MethodInfo GetEncodedPropertyName = ExpressionUtility.GetMethodInfo<byte[]>((Expression<Func<byte[]>>) (() => Utf8Json.JsonWriter.GetEncodedPropertyName(default (string))));
        public static readonly MethodInfo WriteNull;
        public static readonly MethodInfo WriteRaw;
        public static readonly MethodInfo WriteBeginObject;
        public static readonly MethodInfo WriteEndObject;
        public static readonly MethodInfo WriteValueSeparator;

        static JsonWriter()
        {
          ParameterExpression parameterExpression1;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteNull = ExpressionUtility.GetMethodInfo<Utf8Json.JsonWriter>(Expression.Lambda<Action<Utf8Json.JsonWriter>>((Expression) Expression.Call(writer, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonWriter.WriteNull)), (Expression[]) Array.Empty<Expression>()), parameterExpression1));
          DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteRaw = ExpressionUtility.GetMethodInfo<Utf8Json.JsonWriter>((Expression<Action<Utf8Json.JsonWriter>>) (writer => writer.WriteRaw(default (byte[]))));
          ParameterExpression parameterExpression2;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteBeginObject = ExpressionUtility.GetMethodInfo<Utf8Json.JsonWriter>(Expression.Lambda<Action<Utf8Json.JsonWriter>>((Expression) Expression.Call(writer, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonWriter.WriteBeginObject)), (Expression[]) Array.Empty<Expression>()), parameterExpression2));
          ParameterExpression parameterExpression3;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteEndObject = ExpressionUtility.GetMethodInfo<Utf8Json.JsonWriter>(Expression.Lambda<Action<Utf8Json.JsonWriter>>((Expression) Expression.Call(writer, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonWriter.WriteEndObject)), (Expression[]) Array.Empty<Expression>()), parameterExpression3));
          ParameterExpression parameterExpression4;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonWriter.WriteValueSeparator = ExpressionUtility.GetMethodInfo<Utf8Json.JsonWriter>(Expression.Lambda<Action<Utf8Json.JsonWriter>>((Expression) Expression.Call(writer, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonWriter.WriteValueSeparator)), (Expression[]) Array.Empty<Expression>()), parameterExpression4));
        }
      }

      internal static class JsonReader
      {
        public static readonly MethodInfo ReadIsNull;
        public static readonly MethodInfo ReadIsBeginObjectWithVerify;
        public static readonly MethodInfo ReadIsEndObjectWithSkipValueSeparator;
        public static readonly MethodInfo ReadPropertyNameSegmentUnsafe;
        public static readonly MethodInfo ReadNextBlock;
        public static readonly MethodInfo GetBufferUnsafe;
        public static readonly MethodInfo GetCurrentOffsetUnsafe;

        static JsonReader()
        {
          ParameterExpression parameterExpression1;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadIsNull = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader, bool>(Expression.Lambda<Func<Utf8Json.JsonReader, bool>>((Expression) Expression.Call(reader, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonReader.ReadIsNull)), (Expression[]) Array.Empty<Expression>()), parameterExpression1));
          ParameterExpression parameterExpression2;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadIsBeginObjectWithVerify = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader>(Expression.Lambda<Action<Utf8Json.JsonReader>>((Expression) Expression.Call(reader, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonReader.ReadIsBeginObjectWithVerify)), (Expression[]) Array.Empty<Expression>()), parameterExpression2));
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadIsEndObjectWithSkipValueSeparator = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader, int, bool>((Expression<Func<Utf8Json.JsonReader, int, bool>>) ((reader, count) => reader.ReadIsEndObjectWithSkipValueSeparator(count)));
          ParameterExpression parameterExpression3;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadPropertyNameSegmentUnsafe = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader, ArraySegment<byte>>(Expression.Lambda<Func<Utf8Json.JsonReader, ArraySegment<byte>>>((Expression) Expression.Call(reader, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonReader.ReadPropertyNameSegmentRaw)), (Expression[]) Array.Empty<Expression>()), parameterExpression3));
          ParameterExpression parameterExpression4;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.ReadNextBlock = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader>(Expression.Lambda<Action<Utf8Json.JsonReader>>((Expression) Expression.Call(reader, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonReader.ReadNextBlock)), (Expression[]) Array.Empty<Expression>()), parameterExpression4));
          ParameterExpression parameterExpression5;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.GetBufferUnsafe = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader, byte[]>(Expression.Lambda<Func<Utf8Json.JsonReader, byte[]>>((Expression) Expression.Call(reader, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonReader.GetBufferUnsafe)), (Expression[]) Array.Empty<Expression>()), parameterExpression5));
          ParameterExpression parameterExpression6;
          // ISSUE: method reference
          DynamicObjectTypeBuilder.EmitInfo.JsonReader.GetCurrentOffsetUnsafe = ExpressionUtility.GetMethodInfo<Utf8Json.JsonReader, int>(Expression.Lambda<Func<Utf8Json.JsonReader, int>>((Expression) Expression.Call(reader, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Utf8Json.JsonReader.GetCurrentOffsetUnsafe)), (Expression[]) Array.Empty<Expression>()), parameterExpression6));
        }
      }

      internal static class JsonFormatterAttr
      {
        internal static readonly MethodInfo FormatterType = ExpressionUtility.GetPropertyInfo<JsonFormatterAttribute, Type>((Expression<Func<JsonFormatterAttribute, Type>>) (attr => attr.FormatterType)).GetGetMethod();
        internal static readonly MethodInfo Arguments = ExpressionUtility.GetPropertyInfo<JsonFormatterAttribute, object[]>((Expression<Func<JsonFormatterAttribute, object[]>>) (attr => attr.Arguments)).GetGetMethod();
      }
    }

    internal class Utf8JsonDynamicObjectResolverException : Exception
    {
      public Utf8JsonDynamicObjectResolverException(string message)
        : base(message)
      {
      }
    }
  }
}
