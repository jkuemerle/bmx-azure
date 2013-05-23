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
    internal sealed class ChangeDeploymentConfigurationActionEditor : AzureActionWithConfigBaseEditor 
    {
        private ValidatingTextBox txtMode;

        public ChangeDeploymentConfigurationActionEditor() 
        {
            this.extensionInstance = new ChangeDeploymentConfigurationAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            var action = (ChangeDeploymentConfigurationAction)extension;
            this.EnsureChildControls();
            base.BindToForm(extension);
            this.txtMode.Text = action.Mode.ToString();
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(new ChangeDeploymentConfigurationAction() 
                {
                    Mode = (ChangeDeploymentConfigurationAction.ChangeModeType)Enum.Parse(typeof(ChangeDeploymentConfigurationAction.ChangeModeType), this.txtMode.Text),
                }
            );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            txtMode = new ValidatingTextBox() {Width = 300, Required = true};
            this.Controls.Add(new FormFieldGroup("Change Deployment Configuration", 
                "Options for the Change Deployment Configuration action",true,
                new StandardFormField("Mode (Auto|Manual):",txtMode)
                )
            );
        }
    }
}
