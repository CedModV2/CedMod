// Decompiled with JetBrains decompiler
// Type: MarkupWriter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

public class MarkupWriter : MonoBehaviour
{
  public List<string> errorLogs = new List<string>();
  private List<GameObject> spawnedElements = new List<GameObject>();
  public static MarkupWriter singleton;
  public GameObject sample;

  public static event MarkupWriter.OnCreateAction OnCreateObject;

  private void Awake()
  {
    MarkupWriter.singleton = this;
  }

  private void ClearAll()
  {
    foreach (Object spawnedElement in this.spawnedElements)
      Object.Destroy(spawnedElement);
    this.spawnedElements.Clear();
  }

  public delegate void OnCreateAction(MarkupElement objectCreated);
}
