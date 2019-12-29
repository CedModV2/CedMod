// Decompiled with JetBrains decompiler
// Type: ParticleMenu
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class ParticleMenu : MonoBehaviour
{
  public ParticleExamples[] particleSystems;
  public GameObject gunGameObject;
  private int currentIndex;
  private GameObject currentGO;
  public Transform spawnLocation;

  private void Start()
  {
    this.Navigate(0);
    this.currentIndex = 0;
  }

  public void Navigate(int i)
  {
    this.currentIndex = (this.particleSystems.Length + this.currentIndex + i) % this.particleSystems.Length;
    if ((Object) this.currentGO != (Object) null)
      Object.Destroy((Object) this.currentGO);
    this.currentGO = Object.Instantiate<GameObject>(this.particleSystems[this.currentIndex].particleSystemGO, this.spawnLocation.position + this.particleSystems[this.currentIndex].particlePosition, Quaternion.Euler(this.particleSystems[this.currentIndex].particleRotation));
    this.gunGameObject.SetActive(this.particleSystems[this.currentIndex].isWeaponEffect);
  }
}
