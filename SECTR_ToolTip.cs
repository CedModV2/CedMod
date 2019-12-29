// Decompiled with JetBrains decompiler
// Type: SECTR_ToolTip
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

[AttributeUsage(AttributeTargets.Field)]
public class SECTR_ToolTip : Attribute
{
  private string tipText;
  private string dependentProperty;
  private float min;
  private float max;
  private Type enumType;
  private bool hasRange;
  private bool devOnly;
  private bool sceneObjectOverride;
  private bool allowSceneObjects;
  private bool treatAsLayer;

  public SECTR_ToolTip(string tipText)
  {
    this.tipText = tipText;
  }

  public SECTR_ToolTip(string tipText, float min, float max)
  {
    this.tipText = tipText;
    this.min = min;
    this.max = max;
    this.hasRange = true;
  }

  public SECTR_ToolTip(string tipText, string dependentProperty)
  {
    this.tipText = tipText;
    this.dependentProperty = dependentProperty;
  }

  public SECTR_ToolTip(string tipText, string dependentProperty, float min, float max)
  {
    this.tipText = tipText;
    this.dependentProperty = dependentProperty;
    this.min = min;
    this.max = max;
    this.hasRange = true;
  }

  public SECTR_ToolTip(string tipText, bool devOnly)
  {
    this.tipText = tipText;
    this.devOnly = devOnly;
  }

  public SECTR_ToolTip(string tipText, bool devOnly, bool treatAsLayer)
  {
    this.tipText = tipText;
    this.devOnly = devOnly;
    this.treatAsLayer = treatAsLayer;
  }

  public SECTR_ToolTip(string tipText, string dependentProperty, Type enumType)
  {
    this.tipText = tipText;
    this.dependentProperty = dependentProperty;
    this.enumType = enumType;
  }

  public SECTR_ToolTip(string tipText, string dependentProperty, bool allowSceneObjects)
  {
    this.tipText = tipText;
    this.dependentProperty = dependentProperty;
    this.sceneObjectOverride = true;
    this.allowSceneObjects = allowSceneObjects;
  }

  public string TipText
  {
    get
    {
      return this.tipText;
    }
  }

  public string DependentProperty
  {
    get
    {
      return this.dependentProperty;
    }
  }

  public float Min
  {
    get
    {
      return this.min;
    }
  }

  public float Max
  {
    get
    {
      return this.max;
    }
  }

  public Type EnumType
  {
    get
    {
      return this.enumType;
    }
  }

  public bool HasRange
  {
    get
    {
      return this.hasRange;
    }
  }

  public bool DevOnly
  {
    get
    {
      return this.devOnly;
    }
  }

  public bool SceneObjectOverride
  {
    get
    {
      return this.sceneObjectOverride;
    }
  }

  public bool AllowSceneObjects
  {
    get
    {
      return this.allowSceneObjects;
    }
  }

  public bool TreatAsLayer
  {
    get
    {
      return this.treatAsLayer;
    }
  }
}
