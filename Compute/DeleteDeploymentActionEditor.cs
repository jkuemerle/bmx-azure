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
    internal sealed class DeleteDeploymentActionEditor : AzureComputeActionBaseEditor 
    {

        public DeleteDeploymentActionEditor() 
        {
            this.extensionInstance = new DeleteDeploymentAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            base.BindToForm(extension);
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(new DeleteDeploymentAction());
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }
    }
}
