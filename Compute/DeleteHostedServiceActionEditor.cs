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
    internal sealed class DeleteHostedServiceActionEditor : AzureComputeActionBaseEditor 
    {

        public DeleteHostedServiceActionEditor() 
        {
            this.extensionInstance = new DeleteHostedServiceAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            base.BindToForm(extension);
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(new DeleteHostedServiceAction());
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }
    }
}
