// Decompiled with JetBrains decompiler
// Type: UserMainInterface
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class UserMainInterface : MonoBehaviour
{
  public float lerpSpeed = 3f;
  public GameObject hpOBJ;
  public GameObject searchOBJ;
  public GameObject overloadMsg;
  public GameObject medkitOBJ;
  public GameObject restrainedOBJ;
  public GameObject summary;
  public static UserMainInterface singleton;
  public float lerpedHP;
  private int _maxhp;
  private bool displayExactHealth;

  private void Awake()
  {
    UserMainInterface.singleton = this;
  }

  private void Start()
  {
    ResolutionManager.RefreshScreen();
  }

  private void Update()
  {
  }
}
