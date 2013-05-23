using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Azure
{
    [ActionProperties(
        "Change Deployment Configuration",
        "Updates the configuration of a deployment in Windows Azure.",
        "Windows Azure")]
    [CustomEditor(typeof(ChangeDeploymentConfigurationActionEditor))]
    public class ChangeDeploymentConfigurationAction : AzureActionWithConfigBase  
    {
        public enum ChangeModeType { Auto, Manual };


        [Persistent]
        public ChangeModeType Mode { get; set; }

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


        public override string ToString()
        {
            return string.Format("Changing {0} deployment configuration for {0}", 
                (string.IsNullOrEmpty(this.DeploymentName) ? this.SlotName : this.DeploymentName),
                this.ServiceName);
        }

        internal string Test()
        {
            return this.ProcessRemoteCommand(null, null);
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

        internal string BuildRequestDocument()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ChangeConfiguration xmlns=\"http://schemas.microsoft.com/windowsazure\">\r\n");
            body.AppendFormat("<Configuration>{0}</Configuration>",this.GetConfigurationFileContents().AsBase64());
            body.AppendFormat("<TreatWarningsAsError>{0}</TreatWarningsAsError>",this.TreatWarningsAsError.ToString().ToLowerInvariant());
            body.AppendFormat("<Mode>{0}</Mode>", this.Mode);
            body.Append(ParseExtendedProperties());
            if (!string.IsNullOrEmpty(this.ExtensionConfiguration))
                body.AppendFormat("<ExtensionConfiguration>{0}</ExtensionConfiguration>", this.ExtensionConfiguration);
            body.Append("</ChangeConfiguration>\r\n");
            return body.ToString();
        }

    }
}
