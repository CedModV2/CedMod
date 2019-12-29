// Decompiled with JetBrains decompiler
// Type: Dissonance.PublicAPIAttribute
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Dissonance
{
  [MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
  internal sealed class PublicAPIAttribute : Attribute
  {
    public PublicAPIAttribute()
    {
    }

    public PublicAPIAttribute([NotNull] string comment)
    {
      this.Comment = comment;
    }

    [CanBeNull]
    public string Comment { get; private set; }
  }
}
