// Decompiled with JetBrains decompiler
// Type: DecalFader
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class DecalFader : MonoBehaviour
{
  public Material template;
  public AnimationCurve fadeoffOverDistance;
  private static Material[] materials;
  private static Transform camera;
  private static CharacterClassManager ccm;
  public static DecalFader fader;

  private void Awake()
  {
    DecalFader.fader = this;
    DecalFader.materials = new Material[31];
    for (float num = 30f; (double) num >= 0.0; --num)
    {
      Material material = new Material(this.template);
      material.SetColor("_Color", Color.Lerp(Color.clear, Color.black, num / 30f));
      DecalFader.materials[(int) num] = material;
    }
  }

  public static void UpdateDistance(Decal decal)
  {
    if ((Object) DecalFader.ccm == (Object) null || (Object) DecalFader.camera == (Object) null)
    {
      if ((Object) PlayerManager.localPlayer != (Object) null)
        DecalFader.ccm = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
      DecalFader.camera = Object.FindObjectOfType<SpectatorCamera>().cam.transform;
    }
    else
    {
      float time = Vector3.Distance(decal.transform.position, DecalFader.camera.position);
      if ((double) DecalFader.camera.transform.position.y > 800.0)
        time = 0.0f;
      decal.quad.transform.LookAt(DecalFader.camera);
      decal.quad.transform.Rotate(180f, 0.0f, 0.0f);
      decal.quad.sharedMaterial = DecalFader.materials[Mathf.RoundToInt((float) (30.0 - (double) DecalFader.fader.fadeoffOverDistance.Evaluate(time) * 30.0))];
      if (DecalFader.ccm.CurClass.Is939())
        decal.transform.parent.transform.position = Vector3.one * 9999f;
      else
        decal.transform.parent.transform.position = decal.startPos;
    }
  }
}
