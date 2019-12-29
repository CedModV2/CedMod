// Decompiled with JetBrains decompiler
// Type: HttpQuery
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class HttpQuery
{
  private static readonly HttpClient Client = new HttpClient();

  static HttpQuery()
  {
    HttpQuery.Client.Timeout = TimeSpan.FromSeconds(10.0);
  }

  public static string Get(string url)
  {
    bool flag = false;
    while (true)
    {
      switch (GameCore.Console.HttpMode)
      {
        case HttpQueryMode.HttpRequest:
          try
          {
            WebRequest webRequest = WebRequest.Create(url);
            ServicePointManager.Expect100Continue = true;
            ((HttpWebRequest) webRequest).UserAgent = "SCP SL";
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            using (WebResponse response = webRequest.GetResponse())
            {
              using (Stream responseStream = response.GetResponseStream())
              {
                using (StreamReader streamReader = new StreamReader(responseStream))
                  return streamReader.ReadToEnd();
              }
            }
          }
          catch (Exception ex)
          {
            if (flag || GameCore.Console.LockHttpMode || !ex.Message.Contains("(ReadDone1)"))
            {
              throw;
            }
            else
            {
              flag = true;
              GameCore.Console.HttpMode = HttpQueryMode.HttpClient;
              GameCore.Console.AddLog("Switched to HttpClient (\"ReadDone1\" exception).", Color.yellow, false);
              continue;
            }
          }
        case HttpQueryMode.HttpClient:
          try
          {
            Task<HttpResponseMessage> async = HttpQuery.Client.GetAsync(url);
            async.Wait();
            return async.Result.Content.ReadAsStringAsync().Result;
          }
          catch (Exception ex)
          {
            if (flag || GameCore.Console.LockHttpMode || !ex.Message.Contains("One or more errors occurred"))
            {
              throw;
            }
            else
            {
              flag = true;
              GameCore.Console.HttpMode = HttpQueryMode.HttpRequest;
              GameCore.Console.AddLog("Switched to HttpRequest (\"One or more errors...\" exception).", Color.yellow, false);
              continue;
            }
          }
        default:
          goto label_22;
      }
    }
label_22:
    return (string) null;
  }

  public static string Post(string url, string data)
  {
    bool flag = false;
    while (true)
    {
      switch (GameCore.Console.HttpMode)
      {
        case HttpQueryMode.HttpRequest:
          try
          {
            byte[] bytes = new UTF8Encoding().GetBytes(data);
            WebRequest webRequest = WebRequest.Create(url);
            ServicePointManager.Expect100Continue = true;
            ((HttpWebRequest) webRequest).UserAgent = "SCP SL";
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = (long) bytes.Length;
            using (Stream requestStream = webRequest.GetRequestStream())
              requestStream.Write(bytes, 0, bytes.Length);
            using (WebResponse response = webRequest.GetResponse())
            {
              using (Stream responseStream = response.GetResponseStream())
              {
                using (StreamReader streamReader = new StreamReader(responseStream))
                  return streamReader.ReadToEnd();
              }
            }
          }
          catch (Exception ex)
          {
            if (flag || GameCore.Console.LockHttpMode || !ex.Message.Contains("(ReadDone1)"))
            {
              throw;
            }
            else
            {
              flag = true;
              GameCore.Console.HttpMode = HttpQueryMode.HttpClient;
              GameCore.Console.AddLog("Switched to HttpClient (\"ReadDone1\" exception).", Color.yellow, false);
              continue;
            }
          }
        case HttpQueryMode.HttpClient:
          try
          {
            Task<HttpResponseMessage> task = HttpQuery.Client.PostAsync(url, (HttpContent) new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded"));
            task.Wait();
            return task.Result.Content.ReadAsStringAsync().Result;
          }
          catch (Exception ex)
          {
            if (flag || GameCore.Console.LockHttpMode || !ex.Message.Contains("One or more errors occurred"))
            {
              throw;
            }
            else
            {
              flag = true;
              GameCore.Console.HttpMode = HttpQueryMode.HttpRequest;
              GameCore.Console.AddLog("Switched to HttpRequest (\"One or more errors...\" exception).", Color.yellow, false);
              continue;
            }
          }
        default:
          goto label_27;
      }
    }
label_27:
    return (string) null;
  }

  public static string ToPostArgs(IEnumerable<string> data)
  {
    return data.Aggregate<string>((Func<string, string, string>) ((current, a) => current + "&" + a.Replace("&", "[AMP]"))).TrimStart('&');
  }
}
