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
        "Delete Hosted Service",
        "Deletes a cloud service in Windows Azure.",
        "Windows Azure")]
    [CustomEditor(typeof(DeleteHostedServiceActionEditor))]
    public class DeleteHostedServiceAction : AzureComputeActionBase 
    {

        public override string ToString()
        {
            return string.Format("Deleting cloud service {0} for subscription {1}", this.ServiceName, this.Credentials.SubscriptionID);
        }

        public DeleteHostedServiceAction()
        {
            this.UsesServiceName = true;
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
            var resp = AzureRequest(RequestType.Delete, null, "https://management.core.windows.net/{0}/services/hostedservices/{1}",this.ServiceName);
            if (HttpStatusCode.OK != resp.StatusCode)
            {
                LogError("Error deleting Hosted Service named {0}. Error code is: {1}, error description: {1}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

    }
}
