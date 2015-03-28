using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.Azure.AzureConfigurer))]

namespace Inedo.BuildMasterExtensions.Azure
{
    [CustomEditor(typeof(AzureConfigurerEditor))]
    public sealed class AzureConfigurer : ExtensionConfigurerBase 
    {
        public AzureConfigurer()
        {
            this.ServerID = 1;
            this.Credentials = new AzureAuthentication();
        }

        [Persistent]
        public string AzureSDKPath { get; set; }

        [Persistent]
        public int ServerID { get; set; }

        [Persistent]
        public AzureAuthentication Credentials { get; set; }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
