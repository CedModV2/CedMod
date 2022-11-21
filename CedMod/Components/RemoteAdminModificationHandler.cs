using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.ApiModals;
using Newtonsoft.Json;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Components
{
    public class RemoteAdminModificationHandler: MonoBehaviour
    {
        public float ReportGetTimer { get; set; }
        public float UiBlinkTimer { get; set; }
        public static List<Reports> ReportsList { get; set; } = new List<Reports>();
        public static RemoteAdminModificationHandler Singleton;
        public static Dictionary<CedModPlayer, Tuple<int, DateTime>> ReportUnHandledState { get; set; } = new Dictionary<CedModPlayer, Tuple<int, DateTime>>();
        public static Dictionary<CedModPlayer, Tuple<int, DateTime>> ReportInProgressState { get; set; } = new Dictionary<CedModPlayer, Tuple<int, DateTime>>();

        public static bool UiBlink { get; set; }

        public void Start()
        {
            Singleton = this;
        }
        
        public static Task UpdateReport(string reportId, string user, HandleStatus status, string reason)
        {
            return Task.Factory.StartNew(() =>
            {
                using (HttpClient client = new HttpClient())
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Updating Report.");
                    var response = client.PutAsync("https://" + QuerySystem.CurrentMaster + $"/Api/v3/Reports/{QuerySystem.QuerySystemKey}?reportId={reportId}&status={status}&userid={user}",
                            new StringContent(reason, Encoding.Default, "text/plain")).Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to update report {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                    }
                    else
                    {
                        Singleton.GetReports();
                    }
                }
            });
        }

        public static void UpdateReportList()
        {
            Singleton.ReportGetTimer = 30;
        }
        
        public void Update()
        {
            ReportGetTimer += Time.deltaTime;

            if (ReportGetTimer >= 30)
            {
                ReportGetTimer = 0;
                Task.Factory.StartNew(() => GetReports());
            }

            UiBlinkTimer += Time.deltaTime;
            if (UiBlinkTimer >= 1)
            {
                UiBlinkTimer = 0;
                UiBlink = !UiBlink;
            }
        }

        public void GetReports()
        {
            using (HttpClient client = new HttpClient())
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Getting Reports.");
                var response = client.GetAsync("https://" + QuerySystem.CurrentMaster + $"/Api/v3/Reports/{QuerySystem.QuerySystemKey}").Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Log.Error($"Failed to check for reports: {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                }
                else
                {
                    var dat = JsonConvert.DeserializeObject<ReportGetresponse>(response.Content.ReadAsStringAsync().Result);
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