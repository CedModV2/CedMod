// Decompiled with JetBrains decompiler
// Type: RadioDisplay
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

public class RadioDisplay : MonoBehaviour
{
  public Text label_t;
  public Text power_t;
  public Text battery_t;
  public GameObject normal;
  public GameObject nobattery;
  public Texture batt1;
  public Texture batt2;
  public RawImage img;
  public static string label;
  public static string power;
  public static string battery;

  private void Start()
  {
  }

  private void Update()
  {
    this.normal.SetActive(RadioDisplay.battery != "0");
    this.nobattery.SetActive(RadioDisplay.battery == "0");
  }

  private void ChangeImg()
  {
  }
}
