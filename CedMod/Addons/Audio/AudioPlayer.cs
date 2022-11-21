using System.Collections.Generic;
using MEC;
using NVorbis;
using PlayerRoles.PlayableScps.Scp939;
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

        public VorbisReader VorbisReader { get; private set; }

        public ReferenceHub Owner { get; private set; }

        public float PlaybackSpeed { get; set; } = 0.1f;
        public int SampleRate { get; set; } = 480;
        public int ReadSampleRate { get; set; } = 480;

        public float[] SendBuffer { get; set; }
        public float[] ReadBuffer { get; set; }
        public byte[] EncodedBuffer { get; } = new byte[512];

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
            SendBuffer = new float[VorbisReader.Channels * VorbisReader.SampleRate / 5];
            ReadBuffer = new float[VorbisReader.Channels * VorbisReader.SampleRate / 5];
            if (PlaybackCoroutine.IsValid)
                Timing.KillCoroutines(PlaybackCoroutine);

            PlaybackCoroutine = Timing.RunCoroutine(Playback());
        }

        public IEnumerator<float> Playback()
        {
            int cnt;
            while ((cnt = VorbisReader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0)
            {
                yield return Timing.WaitForSeconds(PlaybackSpeed);
                PlaybackBuffer.Write(ReadBuffer, ReadBuffer.Length);
            }
            yield break;
        }

        public void Update()
        {
            if (Owner == null) return;

            while (PlaybackBuffer.Length >= 2000)
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