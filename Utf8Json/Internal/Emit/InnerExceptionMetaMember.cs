// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.InnerExceptionMetaMember
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Utf8Json.Internal.Emit
{
  internal class InnerExceptionMetaMember : MetaMember
  {
    private static readonly MethodInfo getInnerException = ExpressionUtility.GetPropertyInfo<Exception, Exception>((Expression<Func<Exception, Exception>>) (ex => ex.InnerException)).GetGetMethod();
    private static readonly MethodInfo nongenericSerialize = ExpressionUtility.GetMethodInfo<JsonWriter>((Expression<Action<JsonWriter>>) (writer => JsonSerializer.NonGeneric.Serialize(writer, default (object), default (IJsonFormatterResolver))));
    internal ArgumentField argWriter;
    internal ArgumentField argValue;
    internal ArgumentField argResolver;

    public InnerExceptionMetaMember(string name)
      : base(typeof (Exception), name, name, false, true)
    {
    }

    public override void EmitLoadValue(ILGenerator il)
    {
      il.Emit(OpCodes.Callvirt, InnerExceptionMetaMember.getInnerException);
    }

    public override void EmitStoreValue(ILGenerator il)
    {
      throw new NotSupportedException();
    }

    public void EmitSerializeDirectly(ILGenerator il)
    {
      this.argWriter.EmitLoad();
      this.argValue.EmitLoad();
      il.Emit(OpCodes.Callvirt, InnerExceptionMetaMember.getInnerException);
      this.argResolver.EmitLoad();
      il.EmitCall(InnerExceptionMetaMember.nongenericSerialize);
    }
  }
}
