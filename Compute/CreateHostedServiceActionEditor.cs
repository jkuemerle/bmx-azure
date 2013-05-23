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
    internal sealed class CreateHostedServiceActionEditor : AzureComputeActionBaseEditor 
    {
        private ValidatingTextBox txtLabel;
        private ValidatingTextBox txtDescripition;
        private ValidatingTextBox txtAffinityGroup;
        private ValidatingTextBox txtLocation;

        public CreateHostedServiceActionEditor() 
        {
            this.extensionInstance = new CreateHostedServiceAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateHostedServiceAction)extension;
            this.EnsureChildControls();
            base.BindToForm(extension);
            this.txtLabel.Text = action.Label;
            this.txtDescripition.Text = action.Description;
            this.txtAffinityGroup.Text = action.AffinityGroup;
            this.txtLocation.Text = action.Location;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(new CreateHostedServiceAction()
                {
                    Label = this.txtLabel.Text,
                    Description = this.txtDescripition.Text,
                    AffinityGroup = this.txtAffinityGroup.Text,
                    Location = this.txtLocation.Text
                }
            );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            txtLabel = new ValidatingTextBox() {Width = 300};
            txtDescripition = new ValidatingTextBox() {Width = 300};
            txtLocation = new ValidatingTextBox() { Width = 300 };
            txtAffinityGroup = new ValidatingTextBox() { Width = 300 };
            this.Controls.Add(new FormFieldGroup("Create Hosted Service Configuration",
                "Options for the Create Hosted Service action", true,
                new StandardFormField("Label:",txtLabel),
                new StandardFormField("Description:",txtDescripition),
                new StandardFormField("Location:",txtLocation),
                new StandardFormField("Affinity Group:",txtAffinityGroup)
                )
            );
        }
    }
}
