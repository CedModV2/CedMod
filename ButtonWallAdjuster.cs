// Decompiled with JetBrains decompiler
// Type: ButtonWallAdjuster
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class ButtonWallAdjuster : MonoBehaviour
{
  public float offset = 0.1f;
  public bool onAwake;
  private bool _adjusted;

  private void Start()
  {
    if (!this.onAwake)
      return;
    this.Adjust();
  }

  public void Adjust()
  {
    if (this._adjusted && !this.onAwake)
      return;
    this._adjusted = true;
    this.transform.position += this.transform.up;
    RaycastHit hitInfo;
    if (!Physics.Raycast(new Ray(this.transform.position, -this.transform.up), out hitInfo, 2.5f))
      return;
    this.transform.position = hitInfo.point;
    this.transform.position -= this.offset * 0.1f * this.transform.up;
  }
}
