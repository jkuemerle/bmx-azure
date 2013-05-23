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
        "Swap Deployment",
        "Swaps a production and staging deployment in Windows Azure.",
        "Windows Azure")]
    [CustomEditor(typeof(SwapDeploymentActionEditor))]
    public class SwapDeploymentAction : AzureComputeActionBase  
    {

        [Persistent]
        public string ProductionDeploymentName { get; set; }

        [Persistent]
        public string SourceDeploymentName { get; set; }

        public override string ToString()
        {
            return string.Format("Swapping production deployment {0} with {1} package for the {2} service in subscription {3}",
                this.ProductionDeploymentName, this.SourceDeploymentName, this.ServiceName, this.Credentials.SubscriptionID);
        }

        public SwapDeploymentAction()
        {
            this.UsesServiceName = true;
            this.UsesWaitForCompletion = true;
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
            var resp = AzureRequest(RequestType.Post, BuildRequestDocument(),
                "https://management.core.windows.net/{0}/services/hostedservices/{1}",
                this.ServiceName);
            if (HttpStatusCode.Accepted != resp.StatusCode)
            {
                LogError("Error swapping production deployment {0} with {1} in service {2} . Error code is: {3}, error description: {4}", 
                    this.ProductionDeploymentName, this.SourceDeploymentName, this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

        internal string BuildRequestDocument()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Swap xmlns=\"http://schemas.microsoft.com/windowsazure\">\r\n");
            body.AppendFormat("<Production>{0}</Production>\r\n",this.ProductionDeploymentName);
            body.AppendFormat("<SourceDeployment>{0}</SourceDeployment>\r\n",this.SourceDeploymentName);
            body.Append("</Swap>\r\n");
            return body.ToString();
        }
    }
}
