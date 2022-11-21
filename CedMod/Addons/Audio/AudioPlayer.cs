using System.Collections.Generic;
using MEC;
using NVorbis;
using PlayerRoles.PlayableScps.Scp939;
using PluginAPI.Core;
using UnityEngine;
using VoiceChat.Codec;
using VoiceChat.Networking;

namespace CedMod.Addons.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public static Dictionary<ReferenceHub, AudioPlayer> AudioPlayers = new Dictionary<ReferenceHub, AudioPlayer>();

        public PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer();

        public VorbisReader VorbisReader { get; private set; }

        public ReferenceHub Owner { get; private set; }

        public float PlaybackSpeed { get; set; } = 0.1f;
        public int SampleRate { get; set; } = 480;
        public int ReadSampleRate { get; set; } = 480;
        public float[] ReadBuffer { get; set; }

        public AudioTransmitter Transmitter;

        public static AudioPlayer Get(ReferenceHub hub)
        {
            if (AudioPlayers.TryGetValue(hub, out AudioPlayer player))
            {
                return player;
            }

            player = hub.gameObject.AddComponent<AudioPlayer>();
            player.Transmitter = hub.gameObject.AddComponent<AudioTransmitter>();
            player.Transmitter.Owner = hub;
            player.Owner = hub;

            AudioPlayers.Add(hub, player);
            return player;
        }

        public CoroutineHandle PlaybackCoroutine;

        public void Play(string path)
        {
            VorbisReader = new NVorbis.VorbisReader(path);
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
                Log.Info($"Doing: {cnt} - {VorbisReader.TotalTime} of {VorbisReader.TimePosition}");
                yield return Timing.WaitForOneFrame;
                PlaybackBuffer.Write(ReadBuffer, ReadBuffer.Length);
            }

            Transmitter.SendVoice(PlaybackBuffer);
            yield break;
        }
    }
}