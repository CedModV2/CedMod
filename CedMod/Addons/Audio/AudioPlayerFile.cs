using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Networking;

namespace CedMod.Addons.Audio
{
    public class AudioPlayerFile : MonoBehaviour
    {
        public static Dictionary<ReferenceHub, AudioPlayerFile> AudioPlayers = new Dictionary<ReferenceHub, AudioPlayerFile>();

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


        private float _allowedSamples;
        private int _samplesPerSecond;
        private const int HeadSamples = 1920;
        

        public static AudioPlayerFile Get(ReferenceHub hub)
        {
            if (AudioPlayers.TryGetValue(hub, out AudioPlayerFile player))
            {
                return player;
            }

            player = hub.gameObject.AddComponent<AudioPlayerFile>();
            player.Owner = hub;

            AudioPlayers.Add(hub, player);
            return player;
        }

        public CoroutineHandle PlaybackCoroutine;

        public void Play(string path)
        {
            VorbisReader = new NVorbis.VorbisReader(path);
            Log.Info($"Playing with samplerate of {VorbisReader.SampleRate}");
            _samplesPerSecond = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;
            //_samplesPerSecond = VorbisReader.Channels * VorbisReader.SampleRate / 5;
            SendBuffer = new float[_samplesPerSecond / 5 + HeadSamples];
            ReadBuffer = new float[_samplesPerSecond / 5 + HeadSamples];
            if (PlaybackCoroutine.IsValid)
                Timing.KillCoroutines(PlaybackCoroutine);
            
            PlaybackCoroutine = Timing.RunCoroutine(Playback(), Segment.FixedUpdate);
        }

        public IEnumerator<float> Playback()
        {
            int cnt;
            
            while (VorbisReader.SamplePosition < VorbisReader.TotalSamples)
            {
                cnt = VorbisReader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length);
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
            Log.Info($"Is done");

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
                    PlaybackBuffer.Write(StreamBuffer.Dequeue());
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
    }
}