using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Azure
{
    [ActionProperties(
        "Delete Deployment",
        "Deletes a deployment by slot or by name from a cloud service in Windows Azure.",
        "Windows Azure")]
    [CustomEditor(typeof(DeleteDeploymentActionEditor))]
    public class DeleteDeploymentAction : AzureComputeActionBase 
    {

        public override string ToString()
        {
            return string.Format("Deleting deployment {0} for service {1}", 
                (string.IsNullOrEmpty(this.DeploymentName) ? this.SlotName : this.DeploymentName),
                this.ServiceName);
        }

        public DeleteDeploymentAction()
        {
            this.UsesServiceName = true;
            this.UsesDeploymentName = true;
            this.UsesSlotName = true;
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
            if(string.IsNullOrEmpty(requestID))
                return null;
            if(this.WaitForCompletion)
                this.WaitForRequestCompletion(requestID);
            return requestID;
        }

        internal string MakeRequest()
        {
            AzureResponse resp = null;
            if(string.IsNullOrEmpty(this.DeploymentName))
                resp = AzureRequest(RequestType.Delete, null, "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}", 
                    this.ServiceName, this.SlotName);
            else
                resp = AzureRequest(RequestType.Delete, null, "https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}",
                    this.ServiceName, this.DeploymentName);
            if (HttpStatusCode.Accepted != resp.StatusCode)
            {
                LogError("Error deleting Hosted Service named {0}. Error code is: {1}, error description: {1}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

    }
}
