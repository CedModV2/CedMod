// Decompiled with JetBrains decompiler
// Type: BloodDrawer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BloodDrawer : NetworkBehaviour
{
  private static Queue<Transform> instances = new Queue<Transform>();
  public int maxBlood = 500;
  public LayerMask mask;
  public BloodDrawer.BloodType[] bloodTypes;

  private void Start()
  {
    BloodDrawer.instances.Clear();
    this.maxBlood = 0;
  }

  public void DrawBlood(Vector3 pos, Quaternion rot, int bloodType)
  {
    if (ServerStatic.IsDedicated || bloodType < 0 || this.maxBlood <= 0)
      return;
    Transform transform;
    if (BloodDrawer.instances.Count < this.maxBlood)
    {
      GameObject[] prefabs = this.bloodTypes[bloodType].prefabs;
      transform = UnityEngine.Object.Instantiate<GameObject>(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], pos, rot).transform;
      BloodDrawer.instances.Enqueue(transform);
    }
    else
    {
      transform = BloodDrawer.instances.Dequeue();
      transform.transform.position = pos;
      transform.transform.rotation = rot;
      BloodDrawer.instances.Enqueue(transform);
    }
    transform.Rotate(0.0f, (float) UnityEngine.Random.Range(0, 360), 0.0f, Space.Self);
    float num = UnityEngine.Random.Range(1.1f, 2f);
    transform.localScale = new Vector3(num, num, num);
    RaycastHit hitInfo;
    if (!Physics.Raycast(transform.position - transform.forward / 4f, transform.forward, out hitInfo, 0.6f, (int) this.mask))
      return;
    if (hitInfo.collider.transform.CompareTag("Door"))
    {
      Vector3 vector3 = transform.localScale;
      vector3 = new Vector3(vector3.x, vector3.y, 0.05f);
      transform.localScale = vector3;
    }
    transform.SetParent(hitInfo.collider.transform);
  }

  public void PlaceUnderneath(Transform obj, int type, float amountMultiplier = 1f)
  {
    this.PlaceUnderneath(obj.position, type, amountMultiplier);
  }

  public void PlaceUnderneath(Vector3 pos, int type, float amountMultiplier = 1f)
  {
    RaycastHit hitInfo;
    if (!Physics.Raycast(pos, Vector3.down, out hitInfo, 3f, (int) this.mask))
      return;
    GameObject[] prefabs = this.bloodTypes[type].prefabs;
    Transform transform = UnityEngine.Object.Instantiate<GameObject>(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal)).transform;
    transform.Rotate(0.0f, (float) UnityEngine.Random.Range(0, 360), 0.0f, Space.Self);
    float num = UnityEngine.Random.Range(0.8f, 1.6f) * amountMultiplier;
    transform.localScale = new Vector3(num, num, num);
  }

  private void MirrorProcessed()
  {
  }

  [Serializable]
  public class BloodType
  {
    public GameObject[] prefabs;
  }
}
