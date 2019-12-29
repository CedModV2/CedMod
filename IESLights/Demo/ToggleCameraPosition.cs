// Decompiled with JetBrains decompiler
// Type: IESLights.Demo.ToggleCameraPosition
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

namespace IESLights.Demo
{
  public class ToggleCameraPosition : MonoBehaviour
  {
    public List<Transform> Positions;
    private int _positionIndex;

    private void Start()
    {
      this.transform.position = this.Positions[this._positionIndex].position;
      this.transform.rotation = this.Positions[this._positionIndex].rotation;
    }

    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.Tab))
      {
        ++this._positionIndex;
        this._positionIndex %= this.Positions.Count;
      }
      this.transform.position = this.Positions[this._positionIndex].position;
      this.transform.rotation = this.Positions[this._positionIndex].rotation;
    }
  }
}
