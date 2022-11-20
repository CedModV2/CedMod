using System;
using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class CedModVersion
    {
        public int Id { get; set; }
        public Guid CedModVersionIdentifier { get; set; }
        public string VersionString { get; set; }
        public string VersionCommit { get; set; }
        public string ReleaseDownloadUser { get; set; }
        public string FileHash { get; set; }
        public string S3Path { get; set; }
        public List<CedModExiledVersionAssociation> SupportedExiledVersions { get; set; }
        public DateTime Released { get; set; }
        public VersionType VersionType { get; set; }
        public List<int> CanUpgrade { get; set; }
    }

    public class CedModExiledVersionAssociation
    {
        public int Id { get; set; }
        public int CedModVersionId { get; set; }
        public CedModVersion CedModVersion { get; set; }
    
        public int ExiledVersionId { get; set; }
        public ExiledVersion ExiledVersion { get; set; }
    }
}