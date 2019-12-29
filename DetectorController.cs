// Decompiled with JetBrains decompiler
// Type: DetectorController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class DetectorController : MonoBehaviour
{
  public float viewRange = 30f;
  public float fov = -0.75f;
  public float detectionProgress;
  public GameObject[] detectors;

  private void Start()
  {
    this.InvokeRepeating("RefreshDetectorsList", 10f, 10f);
  }

  public void RefreshDetectorsList()
  {
    this.detectors = GameObject.FindGameObjectsWithTag("Detector");
  }

  private void Update()
  {
    if (this.detectors.Length == 0)
      return;
    bool flag = false;
    foreach (GameObject detector in this.detectors)
    {
      if ((double) Vector3.Distance(detector.transform.position, this.transform.position) > (double) this.viewRange)
      {
        Vector3 normalized = (this.transform.position - detector.transform.position).normalized;
        RaycastHit hitInfo;
        if ((double) Vector3.Dot(detector.transform.forward, normalized) < (double) this.fov && Physics.Raycast(detector.transform.position, normalized, out hitInfo) && hitInfo.transform.CompareTag("Detector"))
        {
          flag = true;
          break;
        }
      }
    }
    this.detectionProgress += Time.deltaTime * (flag ? 0.3f : -0.5f);
    this.detectionProgress = Mathf.Clamp01(this.detectionProgress);
  }
}
