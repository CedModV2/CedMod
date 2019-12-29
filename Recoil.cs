// Decompiled with JetBrains decompiler
// Type: Recoil
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class Recoil : MonoBehaviour
{
  public static Recoil singleton;
  public GameObject plyCam;
  public Vector3 animatedRotation;
  public float positionOffset;

  public void DoRecoil(RecoilProperties r, float multip)
  {
  }

  private void Start()
  {
  }

  public static void StaticDoRecoil(RecoilProperties r, float multip)
  {
  }
}
