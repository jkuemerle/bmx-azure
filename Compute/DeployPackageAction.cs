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

        public override ActionDescription GetActionDescription()
        {
            return new ActionDescription(
                new ShortActionDescription(
                    "Deploy ",
                    !string.IsNullOrEmpty(this.PackageFile) ? (object)new DirectoryHilite(this.OverriddenSourceDirectory, this.PackageFile) : "package"
                ),
                new LongActionDescription(
                    "to Azure subscription ",
                    new Hilite(this.Credentials != null ? this.Credentials.SubscriptionID : string.Empty)
                )
            );
        }

        protected override void Execute()
        {
            var blobUri = this.ExecuteRemoteCommand("upload");
            if (string.IsNullOrEmpty(blobUri))
                return;

            var requestDocument = this.BuildRequestDocument(blobUri);

            this.ExecuteRemoteCommand("request", requestDocument, blobUri);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            if (name == "upload")
            {
                var blobUri = this.UploadPackage();
                if (string.IsNullOrEmpty(blobUri))
                {
                    this.LogError("Error uploading package.");
                    return null;
                }

                return blobUri;
            }
            else if (name == "request")
            {
                var requestId = this.MakeRequest(args[0]);
                if (string.IsNullOrEmpty(requestId))
                    return null;

                if (this.WaitForCompletion)
                    this.WaitForRequestCompletion(requestId);

                if (this.DeletePackageFromStorage)
                    this.DeletePackage(args[1]);

                return string.Empty;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private string MakeRequest(string requestDocument)
        {
            var resp = this.AzureRequest(
                RequestType.Post,
                requestDocument,
                "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}",
                Uri.EscapeUriString(this.ServiceName),
                Uri.EscapeUriString(this.SlotName.ToLowerInvariant())
            );

            if (resp.StatusCode != HttpStatusCode.Accepted)
            {
                this.LogError("Error deploying package to {0}. Error code is: {1}, error description: {2}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }

            return resp.Headers.Get("x-ms-request-id");
        }

        private string BuildRequestDocument(string packageUrl)
        {
            var body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CreateDeployment xmlns=\"http://schemas.microsoft.com/windowsazure\">\r\n");
            body.AppendFormat("<Name>{0}</Name>\r\n", this.DeploymentName);
            body.AppendFormat("<PackageUrl>{0}</PackageUrl>", packageUrl);
            body.AppendFormat("<Label>{0}</Label>", Base64Encode(this.Label));
            body.AppendFormat("<Configuration>{0}</Configuration>", Base64Encode(this.GetConfigurationFileContents()));
            body.AppendFormat("<StartDeployment>{0}</StartDeployment>", this.StartDeployment.ToString().ToLowerInvariant());
            body.AppendFormat("<TreatWarningsAsError>{0}</TreatWarningsAsError>", this.TreatWarningsAsError.ToString().ToLowerInvariant());
            body.Append(ParseExtendedProperties());
            if (!string.IsNullOrEmpty(this.ExtensionConfiguration))
                body.AppendFormat("<ExtensionConfiguration>{0}</ExtensionConfiguration>", this.ExtensionConfiguration);
            body.Append("</CreateDeployment>\r\n");
            return body.ToString();
        }

        private string UploadPackage()
        {
            if (!string.IsNullOrEmpty(this.PackageFileStorageLocation))
            {
                this.LogDebug("Using stored file at location {0}", this.PackageFileStorageLocation);
                return this.PackageFileStorageLocation;
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
                    this.LogError("UploadPackage unable to locate package file at: {0}", package);
                    return null;
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

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                this.LogError("UploadPackage error: {0}", ex.ToString());
                return null;
            }
        }

        private bool DeletePackage(string blobFileUrl)
        {
            try
            {
                var account = new CloudStorageAccount(new StorageCredentials(this.StorageAccountName, this.StorageAccessKey), true);
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(BlobContainer);
                container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference(blobFileUrl);
                if (blob != null)
                    blob.Delete();

                return true;
            }
            catch (Exception ex)
            {
                this.LogError("Delete Package error: {0}", ex.ToString());
                return false;
            }
        }
    }
}