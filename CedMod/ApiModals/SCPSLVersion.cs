using System;
using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class SCPSLVersion
    {
        public int Id { get; set; }
        public string VersionString { get; set; }
        public List<SCPSLExiledVersionAssociation> SupportedExiledVersions { get; set; }
        public DateTime Released { get; set; }
        public VersionType VersionType { get; set; }
    }

    public enum VersionType
    {
        Release,
        PublicBeta,
        PrivateRelease,
    }
}