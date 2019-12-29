// Decompiled with JetBrains decompiler
// Type: IESLights.RuntimeIESImporter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.IO;
using UnityEngine;

namespace IESLights
{
  public class RuntimeIESImporter : MonoBehaviour
  {
    public static void Import(
      string path,
      out Texture2D spotlightCookie,
      out Cubemap pointLightCookie,
      int resolution = 128,
      bool enhancedImport = false,
      bool applyVignette = true)
    {
      spotlightCookie = (Texture2D) null;
      pointLightCookie = (Cubemap) null;
      if (!RuntimeIESImporter.IsFileValid(path))
        return;
      GameObject cubemapSphere;
      IESConverter iesConverter;
      RuntimeIESImporter.GetIESConverterAndCubeSphere(enhancedImport, resolution, out cubemapSphere, out iesConverter);
      RuntimeIESImporter.ImportIES(path, iesConverter, true, applyVignette, out spotlightCookie, out pointLightCookie);
      UnityEngine.Object.Destroy((UnityEngine.Object) cubemapSphere);
    }

    public static Texture2D ImportSpotlightCookie(
      string path,
      int resolution = 128,
      bool enhancedImport = false,
      bool applyVignette = true)
    {
      if (!RuntimeIESImporter.IsFileValid(path))
        return (Texture2D) null;
      GameObject cubemapSphere;
      IESConverter iesConverter;
      RuntimeIESImporter.GetIESConverterAndCubeSphere(enhancedImport, resolution, out cubemapSphere, out iesConverter);
      Texture2D spotlightCookie;
      RuntimeIESImporter.ImportIES(path, iesConverter, true, applyVignette, out spotlightCookie, out Cubemap _);
      UnityEngine.Object.Destroy((UnityEngine.Object) cubemapSphere);
      return spotlightCookie;
    }

    public static Cubemap ImportPointLightCookie(
      string path,
      int resolution = 128,
      bool enhancedImport = false)
    {
      if (!RuntimeIESImporter.IsFileValid(path))
        return (Cubemap) null;
      GameObject cubemapSphere;
      IESConverter iesConverter;
      RuntimeIESImporter.GetIESConverterAndCubeSphere(enhancedImport, resolution, out cubemapSphere, out iesConverter);
      Cubemap pointlightCookie;
      RuntimeIESImporter.ImportIES(path, iesConverter, false, false, out Texture2D _, out pointlightCookie);
      UnityEngine.Object.Destroy((UnityEngine.Object) cubemapSphere);
      return pointlightCookie;
    }

    private static void GetIESConverterAndCubeSphere(
      bool logarithmicNormalization,
      int resolution,
      out GameObject cubemapSphere,
      out IESConverter iesConverter)
    {
      UnityEngine.Object original = Resources.Load("IES cubemap sphere");
      cubemapSphere = (GameObject) UnityEngine.Object.Instantiate(original);
      iesConverter = cubemapSphere.GetComponent<IESConverter>();
      iesConverter.NormalizationMode = logarithmicNormalization ? NormalizationMode.Logarithmic : NormalizationMode.Linear;
      iesConverter.Resolution = resolution;
    }

    private static void ImportIES(
      string path,
      IESConverter iesConverter,
      bool allowSpotlightCookies,
      bool applyVignette,
      out Texture2D spotlightCookie,
      out Cubemap pointlightCookie)
    {
      string targetFilename = (string) null;
      spotlightCookie = (Texture2D) null;
      pointlightCookie = (Cubemap) null;
      try
      {
        iesConverter.ConvertIES(path, "", allowSpotlightCookies, false, applyVignette, out pointlightCookie, out spotlightCookie, out EXRData _, out targetFilename);
      }
      catch (IESParseException ex)
      {
        Debug.LogError((object) string.Format("[IES] Encountered invalid IES data in {0}. Error message: {1}", (object) path, (object) ex.Message));
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("[IES] Error while parsing {0}. Please contact me through the forums or thomasmountainborn.com. Error message: {1}", (object) path, (object) ex.Message));
      }
    }

    private static bool IsFileValid(string path)
    {
      if (!File.Exists(path))
      {
        Debug.LogWarningFormat("[IES] The file \"{0}\" does not exist.", (object) path);
        return false;
      }
      if (!(Path.GetExtension(path).ToLower() != ".ies"))
        return true;
      Debug.LogWarningFormat("[IES] The file \"{0}\" is not an IES file.", (object) path);
      return false;
    }
  }
}
