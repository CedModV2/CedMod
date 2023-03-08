using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MEC;
using PluginAPI.Core;
using SCPSLAudioApi.AudioCore;
using UnityEngine.Networking;
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
            
            Log.Info($"Loading Audio");
            UnityWebRequest www = new UnityWebRequest($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/RetrieveAudio/{QuerySystem.QuerySystemKey}?track={CurrentPlay}", "GET");
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            
            yield return Timing.WaitUntilDone(www.SendWebRequest());
            
            int cnt;
            if (www.responseCode != 200)
            {
                Log.Error($"Failed to retrieve audio {www.responseCode} {www.downloadHandler.text}");
                if (Continue && AudioToPlay.Count >= 1)
                {
                    yield return Timing.WaitForSeconds(1);
                    Timing.RunCoroutine(Playback(0));
                }
                yield break;
            }

            try
            {
                CurrentPlayStream = new MemoryStream(www.downloadHandler.data);
                CurrentPlayStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                Log.Error($"{e} {www.responseCode} {www.downloadedBytes}");
            }
            www.Dispose();
            VorbisReader = new NVorbis.VorbisReader(CurrentPlayStream);
            Log.Info($"Playing with samplerate of {VorbisReader.SampleRate}");
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
            Log.Info($"Track Complete.");

            if (Continue && AudioToPlay.Count >= 1)
            {
                Timing.RunCoroutine(Playback(0));
            }
            yield break;
        }
    }
}