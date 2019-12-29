// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.MaterialFactory
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing
{
  public sealed class MaterialFactory : IDisposable
  {
    private readonly Dictionary<string, Material> m_Materials;

    public MaterialFactory()
    {
      this.m_Materials = new Dictionary<string, Material>();
    }

    public void Dispose()
    {
      foreach (KeyValuePair<string, Material> material in this.m_Materials)
        GraphicsUtils.Destroy((UnityEngine.Object) material.Value);
      this.m_Materials.Clear();
    }

    public Material Get(string shaderName)
    {
      Material material1;
      if (!this.m_Materials.TryGetValue(shaderName, out material1))
      {
        Shader shader = Shader.Find(shaderName);
        if ((UnityEngine.Object) shader == (UnityEngine.Object) null)
          throw new ArgumentException("Shader not found (" + shaderName + ")");
        Material material2 = new Material(shader);
        material2.name = "PostFX - " + shaderName.Substring(shaderName.LastIndexOf("/") + 1);
        material2.hideFlags = HideFlags.DontSave;
        material1 = material2;
        this.m_Materials.Add(shaderName, material1);
      }
      return material1;
    }
  }
}
