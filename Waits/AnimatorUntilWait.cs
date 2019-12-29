// Decompiled with JetBrains decompiler
// Type: Waits.AnimatorUntilWait
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace Waits
{
  public class AnimatorUntilWait : UntilWait
  {
    public Animator animator;

    protected override bool Predicate()
    {
      return (double) this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0;
    }
  }
}
