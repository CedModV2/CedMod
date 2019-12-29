// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.ILGeneratorExtensions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Utf8Json.Internal.Emit
{
  internal static class ILGeneratorExtensions
  {
    public static void EmitLdloc(this ILGenerator il, int index)
    {
      switch (index)
      {
        case 0:
          il.Emit(OpCodes.Ldloc_0);
          break;
        case 1:
          il.Emit(OpCodes.Ldloc_1);
          break;
        case 2:
          il.Emit(OpCodes.Ldloc_2);
          break;
        case 3:
          il.Emit(OpCodes.Ldloc_3);
          break;
        default:
          if (index <= (int) byte.MaxValue)
          {
            il.Emit(OpCodes.Ldloc_S, (byte) index);
            break;
          }
          il.Emit(OpCodes.Ldloc, (short) index);
          break;
      }
    }

    public static void EmitLdloc(this ILGenerator il, LocalBuilder local)
    {
      il.EmitLdloc(local.LocalIndex);
    }

    public static void EmitStloc(this ILGenerator il, int index)
    {
      switch (index)
      {
        case 0:
          il.Emit(OpCodes.Stloc_0);
          break;
        case 1:
          il.Emit(OpCodes.Stloc_1);
          break;
        case 2:
          il.Emit(OpCodes.Stloc_2);
          break;
        case 3:
          il.Emit(OpCodes.Stloc_3);
          break;
        default:
          if (index <= (int) byte.MaxValue)
          {
            il.Emit(OpCodes.Stloc_S, (byte) index);
            break;
          }
          il.Emit(OpCodes.Stloc, (short) index);
          break;
      }
    }

    public static void EmitStloc(this ILGenerator il, LocalBuilder local)
    {
      il.EmitStloc(local.LocalIndex);
    }

    public static void EmitLdloca(this ILGenerator il, int index)
    {
      if (index <= (int) byte.MaxValue)
        il.Emit(OpCodes.Ldloca_S, (byte) index);
      else
        il.Emit(OpCodes.Ldloca, (short) index);
    }

    public static void EmitLdloca(this ILGenerator il, LocalBuilder local)
    {
      il.EmitLdloca(local.LocalIndex);
    }

    public static void EmitTrue(this ILGenerator il)
    {
      il.EmitBoolean(true);
    }

    public static void EmitFalse(this ILGenerator il)
    {
      il.EmitBoolean(false);
    }

    public static void EmitBoolean(this ILGenerator il, bool value)
    {
      il.EmitLdc_I4(value ? 1 : 0);
    }

    public static void EmitLdc_I4(this ILGenerator il, int value)
    {
      switch (value)
      {
        case -1:
          il.Emit(OpCodes.Ldc_I4_M1);
          break;
        case 0:
          il.Emit(OpCodes.Ldc_I4_0);
          break;
        case 1:
          il.Emit(OpCodes.Ldc_I4_1);
          break;
        case 2:
          il.Emit(OpCodes.Ldc_I4_2);
          break;
        case 3:
          il.Emit(OpCodes.Ldc_I4_3);
          break;
        case 4:
          il.Emit(OpCodes.Ldc_I4_4);
          break;
        case 5:
          il.Emit(OpCodes.Ldc_I4_5);
          break;
        case 6:
          il.Emit(OpCodes.Ldc_I4_6);
          break;
        case 7:
          il.Emit(OpCodes.Ldc_I4_7);
          break;
        case 8:
          il.Emit(OpCodes.Ldc_I4_8);
          break;
        default:
          if (value >= (int) sbyte.MinValue && value <= (int) sbyte.MaxValue)
          {
            il.Emit(OpCodes.Ldc_I4_S, (sbyte) value);
            break;
          }
          il.Emit(OpCodes.Ldc_I4, value);
          break;
      }
    }

    public static void EmitUnboxOrCast(this ILGenerator il, Type type)
    {
      if (type.IsValueType)
        il.Emit(OpCodes.Unbox_Any, type);
      else
        il.Emit(OpCodes.Castclass, type);
    }

    public static void EmitBoxOrDoNothing(this ILGenerator il, Type type)
    {
      if (!type.IsValueType)
        return;
      il.Emit(OpCodes.Box, type);
    }

    public static void EmitLdarg(this ILGenerator il, int index)
    {
      switch (index)
      {
        case 0:
          il.Emit(OpCodes.Ldarg_0);
          break;
        case 1:
          il.Emit(OpCodes.Ldarg_1);
          break;
        case 2:
          il.Emit(OpCodes.Ldarg_2);
          break;
        case 3:
          il.Emit(OpCodes.Ldarg_3);
          break;
        default:
          if (index <= (int) byte.MaxValue)
          {
            il.Emit(OpCodes.Ldarg_S, (byte) index);
            break;
          }
          il.Emit(OpCodes.Ldarg, index);
          break;
      }
    }

    public static void EmitLoadThis(this ILGenerator il)
    {
      il.EmitLdarg(0);
    }

    public static void EmitLdarga(this ILGenerator il, int index)
    {
      if (index <= (int) byte.MaxValue)
        il.Emit(OpCodes.Ldarga_S, (byte) index);
      else
        il.Emit(OpCodes.Ldarga, index);
    }

    public static void EmitStarg(this ILGenerator il, int index)
    {
      if (index <= (int) byte.MaxValue)
        il.Emit(OpCodes.Starg_S, (byte) index);
      else
        il.Emit(OpCodes.Starg, index);
    }

    public static void EmitPop(this ILGenerator il, int count)
    {
      for (int index = 0; index < count; ++index)
        il.Emit(OpCodes.Pop);
    }

    public static void EmitCall(this ILGenerator il, MethodInfo methodInfo)
    {
      if (methodInfo.IsFinal || !methodInfo.IsVirtual)
        il.Emit(OpCodes.Call, methodInfo);
      else
        il.Emit(OpCodes.Callvirt, methodInfo);
    }

    public static void EmitLdfld(this ILGenerator il, FieldInfo fieldInfo)
    {
      il.Emit(OpCodes.Ldfld, fieldInfo);
    }

    public static void EmitLdsfld(this ILGenerator il, FieldInfo fieldInfo)
    {
      il.Emit(OpCodes.Ldsfld, fieldInfo);
    }

    public static void EmitRet(this ILGenerator il)
    {
      il.Emit(OpCodes.Ret);
    }

    public static void EmitIntZeroReturn(this ILGenerator il)
    {
      il.EmitLdc_I4(0);
      il.Emit(OpCodes.Ret);
    }

    public static void EmitNullReturn(this ILGenerator il)
    {
      il.Emit(OpCodes.Ldnull);
      il.Emit(OpCodes.Ret);
    }

    public static void EmitULong(this ILGenerator il, ulong value)
    {
      il.Emit(OpCodes.Ldc_I8, (long) value);
    }

    public static void EmitThrowNotimplemented(this ILGenerator il)
    {
      il.Emit(OpCodes.Newobj, IntrospectionExtensions.GetTypeInfo(typeof (NotImplementedException)).get_DeclaredConstructors().First<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.GetParameters().Length == 0)));
      il.Emit(OpCodes.Throw);
    }

    public static void EmitIncrementFor(
      this ILGenerator il,
      LocalBuilder conditionGreater,
      Action<LocalBuilder> emitBody)
    {
      Label label1 = il.DefineLabel();
      Label label2 = il.DefineLabel();
      LocalBuilder local = il.DeclareLocal(typeof (int));
      il.EmitLdc_I4(0);
      il.EmitStloc(local);
      il.Emit(OpCodes.Br, label2);
      il.MarkLabel(label1);
      emitBody(local);
      il.EmitLdloc(local);
      il.EmitLdc_I4(1);
      il.Emit(OpCodes.Add);
      il.EmitStloc(local);
      il.MarkLabel(label2);
      il.EmitLdloc(local);
      il.EmitLdloc(conditionGreater);
      il.Emit(OpCodes.Blt, label1);
    }
  }
}
