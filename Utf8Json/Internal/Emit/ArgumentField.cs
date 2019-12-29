// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.ArgumentField
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection.Emit;

namespace Utf8Json.Internal.Emit
{
  internal struct ArgumentField
  {
    private readonly int i;
    private readonly bool @ref;
    private readonly ILGenerator il;

    public ArgumentField(ILGenerator il, int i, bool @ref = false)
    {
      this.il = il;
      this.i = i;
      this.@ref = @ref;
    }

    public ArgumentField(ILGenerator il, int i, Type type)
    {
      this.il = il;
      this.i = i;
      this.@ref = !type.IsClass && !type.IsInterface && !type.IsAbstract;
    }

    public void EmitLoad()
    {
      if (this.@ref)
        this.il.EmitLdarga(this.i);
      else
        this.il.EmitLdarg(this.i);
    }

    public void EmitStore()
    {
      this.il.EmitStarg(this.i);
    }
  }
}
