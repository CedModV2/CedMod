// Decompiled with JetBrains decompiler
// Type: HoloSight
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class HoloSight : MonoBehaviour
{
  private static readonly int OffsetX = Shader.PropertyToID("_OffsetX");
  private static readonly int OffsetY = Shader.PropertyToID("_OffsetY");
  public float backSpeed = 2f;
  public float intensity = 1f;
  public float blurScale = 1f;
  public float maxOffset = 3f;
  public bool changePositions;
  public bool invertX;
  public bool lockRotations;
  private Vector3 startSize;
  private Vector3 startRot;
  public Color mainColor;
  private float _xSize;
  private float _ySize;

  private void Awake()
  {
    this.startSize = this.transform.localScale;
    this.startRot = this.transform.localRotation.eulerAngles;
    if (!((Object) this.GetComponentInParent<WeaponManager>() == (Object) null))
      return;
    Object.Destroy((Object) this);
  }

  private void LateUpdate()
  {
    this._xSize = Mathf.Lerp(this._xSize, 0.0f, this.backSpeed * Time.deltaTime);
    this._ySize = Mathf.Lerp(this._ySize, 0.0f, this.backSpeed * Time.deltaTime);
    this._xSize += this.GetComponentInParent<WeaponManager>().ZoomInProgress() ? 1f : Input.GetAxis("Mouse X") * this.intensity;
    this._ySize += (float) ((double) Input.GetAxis("Mouse Y") * (double) this.intensity * ((double) Input.GetAxis("Mouse X") > 0.0 || this.lockRotations ? -1.0 : 1.0));
    Vector2 vector2 = new Vector2(this._xSize, this._ySize);
    if ((double) vector2.magnitude > 5.0)
      vector2 = vector2.normalized * 5f;
    this._ySize = vector2.y;
    this._xSize = vector2.x;
    MeshRenderer component = this.GetComponent<MeshRenderer>();
    component.sharedMaterial.SetColor("_RedDotColor", Color.Lerp(this.mainColor, Color.clear, vector2.magnitude / 10f));
    if (this.lockRotations)
    {
      this.transform.localScale = new Vector3(this.startSize.x / (float) ((double) Mathf.Abs(vector2.y) * (double) this.blurScale + 1.0), this.startSize.y / (float) ((double) Mathf.Abs(vector2.x) * (double) this.blurScale + 1.0), this.startSize.z);
    }
    else
    {
      Transform transform = this.transform;
      transform.localRotation = Quaternion.Euler(this.startRot + Vector3.forward * (float) (90.0 * ((double) vector2.y / 5.0)));
      transform.localScale = new Vector3((float) ((double) vector2.magnitude * (double) this.blurScale + 1.0) * this.startSize.x, this.startSize.y, this.startSize.z);
    }
    if (this.changePositions)
    {
      component.sharedMaterial.SetFloat(HoloSight.OffsetX, (float) ((double) this._xSize * (double) this.maxOffset / 5.0));
      component.sharedMaterial.SetFloat(HoloSight.OffsetY, (float) ((double) this._ySize * (double) this.maxOffset / 5.0));
    }
    else
    {
      component.sharedMaterial.SetFloat(HoloSight.OffsetX, 0.0f);
      component.sharedMaterial.SetFloat(HoloSight.OffsetY, 0.0f);
    }
  }
}
