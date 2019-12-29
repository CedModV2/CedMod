// Decompiled with JetBrains decompiler
// Type: WMTablet
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

public class WMTablet : MonoBehaviour
{
  public GameObject[] submenus;
  public int curMenu;
  public int selectedWeapon;
  public GameObject list_template;
  public Transform list_parent;
  public Color list_normalColor;
  public Color list_selectedColor;
  public Text ct_text;
  public Text list_info;

  private void Start()
  {
  }

  private void Update()
  {
  }
}
