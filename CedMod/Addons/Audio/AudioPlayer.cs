using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NVorbis;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Voice;
using PluginAPI.Core;
using UnityEngine;
using UnityEngine.Networking;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Networking;
using Random = UnityEngine.Random;

namespace CedMod.Addons.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public static Dictionary<ReferenceHub, AudioPlayer> AudioPlayers = new Dictionary<ReferenceHub, AudioPlayer>();

        public static OpusEncoder Encoder { get; } = new OpusEncoder(VoiceChat.Codec.Enums.OpusApplicationType.Voip);

        public PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer();
        public Queue<float> StreamBuffer { get; } = new Queue<float>();

        public VorbisReader VorbisReader { get; private set; }

        public ReferenceHub Owner { get; private set; }

        public float PlaybackSpeed { get; set; } = 0.1f;
        public int SampleRate { get; set; } = 480;
        public int ReadSampleRate { get; set; } = 480;

        public float[] SendBuffer { get; set; }
        public float[] ReadBuffer { get; set; }
        public byte[] EncodedBuffer { get; } = new byte[512];
        public float Volume { get; set; } = 100f;


        public List<string> AudioToPlay = new List<string>();
        public string CurrentPlay;
        public MemoryStream CurrentPlayStream;
        public bool Loop = false;
        public bool Shuffle = false;
        public bool Continue = true;
        private bool _stopTrack = false;
        public bool ShouldPlay = true;


        private float _allowedSamples;
        private int _samplesPerSecond;
        private const int HeadSamples = 1920;

        public void Stoptrack(bool Clear)
        {
            if (Clear)
                AudioToPlay.Clear();
            _stopTrack = true;
        }

        public static AudioPlayer Get(ReferenceHub hub)
        {
            if (AudioPlayers.TryGetValue(hub, out AudioPlayer player))
            {
                return player;
            }

            player = hub.gameObject.AddComponent<AudioPlayer>();
            player.Owner = hub;

            AudioPlayers.Add(hub, player);
            return player;
        }

        public CoroutineHandle PlaybackCoroutine;

        public void Play(int queuePos)
        {
            if (PlaybackCoroutine.IsRunning)
                Timing.KillCoroutines(PlaybackCoroutine);
            PlaybackCoroutine = Timing.RunCoroutine(Playback(queuePos), Segment.FixedUpdate);

            if (Shuffle)
                AudioToPlay = AudioToPlay.OrderBy(i => Random.value).ToList();
        }

        public IEnumerator<float> Playback(int index)
        {
            int cnt;
            if (Shuffle)
                AudioToPlay = AudioToPlay.OrderBy(i => Random.value).ToList();
            CurrentPlay = AudioToPlay[index];
            AudioToPlay.RemoveAt(index);
            if (Loop)
            {
                AudioToPlay.Add(CurrentPlay);
            }
            
            Log.Info($"Loading Audio");
            UnityWebRequest www = new UnityWebRequest("https://" + QuerySystem.QuerySystem.CurrentMaster + $"/Api/v3/RetrieveAudio/{QuerySystem.QuerySystem.QuerySystemKey}?track={CurrentPlay}", "GET");
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            
            yield return Timing.WaitUntilDone(www.SendWebRequest());

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

            CurrentPlayStream = new MemoryStream(www.downloadHandler.data);
            CurrentPlayStream.Seek(0, SeekOrigin.Begin);
            
            VorbisReader = new NVorbis.VorbisReader(CurrentPlayStream);
            Log.Info($"Playing with samplerate of {VorbisReader.SampleRate}");
            _samplesPerSecond = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;
            //_samplesPerSecond = VorbisReader.Channels * VorbisReader.SampleRate / 5;
            SendBuffer = new float[_samplesPerSecond / 5 + HeadSamples];
            ReadBuffer = new float[_samplesPerSecond / 5 + HeadSamples];

            while ((cnt = VorbisReader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0)
            {
                if (_stopTrack)
                {
                    VorbisReader.SeekTo(VorbisReader.TotalSamples - 1);
                    _stopTrack = false;
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

        public void OnDestroy()
        {
            if (PlaybackCoroutine.IsValid)
                Timing.KillCoroutines(PlaybackCoroutine);
        }

        private bool ready = false;

        public void Update()
        {
            if (Owner == null || !ready || StreamBuffer.Count == 0) return;

            _allowedSamples += Time.deltaTime * _samplesPerSecond;
            int toCopy = Mathf.Min(Mathf.FloorToInt(_allowedSamples), StreamBuffer.Count);
            Log.Info($"1 {toCopy} {_allowedSamples} {_samplesPerSecond} {StreamBuffer.Count} {PlaybackBuffer.Length} {PlaybackBuffer.WriteHead}");
            if (toCopy > 0)
            {
                for (int i = 0; i < toCopy; i++)
                {
                    PlaybackBuffer.Write(StreamBuffer.Dequeue() * (Volume / 100f));
                }
            }
            Log.Info($"2 {toCopy} {_allowedSamples} {_samplesPerSecond} {StreamBuffer.Count} {PlaybackBuffer.Length} {PlaybackBuffer.WriteHead}");
            
            _allowedSamples -= toCopy;

            while (PlaybackBuffer.Length >= 480)
            {
                PlaybackBuffer.ReadTo(SendBuffer, (long)480, 0L);
                int dataLen = Encoder.Encode(SendBuffer, EncodedBuffer, 480);
                
                foreach (var plr in ReferenceHub.AllHubs)
                {
                    if (plr.connectionToClient == null) continue;

                    if (TestingCommand.FakeConnectionsIds.ContainsKey(plr.connectionToClient.connectionId)) continue;
                    plr.connectionToClient.Send(new VoiceMessage(Owner, VoiceChat.VoiceChatChannel.Intercom, EncodedBuffer, dataLen, false));
                }
            }
        }

        public void Enqueue(string audio, int pos)
        {
            if (pos == -1)
                AudioToPlay.Add(audio);
            else
                AudioToPlay.Insert(pos, audio);
        }
    }
}