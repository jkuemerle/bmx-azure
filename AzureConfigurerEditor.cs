using System;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class AzureConfigurerEditor : ExtensionConfigurerEditorBase
    {
        private SourceControlFileFolderPicker sdkPath;
        private ValidatingTextBox txtSubscriptionID;
        private ValidatingTextBox txtCertificateName;

        public AzureConfigurerEditor()
        {
            this.sdkPath = new SourceControlFileFolderPicker();
            this.sdkPath.ID = "sdkPath";
            this.sdkPath.DisplayMode = SourceControlBrowser.DisplayModes.Folders;

            this.txtSubscriptionID = new ValidatingTextBox() { Width = 300 };
            this.txtCertificateName = new ValidatingTextBox() { Width = 300 };
        }

        public override void BindToForm(ExtensionConfigurerBase extension)
        {
            var configurer = (AzureConfigurer)extension;
            this.sdkPath.Text = configurer.AzureSDKPath;
            this.txtSubscriptionID.Text = configurer.Credentials.SubscriptionID;
            this.txtCertificateName.Text = configurer.Credentials.CertificateName;
        }

        public override ExtensionConfigurerBase CreateFromForm()
        {
            return new AzureConfigurer
            {
                ServerID = 1,
                AzureSDKPath = this.sdkPath.Text,
                Credentials = new AzureAuthentication() { SubscriptionID = this.txtSubscriptionID.Text, CertificateName = this.txtCertificateName.Text }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            int sourceId = 1;
            sdkPath.ServerId = sourceId;
            base.OnLoad(e);
        }
        protected override void OnInit(EventArgs e)
        {
            this.Controls.Add(
                new FormFieldGroup("SDK", "The location of the Windows Azure SDK bin directory.", false,
                    new StandardFormField("Path:", sdkPath)
                ),
                new FormFieldGroup("Authentication", "Authentication Information used for all Windows Azure actions unless overridden in the action.",
                    true,
                    new StandardFormField("Subscription ID:", txtSubscriptionID),
                    new StandardFormField("Certificate Name:", txtCertificateName)
                )
            );

            base.OnInit(e);
        }
    }
}
