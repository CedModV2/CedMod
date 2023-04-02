using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.ApiModals;
using MEC;
using Newtonsoft.Json;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using UnityEngine;
using UnityEngine.Networking;

namespace CedMod.Components
{
    public class RemoteAdminModificationHandler: MonoBehaviour
    {
        public float ReportGetTimer { get; set; }
        public float WatchlistGetTimer { get; set; }
        public float UiBlinkTimer { get; set; }
        
        public static List<WatchList> Watchlist { get; set; } = new List<WatchList>();
        public static List<WatchListGroup> GroupWatchlist { get; set; } = new List<WatchListGroup>();
        
        public static List<Reports> ReportsList { get; set; } = new List<Reports>();
        public static RemoteAdminModificationHandler Singleton;
        public static Dictionary<CedModPlayer, Tuple<int, DateTime>> ReportUnHandledState { get; set; } = new Dictionary<CedModPlayer, Tuple<int, DateTime>>();
        public static Dictionary<CedModPlayer, Tuple<int, DateTime>> ReportInProgressState { get; set; } = new Dictionary<CedModPlayer, Tuple<int, DateTime>>();

        public static Dictionary<CedModPlayer, IngameUserPreferences> IngameUserPreferencesMap = new Dictionary<CedModPlayer, IngameUserPreferences>();

        public static bool UiBlink { get; set; }

        public void Start()
        {
            Singleton = this;
        }
        
        public static Task UpdateReport(string reportId, string user, HandleStatus status, string reason)
        {
            return Task.Factory.StartNew(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Updating Report.");
                    var response = await client.PutAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/Reports/{QuerySystem.QuerySystemKey}?reportId={reportId}&status={status}&userid={user}", new StringContent(reason, Encoding.Default, "text/plain"));
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to update report {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        await Singleton.GetReports();
                    }
                }
            });
        }

        public static void UpdateReportList()
        {
            Singleton.ReportGetTimer = 30;
        }
        
        public static void UpdateWatchList()
        {
            Singleton.WatchlistGetTimer = 30;
        }
        
        public void Update()
        {
            ReportGetTimer += Time.deltaTime;
            WatchlistGetTimer += Time.deltaTime;

            if (ReportGetTimer >= 30)
            {
                ReportGetTimer = 0;
                Task.Factory.StartNew(async () => await GetReports());
            }
            
            if (WatchlistGetTimer >= 30)
            {
                WatchlistGetTimer = 0;
                Task.Factory.StartNew(async () => await GetWatchlist());
            }

            UiBlinkTimer += Time.deltaTime;
            if (UiBlinkTimer >= 1)
            {
                UiBlinkTimer = 0;
                UiBlink = !UiBlink;
            }
        }

        public async Task GetReports()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Getting Reports.");
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/Reports/{QuerySystem.QuerySystemKey}");
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to check for reports: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        var dat = JsonConvert.DeserializeObject<ReportGetresponse>(await response.Content.ReadAsStringAsync());
                        List<Reports> reportsList = new List<Reports>();
                        foreach (var rept in dat.Reports)
                        {
                            if (dat.ReportUserIdMap.ContainsKey(rept.Id.ToString()))
                            {
                                rept.AssignedHandler = dat.UserIdMap.FirstOrDefault(s => s.Key == dat.ReportUserIdMap[rept.Id.ToString()]).Value;
                            }
                            reportsList.Add(rept);
                        }

                        ReportsList = reportsList;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error ocurred when trying to load Reports: {e}");
            }
        }
        
        public async Task GetWatchlist()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Getting Watchlist.");
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/Watchlist/{QuerySystem.QuerySystemKey}");
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to check for reports: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        var dat = JsonConvert.DeserializeObject<WatchListGetResponse>(await response.Content.ReadAsStringAsync());
                    
                        List<WatchList> watchLists = new List<WatchList>();
                        foreach (var rept in dat.WatchList.List)
                        {
                            if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                                Log.Debug($"Resolving user for {rept.Id}");
                            if (dat.WatchList.IdMap.ContainsKey(rept.Id.ToString()))
                            {
                                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                                    Log.Debug($"Found user in map for {rept.Id} as {dat.WatchList.IdMap[rept.Id.ToString()]} {dat.UserIdMap.Any(s => s.Key == dat.WatchList.IdMap[rept.Id.ToString()])}");
                                rept.Issuer = dat.UserIdMap.FirstOrDefault(s => s.Key == dat.WatchList.IdMap[rept.Id.ToString()]).Value;
                            }
                            watchLists.Add(rept);
                        }

                        Watchlist = watchLists;
                    
                        List<WatchListGroup> watchListGroups = new List<WatchListGroup>();
                        foreach (var rept in dat.WatchListGroup.List)
                        {
                            if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                                Log.Debug($"Resolving user for {rept.Id}");
                            if (dat.WatchListGroup.IdMap.ContainsKey(rept.Id.ToString()))
                            {
                                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                                    Log.Debug($"Found user in map for {rept.Id} as {dat.WatchListGroup.IdMap[rept.Id.ToString()]} {dat.UserIdMap.Any(s => s.Key == dat.WatchListGroup.IdMap[rept.Id.ToString()])}");
                                rept.Issuer = dat.UserIdMap.FirstOrDefault(s => s.Key == dat.WatchListGroup.IdMap[rept.Id.ToString()]).Value;
                            }
                            watchListGroups.Add(rept);
                        }

                        GroupWatchlist = watchListGroups;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error ocurred when trying to load Watchlist: {e}");
            }
        }
        
        public List<string> Requesting = new List<string>();

        public IEnumerator<float> ResolvePreferences(CedModPlayer player, Action callback)
        {
            if (Requesting.Contains(player.UserId))
                yield break; 
            
            Requesting.Add(player.UserId);
            
            UnityWebRequest www = new UnityWebRequest($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/GetUserPreferences/{QuerySystem.QuerySystemKey}?id={player.UserId}", "OPTIONS");
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            
            yield return Timing.WaitUntilDone(www.SendWebRequest());
            try
            {
                if (www.responseCode != 200)
                {
                    if (www.responseCode == 400 && (www.downloadHandler.text == "user does not have prefs setup" || www.downloadHandler.text == "User could not be found, please ensure you have a CedMod account linked to the correct SteamId and DiscordId"))
                    {
                        
                    }
                    else
                    {
                        Log.Error($"Failed to Request UserPreferences: {www.responseCode} | {www.downloadHandler.text}");
                    }
                }
                else
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Got preferences: {www.downloadHandler.text}");
                    IngameUserPreferences cmData = JsonConvert.DeserializeObject<IngameUserPreferences>(www.downloadHandler.text);
                    if (IngameUserPreferencesMap.ContainsKey(player))
                        IngameUserPreferencesMap[player] = cmData;
                    else 
                        IngameUserPreferencesMap.Add(player, cmData);
                    
                    if (callback != null)
                        callback.Invoke();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            www.Dispose();
            Requesting.Remove(player.UserId);
        }
    }
    
    public class WatchListGroupResponse
    {
        public List<WatchListGroup> List { get; set; }
        public Dictionary<string, string> IdMap { get; set; }
    }
    
    public class WatchListResponse
    {
        public List<WatchList> List { get; set; }
        public Dictionary<string, string> IdMap { get; set; }
    }
    
    public class WatchListGetResponse
    {
        public WatchListResponse WatchList { get; set; }
        public WatchListGroupResponse WatchListGroup { get; set; }
        public Dictionary<string, UserObject> UserIdMap { get; set; }
    }
    
    public class WatchListGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public List<string> UserIds { get; set; }
        public UserObject Issuer { get; set; }
        public string Reason { get; set; }
        public bool DiscordNotify { get; set; }
        public DateTime LastNotified { get; set; }
        public DateTime Added { get; set; }
        public TimeSpan? TimedEntry { get; set; }
    }
    
    public class WatchList
    {
        public int Id { get; set; }
        public string Userid { get; set; }
        public UserObject Issuer { get; set; }
        public string Reason { get; set; }
        public bool DiscordNotify { get; set; }
        public bool BanWatch { get; set; } = false;
        public DateTime LastNotified { get; set; }
        public DateTime Added { get; set; }
        public TimeSpan? TimedEntry { get; set; }
    }

    public class ReportGetresponse
    {
        public List<Reports> Reports { get; set; }
        public Dictionary<string, UserObject> UserIdMap { get; set; }
        public Dictionary<string, string> ReportUserIdMap { get; set; }
    }

    public class Reports
    {
        public int Id { get; set; }
        public string ReporterId { get; set; }
        public string ReportedId { get; set; }
        public string Reason { get; set; }
        public DateTime Created { get; set; }
        public DateTime? HandleTime { get; set; }
        public HandleStatus Status { get; set; }
        public string QueryServer { get; set; }
        public string ClosureData { get; set; }
        public bool IsCheatReport { get; set; }
        public bool IsDiscordReport { get; set; }
        public UserObject AssignedHandler { get; set; }
    }

    public class UserObject
    {
        public string UserName { get; set; }
        public string SteamId { get; set; }
        public string DiscordId { get; set; }
        public string AdditionalId { get; set; }
    }

    public enum HandleStatus
    {
        NoResponse,
        InProgress,
        Ignored,
        Handled
    }
}