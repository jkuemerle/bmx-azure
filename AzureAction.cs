using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Text;
using System.IO;
using System.Linq;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;

namespace Inedo.BuildMasterExtensions.Azure
{
    public abstract class AzureAction : RemoteActionBase  
    {
        internal protected enum RequestType { Get, Post, Delete };

        internal AzureConfigurer TestConfigurer { get; set; }
        protected static string OperationVersion = "2012-03-01";
        protected static XNamespace ns = "http://schemas.microsoft.com/windowsazure";
        
        [Persistent]
        public AzureAuthentication ActionCredentials {get;set;}

        protected AzureConfigurer Configurer
        {
            get
            {
                return this.GetExtensionConfigurer() as AzureConfigurer;
            }
        }

        protected AzureAuthentication Credentials
        {
            get
            {
                // use local first
                if (null != ActionCredentials)
                    return ActionCredentials;
                return Configurer.Credentials;
            }
        }

        protected string ResolveDirectory(string FilePath)
        {
            using (var sourceAgent2 = Util.Agents.CreateLocalAgent())
            {
                var sourceAgent = sourceAgent2.GetService<IFileOperationsExecuter>();

                char srcSeparator = sourceAgent.GetDirectorySeparator();
                var srcPath = sourceAgent.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);

                LogInformation("Source Path: " + srcPath);
                return srcPath;
            }
        }

        internal protected IDictionary<string, string> ListLocations()
        {
            Dictionary<string,string> retVal = new Dictionary<string,string>();
            var resp = AzureRequest(RequestType.Get, null, "https://management.core.windows.net/{0}/locations");
            if (HttpStatusCode.OK == resp.StatusCode)
            {
                XElement locations = resp.Document.Element(ns + "Locations");
                foreach (XElement location in locations.Elements(ns + "Location"))
                {
                    retVal.Add(location.Element(ns + "Name").Value, location.Element(ns + "DisplayName").Value);
                }
            }
            return retVal;
        }


        internal protected IDictionary<string, string> ListAffinityGroups()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            var resp = AzureRequest(RequestType.Get, null, "https://management.core.windows.net/{0}/affinitygroups");
            if (HttpStatusCode.OK == resp.StatusCode)
            {
                XElement groups = resp.Document.Element(ns + "AffinityGroups");
                foreach (XElement group in groups.Elements(ns + "AffinityGroup"))
                {
                    retVal.Add(group.Element(ns + "Name").Value, group.Element(ns + "Description").Value);
                }
            }
            return retVal;
        }

        internal protected AzureResponse WaitForRequestCompletion(string RequestID)
        {
            AzureResponse retVal = new AzureResponse() { OperationStatus = AzureResponse.OperationStatusResult.Failed, ErrorCode = "-1", ErrorMessage = "General Failure" };
            try
            {
                bool done = false;
                var startTime = DateTime.Now;
                while (!done)
                {
                    if (DateTime.Now.Subtract(startTime) > TimeSpan.FromHours(24))
                        throw new Exception(string.Format("Wait For Completion timeout error. The build has taken more than 24 hours."));
                    var resp = AzureRequest(RequestType.Get, null, "https://management.core.windows.net/{0}/operations/{1}",RequestID);
                    if (HttpStatusCode.OK != resp.StatusCode)
                    {
                        LogError("HTTP Error {0} waiting for the completion of request {1}", resp.StatusCode, RequestID);
                        retVal.ErrorMessage = string.Format("HTTP Error {0} waiting for the completion of request {1}", resp.StatusCode, RequestID);
                        done = true;
                    }
                    else
                    {
                        var result = (AzureResponse.OperationStatusResult)Enum.Parse(typeof(AzureResponse.OperationStatusResult), resp.Document.Root.Element(ns + "Status").Value);
                        if (AzureResponse.OperationStatusResult.InProgress == result)
                            Thread.Sleep(2000);
                        else
                        {
                            retVal.OperationStatus = result;
                            retVal.Document = resp.Document;
                            retVal.StatusCode = (HttpStatusCode) Enum.Parse(typeof(HttpStatusCode),  resp.Document.Root.Element(ns + "HttpStatusCode").Value);
                            if (AzureResponse.OperationStatusResult.Succeeded == result)
                            {
                                retVal.ErrorCode = string.Empty;
                                retVal.ErrorMessage = string.Empty;
                                LogInformation("Succeeded waiting for request {0}",RequestID);
                            }
                            else
                            {
                                retVal.ErrorCode = resp.Document.Root.Element(ns + "Code").Value;
                                retVal.ErrorMessage = resp.Document.Root.Element(ns + "Message").Value;
                                LogInformation("Failed waiting for request {0}. Error code is {1} and message is: {2}",RequestID,retVal.ErrorCode,retVal.ErrorMessage);
                            }
                            done = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Unable to wait for the completion of request {0}. Error is: {1}", RequestID, ex.ToString());
            }
            return retVal;
        }

        internal AzureResponse AzureRequest(RequestType RequestType, string Payload, string UriFormat, params object[] args)
        {
            AzureResponse retval = new AzureResponse();
            var newargs = new List<object>();
            newargs.Add(Credentials.SubscriptionID);
            var uri = new Uri(string.Format(UriFormat,newargs.Concat(args).ToArray<object>()));
            var req = (HttpWebRequest)HttpWebRequest.Create(uri);
            switch (RequestType)
            {
                case AzureAction.RequestType.Get :
                    req.Method = "GET";
                    break;
                case AzureAction.RequestType.Post :
                    req.Method = "POST";
                    break;
                case AzureAction.RequestType.Delete :
                    req.Method = "DELETE";
                    break;
                default :
                    req.Method = "GET";
                    break;
            }
            req.Headers.Add("x-ms-version", OperationVersion);
            req.ClientCertificates.Add(this.Credentials.Certificate);
            req.ContentType = "application/xml";
            if(!string.IsNullOrEmpty(Payload))
            {
                var buffer = Encoding.UTF8.GetBytes(Payload);
                req.ContentLength = buffer.Length;
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();
            }
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                resp = (HttpWebResponse)ex.Response;
            }
            retval.StatusCode = resp.StatusCode;
            retval.Headers = resp.Headers;
            if (resp.ContentLength > 0)
            {
                using (XmlReader reader = XmlReader.Create(resp.GetResponseStream()))
                {
                    retval.Document = XDocument.Load(reader);
                }
            }
            return retval;
        }


    }
}
