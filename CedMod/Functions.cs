using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CedMod.CedMod.INIT;
using CedMod.Commands;
using CedMod.Commands.Stuiter;
using CommandSystem;
using EXILED.Extensions;
using GameCore;
using Grenades;
using MEC;
using Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using UnityEngine;
using Console = System.Console;
using Log = EXILED.Log;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CedMod
{
    public class FunctionsNonStatic
    {
        public Plugin Plugin;

        public FunctionsNonStatic(Plugin plugin)
        {
            Plugin = plugin;
        }

        public static void Roundrestart()
        {
            Timing.KillCoroutines("airstrike");
        }

        public void Waitingforplayers()
        {
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
                GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        

        public void SetScale(GameObject target, float x, float y, float z) //this code may have been yoinked
        {
            try
            {
                var identity = target.GetComponent<NetworkIdentity>();


                target.transform.localScale = new Vector3(x, y, z);

                var destroyMessage = new ObjectDestroyMessage();
                destroyMessage.netId = identity.netId;


                foreach (var player in PlayerManager.players)
                {
                    if (player == target)
                        continue;

                    var playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;

                    playerCon.Send(destroyMessage);

                    object[] parameters = {identity, playerCon};
                    typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
                }
            }
            catch (Exception e)
            {
                Log.Info($"Set Scale error: {e}");
            }
        }
    }

    public static class Functions
    {
        public enum GrenadeId
        {
            FragNade = 0,
            FlashNade = 1,
            Scp018Nade = 2
        }

        private static string _geoString = "";

        public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
        {
            var flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic |
                        BindingFlags.Static | BindingFlags.Public;
            var info = type.GetMethod(methodName, flags);
            info?.Invoke(null, param);
        }
        
        public static ItemType GetRandomItem()
        {
            Random random = new Random();
            int index = UnityEngine.Random.Range(0, Plugin.items.Count);
            return Plugin.items[index];
        }
        
        public static void PlayAmbientSound(int id)
        {
            PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 31));
        }

        public static void SpawnGrenade(Vector3 position, bool isFlash = false, float fusedur = -1,
            ReferenceHub player = null, float scalex = 1, float scaley = 1, float scalez = 1)
        {
            if (player == null) player = ReferenceHub.GetHub(PlayerManager.localPlayer);
            var gm = player.GetComponent<GrenadeManager>();
            var component = Object
                .Instantiate(gm.availableGrenades[isFlash ? (int) GrenadeId.FlashNade : (int) GrenadeId.FragNade]
                    .grenadeInstance).GetComponent<Grenade>();
            if (fusedur != -1) component.fuseDuration = fusedur;
            component.FullInitData(gm, position, Quaternion.Euler(component.throwStartAngle), Vector3.zero,
                component.throwAngularVelocity);
            GameObject gameObject = component.gameObject;
            gameObject.transform.localScale = new Vector3(scalex, scaley, scalez);
            NetworkServer.Spawn(gameObject);
        }

        public static Dictionary<string, string> GetBandetails(ReferenceHub player, bool usegeoifenabled = true)
        {
            if (usegeoifenabled)
            {
                var geoList = ConfigFile.ServerConfig.GetStringList("bansystem_geo");
                if (_geoString == "")
                    foreach (var s in geoList)
                        _geoString = _geoString + s + "+";
                if (geoList != null) ServerConsole.AccessRestriction = true;
            }

            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (var webClient3 = new WebClient())
                {
                    webClient3.Credentials =
                        new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey"),
                            ConfigFile.ServerConfig.GetString("bansystem_apikey"));
                    webClient3.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" ")) alias = alias.Replace(" ", "");
                    webClient3.Headers.Add("Alias", alias);
                    webClient3.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient3.Headers.Add("Ip", ServerConsole.Ip);
                    var text3 = BanSystem.Testusers.Contains(player.characterClassManager.UserId) ||
                                Initializer.TestApiOnly
                        ? webClient3.DownloadString("https://test.cedmod.nl/auth/auth.php?id=" + player.GetUserId() +
                                                    "&ip=" +
                                                    player.GetComponent<NetworkIdentity>().connectionToClient.address +
                                                    "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias",
                                                        "none") + "&geo=" + _geoString)
                        : webClient3.DownloadString("https://api.cedmod.nl/auth/auth.php?id=" + player.GetUserId() +
                                                    "&ip=" +
                                                    player.GetComponent<NetworkIdentity>().connectionToClient.address +
                                                    "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias",
                                                        "none") + "&geo=" + _geoString);
                    Initializer.Logger.Info("BANSYSTEM",
                        "Checking ban status of user: " + player.GetComponent<CharacterClassManager>().UserId +
                        " Response from API: " + text3);
                    BanSystem.LastApiRequestSuccessfull = true;
                    var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text3);
                    return jsonObj;
                }
            }
            catch (WebException ex)
            {
                Initializer.Logger.Error("BANSYSTEN",
                    "Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                BanSystem.LastApiRequestSuccessfull = false;
                return null;
            }
        }

        public static Dictionary<string, string> CheckBanExpired(ReferenceHub player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Initializer.Logger.Debug("BANSYSTEM1", Thread.CurrentThread.Name);
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Credentials =
                        new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"),
                            ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" ")) alias = alias.Replace(" ", "");
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip);
                    var text2 = BanSystem.Testusers.Contains(player.characterClassManager.UserId) ||
                                Initializer.TestApiOnly
                        ? webClient.DownloadString("https://test.cedmod.nl/auth/preauth.php?id=" +
                                                   player.GetComponent<CharacterClassManager>().UserId + "&ip=" +
                                                   player.GetComponent<NetworkIdentity>().connectionToClient.address +
                                                   "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias",
                                                       "none"))
                        : webClient.DownloadString("https://api.cedmod.nl/auth/preauth.php?id=" +
                                                   player.GetComponent<CharacterClassManager>().UserId + "&ip=" +
                                                   player.GetComponent<NetworkIdentity>().connectionToClient.address +
                                                   "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias",
                                                       "none"));
                    Initializer.Logger.Info("BANSYSTEM",
                        "checking ban status for user: " + player.GetComponent<CharacterClassManager>().UserId +
                        " Response from API: " + text2);
                    BanSystem.LastApiRequestSuccessfull = true;
                    var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text2);
                    return jsonObj;
                }
            }
            catch (WebException ex)
            {
                BanSystem.LastApiRequestSuccessfull = false;
                Initializer.Logger.Error("BANSYSTEN",
                    "Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }

        public static Dictionary<string, string> Unban(ReferenceHub player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Credentials =
                        new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"),
                            ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" ")) alias = alias.Replace(" ", "");
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip);
                    var text2 = BanSystem.Testusers.Contains(player.characterClassManager.UserId) ||
                                Initializer.TestApiOnly
                        ? webClient.DownloadString("https://test.cedmod.nl/banning/unban.php?id=" +
                                                   player.GetComponent<CharacterClassManager>().UserId + "&ip=" +
                                                   player.GetComponent<NetworkIdentity>().connectionToClient.address +
                                                   "&reason=Expired&aname=Server&webhook=" +
                                                   ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") +
                                                   "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias",
                                                       "none"))
                        : webClient.DownloadString("https://api.cedmod.nl/banning/unban.php?id=" +
                                                   player.GetComponent<CharacterClassManager>().UserId + "&ip=" +
                                                   player.GetComponent<NetworkIdentity>().connectionToClient.address +
                                                   "&reason=Expired&aname=Server&webhook=" +
                                                   ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") +
                                                   "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias",
                                                       "none"));
                    Initializer.Logger.Info("BANSYSTEM",
                        "user: " + player.GetComponent<CharacterClassManager>().UserId + " unban, Response from API: " +
                        text2);
                    BanSystem.LastApiRequestSuccessfull = true;
                    var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text2);
                    return jsonObj;
                }
            }
            catch (WebException ex)
            {
                Initializer.Logger.Error("BANSYSTEN",
                    "Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                BanSystem.LastApiRequestSuccessfull = false;
                return null;
            }
        }

        public static object GetPriors(ReferenceHub player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Credentials =
                        new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"),
                            ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" ")) alias = alias.Replace(" ", "");
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip);
                    var text2 = BanSystem.Testusers.Contains(player.characterClassManager.UserId) ||
                                Initializer.TestApiOnly
                        ? webClient.DownloadString("https://test.cedmod.nl/banning/userdetails.php?id=" +
                                                   player.GetComponent<CharacterClassManager>().UserId + "&alias=" +
                                                   ConfigFile.ServerConfig.GetString("bansystem_alias", "none") +
                                                   "&priors=1")
                        : webClient.DownloadString("https://api.cedmod.nl/banning/userdetails.php?id=" +
                                                   player.GetComponent<CharacterClassManager>().UserId + "&alias=" +
                                                   ConfigFile.ServerConfig.GetString("bansystem_alias", "none") +
                                                   "&priors=1");
                    var json = JsonConvert.DeserializeObject(text2);
                    return json;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("BANSYSTEN Unable to properly connect to CedMod API: " + ex.Status + " | " +
                                  ex.Message);
                return null;
            }
        }

        public static string GetTotalBans(ReferenceHub player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Credentials =
                        new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"),
                            ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" ")) alias = alias.Replace(" ", "");
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip);
                    var text2 = webClient.DownloadString("https://api.cedmod.nl/banning/userdetails.php?id=" + 
                                                         player.GetComponent<CharacterClassManager>().UserId + 
                                                         "&alias=" +
                                                         ConfigFile.ServerConfig.GetString("bansystem_alias", "none") +
                                                         "&total=1");
                    if (text2 == "") text2 = "0";
                    return text2;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("BANSYSTEN Unable to properly connect to CedMod API: " + ex.Status + " | " +
                                  ex.Message);
                return null;
            }
        }

        public static void Ban(GameObject player, long duration, string sender, string reason, bool bc = true)
        {
            Thread.CurrentThread.Name = "CedModV3 queue worker";
            Initializer.Logger.Debug("BANSYSTEM4", Thread.CurrentThread.Name);
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (duration >= 1)
            {
                if (bc)
                    Map.Broadcast(player.GetComponent<NicknameSync>().MyNick + " Has been banned from the server", 9);
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Credentials = new NetworkCredential(
                            ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"),
                            ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                        webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                        var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                        if (alias.Contains(" ")) alias = alias.Replace(" ", "");
                        webClient.Headers.Add("Alias", alias);
                        webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                        webClient.Headers.Add("Ip", ServerConsole.Ip);
                        string text;
                        if (Initializer.TestApiOnly)
                            text = webClient.DownloadString(string.Concat("https://test.cedmod.nl/banning/ban.php?id=",
                                player.GetComponent<CharacterClassManager>().UserId, "&ip=",
                                player.GetComponent<NetworkIdentity>().connectionToClient.address, "&reason=", reason,
                                "&aname=", sender, "&bd=", duration,
                                "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" +
                                ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")));
                        else
                            text = webClient.DownloadString(string.Concat("https://api.cedmod.nl/banning/ban.php?id=",
                                player.GetComponent<CharacterClassManager>().UserId, "&ip=",
                                player.GetComponent<NetworkIdentity>().connectionToClient.address, "&reason=", reason,
                                "&aname=", sender, "&bd=", duration,
                                "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" +
                                ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")));
                        Log.Info(string.Concat("User: ", player.GetComponent<CharacterClassManager>().UserId,
                            " has been banned by: ", sender, " for the reason: ", reason, " duration: ", duration));
                        Log.Info("BANSYSTEM: Response from ban API: " + text);
                        var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                        ServerConsole.Disconnect(player, jsonObj["preformattedmessage"]);
                    }
                }
                catch (WebException ex)
                {
                    Initializer.Logger.Error("BANSYSTEM",
                        string.Concat("An error occured: ", ex.Message, " ", ex.Status,
                            " Adding to UserIDBans instead"));
                    var issuanceTime = TimeBehaviour.CurrentTimestamp();
                    var banExpieryTime = TimeBehaviour.GetBanExpieryTime((uint) duration);
                    BanHandler.IssueBan(new BanDetails
                    {
                        OriginalName = player.GetComponent<NicknameSync>().MyNick,
                        Id = player.GetComponent<CharacterClassManager>().UserId,
                        IssuanceTime = issuanceTime,
                        Expires = banExpieryTime,
                        Reason = reason,
                        Issuer = sender
                    }, BanHandler.BanType.UserId);
                    ServerConsole.Disconnect(player, reason);
                }
            }
            else
            {
                if (duration <= 0)
                {
                    string text3;
                    text3 = "Reason: " + reason;
                    ServerConsole.Disconnect(player, text3);
                }
            }
        }
        public static IEnumerator<float> LightsOut(bool heavyOnly)
        {
            Generator079.mainGenerator.RpcCustomOverchargeForOurBeautifulModCreators(9.5f, heavyOnly);
            yield return Timing.WaitForSeconds(10f);
            Timing.RunCoroutine(LightsOut(Convert.ToBoolean(heavyOnly)), "LightsOut");
        }

        public static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions perm,
            string replyScreen = "", bool reply = true)
        {
            if (ServerStatic.IsDedicated && sender.FullPermissions) return true;
            if (PermissionsHandler.IsPermitted(sender.Permissions, perm)) return true;
            if (reply)
                sender.RaReply(
                    queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm,
                    false, true, replyScreen);
            return false;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == SslPolicyErrors.None) return true;

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }

        public static bool IsTeamKill(CharacterClassManager victim, CharacterClassManager killer)
        {
            var flag = false;
            var team = victim.Classes.SafeGet(victim.CurClass).team;
            var team2 = killer.Classes.SafeGet(killer.CurClass).team;
            FfaLog(killer, victim, team, team2);
            if (killer.CurClass != RoleType.Tutorial)
            {
                if (victim.CurClass == killer.CurClass &&
                    victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() !=
                    killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
                {
                    if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ClassD &&
                        ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true))
                    {
                        flag = true;
                        Initializer.Logger.Debug("FFA", "Teamkill1");
                        FfaLog(killer, victim, team, team2);
                    }
                    else if (victim.CurClass != RoleType.ClassD || killer.CurClass != RoleType.ClassD ||
                             ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true))
                    {
                        flag = true;
                        Initializer.Logger.Debug("FFA", "Teamkill1");
                        FfaLog(killer, victim, team, team2);
                    }
                }
                else if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ChaosInsurgency &&
                         victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() !=
                         killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.Logger.Debug("FFA", "Teamkill2");
                    FfaLog(killer, victim, team, team2);
                }
                else if (victim.CurClass == RoleType.ChaosInsurgency && killer.CurClass == RoleType.ClassD &&
                         victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() !=
                         killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.Logger.Debug("FFA", "Teamkill3");
                    FfaLog(killer, victim, team, team2);
                }
                else if (team == team2 && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() !=
                    killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.Logger.Debug("FFA", "Teamkill4");
                    FfaLog(killer, victim, team, team2);
                }
                else if (team2 == Team.MTF && victim.CurClass == RoleType.Scientist &&
                         victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() !=
                         killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.Logger.Debug("FFA", "Teamkill5");
                    FfaLog(killer, victim, team, team2);
                }
                else if (team == Team.MTF && killer.CurClass == RoleType.Scientist &&
                         victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() !=
                         killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.Logger.Debug("FFA", "Teamkill6");
                    FfaLog(killer, victim, team, team2);
                }
                else if (victim.CurClass == RoleType.ClassD && victim.GetComponent<Handcuffs>().NetworkCufferId >= 1 &&
                         !ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedclassdsallowed"))
                {
                    if (killer.CurClass != RoleType.Tutorial && team2 != Team.SCP && killer.CurClass != RoleType.ClassD)
                    {
                        flag = true;
                        Initializer.Logger.Debug("FFA", "Teamkill5");
                        FfaLog(killer, victim, team, team2);
                    }
                }
                else if (victim.CurClass == RoleType.Scientist &&
                         victim.GetComponent<Handcuffs>().NetworkCufferId >= 1 &&
                         !ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedscientistallowed") &&
                         killer.CurClass != RoleType.Tutorial && team2 != Team.SCP &&
                         killer.CurClass != RoleType.ClassD)
                {
                    flag = true;
                    Initializer.Logger.Debug("FFA", "Teamkill5");
                    FfaLog(killer, victim, team, team2);
                }
            }

            return flag;
        }

        public static void FfaLog(CharacterClassManager killer, CharacterClassManager victim, Team victimteam,
            Team killerteam)
        {
            Initializer.Logger.Debug("FFA",
                string.Concat("VictimDetails: ", victim.gameObject.GetComponent<CharacterClassManager>().UserId, " ",
                    victim.gameObject.GetComponent<NicknameSync>().MyNick, " ",
                    victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString(), " ", victimteam, " ",
                    victim.CurClass));
            Initializer.Logger.Debug("FFA",
                string.Concat("KillerDetails: ", killer.gameObject.GetComponent<CharacterClassManager>().UserId, " ",
                    killer.gameObject.GetComponent<NicknameSync>().MyNick, " ",
                    killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString(), " ", killerteam, " ",
                    killer.CurClass));
        }

        public static class OutsideRandomAirbombPos
        {
            public static List<Vector3> Load()
            {
                return new List<Vector3>
                {
                    new Vector3(Random.Range(175, 182), 984, Random.Range(25, 29)),
                    new Vector3(Random.Range(174, 182), 984, Random.Range(36, 39)),
                    new Vector3(Random.Range(174, 182), 984, Random.Range(36, 39)),
                    new Vector3(Random.Range(166, 174), 984, Random.Range(26, 39)),
                    new Vector3(Random.Range(169, 171), 987, Random.Range(9, 24)),
                    new Vector3(Random.Range(174, 175), 988, Random.Range(10, -2)),
                    new Vector3(Random.Range(186, 174), 990, Random.Range(-1, -2)),
                    new Vector3(Random.Range(186, 189), 991, Random.Range(-1, -24)),
                    new Vector3(Random.Range(186, 189), 991, Random.Range(-1, -24)),
                    new Vector3(Random.Range(185, 189), 993, Random.Range(-26, -34)),
                    new Vector3(Random.Range(180, 195), 995, Random.Range(-36, -91)),
                    new Vector3(Random.Range(148, 179), 995, Random.Range(-45, -72)),
                    new Vector3(Random.Range(118, 148), 995, Random.Range(-47, -65)),
                    new Vector3(Random.Range(83, 118), 995, Random.Range(-47, -65)),
                    new Vector3(Random.Range(13, 15), 995, Random.Range(-18, -48)),
                    new Vector3(Random.Range(84, 88), 988, Random.Range(-67, -70)),
                    new Vector3(Random.Range(68, 83), 988, Random.Range(-52, -66)),
                    new Vector3(Random.Range(53, 68), 988, Random.Range(-53, -63)),
                    new Vector3(Random.Range(12, 49), 988, Random.Range(-47, -66)),
                    new Vector3(Random.Range(38, 42), 988, Random.Range(-40, -47)),
                    new Vector3(Random.Range(38, 43), 988, Random.Range(-32, -38)),
                    new Vector3(Random.Range(-25, 12), 988, Random.Range(-50, -66)),
                    new Vector3(Random.Range(-26, -56), 988, Random.Range(-50, -66)),
                    new Vector3(Random.Range(-3, -24), 1001, Random.Range(-66, -73)),
                    new Vector3(Random.Range(5, 28), 1001, Random.Range(-66, -73)),
                    new Vector3(Random.Range(29, 55), 1001, Random.Range(-66, -73)),
                    new Vector3(Random.Range(50, 54), 1001, Random.Range(-49, -66)),
                    new Vector3(Random.Range(24, 48), 1001, Random.Range(-41, -46)),
                    new Vector3(Random.Range(5, 24), 1001, Random.Range(-41, -46)),
                    new Vector3(Random.Range(-4, -17), 1001, Random.Range(-41, -46)),
                    new Vector3(Random.Range(4, -4), 1001, Random.Range(-25, -40)),
                    new Vector3(Random.Range(11, -11), 1001, Random.Range(-18, -21)),
                    new Vector3(Random.Range(3, -3), 1001, Random.Range(-4, -17)),
                    new Vector3(Random.Range(2, 14), 1001, Random.Range(3, -3)),
                    new Vector3(Random.Range(-1, -13), 1001, Random.Range(4, -3))
                };
            }
        }

        internal static class Coroutines
        {
            public static bool IsAirBombGoing;

            public static IEnumerator<float> AirSupportBomb(int waitforready = 5, int duration = 30)
            {
                Log.Info("[AirSupportBomb] booting...");
                if (IsAirBombGoing)
                {
                    Log.Info("[Airbomb] already booted, cancel.");
                    yield break;
                }

                IsAirBombGoing = true;

                Cassie.CassieMessage("danger . outside zone emergency termination sequence activated .",
                        false, true);
                yield return Timing.WaitForSeconds(5f);

                Log.Info("[AirSupportBomb] charging...");
                while (waitforready > 0)
                {
                    PlayAmbientSound(7);
                    waitforready--;
                    yield return Timing.WaitForSeconds(1f);
                }

                Timing.RunCoroutine(AirSupportstop(duration), "airstrike");
                Log.Info("[AirSupportBomb] throwing...");
                var throwcount = 0;
                while (IsAirBombGoing)
                {
                    var randampos = OutsideRandomAirbombPos.Load().OrderBy(x => Guid.NewGuid()).ToList();
                    foreach (var pos in randampos)
                    {
                        SpawnGrenade(pos, false, 0.1f);
                        yield return Timing.WaitForSeconds(0.1f);
                    }

                    throwcount++;
                    Log.Info($"[AirSupportBomb] throwcount:{throwcount}");
                    yield return Timing.WaitForSeconds(0.25f);
                }

                Cassie.CassieMessage("outside zone termination completed .", false, true);

                Log.Info("[AirSupportBomb] Ended.");
            }

            public static IEnumerator<float> AirSupportstop(int duration = 30)
            {
                yield return Timing.WaitForSeconds(duration);
                IsAirBombGoing = false;
            }
        }
    }
}