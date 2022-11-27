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
    public class AudioPlayer : MonoBehaviour
    {
        public static Dictionary<ReferenceHub, AudioPlayer> AudioPlayers = new Dictionary<ReferenceHub, AudioPlayer>();

        public static OpusEncoder Encoder { get; } = new OpusEncoder(VoiceChat.Codec.Enums.OpusApplicationType.Voip);

        public PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer();
        public PlaybackBuffer StreamBuffer { get; } = new PlaybackBuffer();

        public VorbisReader VorbisReader { get; private set; }

        public ReferenceHub Owner { get; private set; }

        public float PlaybackSpeed { get; set; } = 0.1f;
        public int SampleRate { get; set; } = 480;
        public int ReadSampleRate { get; set; } = 480;

        public float[] SendBuffer { get; set; }
        public float[] ReadBuffer { get; set; }
        public List<float> Buffer { get; set; }
        public byte[] EncodedBuffer { get; } = new byte[512];


        private float _allowedSamples;
        private int _samplesPerSecond;
        private const int HeadSamples = 1920;
        

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

        public void Play(string path)
        {
            VorbisReader = new NVorbis.VorbisReader(path);
            Log.Info($"Playing with samplerate of {VorbisReader.SampleRate}");
            Buffer = new List<float>();
            _samplesPerSecond = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;
            //_samplesPerSecond = VorbisReader.Channels * VorbisReader.SampleRate / 5;
            SendBuffer = new float[VorbisReader.Channels * VorbisReader.SampleRate / 5];
            ReadBuffer = new float[VorbisReader.Channels * VorbisReader.SampleRate / 5];
            if (PlaybackCoroutine.IsValid)
                Timing.KillCoroutines(PlaybackCoroutine);
            
            PlaybackCoroutine = Timing.RunCoroutine(Playback(), Segment.FixedUpdate);
        }

        public IEnumerator<float> Playback()
        {
            int cnt;
            
            Task.Factory.StartNew(() =>
            {
                while (VorbisReader.SamplePosition < VorbisReader.TotalSamples)
                {
                    cnt = VorbisReader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length);
                    Buffer.AddRange(ReadBuffer);
                }
                Log.Info($"Is ready {Buffer.Count}");
                tmpBuffer = Buffer.ToArray();
                ready = true;
            });
            
            yield break;
        }

        public void OnDestroy()
        {
            if (PlaybackCoroutine.IsValid)
                Timing.KillCoroutines(PlaybackCoroutine);
        }

        private bool ready = false;
        private float[] tmpBuffer;
        
        public void Update()
        {
            if (Owner == null || !ready) return;

            _allowedSamples += Time.deltaTime * _samplesPerSecond;
            int toCopy = Mathf.Min(Mathf.FloorToInt(_allowedSamples), tmpBuffer.Length);
            if (toCopy > 0)
            {
                PlaybackBuffer.Write(tmpBuffer, toCopy, (int)PlaybackBuffer.WriteHead);
            }

            Log.Info($"{toCopy} {_allowedSamples} {_samplesPerSecond} {tmpBuffer.Length} {PlaybackBuffer.Length} {PlaybackBuffer.WriteHead}");
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