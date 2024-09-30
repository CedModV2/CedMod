using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using Cryptography;
using LabApi.Features.Console;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;

namespace CedMod
{
    public class VerificationChallenge
    {
        public static bool CompletedChallenge = false;
        public static bool ChallengeStarted = false;
        public static int Time = 0;
        public static Stopwatch verificationTime = new Stopwatch();

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
                    //Logger.Info("Waiting");
                }
                //Logger.Info("Exit waiting");
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;
                Logger.Error($"Failed to await verification process {e}");
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

            verificationTime.Stop();
            verificationTime.Reset();
            ChallengeStarted = true;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(QuerySystem.QuerySystemKey))
                        return;
                    
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                        var response = client.GetAsync("https://challenge.cedmod.nl/Check/ShouldChallenge").Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            CompletedChallenge = true;
                            verificationTime.Start();
                            return;
                        }

                        Logger.Info("Performing security challenge");

                        AsymmetricKeyParameter keyParameter = null;
                        client.DefaultRequestHeaders.Add("X-Challenge-Time", "1" + DateTime.UtcNow.Ticks);
                        client.DefaultRequestHeaders.Add("X-Challenge-Id", "CHAL-0");
                        response = client.GetAsync("https://challenge.cedmod.nl/Key/GetKey").Result;

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Logger.Error($"Failed to perform security challenge, {response.Content.ReadAsStringAsync().Result}");
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
                        
                        response = client.PostAsync($"https://challenge.cedmod.nl/ChallengeResponse/ProcessResponse?key={(string.IsNullOrEmpty(key) ? QuerySystem.QuerySystemKey : key)}&i={Time}&ip={ServerConsole.Ip}", new StringContent(solution, Encoding.UTF8)).Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            client.DefaultRequestHeaders.Remove("X-Challenge-Id");
                            client.DefaultRequestHeaders.Remove("X-Challenge-Time");
                            response = client.GetAsync("https://challenge.cedmod.nl/Check/ShouldChallenge").Result;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                CompletedChallenge = true;
                                Logger.Info("Completed server security challenge.");
                            }
                            else
                            {
                                if (Time >= 6)
                                {
                                    Logger.Error($"Failed to perform security challenge, {response.Content.ReadAsStringAsync().Result}");
                                }

                                Time++;
                                Thread.Sleep(1000);
                                PerformVerification(key, true).Wait();
                            }
                        }
                        else
                        {
                            Logger.Error($"Failed to perform security challenge, {response.Content.ReadAsStringAsync().Result}");
                            Thread.Sleep(1000);
                            PerformVerification(key, true).Wait();
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to perform security challenge, {e}");
                    Thread.Sleep(1000);
                    PerformVerification(key, true).Wait();
                }
            });
        }
    }
}