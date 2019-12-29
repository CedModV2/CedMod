// Decompiled with JetBrains decompiler
// Type: MapScroll
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class MapScroll : MonoBehaviour
{
  public RectTransform map;
  public RectTransform rootTransf;
  public float minZoom;
  public float maxZoom;
  public float speed;

  private void Start()
  {
    this.rootTransf = this.GetComponent<RectTransform>();
  }

  private void Update()
  {
    this.rootTransf.localScale = Vector3.one * Mathf.Clamp((this.rootTransf.localScale + Input.GetAxis("Mouse ScrollWheel") * 2f * this.minZoom * Vector3.one).x, this.minZoom, this.maxZoom);
    if (Input.GetKey(NewInput.GetKey("Fire1")))
    {
      RectTransform map = this.map;
      map.localPosition = map.localPosition + this.speed * (2f / this.rootTransf.localScale.x) * new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0.0f);
    }
    if (!Input.GetKey(NewInput.GetKey("Zoom")))
      return;
    this.rootTransf.localScale = Vector3.one;
    this.map.localPosition = Vector3.zero;
  }
}
