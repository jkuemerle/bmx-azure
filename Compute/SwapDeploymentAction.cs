﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Microsoft.WindowsAzure;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Azure
{
    [ActionProperties(
        "Swap Deployment",
        "Swaps a production and staging deployment in Windows Azure.")]
    [Tag("windows-azure")]
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
            requestID = MakeSwapDeploymentRequest();
            if (string.IsNullOrEmpty(requestID))
                return null;
            if (this.WaitForCompletion)
                this.WaitForRequestCompletion(requestID);
            return requestID;
        }

        internal string MakeSwapDeploymentRequest()
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

        private DeploymentNames GetDeploymentNames()
        {
            var names = new DeploymentNames();

            if (string.IsNullOrEmpty(this.ProductionDeploymentName))
            {
                var prod = AzureRequest(RequestType.Get, null,
                    "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/production",
                    this.ServiceName);
                names.Production = (string)prod.Document.Root.Element(ns + "Name");
            }
            else
            {
                names.Production = this.ProductionDeploymentName;
            }

            if (string.IsNullOrEmpty(this.SourceDeploymentName))
            {
                var source = AzureRequest(RequestType.Get, null,
                    "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/staging",
                    this.ServiceName);
                names.Source = (string)source.Document.Root.Element(ns + "Name");
            }
            else
            {
                names.Source = this.SourceDeploymentName;
            }

            return names;
        }

        internal string BuildRequestDocument()
        {
            var names = GetDeploymentNames();

            this.LogInformation("Swapping production deployment \"{0}\" with source deployment \"{1}\"...", names.Production, names.Source);

            StringBuilder body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Swap xmlns=\"http://schemas.microsoft.com/windowsazure\">\r\n");
            body.AppendFormat("<Production>{0}</Production>\r\n", names.Production);
            body.AppendFormat("<SourceDeployment>{0}</SourceDeployment>\r\n", names.Source);
            body.Append("</Swap>\r\n");
            return body.ToString();
        }

        private sealed class DeploymentNames 
        {
            public string Production { get; set; }
            public string Source {get; set;}
        }
    }
}
