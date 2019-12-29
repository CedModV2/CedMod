// Decompiled with JetBrains decompiler
// Type: RadialMenu079
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class RadialMenu079 : MonoBehaviour
{
  private Color readyColor = new Color(0.0f, 0.0f, 0.0f, 0.1f);
  private Color lockedColor = new Color(1f, 0.0f, 0.0f, 0.05f);
  public float resizeAnimationScale = 3f;
  private float prevSize = 1f;
  public RadialMenu079.SubAbilities079[] allAbilities;
  public MeshRenderer[] imagePositions;
  public MeshRenderer mainImage;
  public Transform subactionHolder;
  private float currentSize;
  private float backupTime;

  public void SetupMenu(string[] abilityNames, Scp079PlayerScript ply)
  {
    if (this.allAbilities.Length > 4)
    {
      Debug.LogError((object) "This amount of abilities isn't supported");
    }
    else
    {
      this.mainImage.material = new Material(this.mainImage.sharedMaterial);
      for (int index = 0; index < this.allAbilities.Length; ++index)
      {
        if (abilityNames != null && index < abilityNames.Length && !string.IsNullOrEmpty(abilityNames[index]))
          this.allAbilities[index].abilityName = abilityNames[index];
        this.imagePositions[index].material = new Material(this.imagePositions[index].sharedMaterial);
      }
      this.currentSize = 0.0f;
      this.Resize();
    }
  }

  private void Update()
  {
    if ((double) this.backupTime > 0.0)
    {
      this.backupTime -= Time.deltaTime;
    }
    else
    {
      this.currentSize = Mathf.Clamp01(this.currentSize - this.resizeAnimationScale * Time.deltaTime);
      this.Resize();
    }
  }

  private void Resize()
  {
    if ((double) this.prevSize == (double) this.currentSize)
      return;
    this.prevSize = this.currentSize;
    this.mainImage.sharedMaterial.color = Color.Lerp(this.readyColor, this.lockedColor, this.currentSize);
    this.mainImage.sharedMaterial.SetColor("_EmissionColor", Color.white);
    this.mainImage.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.9f, this.currentSize);
    this.mainImage.GetComponent<BoxCollider>().size = new Vector3(1f, 1f, 0.01f) * Mathf.Lerp(1f, 1.1112f, this.currentSize);
    this.subactionHolder.localScale = Vector3.one * this.currentSize;
  }

  public void Display()
  {
    this.backupTime = 0.2f;
    this.currentSize = Mathf.Clamp01(this.currentSize + (float) ((double) this.resizeAnimationScale * (double) Time.deltaTime * 2.0));
    this.Resize();
  }

  [Serializable]
  public struct SubAbilities079 : IEquatable<RadialMenu079.SubAbilities079>
  {
    public string abilityName;
    public string callbackName;
    public Texture[] icons;
    private RadialMenu079.SubAbilities079.PrivateSubAbilityInfo info;

    public RadialMenu079.SubAbilities079.PrivateSubAbilityInfo GetInfo()
    {
      return this.info;
    }

    public SubAbilities079(
      string _abilityName,
      string _callbackName,
      Texture[] _icons,
      float _manaCost,
      int _iconId,
      bool _isBlocked)
    {
      this.abilityName = _abilityName;
      this.callbackName = _callbackName;
      this.icons = _icons;
      this.info = new RadialMenu079.SubAbilities079.PrivateSubAbilityInfo(_manaCost, _iconId, _isBlocked);
    }

    public bool Equals(RadialMenu079.SubAbilities079 other)
    {
      return string.Equals(this.abilityName, other.abilityName) && string.Equals(this.callbackName, other.callbackName) && this.icons == other.icons && this.info == other.info;
    }

    public override bool Equals(object obj)
    {
      return obj is RadialMenu079.SubAbilities079 other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((this.abilityName != null ? this.abilityName.GetHashCode() : 0) * 397 ^ (this.callbackName != null ? this.callbackName.GetHashCode() : 0)) * 397 ^ (this.icons != null ? this.icons.GetHashCode() : 0)) * 397 ^ this.info.GetHashCode();
    }

    public static bool operator ==(
      RadialMenu079.SubAbilities079 left,
      RadialMenu079.SubAbilities079 right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      RadialMenu079.SubAbilities079 left,
      RadialMenu079.SubAbilities079 right)
    {
      return !left.Equals(right);
    }

    public struct PrivateSubAbilityInfo : IEquatable<RadialMenu079.SubAbilities079.PrivateSubAbilityInfo>
    {
      public float manaCost;
      public int setIcon;
      public bool isBlocked;

      public PrivateSubAbilityInfo(float _m, int _i, bool _b)
      {
        this.manaCost = _m;
        this.setIcon = _i;
        this.isBlocked = _b;
      }

      public bool Equals(
        RadialMenu079.SubAbilities079.PrivateSubAbilityInfo other)
      {
        return (double) this.manaCost == (double) other.manaCost && this.setIcon == other.setIcon && this.isBlocked == other.isBlocked;
      }

      public override bool Equals(object obj)
      {
        return obj is RadialMenu079.SubAbilities079.PrivateSubAbilityInfo other && this.Equals(other);
      }

      public override int GetHashCode()
      {
        return (this.manaCost.GetHashCode() * 397 ^ this.setIcon) * 397 ^ this.isBlocked.GetHashCode();
      }

      public static bool operator ==(
        RadialMenu079.SubAbilities079.PrivateSubAbilityInfo left,
        RadialMenu079.SubAbilities079.PrivateSubAbilityInfo right)
      {
        return left.Equals(right);
      }

      public static bool operator !=(
        RadialMenu079.SubAbilities079.PrivateSubAbilityInfo left,
        RadialMenu079.SubAbilities079.PrivateSubAbilityInfo right)
      {
        return !left.Equals(right);
      }
    }
  }
}
