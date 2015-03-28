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
        "Create Hosted Service",
        "Creates a new cloud service in Windows Azure.")]
    [Tag("windows-azure")]
    [CustomEditor(typeof(CreateHostedServiceActionEditor))]
    public class CreateHostedServiceAction : AzureComputeActionBase  
    {

        [Persistent]
        public string Label { get; set; }

        [Persistent]
        public string Description { get; set; }

        [Persistent]
        public string Location { get; set; }

        [Persistent]
        public string AffinityGroup { get; set; }

        public override string ToString()
        {
            return string.Format("Creating cloud service {0} for subscription {1}", this.ServiceName, this.Credentials.SubscriptionID);
        }

        public CreateHostedServiceAction()
        {
            this.UsesServiceName = true;
            this.UsesWaitForCompletion = true;
            this.UsesExtendedProperties = true;
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
            var resp = AzureRequest(RequestType.Post, BuildRequestDocument(),"https://management.core.windows.net/{0}/services/hostedservices");
            if (HttpStatusCode.Created != resp.StatusCode)
            {
                LogError("Error creating Hosted Service named {0}. Error code is: {1}, error description: {2}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

        internal string BuildRequestDocument()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CreateHostedService xmlns=\"http://schemas.microsoft.com/windowsazure\">");
            body.AppendFormat("<ServiceName>{0}</ServiceName>\r\n",this.ServiceName);
            body.AppendFormat("<Label>{0}</Label>\r\n", Base64Encode(this.Label));
            body.AppendFormat("<Description>{0}</Description>\r\n",this.Description);
            if(!string.IsNullOrEmpty(this.Location))
                body.AppendFormat("<Location>{0}</Location>\r\n",this.Location);
            else
                body.AppendFormat("<AffinityGroup>{0}</AffinityGroup>\r\n",this.AffinityGroup);
            body.Append(ParseExtendedProperties());
            body.Append("</CreateHostedService>\r\n");
            return body.ToString();
        }

    }
}
