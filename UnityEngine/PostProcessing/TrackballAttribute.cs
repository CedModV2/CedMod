// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.TrackballAttribute
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace UnityEngine.PostProcessing
{
  public sealed class TrackballAttribute : PropertyAttribute
  {
    public readonly string method;

    public TrackballAttribute(string method)
    {
      this.method = method;
    }
  }
}
