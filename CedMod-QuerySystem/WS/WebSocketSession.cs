using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CedMod.INIT;
using Newtonsoft.Json;
using RemoteAdmin;
using UnityEngine;
using Random = System.Random;
using ThreadPriority = System.Threading.ThreadPriority;

namespace CedMod.QuerySystem.WS
{
    public class WebSocketSession : IDisposable
    {
        public bool IsAuthenticated = false;
        private static readonly Random Random = new Random();

        private TcpClient Client { get; }
        private Stream ClientStream { get; }

        public string Id { get; }
        public bool IsMasking { get; private set; }

        public event EventHandler<WebSocketSession> HandshakeCompleted;
        public event EventHandler<WebSocketSession> Disconnected;
        public event EventHandler<Exception> Error;
        public event EventHandler<byte[]> AnyMessageReceived;
        public event EventHandler<string> TextMessageReceived;
        public event EventHandler<string> BinaryMessageReceived;

        public WebSocketSession(TcpClient client)
        {
            Client = client;
            ClientStream = client.GetStream();
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Internal, do not use :)
        /// </summary>
        internal void Start()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                if (!DoHandshake())
                {
                    Error?.Invoke(this, new Exception("Handshake Failed."));
                    Disconnected?.Invoke(this, this);
                    return;
                }

                HandshakeCompleted?.Invoke(this, this);
                StartMessageLoop();
            });
        }

        private void StartMessageLoop()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                try
                {
                    MessageLoop();
                }
                catch (Exception e)
                {
                    Error?.Invoke(this, e);
                }
                finally
                {
                    Disconnected?.Invoke(this, this);
                }
            });
        }

        private bool DoHandshake()
        {
            while (Client.Available == 0 && Client.Connected)
            {
                Thread.Sleep(5);
            }

            if (!Client.Connected) return false;

            byte[] handshake;
            using (var handshakeBuffer = new MemoryStream())
            {
                while (Client.Available > 0)
                {
                    Thread.Sleep(5);
                    var buffer = new byte[Client.Available];
                    ClientStream.Read(buffer, 0, buffer.Length);
                    handshakeBuffer.Write(buffer, 0, buffer.Length);
                }

                handshake = handshakeBuffer.ToArray();
            }

            if (Encoding.UTF8.GetString(handshake).StartsWith("POST"))
            {
                string[] str = Encoding.UTF8.GetString(handshake).Split('{');
                string jsonstring = "{" + str[1];
                Initializer.Logger.Debug("LEGACY-CedModWebAPI", jsonstring);
                Dictionary<string, string> jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonstring);
                if (jsonData["key"] != QuerySystem.SecurityKey)
                {
                    SendMessage("Authentication failed");
                    return false;
                }

                if (jsonData["action"] == "kicksteamid")
                {
                    foreach (GameObject player in PlayerManager.players)
                    {
                        CharacterClassManager component =
                            player.GetComponent<CharacterClassManager>();
                        if (component.UserId == jsonData["steamid"])
                        {
                            ServerConsole.Disconnect(player, jsonData["reason"]);
                            Initializer.Logger.Debug("LEGACY-CedModWebAPI", "kicking");
                        }
                    }
                    SendMessage("User has been kicked");
                }
                return false;
            }

            if (!Encoding.UTF8.GetString(handshake).StartsWith("GET"))
            {
                SendMessage("Handshake failed");
                return false;
            }

            if (!Encoding.UTF8.GetString(handshake).Contains("GET /WSAPI"))
            {
                SendMessage("Welcome to the CedMod QuerySystem For documentation on how to use it visit https://cedmod.nl/QuerySystem");
                return false;
            }

            var response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                                                                                     + "Connection: Upgrade" +
                                                                                     Environment.NewLine
                                                                                     + "Upgrade: websocket" +
                                                                                     Environment.NewLine
                                                                                     + "Sec-WebSocket-Accept: " +
                                                                                     Convert.ToBase64String(
                                                                                         SHA1.Create().ComputeHash(
                                                                                             Encoding.UTF8.GetBytes(
                                                                                                 new Regex(
                                                                                                         "Sec-WebSocket-Key: (.*)")
                                                                                                     .Match(Encoding
                                                                                                         .UTF8
                                                                                                         .GetString(
                                                                                                             handshake))
                                                                                                     .Groups[1].Value
                                                                                                     .Trim() +
                                                                                                 "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                                                                             )
                                                                                         )
                                                                                     ) + Environment.NewLine
                                                                                     + Environment.NewLine);

            ClientStream.Write(response, 0, response.Length);
            TextMessageReceived += OnRecieve;
            lastkeepalivetime = DateTime.Now;
            Task.Factory.StartNew(() => { DieIfNotAuthed(); });
            Task.Factory.StartNew(() => { DieIfNotKepAlive(); });
            return true;
        }

        private void DieIfNotAuthed()
        {
            Thread.Sleep(2000);
            if (!IsAuthenticated)
            {
                SendMessage("Failed to authenticate in time");
                Close();
            }
        }

        private void MessageLoop()
        {
            var session = this;
            var client = session.Client;
            var stream = session.ClientStream;

            var packet = new List<byte>();

            var messageOpcode = 0x0;
            using (var messageBuffer = new MemoryStream())
                while (client.Connected)
                {
                    Thread.Sleep(5);
                    packet.Clear();

                    var ab = client.Available;
                    if (ab == 0) continue;

                    packet.Add((byte) stream.ReadByte());
                    var fin = (packet[0] & (1 << 7)) != 0;
                    var rsv1 = (packet[0] & (1 << 6)) != 0;
                    var rsv2 = (packet[0] & (1 << 5)) != 0;
                    var rsv3 = (packet[0] & (1 << 4)) != 0;

                    // Must error if is set.
                    //if (rsv1 || rsv2 || rsv3)
                    //    return;

                    var opcode = packet[0] & ((1 << 4) - 1);

                    switch (opcode)
                    {
                        case 0x0: // Continuation Frame
                            break;
                        case 0x1: // Text
                        case 0x2: // Binary
                        case 0x8: // Connection Close
                            messageOpcode = opcode;
                            break;
                        case 0x9:
                            continue; // Ping
                        case 0xA:
                            continue; // Pong
                        default:
                            continue; // Reserved
                    }

                    packet.Add((byte) stream.ReadByte());
                    var masked = IsMasking = (packet[1] & (1 << 7)) != 0;
                    var pseudoLength = packet[1] - (masked ? 128 : 0);

                    ulong actualLength = 0;
                    if (pseudoLength > 0 && pseudoLength < 125) actualLength = (ulong) pseudoLength;
                    else if (pseudoLength == 126)
                    {
                        var length = new byte[2];
                        stream.Read(length, 0, length.Length);
                        packet.AddRange(length);
                        Array.Reverse(length);
                        actualLength = BitConverter.ToUInt16(length, 0);
                    }
                    else if (pseudoLength == 127)
                    {
                        var length = new byte[8];
                        stream.Read(length, 0, length.Length);
                        packet.AddRange(length);
                        Array.Reverse(length);
                        actualLength = BitConverter.ToUInt64(length, 0);
                    }

                    var mask = new byte[4];
                    if (masked)
                    {
                        stream.Read(mask, 0, mask.Length);
                        packet.AddRange(mask);
                    }

                    if (actualLength > 0)
                    {
                        var data = new byte[actualLength];
                        stream.Read(data, 0, data.Length);
                        packet.AddRange(data);

                        if (masked)
                            data = ApplyMask(data, mask);

                        messageBuffer.Write(data, 0, data.Length);
                    }
                    

                    if (!fin) continue;
                    var message = messageBuffer.ToArray();

                    switch (messageOpcode)
                    {
                        case 0x1:
                            AnyMessageReceived?.Invoke(session, message);
                            TextMessageReceived?.Invoke(session, Encoding.UTF8.GetString(message));
                            break;
                        case 0x2:
                            AnyMessageReceived?.Invoke(session, message);
                            BinaryMessageReceived?.Invoke(session, Encoding.UTF8.GetString(message));
                            break;
                        case 0x8:
                            Close();
                            break;
                        default:
                            throw new Exception("Invalid opcode: " + messageOpcode);
                    }

                    messageBuffer.SetLength(0);
                }
        }

        public void Close()
        {
            if (!Client.Connected) return;

            var mask = new byte[4];
            if (IsMasking) Random.NextBytes(mask);
            SendMessage(new byte[] { }, 0x8, IsMasking, mask);

            Client.Close();
        }

        public void SendMessage(string payload) => SendMessage(Client, payload, false);

        public void SendMessage(byte[] payload, bool isBinary = false) =>
            SendMessage(Client, payload, isBinary, false);

        public void SendMessage(byte[] payload, bool isBinary = false, bool masking = false) =>
            SendMessage(Client, payload, isBinary, false);

        public void SendMessage(byte[] payload, int opcode, bool masking, byte[] mask) =>
            SendMessage(Client, payload, opcode, false, mask);

        static void SendMessage(TcpClient client, string payload, bool masking = false) =>
            SendMessage(client, Encoding.UTF8.GetBytes(payload), false, false);

        static void SendMessage(TcpClient client, byte[] payload, bool isBinary = false, bool masking = false)
        {
            var mask = new byte[4];
            if (masking) Random.NextBytes(mask);
            SendMessage(client, payload, isBinary ? 0x2 : 0x1, false, mask);
        }

        static void SendMessage(TcpClient client, byte[] payload, int opcode, bool masking, byte[] mask)
        {
            if (masking && mask == null) throw new ArgumentException(nameof(mask));

            using (var packet = new MemoryStream())
            {
                byte firstbyte = 0b0_0_0_0_0000; // fin | rsv1 | rsv2 | rsv3 | [ OPCODE | OPCODE | OPCODE | OPCODE ]

                firstbyte |= 0b1_0_0_0_0000; // fin
                //firstbyte |= 0b0_1_0_0_0000; // rsv1
                //firstbyte |= 0b0_0_1_0_0000; // rsv2
                //firstbyte |= 0b0_0_0_1_0000; // rsv3

                firstbyte += (byte) opcode; // Text
                packet.WriteByte(firstbyte);

                // Set bit: bytes[byteIndex] |= mask;

                byte secondbyte = 0b0_0000000; // mask | [SIZE | SIZE  | SIZE  | SIZE  | SIZE  | SIZE | SIZE]

                if (masking)
                    secondbyte |= 0b1_0000000; // mask

                if (payload.LongLength <= 0b0_1111101) // 125
                {
                    secondbyte |= (byte) payload.Length;
                    packet.WriteByte(secondbyte);
                }
                else if (payload.LongLength <= UInt16.MaxValue) // If length takes 2 bytes
                {
                    secondbyte |= 0b0_1111110; // 126
                    packet.WriteByte(secondbyte);

                    var len = BitConverter.GetBytes(payload.LongLength);
                    Array.Reverse(len, 0, 2);
                    packet.Write(len, 0, 2);
                }
                else // if (payload.LongLength <= Int64.MaxValue) // If length takes 8 bytes
                {
                    secondbyte |= 0b0_1111111; // 127
                    packet.WriteByte(secondbyte);

                    var len = BitConverter.GetBytes(payload.LongLength);
                    Array.Reverse(len, 0, 8);
                    packet.Write(len, 0, 8);
                }

                if (masking)
                {
                    packet.Write(mask, 0, 4);
                    payload = ApplyMask(payload, mask);
                }

                // Write all data to the packet
                packet.Write(payload, 0, payload.Length);

                // Get client's stream
                var stream = client.GetStream();

                var finalPacket = packet.ToArray();

                // Send the packet
                foreach (var b in finalPacket)
                    stream.WriteByte(b);
            }
        }

        static byte[] ApplyMask(IReadOnlyList<byte> msg, IReadOnlyList<byte> mask)
        {
            var decoded = new byte[msg.Count];
            for (var i = 0; i < msg.Count; i++)
                decoded[i] = (byte) (msg[i] ^ mask[i % 4]);
            return decoded;
        }

        public void Dispose()
        {
            Close();

            ((IDisposable) Client)?.Dispose();
            ClientStream?.Dispose();
        }

        private DateTime lastkeepalivetime;

        public void DieIfNotKepAlive()
        {
            Thread.Sleep(5000);
            if(DateTime.Now.Subtract(lastkeepalivetime) >= TimeSpan.FromSeconds(5)) 
                Close();
            DieIfNotKepAlive();
        }
        private void OnRecieve(object sender, string e)
        {
            if (e.StartsWith("STAYALIVE"))
            {
                lastkeepalivetime = DateTime.Now;
                return;
            }
            Task.Factory.StartNew(() => { DieIfNotAuthed(); });
            if (!e.Contains("command\":\"PLAYERLISTCOLORED SILENT"))
                INIT.Initializer.Logger.Debug("CedMod-WebAPI", $"Message recieved: {e}");
            try
            {
                if (e.StartsWith("CMAUTHENTICATE"))
                {
                    if (!IsAuthenticated)
                    {
                        string password = QuerySystem.SecurityKey;
                        SHA256 mySHA256 = SHA256Managed.Create();
                        byte[] iv = new byte[16]
                            {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};
                        byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
                        string decrypted = QuerySystem.DecryptString(e.Split(' ')[1], key, iv);
                        if (decrypted.Split(':')[0] == "youcantrustme")
                        {
                            IsAuthenticated = true;
                            return;
                        }
                        else
                        {
                            SendMessage("Authentication failed");
                            Close();
                        }
                    }
                    else
                    {
                        SendMessage("You are already authenticated");
                    }
                }

                if (IsAuthenticated)
                {
                    Dictionary<string, string> jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(e);
                    string password = QuerySystem.SecurityKey;
                    SHA256 mySHA256 = SHA256Managed.Create();
                    byte[] iv = new byte[16]
                        {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};
                    byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
                    string decrypted = QuerySystem.DecryptString(jsonData["key"], key, iv);
                    if (decrypted.Split(':')[0] == "youcantrustme")
                    {
                        switch (jsonData["action"])
                        {
                            case "kicksteamid":
                                foreach (GameObject player in PlayerManager.players)
                                {
                                    CharacterClassManager component =
                                        player.GetComponent<CharacterClassManager>();
                                    if (component.UserId == jsonData["steamid"])
                                    {
                                        ServerConsole.Disconnect(player, jsonData["reason"]);
                                    }
                                }

                                SendMessage("User kicked");

                                break;
                            case "custom":
                                if (!jsonData.ContainsKey("command"))
                                {
                                    SendMessage("Missing argument");
                                    return;
                                }

                                string[] command = jsonData["command"].Split(' ');
                                if (jsonData["command"].ToUpper().Contains("REQUEST_DATA AUTH") || jsonData["command"].ToUpper().Contains("FC") || jsonData["command"].ToUpper().Contains("FORCECLASS") || jsonData["command"].ToUpper().Contains("SUDO QUIT"))
                                {
                                    SendMessage("This command is disabled");
                                    return;
                                }

                                if (QuerySystem.config.DisallowedWebCommands.Contains(
                                    command[0].ToUpper()))
                                {
                                    SendMessage("This command is disabled");
                                    return;
                                }

                                if (!string.IsNullOrWhiteSpace(decrypted.Split(':')[1]))
                                {
                                    string group =
                                        ServerStatic.PermissionsHandler._members[decrypted.Split(':')[1]];
                                    UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(group);
                                    CommandProcessor.ProcessQuery(jsonData["command"],
                                        new CmSender(this, jsonData["user"], decrypted.Split(':')[1],
                                            ugroup));
                                }
                                else
                                {
                                    CommandProcessor.ProcessQuery(jsonData["command"],
                                        new CmSender(this, jsonData["user"]));
                                }

                                break;
                        }
                    }
                    else
                    {
                        SendMessage("Authentication failed");
                        IsAuthenticated = false;
                        Close();
                    }
                }
                else
                {
                    SendMessage("Authentication failed");
                    Close();
                }
            }
            catch (Exception exception)
            {
                CedMod.INIT.Initializer.Logger.Error("CedMod-WebAPI", exception.ToString());
                this.Close();
            }
        }
    }

    class CmSender : CommandSender
    {
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            Ses.SendMessage(text);
        }

        public override void Print(string text)
        {
            Ses.SendMessage(text);
        }

        public WebSocketSession Ses;
        public string Name;
        public string senderId;
        public ulong permissions;
        public bool fullPermissions;

        public CmSender(WebSocketSession ses, string name, string userid, UserGroup group)
        {
            Ses = ses;
            Name = name;
            senderId = userid;
            permissions = group.Permissions;
            fullPermissions = false;
        }

        public CmSender(WebSocketSession ses, string name)
        {
            Ses = ses;
            Name = name;
            senderId = "SERVER CONSOLE";
            permissions = ServerStatic.PermissionsHandler.FullPerm;
            fullPermissions = true;
        }

        public override void Respond(string text, bool success = true)
        {
            Ses.SendMessage(text);
        }


        public override string SenderId => senderId;
        public override string Nickname => Name;
        public override ulong Permissions => permissions;
        public override byte KickPower => byte.MaxValue;
        public override bool FullPermissions => fullPermissions;


        public override string LogName
        {
            get { return Nickname + " (" + SenderId + ")"; }
        }
    }
}