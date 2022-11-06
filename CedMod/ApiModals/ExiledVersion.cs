using System;
using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class ExiledVersion
    {
        public int Id { get; set; }
        public string VersionString { get; set; }
        public List<SCPSLExiledVersionAssociation> SupportedSCPSLVersions { get; set; }
        public List<CedModExiledVersionAssociation> SupportedCedModVersions { get; set; }
        public DateTime Released { get; set; }
        public VersionType VersionType { get; set; }
    }

    public class SCPSLExiledVersionAssociation
    {
        public int Id { get; set; }
        public int ScpSlVersionId { get; set; }
        public SCPSLVersion ScpSlVersion { get; set; }
    
        public int ExiledVersionId { get; set; }
        public ExiledVersion ExiledVersion { get; set; }
    }
}