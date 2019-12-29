// Decompiled with JetBrains decompiler
// Type: TextMessage
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class TextMessage : MonoBehaviour
{
  public float spacing = 15.5f;
  public float xOffset = 3f;
  public float lerpSpeed = 3f;
  public float position;
  public float remainingLife;

  private Vector3 GetPosition()
  {
    return new Vector3(this.xOffset, this.spacing * this.position, 0.0f);
  }

  private void Start()
  {
  }

  private void Update()
  {
    this.remainingLife -= Time.deltaTime;
  }
}
