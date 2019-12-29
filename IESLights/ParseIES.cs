// Decompiled with JetBrains decompiler
// Type: IESLights.ParseIES
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IESLights
{
  public static class ParseIES
  {
    private const float SpotlightCutoff = 0.1f;

    public static IESData Parse(string path, NormalizationMode normalizationMode)
    {
      string[] lines = File.ReadAllLines(path);
      int lineNumber = 0;
      ParseIES.FindNumberOfAnglesLine(lines, ref lineNumber);
      if (lineNumber == lines.Length - 1)
        throw new IESParseException("No line containing number of angles found.");
      int numberOfVerticalAngles;
      int numberOfHorizontalAngles;
      PhotometricType photometricType;
      ParseIES.ReadProperties(lines, ref lineNumber, out numberOfVerticalAngles, out numberOfHorizontalAngles, out photometricType);
      List<float> floatList1 = ParseIES.ReadValues(lines, numberOfVerticalAngles, ref lineNumber);
      List<float> floatList2 = ParseIES.ReadValues(lines, numberOfHorizontalAngles, ref lineNumber);
      List<List<float>> floatListList = new List<List<float>>();
      for (int index = 0; index < numberOfHorizontalAngles; ++index)
        floatListList.Add(ParseIES.ReadValues(lines, numberOfVerticalAngles, ref lineNumber));
      IESData iesData = new IESData()
      {
        VerticalAngles = floatList1,
        HorizontalAngles = floatList2,
        CandelaValues = floatListList,
        PhotometricType = photometricType
      };
      ParseIES.NormalizeValues(iesData, normalizationMode == NormalizationMode.Logarithmic);
      if (normalizationMode == NormalizationMode.EqualizeHistogram)
        ParseIES.EqualizeHistogram(iesData);
      if (photometricType != PhotometricType.TypeA)
      {
        ParseIES.DiscardUnusedVerticalHalf(iesData);
        ParseIES.SetVerticalAndHorizontalType(iesData);
        iesData.HalfSpotlightFov = ParseIES.CalculateHalfSpotFov(iesData);
      }
      else
        ParseIES.PadToSquare(iesData);
      return iesData;
    }

    private static void DiscardUnusedVerticalHalf(IESData iesData)
    {
      if ((double) iesData.VerticalAngles[0] != 0.0 || (double) iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] != 180.0)
        return;
      for (int i = 0; i < iesData.VerticalAngles.Count && !iesData.NormalizedValues.Any<List<float>>((Func<List<float>, bool>) (slice => (double) slice[i] > 0.100000001490116)); i++)
      {
        if ((double) iesData.VerticalAngles[i] == 90.0)
        {
          ParseIES.DiscardBottomHalf(iesData);
          return;
        }
        if ((double) iesData.VerticalAngles[i] > 90.0)
        {
          iesData.VerticalAngles[i] = 90f;
          ParseIES.DiscardBottomHalf(iesData);
          return;
        }
      }
      for (int i = iesData.VerticalAngles.Count - 1; i >= 0 && !iesData.NormalizedValues.Any<List<float>>((Func<List<float>, bool>) (slice => (double) slice[i] > 0.100000001490116)); i--)
      {
        if ((double) iesData.VerticalAngles[i] == 90.0)
        {
          ParseIES.DiscardTopHalf(iesData);
          break;
        }
        if ((double) iesData.VerticalAngles[i] < 90.0)
        {
          iesData.VerticalAngles[i] = 90f;
          ParseIES.DiscardTopHalf(iesData);
          break;
        }
      }
    }

    private static void DiscardBottomHalf(IESData iesData)
    {
      int range = 0;
      for (int index = 0; index < iesData.VerticalAngles.Count && (double) iesData.VerticalAngles[index] != 90.0; ++index)
        ++range;
      ParseIES.DiscardHalf(iesData, 0, range);
    }

    private static void DiscardTopHalf(IESData iesData)
    {
      int start = 0;
      for (int index = 0; index < iesData.VerticalAngles.Count; ++index)
      {
        if ((double) iesData.VerticalAngles[index] == 90.0)
        {
          start = index + 1;
          break;
        }
      }
      int range = iesData.VerticalAngles.Count - start;
      ParseIES.DiscardHalf(iesData, start, range);
    }

    private static void DiscardHalf(IESData iesData, int start, int range)
    {
      iesData.VerticalAngles.RemoveRange(start, range);
      for (int index = 0; index < iesData.CandelaValues.Count; ++index)
      {
        iesData.CandelaValues[index].RemoveRange(start, range);
        iesData.NormalizedValues[index].RemoveRange(start, range);
      }
    }

    private static void PadToSquare(IESData iesData)
    {
      if (Mathf.Abs(iesData.HorizontalAngles.Count - iesData.VerticalAngles.Count) <= 1)
        return;
      int longestSide = Mathf.Max(iesData.HorizontalAngles.Count, iesData.VerticalAngles.Count);
      if (iesData.HorizontalAngles.Count < longestSide)
        ParseIES.PadHorizontal(iesData, longestSide);
      else
        ParseIES.PadVertical(iesData, longestSide);
    }

    private static void PadHorizontal(IESData iesData, int longestSide)
    {
      int num1 = longestSide - iesData.HorizontalAngles.Count;
      int num2 = num1 / 2;
      iesData.PadBeforeAmount = iesData.PadAfterAmount = num2;
      List<float> list = Enumerable.Repeat<float>(0.0f, iesData.VerticalAngles.Count).ToList<float>();
      for (int index = 0; index < num2; ++index)
        iesData.NormalizedValues.Insert(0, list);
      for (int index = 0; index < num1 - num2; ++index)
        iesData.NormalizedValues.Add(list);
    }

    private static void PadVertical(IESData iesData, int longestSide)
    {
      int num = longestSide - iesData.VerticalAngles.Count;
      if ((double) Mathf.Sign(iesData.VerticalAngles[0]) == (double) Math.Sign(iesData.VerticalAngles[iesData.VerticalAngles.Count - 1]))
      {
        int length = num / 2;
        iesData.PadBeforeAmount = length;
        iesData.PadAfterAmount = num - length;
        foreach (List<float> normalizedValue in iesData.NormalizedValues)
        {
          normalizedValue.InsertRange(0, (IEnumerable<float>) new List<float>((IEnumerable<float>) new float[length]));
          normalizedValue.AddRange((IEnumerable<float>) new List<float>((IEnumerable<float>) new float[num - length]));
        }
      }
      else
      {
        int length = longestSide / 2 - iesData.VerticalAngles.Count<float>((Func<float, bool>) (v => (double) v >= 0.0));
        if ((double) iesData.VerticalAngles[0] < 0.0)
        {
          iesData.PadBeforeAmount = num - length;
          iesData.PadAfterAmount = length;
          foreach (List<float> normalizedValue in iesData.NormalizedValues)
          {
            normalizedValue.InsertRange(0, (IEnumerable<float>) new List<float>((IEnumerable<float>) new float[num - length]));
            normalizedValue.AddRange((IEnumerable<float>) new List<float>((IEnumerable<float>) new float[length]));
          }
        }
        else
        {
          iesData.PadBeforeAmount = length;
          iesData.PadAfterAmount = num - length;
          foreach (List<float> normalizedValue in iesData.NormalizedValues)
          {
            normalizedValue.InsertRange(0, (IEnumerable<float>) new List<float>((IEnumerable<float>) new float[length]));
            normalizedValue.AddRange((IEnumerable<float>) new List<float>((IEnumerable<float>) new float[num - length]));
          }
        }
      }
    }

    private static void SetVerticalAndHorizontalType(IESData iesData)
    {
      iesData.VerticalType = (double) iesData.VerticalAngles[0] == 0.0 && (double) iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 90.0 || (double) iesData.VerticalAngles[0] == -90.0 && (double) iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 0.0 ? VerticalType.Bottom : ((double) iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] != 180.0 || (double) iesData.VerticalAngles[0] != 90.0 ? VerticalType.Full : VerticalType.Top);
      if (iesData.HorizontalAngles.Count == 1)
        iesData.HorizontalType = HorizontalType.None;
      else if ((double) iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] - (double) iesData.HorizontalAngles[0] == 90.0)
        iesData.HorizontalType = HorizontalType.Quadrant;
      else if ((double) iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] - (double) iesData.HorizontalAngles[0] == 180.0)
      {
        iesData.HorizontalType = HorizontalType.Half;
      }
      else
      {
        iesData.HorizontalType = HorizontalType.Full;
        if ((double) iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] == 360.0)
          return;
        ParseIES.StitchHorizontalAssymetry(iesData);
      }
    }

    private static void StitchHorizontalAssymetry(IESData iesData)
    {
      iesData.HorizontalAngles.Add(360f);
      iesData.CandelaValues.Add(iesData.CandelaValues[0]);
      iesData.NormalizedValues.Add(iesData.NormalizedValues[0]);
    }

    private static float CalculateHalfSpotFov(IESData iesData)
    {
      if (iesData.VerticalType == VerticalType.Bottom && (double) iesData.VerticalAngles[0] == 0.0)
        return ParseIES.CalculateHalfSpotlightFovForBottomHalf(iesData);
      return iesData.VerticalType == VerticalType.Top || iesData.VerticalType == VerticalType.Bottom && (double) iesData.VerticalAngles[0] == -90.0 ? ParseIES.CalculateHalfSpotlightFovForTopHalf(iesData) : -1f;
    }

    private static float CalculateHalfSpotlightFovForBottomHalf(IESData iesData)
    {
      for (int index1 = iesData.VerticalAngles.Count - 1; index1 >= 0; --index1)
      {
        for (int index2 = 0; index2 < iesData.NormalizedValues.Count; ++index2)
        {
          if ((double) iesData.NormalizedValues[index2][index1] >= 0.100000001490116)
            return index1 < iesData.VerticalAngles.Count - 1 ? iesData.VerticalAngles[index1 + 1] : iesData.VerticalAngles[index1];
        }
      }
      return 0.0f;
    }

    private static float CalculateHalfSpotlightFovForTopHalf(IESData iesData)
    {
      for (int index1 = 0; index1 < iesData.VerticalAngles.Count; ++index1)
      {
        for (int index2 = 0; index2 < iesData.NormalizedValues.Count; ++index2)
        {
          if ((double) iesData.NormalizedValues[index2][index1] >= 0.100000001490116)
            return iesData.VerticalType == VerticalType.Top ? (index1 > 0 ? 180f - iesData.VerticalAngles[index1 - 1] : 180f - iesData.VerticalAngles[index1]) : (index1 > 0 ? -iesData.VerticalAngles[index1 - 1] : -iesData.VerticalAngles[index1]);
        }
      }
      return 0.0f;
    }

    private static void NormalizeValues(IESData iesData, bool squashHistogram)
    {
      iesData.NormalizedValues = new List<List<float>>();
      float f = iesData.CandelaValues.SelectMany<List<float>, float>((Func<List<float>, IEnumerable<float>>) (v => (IEnumerable<float>) v)).Max();
      if (squashHistogram)
        f = Mathf.Log(f);
      foreach (List<float> candelaValue in iesData.CandelaValues)
      {
        List<float> floatList = new List<float>();
        if (squashHistogram)
        {
          for (int index = 0; index < candelaValue.Count; ++index)
            floatList.Add(Mathf.Log(candelaValue[index]));
        }
        else
          floatList.AddRange((IEnumerable<float>) candelaValue);
        for (int index = 0; index < candelaValue.Count; ++index)
        {
          floatList[index] /= f;
          floatList[index] = Mathf.Clamp01(floatList[index]);
        }
        iesData.NormalizedValues.Add(floatList);
      }
    }

    private static void EqualizeHistogram(IESData iesData)
    {
      int length = Mathf.Min((int) iesData.CandelaValues.SelectMany<List<float>, float>((Func<List<float>, IEnumerable<float>>) (v => (IEnumerable<float>) v)).Max(), 10000);
      float[] numArray1 = new float[length];
      float[] numArray2 = new float[length];
      foreach (List<float> normalizedValue in iesData.NormalizedValues)
      {
        foreach (float num in normalizedValue)
          ++numArray1[(int) ((double) num * (double) (length - 1))];
      }
      float num1 = (float) (iesData.HorizontalAngles.Count * iesData.VerticalAngles.Count);
      for (int index = 0; index < numArray1.Length; ++index)
        numArray1[index] /= num1;
      for (int index = 0; index < length; ++index)
        numArray2[index] = ((IEnumerable<float>) numArray1).Take<float>(index + 1).Sum();
      foreach (List<float> normalizedValue in iesData.NormalizedValues)
      {
        for (int index1 = 0; index1 < normalizedValue.Count; ++index1)
        {
          int index2 = (int) ((double) normalizedValue[index1] * (double) (length - 1));
          normalizedValue[index1] = numArray2[index2] * (float) (length - 1) / (float) length;
        }
      }
    }

    private static void FindNumberOfAnglesLine(string[] lines, ref int lineNumber)
    {
      int index;
      for (index = 0; index < lines.Length; ++index)
      {
        if (lines[index].Trim().StartsWith("TILT"))
        {
          try
          {
            if (lines[index].Split('=')[1].Trim() != "NONE")
            {
              index += 5;
              break;
            }
            ++index;
            break;
          }
          catch (ArgumentOutOfRangeException ex)
          {
            throw new IESParseException("No TILT line present.");
          }
        }
      }
      lineNumber = index;
    }

    private static void ReadProperties(
      string[] lines,
      ref int lineNumber,
      out int numberOfVerticalAngles,
      out int numberOfHorizontalAngles,
      out PhotometricType photometricType)
    {
      List<float> floatList = ParseIES.ReadValues(lines, 13, ref lineNumber);
      numberOfVerticalAngles = (int) floatList[3];
      numberOfHorizontalAngles = (int) floatList[4];
      photometricType = (PhotometricType) floatList[5];
    }

    private static List<float> ReadValues(
      string[] lines,
      int numberOfValuesToFind,
      ref int lineNumber)
    {
      List<float> floatList = new List<float>();
      while (floatList.Count < numberOfValuesToFind)
      {
        if (lineNumber >= lines.Length)
          throw new IESParseException("Reached end of file before the given number of values was read.");
        char[] separator = (char[]) null;
        if (lines[lineNumber].Contains(","))
          separator = new char[1]{ ',' };
        foreach (string s in lines[lineNumber].Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
          try
          {
            floatList.Add(float.Parse(s));
          }
          catch (Exception ex)
          {
            throw new IESParseException("Invalid value declaration.", ex);
          }
        }
        ++lineNumber;
      }
      return floatList;
    }
  }
}
