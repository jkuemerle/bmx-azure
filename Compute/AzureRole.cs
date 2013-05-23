using System;

namespace Inedo.BuildMasterExtensions.Azure
{
    [Serializable]
    public class AzureRole
    {
        public string RoleName { get; set; }

        public string RoleBinDirectory { get; set; }

        public string RoleAssemblyName { get; set; }
    }
}
