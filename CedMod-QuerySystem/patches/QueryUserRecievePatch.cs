using System;
using System.Net.Sockets;
using System.Threading;
using CedMod.INIT;
using HarmonyLib;

namespace CedMod.QuerySystem.patches
{
    [HarmonyPatch(typeof(QueryUser), nameof(QueryUser.Receive))]
    public static class QueryUserRecievePatch
    {
        static bool Prefix(QueryUser __instance)
        {
            __instance._s.ReadTimeout = 200;
            __instance._s.WriteTimeout = 200;
            while (!__instance._closing)
            {
                try
                {
                    byte[] array = new byte[4096];
                    int num;
                    try
                    {
                        num = __instance._s.Read(array, 0, 4096);
                    }
                    catch
                    {
                        num = -1;
                        Thread.Sleep(5);
                    }

                    if (num > -1)
                    {
                        foreach (byte[] bytes in global::AuthenticatedMessage.Decode(array))
                        {
                            string text = __instance._encoder.GetString(bytes);
                            global::AuthenticatedMessage authenticatedMessage = null;
                            try
                            {
                                text = text.Substring(0, text.LastIndexOf(';'));
                            }
                            catch
                            {
                                __instance._invalidPackets++;
                                text = text.TrimEnd(new char[1]);
                                if (text.EndsWith(";"))
                                {
                                    text = text.Substring(0, text.Length - 1);
                                }
                            }

                            if (__instance._invalidPackets >= 5)
                            {
                                if (!__instance._closing)
                                {
                                    __instance.Send("Too many invalid packets sent.");
                                    global::ServerConsole.AddLog(
                                        "Query connection from " + __instance.Ip +
                                        " dropped due to too many invalid packets sent.", ConsoleColor.Gray);
                                    __instance._server.Users.Remove(__instance);
                                    __instance.CloseConn(false);
                                    break;
                                }

                                break;
                            }
                            else
                            {
                                try
                                {
                                    authenticatedMessage = global::AuthenticatedMessage.AuthenticateMessage(text,
                                        global::TimeBehaviour.CurrentTimestamp(), __instance._querypassword);
                                }
                                catch (global::MessageAuthenticationFailureException ex)
                                {
                                    Initializer.Logger.LogException(ex, "CedMod.PluginInterface", "QueryUserRecievePatch");
                                    __instance.Send("Message can't be authenticated - " + ex.Message);
                                    global::ServerConsole.AddLog(
                                        "Query command from " + __instance.Ip + " can't be authenticated - " +
                                        ex.Message, ConsoleColor.Gray);
                                }
                                catch (global::MessageExpiredException)
                                {
                                    __instance.Send("Message expired");
                                    global::ServerConsole.AddLog("Query command from " + __instance.Ip + " is expired.",
                                        ConsoleColor.Gray);
                                }
                                catch (Exception ex2)
                                {
                                    Initializer.Logger.LogException(ex2, "CedMod.PluginInterface", "QueryUserRecievePatch");
                                    __instance.Send("Error during processing your message.");
                                    global::ServerConsole.AddLog(string.Concat(new string[]
                                    {
                                        "Query command from ",
                                        __instance.Ip,
                                        " can't be processed - ",
                                        ex2.Message,
                                        "."
                                    }), ConsoleColor.Gray);
                                }

                                if (authenticatedMessage != null)
                                {
                                    if (!__instance._authenticated && authenticatedMessage.Administrator)
                                    {
                                        __instance._authenticated = true;
                                    }

                                    string text2 = authenticatedMessage.Message;
                                    if (text2.Contains(" "))
                                    {
                                        text2 = text2.Split(new char[]
                                        {
                                            ' '
                                        })[0];
                                        authenticatedMessage.Message.Substring(text2.Length + 1).Split(new char[]
                                        {
                                            ' '
                                        });
                                    }

                                    text2 = text2.ToLower();
                                    if (authenticatedMessage.Message == "Ping")
                                    {
                                        __instance._invalidPackets = 0;
                                        __instance._lastping =
                                            Convert.ToInt32(__instance._server.Stopwatch.Elapsed.TotalSeconds);
                                        __instance.Send("Pong");
                                    }
                                    else if (authenticatedMessage.Message.Contains("authenticate"))
                                    {
                                        if (QuerySystem.autheduers.ContainsKey(__instance))
                                        {
                                            __instance.Send("You have already authenticated");
                                        }
                                        else
                                        {
                                            string[] cmd = authenticatedMessage.Message.Split(new char[]
                                            {
                                                ' '
                                            });
                                            string group =
                                                ServerStatic.PermissionsHandler._members[cmd[1]];
                                            UserGroup groupq = ServerStatic.PermissionsHandler.GetGroup(group);
                                            __instance.Permissions = groupq.Permissions;
                                            __instance.KickPower = groupq.KickPower;
                                            QuerySystem.autheduers.Add(__instance, $"{cmd[1]}:{cmd[2]}:{group}");
                                            __instance.Send("You have successfully authenticated");
                                            if (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.ServerConsoleCommands))
                                                __instance.Send("Permission to view serverconsole output has been granted");
                                            else 
                                                __instance.Send("Permission to view serverconsole output has been denied: Missing permission, ServerConsoleCommands");
                                        }
                                    }
                                    else if (__instance.AdminCheck(authenticatedMessage.Administrator) && QuerySystem.autheduers.ContainsKey(__instance))
                                    {
                                        ConsoleColor consoleColor;
                                        global::ServerConsole.EnterCommand(authenticatedMessage.Message,
                                            out consoleColor, new UserPrint(__instance, QuerySystem.autheduers[__instance]));
                                    }
                                    else if (!QuerySystem.autheduers.ContainsKey(__instance))
                                        __instance.Send("Authentication is required in order to send  commands");
                                }
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    global::ServerConsole.AddLog(
                        "Query connection from " + __instance.Ip + " dropped (SocketException).", ConsoleColor.Gray);
                    if (!__instance._closing)
                    {
                        __instance._server.Users.Remove(__instance);
                        __instance.CloseConn(false);
                    }

                    break;
                }
                catch(Exception ex)
                {
                    Initializer.Logger.LogException(ex, "CedMod.PluginInterface", "QueryUserRecievePatch");
                    global::ServerConsole.AddLog("Query connection from " + __instance.Ip + " dropped." + ex.ToString(),
                        ConsoleColor.Gray);
                    if (!__instance._closing)
                    {
                        __instance._server.Users.Remove(__instance);
                        __instance.CloseConn(false);
                    }

                    break;
                }
            }

            return false;
        }
    }
    public class UserPrint : CommandSender
    {
        private string senderid;
        public override string SenderId
        {
            get
            {
                return senderid;
            }
        }

        private string nickname;
        public override string Nickname
        {
            get
            {
                return nickname;
            }
        }

        private ulong permissions;
        public override ulong Permissions
        {
            get
            {
                return permissions;
            }
        }

        private byte kickpower;
        public override byte KickPower
        {
            get
            {
                return kickpower;
            }
        }
        
        public override bool FullPermissions
        {
            get
            {
                return false;
            }
        }
        
        public UserPrint(QueryUser usr, string combo)
        {
            this._qu = usr;
            senderid = combo.Split(':')[0];
            nickname = combo.Split(':')[1];
            UserGroup group = ServerStatic.PermissionsHandler.GetGroup(combo.Split(':')[2]);
            permissions = group.Permissions;
            kickpower = group.KickPower;
        }
        
        public override void Print(string text)
        {
            this._qu.Send(text);
        }
        
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            this._qu.Send(JsonSerialize.ToJson(new QueryRaReply(text, success, logToConsole, overrideDisplay)));
        }

        private readonly QueryUser _qu;
    }
}