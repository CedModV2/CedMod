// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.StringConstantValueMetaMember
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection.Emit;

namespace Utf8Json.Internal.Emit
{
  internal class StringConstantValueMetaMember : MetaMember
  {
    private readonly string constant;

    public StringConstantValueMetaMember(string name, string constant)
      : base(typeof (string), name, name, false, true)
    {
      this.constant = constant;
    }

    public override void EmitLoadValue(ILGenerator il)
    {
      il.Emit(OpCodes.Pop);
      il.Emit(OpCodes.Ldstr, this.constant);
    }

    public override void EmitStoreValue(ILGenerator il)
    {
      throw new NotSupportedException();
    }
  }
}
