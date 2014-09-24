using System.Net;
using System.Xml.Linq;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class AzureResponse
    {
        public enum OperationStatusResult { Unknown, InProgress, Succeeded, Failed };

        public HttpStatusCode StatusCode { get; set; }

        public XDocument Document { get; set; }

        public OperationStatusResult OperationStatus {get; set;}

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public WebHeaderCollection Headers { get; set; }
    }
}
