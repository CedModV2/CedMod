// Decompiled with JetBrains decompiler
// Type: WindowsUpdateWarning
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindowsUpdateWarning : MonoBehaviour
{
  public GameObject warning;
  public GameObject menu;

  private void Start()
  {
    this.warning.SetActive(SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows && OperatingSystem.GetSystemVersion().Major < 10 && !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System) + Path.DirectorySeparatorChar.ToString() + "API-MS-WIN-CRT-MATH-L1-1-0.dll"));
    this.menu.SetActive(SceneManager.GetActiveScene().buildIndex == 3 || !this.warning.activeSelf);
  }
}
