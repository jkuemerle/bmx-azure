using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;


namespace Inedo.BuildMasterExtensions.Azure
{
    public abstract class AzureComputeActionBaseEditor : ActionEditorBase
    {
        protected ValidatingTextBox txtServiceName;
        protected ValidatingTextBox txtDeploymentName;
        protected ValidatingTextBox txtSlotName;
        protected TextBox txtExtendedProperties;
        protected TextBox txtExtensionConfiguration;
        protected CheckBox chkWarningsAsError;
        protected CheckBox chkWaitForCompletion;
        protected ValidatingTextBox txtSubscriptionID;
        protected ValidatingTextBox txtCertificateName;

        protected AzureComputeActionBase extensionInstance;

        public AzureComputeActionBaseEditor()
        {
            this.txtServiceName = new ValidatingTextBox() { Width = 300, Required = true };
            this.txtDeploymentName = new ValidatingTextBox() { Width = 300 };
            this.txtSlotName = new ValidatingTextBox() { Width = 300 };
            this.txtExtendedProperties = new TextBox() { TextMode = TextBoxMode.MultiLine, Width = 300, Rows = 4 };
            this.txtExtensionConfiguration = new TextBox() { TextMode = TextBoxMode.MultiLine, Width = 300, Rows = 4 };
            this.chkWarningsAsError = new CheckBox() { Width = 300, TextAlign = TextAlign.Right };
            this.chkWaitForCompletion = new CheckBox() { Width = 300, TextAlign = TextAlign.Right };
            this.txtSubscriptionID = new ValidatingTextBox() { Width = 300 };
            this.txtCertificateName = new ValidatingTextBox() { Width = 300 };
        }

        protected virtual AzureComputeActionBase PopulateProperties(AzureComputeActionBase Value)
        {
            if(Value.UsesServiceName)
                Value.ServiceName = txtServiceName.Text;
            if (Value.UsesDeploymentName)
                Value.DeploymentName = txtDeploymentName.Text;
            if (Value.UsesSlotName)
                Value.SlotName = txtSlotName.Text;
            if (Value.UsesExtendedProperties)
                Value.ExtendedProperties = txtExtendedProperties.Text;
            if (Value.UsesExtensionConfiguration)
                Value.ExtensionConfiguration = txtExtensionConfiguration.Text;
            if (Value.UsesTreatWarningsAsError)
                Value.TreatWarningsAsError = chkWarningsAsError.Checked;
            if (Value.UsesWaitForCompletion)
                Value.WaitForCompletion = chkWaitForCompletion.Checked;
            if (!string.IsNullOrEmpty(this.txtSubscriptionID.Text))
                Value.ActionCredentials = new AzureAuthentication() { SubscriptionID = this.txtSubscriptionID.Text, CertificateName = this.txtCertificateName.Text };
            else
                Value.ActionCredentials = null;
            return Value;
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (AzureComputeActionBase)extension;
            this.txtServiceName.Text = action.ServiceName;
            this.txtDeploymentName.Text = action.DeploymentName;
            this.txtSlotName.Text = action.SlotName;
            this.txtExtendedProperties.Text = action.ExtendedProperties;
            this.txtExtensionConfiguration.Text = action.ExtensionConfiguration;
            this.chkWarningsAsError.Checked = action.TreatWarningsAsError;
            this.chkWaitForCompletion.Checked = action.WaitForCompletion;
            if (null != action.ActionCredentials)
            {
                this.txtSubscriptionID.Text = action.ActionCredentials.SubscriptionID;
                this.txtCertificateName.Text = action.ActionCredentials.CertificateName;
            }
        }

        protected override void CreateChildControls()
        {
            AddActionAuthentication();
            AddServiceInformation();
            AddDeploymentInformation();
            AddExtendedInformation();
            AddActionOptions();
        }

        private void AddActionAuthentication()
        {
            this.Controls.Add(new FormFieldGroup("Custom Authentication",
                "Authentication used for this action. If not populated then the credentials defined for the Windows Azure extension will be used",
                false,
                new StandardFormField("Subscription ID:", txtSubscriptionID),
                new StandardFormField("Certificate Name:",txtCertificateName)
                )
            );

        }

        private void AddServiceInformation()
        {

            if (extensionInstance.UsesServiceName)
            {
                var ff = new FormFieldGroup("Cloud Service",
                            "Information about the cloud service.",
                            false);
                ff.FormFields.Add(new StandardFormField("Service Name:", this.txtServiceName));
                this.Controls.Add(ff);
            }
        }

        private void AddDeploymentInformation()
        {
            var ff = new FormFieldGroup("Deployment",
                        "Information about the deployment and environment (production|staging).",
                        false);
            if(extensionInstance.UsesDeploymentName)
                ff.FormFields.Add(new StandardFormField("Deployment Name:", txtDeploymentName));
            if (extensionInstance.UsesSlotName)
                ff.FormFields.Add(new StandardFormField("Deployment Slot:", txtSlotName));
            if (extensionInstance.UsesDeploymentName || extensionInstance.UsesSlotName)
                this.Controls.Add(ff);
        }

        private void AddExtendedInformation()
        {
            var ff = new FormFieldGroup("Extended Information",
                        "Extended information for this action.",
                        false);
            var foo = new StandardFormField();
            this.txtExtendedProperties.Rows = 4;
            this.txtExtensionConfiguration.Rows = 4;
            if (extensionInstance.UsesExtendedProperties)
                ff.FormFields.Add(new StandardFormField("Extended Propeties (name=value):",txtExtendedProperties)); 
            if(extensionInstance.UsesExtensionConfiguration)
                ff.FormFields.Add(new StandardFormField("Extension Configuration (XML fragment):",txtExtensionConfiguration));
            if (extensionInstance.UsesExtendedProperties || extensionInstance.UsesExtensionConfiguration)
                this.Controls.Add(ff);
        }

        private void AddActionOptions()
        {
            var ff = new FormFieldGroup("Action Options",
                        "Other options for the action.",
                        false);
            if (extensionInstance.UsesTreatWarningsAsError)
                ff.FormFields.Add(new StandardFormField("Treat Warnings as Errors",chkWarningsAsError));
            if (extensionInstance.UsesWaitForCompletion)
                ff.FormFields.Add(new StandardFormField("Wait For Completion", chkWaitForCompletion));
            if (extensionInstance.UsesTreatWarningsAsError || extensionInstance.UsesWaitForCompletion)
                this.Controls.Add(ff);
        }

    }
}
