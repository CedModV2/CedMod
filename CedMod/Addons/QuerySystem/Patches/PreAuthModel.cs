using System;
using System.Text;
using LiteNetLib.Utils;

namespace SlProxy
{
    public class PreAuthModel
    {
        public NetDataWriter RawPreAuth;
        public bool IsChallenge { get; set; }
        public static PreAuthModel ReadPreAuth(NetDataReader reader)
        {
            PreAuthModel model = new PreAuthModel();
            model.RawPreAuth = NetDataWriter.FromBytes(reader.RawData, reader.UserDataOffset, reader.UserDataSize);

            if (reader.TryGetByte(out byte b))
                model.b = b;
            byte cBackwardRevision = 0;
            byte cMajor;
            byte cMinor;
            byte cRevision;
            bool cflag;
            if (!reader.TryGetByte(out cMajor) || !reader.TryGetByte(out cMinor) || !reader.TryGetByte(out cRevision) || !reader.TryGetBool(out cflag) || (cflag && !reader.TryGetByte(out cBackwardRevision)))
            {
                return null;
            }

            model.Major = cMajor;
            model.Minor = cMinor;
            model.Revision = cRevision;
            model.BackwardRevision = cBackwardRevision;
            model.flag = cflag;

            if (reader.TryGetInt(out int challengeid))
            {
                model.IsChallenge = true;
                model.ChallengeID = challengeid;
            }
            if (reader.TryGetBytesWithLength(out byte[] challenge))
                model.Challenge = challenge;
            if (reader.TryGetString(out string userid))
                model.UserID = userid;
            if (reader.TryGetLong(out long expiration))
                model.Expiration = expiration;
            if (reader.TryGetByte(out byte flags))
                model.Flags = flags;
            if (reader.TryGetString(out string region))
                model.Region = region;
            if (reader.TryGetBytesWithLength(out byte[] signature))
                model.Signature = signature;
            Console.WriteLine(model);
            return model;
        }

        public byte b { get; set; }
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Revision { get; set; }
        public byte BackwardRevision { get; set; } = 0;
        public bool flag { get; set; }
        public int ChallengeID { get; set; }
        public byte[] Challenge { get; set; }
        public string UserID { get; set; } = "Unknown UserID";
        public long Expiration { get; set; }
        public byte Flags { get; set; }
        public string Region { get; set; } = "Unknown Region";
        public byte[] Signature { get; set; } = new byte[0];
        public override string ToString()
        {
            return string.Concat(
                $"Version: {Major}.{Minor}.{Revision}, Backward revision: {BackwardRevision}",
                Environment.NewLine,
                $"Challenge ID: {ChallengeID}",
                Environment.NewLine,
                $"Challenge: {Encoding.Default.GetString(Challenge)}",
                Environment.NewLine,
                $"UserID: {UserID}",
                Environment.NewLine,
                $"Expiration: {Expiration}",
                Environment.NewLine,
                $"Flags: {Flags}",
                Environment.NewLine,
                $"Region: {Region}",
                Environment.NewLine,
                $"Signature length: {Signature.Length}",
                Environment.NewLine,
                $"Signature: {Encoding.Default.GetString(Signature)}");
        }
    }
}