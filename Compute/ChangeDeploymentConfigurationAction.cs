using System.Linq;
using System.Net;
using System.Xml.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Azure
{
    [ActionProperties(
        "Change Deployment Configuration",
        "Updates the configuration of a deployment in Windows Azure.")]
    [Tag("windows-azure")]
    [CustomEditor(typeof(ChangeDeploymentConfigurationActionEditor))]
    public class ChangeDeploymentConfigurationAction : AzureActionWithConfigBase  
    {
        public ChangeDeploymentConfigurationAction()
        {
            this.UsesServiceName = true;
            this.UsesDeploymentName = true;
            this.UsesSlotName = true;
            this.UsesTreatWarningsAsError = true;
            this.UsesWaitForCompletion = true;
            this.UsesExtendedProperties = true;
            this.UsesExtensionConfiguration = true;
        }

        public enum ChangeModeType { Auto, Manual };

        [Persistent]
        public ChangeModeType Mode { get; set; }

        public override string ToString()
        {
            return string.Format("Changing {0} deployment configuration for {0}", 
                (string.IsNullOrEmpty(this.DeploymentName) ? this.SlotName : this.DeploymentName),
                this.ServiceName);
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            string requestID = string.Empty;
            requestID = MakeRequest();
            if (string.IsNullOrEmpty(requestID))
                return null;
            if (this.WaitForCompletion)
                this.WaitForRequestCompletion(requestID);
            return requestID;
        }

        internal string MakeRequest()
        {
            AzureResponse resp;
            if(string.IsNullOrEmpty(this.DeploymentName))
                resp = AzureRequest(RequestType.Post, BuildRequestDocument(),
                    "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}/?comp=config",
                    this.ServiceName, this.SlotName.ToString().ToLowerInvariant());
            else
                resp = AzureRequest(RequestType.Post, BuildRequestDocument(),
                    "https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/?comp=config",
                    this.ServiceName, this.DeploymentName);
            if (HttpStatusCode.Accepted != resp.StatusCode)
            {
                LogError("Error changing configuration for the {0} deployment in {1}. Error code is: {2}, error description: {3}", 
                    (string.IsNullOrEmpty(this.DeploymentName) ? this.SlotName : this.DeploymentName), this.ServiceName, 
                    resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

        private string BuildRequestDocument()
        {
            return new XDocument(
                new XElement(ns + "ChangeConfiguration",
                    new XElement(ns + "Configuration", Base64Encode(this.GetConfigurationFileContents())),
                    new XElement(ns + "TreatWarningsAsError", this.TreatWarningsAsError.ToString().ToLowerInvariant()),
                    new XElement(ns + "Mode", this.Mode),
                    this.ParseExtendedProperties2(),
                    !string.IsNullOrEmpty(this.ExtensionConfiguration) ? (object)new XElement(ns + "ExtensionConfiguration", this.ExtensionConfiguration) : Enumerable.Empty<XElement>()
                )
            ).ToString(SaveOptions.DisableFormatting);
        }
    }
}
