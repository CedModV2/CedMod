// Decompiled with JetBrains decompiler
// Type: GunShoot
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class GunShoot : MonoBehaviour
{
  public float fireRate = 0.25f;
  public float weaponRange = 20f;
  public Transform gunEnd;
  public ParticleSystem muzzleFlash;
  public ParticleSystem cartridgeEjection;
  public GameObject metalHitEffect;
  public GameObject sandHitEffect;
  public GameObject stoneHitEffect;
  public GameObject waterLeakEffect;
  public GameObject waterLeakExtinguishEffect;
  public GameObject[] fleshHitEffects;
  public GameObject woodHitEffect;
  private float nextFire;
  private Animator anim;
  private GunAim gunAim;

  private void Start()
  {
    this.anim = this.GetComponent<Animator>();
    this.gunAim = this.GetComponentInParent<GunAim>();
  }

  private void Update()
  {
    if (!Input.GetKeyDown(NewInput.GetKey("Fire1")) || (double) Time.time <= (double) this.nextFire || this.gunAim.GetIsOutOfBounds())
      return;
    this.nextFire = Time.time + this.fireRate;
    this.muzzleFlash.Play();
    this.cartridgeEjection.Play();
    this.anim.SetTrigger("Fire");
    RaycastHit hitInfo;
    if (!Physics.Raycast(this.gunEnd.position, this.gunEnd.forward, out hitInfo, this.weaponRange))
      return;
    this.HandleHit(hitInfo);
  }

  private void HandleHit(RaycastHit hit)
  {
    if (!((Object) hit.collider.sharedMaterial != (Object) null))
      return;
    string name = hit.collider.sharedMaterial.name;
    // ISSUE: reference to a compiler-generated method
    switch (\u003CPrivateImplementationDetails\u003E.ComputeStringHash(name))
    {
      case 81868168:
        if (!(name == "Wood"))
          break;
        this.SpawnDecal(hit, this.woodHitEffect);
        break;
      case 329707512:
        if (!(name == "WaterFilledExtinguish"))
          break;
        this.SpawnDecal(hit, this.waterLeakExtinguishEffect);
        this.SpawnDecal(hit, this.metalHitEffect);
        break;
      case 970575400:
        if (!(name == "WaterFilled"))
          break;
        this.SpawnDecal(hit, this.waterLeakEffect);
        this.SpawnDecal(hit, this.metalHitEffect);
        break;
      case 1044434307:
        if (!(name == "Sand"))
          break;
        this.SpawnDecal(hit, this.sandHitEffect);
        break;
      case 1842662042:
        if (!(name == "Stone"))
          break;
        this.SpawnDecal(hit, this.stoneHitEffect);
        break;
      case 2840670588:
        if (!(name == "Metal"))
          break;
        this.SpawnDecal(hit, this.metalHitEffect);
        break;
      case 3966976176:
        if (!(name == "Character"))
          break;
        this.SpawnDecal(hit, this.fleshHitEffects[Random.Range(0, this.fleshHitEffects.Length)]);
        break;
      case 4022181330:
        if (!(name == "Meat"))
          break;
        this.SpawnDecal(hit, this.fleshHitEffects[Random.Range(0, this.fleshHitEffects.Length)]);
        break;
    }
  }

  private void SpawnDecal(RaycastHit hit, GameObject prefab)
  {
    Object.Instantiate<GameObject>(prefab, hit.point, Quaternion.LookRotation(hit.normal)).transform.SetParent(hit.collider.transform);
  }
}
