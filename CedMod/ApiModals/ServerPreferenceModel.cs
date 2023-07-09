using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class ServerPreferenceModel
    {
        public long ServerId { get; set; }
        public List<BanList> BanListReadBans { get; set; } = new List<BanList>();
        public List<BanList> BanListWriteBans { get; set; } = new List<BanList>();
        public List<BanList> BanListReadMutes { get; set; } = new List<BanList>();
        public List<BanList> BanListWriteMutes { get; set; } = new List<BanList>();
        public List<BanList> BanListReadWarns { get; set; } = new List<BanList>();
        public List<BanList> BanListWriteWarns { get; set; } = new List<BanList>();
    }
    
    public class BanList
    {
        public long Id { get; set; }
        public bool IsMaster { get; set; }
        public bool IsDefaultPanel { get; set; }
        public string ServersWriteBans { get; set; } = "";
        public string ServersReadBans { get; set; } = "";
        public string ServersWriteMutes { get; set; } = "";
        public string ServersReadMutes { get; set; } = "";
        public string ServersWriteWarns { get; set; } = "";
        public string ServersReadWarns { get; set; } = "";
        public string Data { get; set; }
    }
}