using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

namespace CedMod.Addons.Audio
{
    public class AudioTransmitter : MonoBehaviour
    {
        public PlaybackBuffer _copierPlayback;
        public PlaybackBuffer _senderPlayback;
        public int _playbackSize;
        public int _allowedSamples;
        public int _samplesPerSecond;
        public const int HeadSamples = 1920;
        public ReferenceHub Owner;

        public void Awake()
        {
            this._samplesPerSecond = 48000;
        }

        public void SendVoice(PlaybackBuffer pb)
        {
            pb.Reorganize();
            int length = pb.Buffer.Length;
            if (this._playbackSize < length)
            {
                this._copierPlayback = new PlaybackBuffer(length);
                this._senderPlayback = new PlaybackBuffer(length);
                this._playbackSize = length;
            }
            else
            {
                this._copierPlayback.Clear();
                this._senderPlayback.Clear();
            }

            this._copierPlayback.Write(pb.Buffer, pb.Length);
            this._allowedSamples = 1920;
        }

        public void ResetObject()
        {
            this._copierPlayback?.Clear();
            this._senderPlayback?.Clear();
        }

        public void Update()
        {
            if (!this.Owner.isLocalPlayer || this._playbackSize == 0)
                return;
            this._allowedSamples += Mathf.CeilToInt(Time.deltaTime * (float)this._samplesPerSecond);
            int readLength = Mathf.Min(this._allowedSamples, this._copierPlayback.Length);
            if (readLength > 0)
            {
                this._copierPlayback.ReadTo(this._senderPlayback.Buffer, (long)readLength,
                    this._senderPlayback.WriteHead);
                this._senderPlayback.WriteHead += (long)readLength;
            }

            this._allowedSamples = 0;
            VoiceTransceiver.ClientSendData(this._senderPlayback, VoiceChatChannel.Proximity, 1);
        }
    }
}