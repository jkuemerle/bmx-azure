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
    internal sealed class SwapDeploymentActionEditor : AzureComputeActionBaseEditor 
    {
        private ValidatingTextBox txtProductionDeploymentName;
        private ValidatingTextBox txtSourceDeploymentName;

        public SwapDeploymentActionEditor() 
        {
            this.extensionInstance = new SwapDeploymentAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            var action = (SwapDeploymentAction)extension;
            this.EnsureChildControls();
            base.BindToForm(extension);
            this.txtProductionDeploymentName.Text = action.ProductionDeploymentName;
            this.txtSourceDeploymentName.Text = action.SourceDeploymentName;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();
            return PopulateProperties(new SwapDeploymentAction() 
                {
                    ProductionDeploymentName = this.txtProductionDeploymentName.Text,
                    SourceDeploymentName = this.txtSourceDeploymentName.Text,
                }
            );

        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            txtProductionDeploymentName = new ValidatingTextBox() { Width = 300 };
            txtSourceDeploymentName = new ValidatingTextBox() { Width = 300 };
            this.Controls.Add(
                new FormFieldGroup(
                    "Swap Deployment Options", 
                    "Specify the Source and Production deployment names to swap. "
                    + "Leave these fields blank to swap Production and Staging.",
                    true,
                    new StandardFormField("Production Deployment Name:",txtProductionDeploymentName),
                    new StandardFormField("Source Deployment Name:",txtSourceDeploymentName)
                )
            );
        }
    }
}
