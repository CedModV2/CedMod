using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using PluginAPI.Core;

namespace CedMod
{
    public class VerificationChallenge
    {
        public static bool CompletedChallenge = false;
        public static bool ChallengeStarted = false;
        public static int Time = 0;

        public static async Task AwaitVerification()
        {
            try
            {
                if (QuerySystem.IsDev)
                {
                    CompletedChallenge = true;
                    return;
                }
                
                if (!ChallengeStarted)
                    await PerformVerification();
                
                while (!CompletedChallenge && !CedModMain.CancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, CedModMain.CancellationToken);
                    //Log.Info("Waiting");
                }
                //Log.Info("Exit waiting");
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;
                Log.Error($"Failed to await verification process {e}");
            }
        }

        public static async Task PerformVerification(string key = "", bool ignore = false)
        {
            if (QuerySystem.IsDev)
            {
                CompletedChallenge = true;
                return;
            }
            if (ChallengeStarted && !ignore)
                return;

            ChallengeStarted = true;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(QuerySystem.QuerySystemKey))
                        return;
                    
                    using (HttpClient client = new HttpClient())
                    {
                        var response = client.GetAsync("https://challenge.cedmod.nl/Check/ShouldChallenge").Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            CompletedChallenge = true;
                            return;
                        }

                        Log.Info("Performing security challenge");

                        AsymmetricKeyParameter keyParameter = null;
                        client.DefaultRequestHeaders.Add("X-Challenge-Time", "1" + DateTime.UtcNow.Ticks);
                        client.DefaultRequestHeaders.Add("X-Challenge-Id", "CHAL-0");
                        response = client.GetAsync("https://challenge.cedmod.nl/Key/GetKey").Result;

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Log.Error($"Failed to perform security challenge, {response.Content.ReadAsStringAsync().Result}");
                            Thread.Sleep(1000);
                            PerformVerification(key, true).Wait();
                            return;
                        }

                        var data = response.Content.ReadAsStringAsync().Result;
                        keyParameter = ECDSA.PublicKeyFromString(data);

                        response = client.GetAsync("https://challenge.cedmod.nl/ChallengeRequest/StartChallenge").Result;
                        client.DefaultRequestHeaders.Remove("X-Challenge-Id");
                        client.DefaultRequestHeaders.Add("X-Challenge-Id", response.Headers.GetValues("X-Challenge-Id").First());
                        var challengeText = response.Content.ReadAsStringAsync().Result;

                        IAsymmetricBlockCipher cipher = new RsaEngine();
                        cipher.Init(true, keyParameter);
                        var bytesArray = Encoding.UTF8.GetBytes(challengeText);
                        byte[] encrypted = cipher.ProcessBlock(bytesArray, 0, bytesArray.Length);
                        string solution = Convert.ToBase64String(encrypted);
                        
                        response = client.PostAsync($"https://challenge.cedmod.nl/ChallengeResponse/ProcessResponse?key={(string.IsNullOrEmpty(key) ? QuerySystem.QuerySystemKey : key)}&i={Time}", new StringContent(solution, Encoding.UTF8)).Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            client.DefaultRequestHeaders.Remove("X-Challenge-Id");
                            client.DefaultRequestHeaders.Remove("X-Challenge-Time");
                            response = client.GetAsync("https://challenge.cedmod.nl/Check/ShouldChallenge").Result;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                CompletedChallenge = true;
                                Log.Info("Completed server security challenge.");
                            }
                            else
                            {
                                if (Time >= 6)
                                {
                                    Log.Error($"Failed to perform security challenge, {response.Content.ReadAsStringAsync().Result}");
                                }

                                Time++;
                                Thread.Sleep(1000);
                                PerformVerification(key, true).Wait();
                            }
                        }
                        else
                        {
                            Log.Error($"Failed to perform security challenge, {response.Content.ReadAsStringAsync().Result}");
                            Thread.Sleep(1000);
                            PerformVerification(key, true).Wait();
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to perform security challenge, {e}");
                    Thread.Sleep(1000);
                    PerformVerification(key, true).Wait();
                }
            });
        }
    }
}