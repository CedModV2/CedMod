// Decompiled with JetBrains decompiler
// Type: UBER_MouseOrbit_DynamicDistance
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

[AddComponentMenu("UBER/Mouse Orbit - Dynamic Distance")]
public class UBER_MouseOrbit_DynamicDistance : MonoBehaviour
{
  public float distance = 1f;
  [Range(0.1f, 4f)]
  public float ZoomWheelSpeed = 4f;
  public float minDistance = 0.5f;
  public float maxDistance = 10f;
  public float xSpeed = 250f;
  public float ySpeed = 120f;
  public float xObjSpeed = 250f;
  public float yObjSpeed = 120f;
  public float yMinLimit = -20f;
  public float yMaxLimit = 80f;
  private float bounds_MaxSize = 20f;
  public GameObject target;
  public Transform targetFocus;
  private float x;
  private float y;
  private float normal_angle;
  private float cur_distance;
  private float cur_xSpeed;
  private float cur_ySpeed;
  private float req_xSpeed;
  private float req_ySpeed;
  private float cur_ObjxSpeed;
  private float cur_ObjySpeed;
  private float req_ObjxSpeed;
  private float req_ObjySpeed;
  private bool DraggingObject;
  private bool lastLMBState;
  private Collider[] surfaceColliders;
  [HideInInspector]
  public bool disableSteering;

  private void Start()
  {
    Vector3 eulerAngles = this.transform.eulerAngles;
    this.x = eulerAngles.y;
    this.y = eulerAngles.x;
    this.Reset();
  }

  public void DisableSteering(bool state)
  {
    this.disableSteering = state;
  }

  public void Reset()
  {
    this.lastLMBState = Input.GetMouseButton(0);
    this.disableSteering = false;
    this.cur_distance = this.distance;
    this.cur_xSpeed = 0.0f;
    this.cur_ySpeed = 0.0f;
    this.req_xSpeed = 0.0f;
    this.req_ySpeed = 0.0f;
    this.surfaceColliders = (Collider[]) null;
    this.cur_ObjxSpeed = 0.0f;
    this.cur_ObjySpeed = 0.0f;
    this.req_ObjxSpeed = 0.0f;
    this.req_ObjySpeed = 0.0f;
    if (!(bool) (Object) this.target)
      return;
    Renderer[] componentsInChildren = this.target.GetComponentsInChildren<Renderer>();
    Bounds bounds = new Bounds();
    bool flag = false;
    foreach (Renderer renderer in componentsInChildren)
    {
      if (!flag)
      {
        flag = true;
        bounds = renderer.bounds;
      }
      else
        bounds.Encapsulate(renderer.bounds);
    }
    Vector3 size = bounds.size;
    float num = (double) size.x > (double) size.y ? size.x : size.y;
    this.bounds_MaxSize = (double) size.z > (double) num ? size.z : num;
    this.cur_distance += this.bounds_MaxSize * 1.2f;
    this.surfaceColliders = this.target.GetComponentsInChildren<Collider>();
  }

  private void LateUpdate()
  {
    if (!(bool) (Object) this.target || !(bool) (Object) this.targetFocus)
      return;
    if (!this.lastLMBState && Input.GetMouseButton(0))
    {
      this.DraggingObject = false;
      if (this.surfaceColliders != null)
      {
        RaycastHit hitInfo = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (Collider surfaceCollider in this.surfaceColliders)
        {
          if (surfaceCollider.Raycast(ray, out hitInfo, float.PositiveInfinity))
          {
            this.DraggingObject = true;
            break;
          }
        }
      }
    }
    else if (this.lastLMBState && !Input.GetMouseButton(0))
      this.DraggingObject = false;
    this.lastLMBState = Input.GetMouseButton(0);
    if (this.DraggingObject)
    {
      if (Input.GetMouseButton(0) && !this.disableSteering)
      {
        this.req_ObjxSpeed += (float) (((double) Input.GetAxis("Mouse X") * (double) this.xObjSpeed * 0.0199999995529652 - (double) this.req_ObjxSpeed) * (double) Time.deltaTime * 10.0);
        this.req_ObjySpeed += (float) (((double) Input.GetAxis("Mouse Y") * (double) this.yObjSpeed * 0.0199999995529652 - (double) this.req_ObjySpeed) * (double) Time.deltaTime * 10.0);
      }
      else
      {
        this.req_ObjxSpeed += (float) ((0.0 - (double) this.req_ObjxSpeed) * (double) Time.deltaTime * 4.0);
        this.req_ObjySpeed += (float) ((0.0 - (double) this.req_ObjySpeed) * (double) Time.deltaTime * 4.0);
      }
      this.req_xSpeed += (float) ((0.0 - (double) this.req_xSpeed) * (double) Time.deltaTime * 4.0);
      this.req_ySpeed += (float) ((0.0 - (double) this.req_ySpeed) * (double) Time.deltaTime * 4.0);
    }
    else
    {
      if (Input.GetMouseButton(0) && !this.disableSteering)
      {
        this.req_xSpeed += (float) (((double) Input.GetAxis("Mouse X") * (double) this.xSpeed * 0.0199999995529652 - (double) this.req_xSpeed) * (double) Time.deltaTime * 10.0);
        this.req_ySpeed += (float) (((double) Input.GetAxis("Mouse Y") * (double) this.ySpeed * 0.0199999995529652 - (double) this.req_ySpeed) * (double) Time.deltaTime * 10.0);
      }
      else
      {
        this.req_xSpeed += (float) ((0.0 - (double) this.req_xSpeed) * (double) Time.deltaTime * 4.0);
        this.req_ySpeed += (float) ((0.0 - (double) this.req_ySpeed) * (double) Time.deltaTime * 4.0);
      }
      this.req_ObjxSpeed += (float) ((0.0 - (double) this.req_ObjxSpeed) * (double) Time.deltaTime * 4.0);
      this.req_ObjySpeed += (float) ((0.0 - (double) this.req_ObjySpeed) * (double) Time.deltaTime * 4.0);
    }
    this.distance -= Input.GetAxis("Mouse ScrollWheel") * this.ZoomWheelSpeed;
    this.distance = Mathf.Clamp(this.distance, this.minDistance, this.maxDistance);
    this.cur_ObjxSpeed += (float) (((double) this.req_ObjxSpeed - (double) this.cur_ObjxSpeed) * (double) Time.deltaTime * 20.0);
    this.cur_ObjySpeed += (float) (((double) this.req_ObjySpeed - (double) this.cur_ObjySpeed) * (double) Time.deltaTime * 20.0);
    this.target.transform.RotateAround(this.targetFocus.position, Vector3.Cross(this.targetFocus.position - this.transform.position, this.transform.right), -this.cur_ObjxSpeed);
    this.target.transform.RotateAround(this.targetFocus.position, Vector3.Cross(this.targetFocus.position - this.transform.position, this.transform.up), -this.cur_ObjySpeed);
    this.cur_xSpeed += (float) (((double) this.req_xSpeed - (double) this.cur_xSpeed) * (double) Time.deltaTime * 20.0);
    this.cur_ySpeed += (float) (((double) this.req_ySpeed - (double) this.cur_ySpeed) * (double) Time.deltaTime * 20.0);
    this.x += this.cur_xSpeed;
    this.y -= this.cur_ySpeed;
    this.y = UBER_MouseOrbit_DynamicDistance.ClampAngle(this.y, this.yMinLimit + this.normal_angle, this.yMaxLimit + this.normal_angle);
    if (this.surfaceColliders != null)
    {
      RaycastHit hitInfo = new RaycastHit();
      Vector3 direction = Vector3.Normalize(this.targetFocus.position - this.transform.position);
      float b = 0.01f;
      bool flag = false;
      foreach (Collider surfaceCollider in this.surfaceColliders)
      {
        if (surfaceCollider.Raycast(new Ray(this.transform.position - direction * this.bounds_MaxSize, direction), out hitInfo, float.PositiveInfinity))
        {
          b = Mathf.Max(Vector3.Distance(hitInfo.point, this.targetFocus.position) + this.distance, b);
          flag = true;
        }
      }
      if (flag)
        this.cur_distance += (float) (((double) b - (double) this.cur_distance) * (double) Time.deltaTime * 4.0);
    }
    Quaternion quaternion = Quaternion.Euler(this.y, this.x, 0.0f);
    Vector3 vector3 = quaternion * new Vector3(0.0f, 0.0f, -this.cur_distance) + this.targetFocus.position;
    this.transform.rotation = quaternion;
    this.transform.position = vector3;
  }

  private static float ClampAngle(float angle, float min, float max)
  {
    if ((double) angle < -360.0)
      angle += 360f;
    if ((double) angle > 360.0)
      angle -= 360f;
    return Mathf.Clamp(angle, min, max);
  }

  public void set_normal_angle(float a)
  {
    this.normal_angle = a;
  }
}
