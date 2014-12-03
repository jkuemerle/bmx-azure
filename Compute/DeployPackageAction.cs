using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Inedo.BuildMasterExtensions.Azure
{
    [ActionProperties(
        "Deploy Package",
        "Deploys a Cloud Service package onto Windows Azure.")]
    [Tag("windows-azure")]
    [CustomEditor(typeof(DeployPackageActionEditor))]
    public class DeployPackageAction : AzureActionWithConfigBase  
    {
        private Uri blobFileUri;

        [Persistent]
        public string PackageFile { get; set; }

        [Persistent]
        public string PackageFileStorageLocation { get; set; }

        [Persistent]
        public string StorageAccountName { get; set; }

        [Persistent]
        public string StorageAccessKey { get; set; }

        [Persistent]
        public string Label { get; set; }

        [Persistent]
        public bool StartDeployment { get; set; }

        [Persistent]
        public DeploymentSlotType DeploymentSlot { get; set; }

        [Persistent]
        public bool DeletePackageFromStorage { get; set; }

        public override string ToString()
        {
            return string.Format("Deploying package to subscription {0}", this.Credentials.SubscriptionID);
        }

        public DeployPackageAction()
        {
            this.UsesServiceName = true;
            this.UsesSlotName = true;
            this.UsesDeploymentName = true;
            this.UsesTreatWarningsAsError = true;
            this.UsesWaitForCompletion = true;
            this.UsesExtendedProperties = true;
            this.UsesExtensionConfiguration = true;
            this.StartDeployment = true;
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
            if (!UploadPackage())
            {
                LogError("Error uploading package");
                return null;
            }
            string requestID = string.Empty;
            requestID = MakeRequest();
            if (string.IsNullOrEmpty(requestID))
                return null;
            if (this.WaitForCompletion)
                this.WaitForRequestCompletion(requestID);
            if (this.DeletePackageFromStorage)
                DeletePackage();
            return requestID;
        }

        internal string MakeRequest()
        {
            var resp = AzureRequest(RequestType.Post, BuildRequestDocument(),
                "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}",
                this.ServiceName, this.SlotName.ToLowerInvariant());
            if (HttpStatusCode.Accepted != resp.StatusCode)
            {
                LogError("Error deploying package to {0}. Error code is: {1}, error description: {2}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

        internal string BuildRequestDocument()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CreateDeployment xmlns=\"http://schemas.microsoft.com/windowsazure\">\r\n");
            body.AppendFormat("<Name>{0}</Name>\r\n",this.DeploymentName);
            body.AppendFormat("<PackageUrl>{0}</PackageUrl>",this.blobFileUri);
            body.AppendFormat("<Label>{0}</Label>", Base64Encode(this.Label));
            body.AppendFormat("<Configuration>{0}</Configuration>", Base64Encode(this.GetConfigurationFileContents()));
            body.AppendFormat("<StartDeployment>{0}</StartDeployment>",this.StartDeployment.ToString().ToLowerInvariant());
            body.AppendFormat("<TreatWarningsAsError>{0}</TreatWarningsAsError>",this.TreatWarningsAsError.ToString().ToLowerInvariant());
            body.Append(ParseExtendedProperties());
            if (!string.IsNullOrEmpty(this.ExtensionConfiguration))
                body.AppendFormat("<ExtensionConfiguration>{0}</ExtensionConfiguration>", this.ExtensionConfiguration);
            body.Append("</CreateDeployment>\r\n");
            return body.ToString();
        }

        internal bool UploadPackage()
        {
            if (!string.IsNullOrEmpty(this.PackageFileStorageLocation))
            {
                this.LogDebug("Using stored file at location {0}", this.PackageFileStorageLocation);
                this.blobFileUri = new Uri(PackageFileStorageLocation);
                return true;
            }

            try
            {
                this.LogDebug("Preparing to upload file...");
                var account = new CloudStorageAccount(new StorageCredentials(this.StorageAccountName, this.StorageAccessKey), true);
                var blobClient = account.CreateCloudBlobClient();
                blobClient.SingleBlobUploadThresholdInBytes = 63 * 1024 * 1024;
                var container = blobClient.GetContainerReference(BlobContainer);
                this.LogDebug("Creating container \"{0}\" if it doesn't already exist...", BlobContainer);
                container.CreateIfNotExists();
                string package = this.ResolveDirectory(this.PackageFile);
                if (!File.Exists(package))
                {
                    LogError("UploadPackage unable to locate package file at: {0}", package);
                    return false;
                }
                string blobFileName = Path.GetFileNameWithoutExtension(package) + Guid.NewGuid().ToString() + (Path.HasExtension(package) ? Path.GetExtension(package) : "");
                var blob = container.GetBlockBlobReference(blobFileName);
                blob.StreamWriteSizeInBytes = 64 * 1024;
                this.LogInformation("Uploading package {0} to blob file {1} for deployment.", package, blobFileName);

                var transfer = new BlobTransfer(blob);
                transfer.TransferProgressChanged += (s, e) =>
                {
                    this.LogDebug("Upload Progress: {0}/{1} ({2}%) - Est. Time Remaining: {3}", e.BytesSent, e.TotalBytesToSend, e.ProgressPercentage, e.TimeRemaining);
                };
                transfer.TransferCompleted += (s, e) =>
                {
                    this.LogInformation("Package {0} uploaded successfully.", package);
                };
                
                var result = transfer.UploadBlobAsync(package);

                int handled = WaitHandle.WaitAny(new[] { result.AsyncWaitHandle, this.Context.CancellationToken.WaitHandle });
                if (handled == 1)
                {
                    result.Cancel();
                    this.ThrowIfCanceledOrTimeoutExpired();
                }
                
                this.blobFileUri = blob.Uri;
                
                return true;
            }
            catch (Exception ex)
            {
                LogError("UploadPackage error: {0}", ex.ToString());
                return false;
            }
        }

        internal bool DeletePackage()
        {
            bool retVal = false;
            try
            {
                var account = new CloudStorageAccount(new StorageCredentials(this.StorageAccountName, this.StorageAccessKey), true);
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(BlobContainer);
                container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference(this.blobFileUri.ToString());
                if (null != blob)
                {
                    blob.Delete();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError("Delete Package error: {0}", ex.ToString());
                retVal = false;
            }
            return retVal;
        }
    }
}