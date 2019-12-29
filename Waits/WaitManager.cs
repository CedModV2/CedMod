// Decompiled with JetBrains decompiler
// Type: Waits.WaitManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Waits
{
  public abstract class WaitManager : MonoBehaviour
  {
    protected Wait[] waits;
    protected CoroutineHandle[] waitHandles;

    protected virtual void Awake()
    {
      this.waits = this.GetComponents<Wait>();
      this.waitHandles = new CoroutineHandle[this.waits.Length];
    }

    protected void StartAll()
    {
      for (int index = 0; index < this.waits.Length; ++index)
        this.waitHandles[index] = Timing.RunCoroutine(this.waits[index]._Run());
    }

    public abstract IEnumerator<float> _Run();
  }
}
