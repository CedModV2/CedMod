// Utf8Json.Internal.Emit.InnerExceptionMetaMember
using System;
using System.Reflection;
using System.Reflection.Emit;
using Utf8Json;
using Utf8Json.Internal.Emit;

internal class InnerExceptionMetaMember : MetaMember
{
    private static readonly MethodInfo getInnerException = ExpressionUtility.GetPropertyInfo((Exception ex) => ex.InnerException).GetGetMethod();

    private static readonly MethodInfo nongenericSerialize = ExpressionUtility.GetMethodInfo((JsonWriter writer) => JsonSerializer.NonGeneric.Serialize(ref writer, null, null));

    internal ArgumentField argWriter;

    internal ArgumentField argValue;

    internal ArgumentField argResolver;

    public InnerExceptionMetaMember(string name)
        : base(typeof(Exception), name, name, isWritable: false, isReadable: true)
    {
    }

    public override void EmitLoadValue(ILGenerator il)
    {
        il.Emit(OpCodes.Callvirt, getInnerException);
    }

    public override void EmitStoreValue(ILGenerator il)
    {
        throw new NotSupportedException();
    }

    public void EmitSerializeDirectly(ILGenerator il)
    {
        argWriter.EmitLoad();
        argValue.EmitLoad();
        il.Emit(OpCodes.Callvirt, getInnerException);
        argResolver.EmitLoad();
        il.EmitCall(nongenericSerialize);
    }
}