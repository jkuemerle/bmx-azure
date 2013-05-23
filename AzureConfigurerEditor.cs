using System;

using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class AzureConfigurerEditor : ExtensionConfigurerEditorBase
    {
        //DropDownList ddlServer;
        SourceControlFileFolderPicker sdkPath;
        //StyledRequiredFieldValidator vreq_ddlServer;
        ValidatingTextBox txtSubscriptionID;
        ValidatingTextBox txtCertificateName;
        

        public AzureConfigurerEditor()
        {
            //ddlServer = new DropDownList();
            //ddlServer.ID = "ddlServer";
            //ddlServer.AutoPostBack = true;
            //ddlServer.SelectedIndexChanged += delegate(object sender, EventArgs e)
            //{
            //    sdkPath.Text = null;
            //};

            //vreq_ddlServer = new StyledRequiredFieldValidator();
            //vreq_ddlServer.ID = "vreq_ddlServer";
            //vreq_ddlServer.InitialValue = "0";

            sdkPath = new SourceControlFileFolderPicker();
            sdkPath.ID = "sdkPath";
            sdkPath.DisplayMode = SourceControlBrowser.DisplayModes.Folders;

            txtSubscriptionID = new ValidatingTextBox() { Width = 300 };
            txtCertificateName = new ValidatingTextBox() { Width = 300 };
        }

        public override void InitializeDefaultValues()
        {
            BindToForm(new AzureConfigurer());
        }

        /// <summary>
        /// Binds to form.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public override void BindToForm(ExtensionConfigurerBase extension)
        {
            var configurer = (AzureConfigurer)extension;
            //this.ddlServer.SelectedValue = configurer.ServerID.ToString();
            this.sdkPath.Text = configurer.AzureSDKPath;
            this.txtSubscriptionID.Text = configurer.Credentials.SubscriptionID;
            this.txtCertificateName.Text = configurer.Credentials.CertificateName;
        }

        /// <summary>
        /// Creates from form.
        /// </summary>
        /// <returns></returns>
        public override ExtensionConfigurerBase CreateFromForm()
        {
            var configurer = new AzureConfigurer()
            {
                ServerID = 1, // int.Parse(this.ddlServer.SelectedValue),
                AzureSDKPath = this.sdkPath.Text,
                Credentials = new AzureAuthentication() { SubscriptionID = this.txtSubscriptionID.Text, CertificateName = this.txtCertificateName.Text  }
            };
            return configurer;
        }

        protected override void OnLoad(EventArgs e)
        {
            int sourceId = 1;
            //if (int.TryParse(ddlServer.SelectedValue, out sourceId))
                sdkPath.ServerId = sourceId;
            base.OnLoad(e);
        }
        protected override void OnInit(EventArgs e)
        {
            CUtil.Add(this,
                new FormFieldGroup("SDK", "The location of the Windows Azure SDK bin directory.", false,
                    //new StandardFormField("Server",ddlServer,vreq_ddlServer),
                    new StandardFormField("Path:",sdkPath)
                ),
                new FormFieldGroup("Authentication","Authentication Information used for all Windows Azure actions unless overridden in the action.",
                    true,
                    new StandardFormField("Subscription ID:",txtSubscriptionID),
                    new StandardFormField("Certificate Name:",txtCertificateName)
                )
            );
            //if (!IsPostBack)
            //{
            //    // Add Blank item
            //    ddlServer.Items.Add(new ListItem("", "0"));

            //    // Add Servers
            //    foreach (DataRow dr in StoredProcs
            //        .Environments_GetServers()
            //        .ExecuteDataTable()
            //        .Rows)
            //    {
                    
            //        if ((string)dr[TableDefs.Servers.ServerGroup_Indicator] == Domains.YN.No)
            //        {
            //                ddlServer.Items.Add(new ListItem(
            //                    dr[TableDefs.Servers.Server_Name].ToString(),
            //                    dr[TableDefs.Servers.Server_Id].ToString()));
            //        }
            //    }
            //}
            base.OnInit(e);
        }

    }
}
