// Decompiled with JetBrains decompiler
// Type: AuthenticatedMessage
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using System;
using System.Collections.Generic;

public class AuthenticatedMessage
{
  public readonly string Message;
  public readonly bool Administrator;

  public AuthenticatedMessage(string m, bool a)
  {
    this.Message = m;
    this.Administrator = a;
  }

  public static string GenerateAuthenticatedMessage(
    string message,
    long timestamp,
    string password)
  {
    if (message.Contains(":[:BR:]:"))
      throw new MessageUnallowedCharsException("Message can't contain :[:BR:]:");
    string data = message + ":[:BR:]:" + Convert.ToString(timestamp);
    return data + ":[:BR:]:" + Sha.HashToString(Sha.Sha512Hmac(Sha.Sha512(password), Utf8.GetBytes(data)));
  }

  public static string GenerateNonAuthenticatedMessage(string message)
  {
    if (message.Contains(":[:BR:]:"))
      throw new MessageUnallowedCharsException("Message can't contain :[:BR:]:");
    return message + ":[:BR:]:Guest";
  }

  public static AuthenticatedMessage AuthenticateMessage(
    string message,
    long timestamp,
    string password)
  {
    if (!message.Contains(":[:BR:]:"))
      throw new MessageAuthenticationFailureException("Malformed message.");
    string[] strArray = message.Split(new string[1]
    {
      ":[:BR:]:"
    }, StringSplitOptions.None);
    if (strArray.Length < 2 || strArray.Length > 3)
      throw new MessageAuthenticationFailureException("Malformed message.");
    if (strArray[1] == "Guest")
      return new AuthenticatedMessage(strArray[0], false);
    try
    {
      if (!TimeBehaviour.ValidateTimestamp(timestamp, Convert.ToInt64(strArray[1]), 1200000L))
        throw new MessageExpiredException();
    }
    catch (MessageExpiredException ex)
    {
      throw new MessageAuthenticationFailureException();
    }
    catch
    {
      throw new MessageAuthenticationFailureException("Malformed message - timestamp can't be converted to long.");
    }
    if (Sha.HashToString(Sha.Sha512Hmac(Sha.Sha512(password), Utf8.GetBytes(strArray[0] + ":[:BR:]:" + strArray[1]))) != strArray[2])
      throw new MessageAuthenticationFailureException("Invalid authentication code.");
    return !string.IsNullOrEmpty(password) && !(password == "none") ? new AuthenticatedMessage(strArray[0], true) : new AuthenticatedMessage(strArray[0], false);
  }

  public static byte[] Encode(byte[] data)
  {
    byte[] numArray = new byte[data.Length + 4];
    byte[] bytes = BitConverter.GetBytes(data.Length);
    Array.Reverse((Array) bytes);
    Array.Copy((Array) bytes, 0, (Array) numArray, 0, bytes.Length);
    Array.Copy((Array) data, 0, (Array) numArray, 4, data.Length);
    return numArray;
  }

  public static List<byte[]> Decode(byte[] data)
  {
    List<byte[]> numArrayList = new List<byte[]>();
    byte[] numArray1;
    for (; data.Length != 0; data = numArray1)
    {
      byte[] numArray2 = new byte[4]
      {
        data[0],
        data[1],
        data[2],
        data[3]
      };
      Array.Reverse((Array) numArray2);
      short int16 = BitConverter.ToInt16(numArray2, 0);
      if (int16 != (short) 0)
      {
        byte[] numArray3 = new byte[(int) int16];
        Array.Copy((Array) data, 4, (Array) numArray3, 0, (int) int16);
        numArrayList.Add(numArray3);
        numArray1 = new byte[data.Length - (int) int16 - 4];
        Array.Copy((Array) data, (int) int16 + 4, (Array) numArray1, 0, data.Length - (int) int16 - 4);
      }
      else
        break;
    }
    return numArrayList;
  }
}
