﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using LabApi.Features.Console;
using MEC;
using SCPSLAudioApi.AudioCore;
using VoiceChat;
using Random = UnityEngine.Random;

namespace CedMod.Addons.Audio
{
    using CedMod.Addons.QuerySystem;

    public class CustomAudioPlayer: AudioPlayerBase
    {
        public static CustomAudioPlayer Get(ReferenceHub hub)
        {
            if (AudioPlayers.TryGetValue(hub, out AudioPlayerBase player))
            {
                if (player is CustomAudioPlayer cplayer1)
                    return cplayer1;
            }

            var cplayer = hub.gameObject.AddComponent<CustomAudioPlayer>();
            cplayer.Owner = hub;
            cplayer.BroadcastChannel = VoiceChatChannel.Proximity;

            AudioPlayers.Add(hub, cplayer);
            return cplayer;
        }
        
        public override IEnumerator<float> Playback(int index)
        {
            if (Shuffle)
                AudioToPlay = AudioToPlay.OrderBy(i => Random.value).ToList();
            CurrentPlay = AudioToPlay[index];
            AudioToPlay.RemoveAt(index);
            if (Loop)
            {
                AudioToPlay.Add(CurrentPlay);
            }
            
            Logger.Info($"Loading Audio");
            byte[] respString;
            HttpStatusCode code = HttpStatusCode.OK;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                var t = VerificationChallenge.AwaitVerification();
                yield return Timing.WaitUntilTrue(() => t.IsCompleted);
                
                var respTask = client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/RetrieveAudio/{QuerySystem.QuerySystemKey}?track={CurrentPlay}");
                yield return Timing.WaitUntilTrue(() => respTask.IsCompleted);
                var resp = respTask.Result;
                var respStringTask = resp.Content.ReadAsByteArrayAsync();
                yield return Timing.WaitUntilTrue(() => respStringTask.IsCompleted);
                respString = respStringTask.Result;
                code = resp.StatusCode;
            }

            int cnt;
            if (code != HttpStatusCode.OK)
            {
                Logger.Error($"Failed to retrieve audio {code} {Encoding.UTF8.GetString(respString)}");
                if (Continue && AudioToPlay.Count >= 1)
                {
                    yield return Timing.WaitForSeconds(1);
                    Timing.RunCoroutine(Playback(0));
                }
                yield break;
            }

            try
            {
                CurrentPlayStream = new MemoryStream(respString);
                CurrentPlayStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                Logger.Error($"{e} {code} {Encoding.UTF8.GetString(respString)}");
            }
            
            VorbisReader = new NVorbis.VorbisReader(CurrentPlayStream);
            Logger.Info($"Playing with samplerate of {VorbisReader.SampleRate}");
            samplesPerSecond = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;
            //_samplesPerSecond = VorbisReader.Channels * VorbisReader.SampleRate / 5;
            SendBuffer = new float[samplesPerSecond / 5 + HeadSamples];
            ReadBuffer = new float[samplesPerSecond / 5 + HeadSamples];

            while ((cnt = VorbisReader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0)
            {
                if (stopTrack)
                {
                    VorbisReader.SeekTo(VorbisReader.TotalSamples - 1);
                    stopTrack = false;
                }
                while (!ShouldPlay)
                {
                    yield return Timing.WaitForOneFrame;
                }
                while (StreamBuffer.Count >= ReadBuffer.Length)
                {
                    ready = true;
                    yield return Timing.WaitForOneFrame;
                }
                for (int i = 0; i < ReadBuffer.Length; i++)
                {
                    StreamBuffer.Enqueue(ReadBuffer[i]);
                }
            }
            Logger.Info($"Track Complete.");

            if (Continue && AudioToPlay.Count >= 1)
            {
                Timing.RunCoroutine(Playback(0));
            }
            yield break;
        }
    }
}