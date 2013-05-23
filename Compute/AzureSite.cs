using System;

namespace Inedo.BuildMasterExtensions.Azure
{
    [Serializable]
    public class AzureSite
    {
        public string RoleName { get; set; }

        public string VirtualPath { get; set; }

        public string PhysicalPath { get; set; }
    }
}
