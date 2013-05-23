using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Inedo.BuildMasterExtensions.Azure
{
    public class AzureResponse
    {
        public enum OperationStatusResult { InProgress, Succeeded, Failed };

        public HttpStatusCode StatusCode { get; set; }

        public XDocument Document { get; set; }

        public OperationStatusResult OperationStatus {get; set;}

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public WebHeaderCollection Headers { get; set; }
    }
}
