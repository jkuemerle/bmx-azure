using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.Web.UI.WebControls;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using Inedo.BuildMaster.Features;
using Inedo.Linq;
using System.Data;

namespace Inedo.BuildMasterExtensions.Azure
{
    public abstract class  AzureActionWithConfigBaseEditor: AzureComputeActionBaseEditor
    {
        //protected ValidatingTextBox txtConfigFileName;
        protected TextBox txtConfigText;
        protected SourceControlFileFolderPicker ffpConfigFilePath;
        protected DropDownList ddlConfigurationFile, ddlInstance;
        private ValidatingTextBox txtConfigurationFileName, txtInstanceName;
        private StandardFormField ctl_ddlInstance, ctl_txtConfigurationFileName, ctl_txtInstanceName;

        public AzureActionWithConfigBaseEditor()
        {
            //this.txtConfigFileName = new ValidatingTextBox() { Width = 300 };
            this.txtConfigText = new TextBox() { TextMode = TextBoxMode.MultiLine, Width = 300, Rows = 4 };
            this.ffpConfigFilePath = new SourceControlFileFolderPicker() { Width = 300, ServerId = 1 };
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            base.BindToForm(extension);
            var action = (AzureActionWithConfigBase)extension;
            //this.txtConfigFileName.Text = action.ConfigurationFileContents;
            //this.txtConfigFileName.Text = action.ConfigurationFileName;
            this.ffpConfigFilePath.Text = action.ConfigurationFilePath;
            this.txtConfigText.Text = action.ConfigurationFileContents;
            if (action.ConfigurationFileId <= 0)
            {
                this.txtConfigurationFileName.Text = action.ConfigurationFileName;
                this.txtInstanceName.Text = action.InstanceName;
                this.ddlConfigurationFile.SelectedValue = "X";
                this.ctl_txtConfigurationFileName.Visible = true;
                this.ctl_ddlInstance.Visible = false;
                this.ctl_txtInstanceName.Visible = true;
            }
            else
            {
                this.PreRender += (_s, _e) =>
                {
                    (ddlConfigurationFile.Items.FindByValue(action.ConfigurationFileId.ToString()) ?? new ListItem())
                        .Selected = true;
                    ddlConfigurationFile_SelectedIndexChanged(ddlConfigurationFile, EventArgs.Empty);

                    (ddlInstance.Items.FindByValue(action.InstanceName) ?? new ListItem())
                        .Selected = true;
                };

            }
        }

        protected override AzureComputeActionBase PopulateProperties(AzureComputeActionBase Value)
        {
            var retVal = (AzureActionWithConfigBase)base.PopulateProperties(Value);
            //retVal.ConfigurationFileName = this.txtConfigFileName.Text;
            retVal.ConfigurationFilePath = this.ffpConfigFilePath.Text;
            retVal.ConfigurationFileContents = this.txtConfigText.Text;
            retVal.ConfigurationFileId = "X" == this.ddlConfigurationFile.SelectedValue ? 0 : int.Parse(this.ddlConfigurationFile.SelectedValue);
            retVal.ConfigurationFileName = this.txtConfigurationFileName.Text;
            retVal.InstanceName = this.ddlInstance.SelectedValue;
            return retVal;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            //ddlConfigurationFile
            this.ddlConfigurationFile = new DropDownList {ID = "ddlConfigurationFile", Width = 300, AutoPostBack = true };
            this.ddlConfigurationFile.Items.Add(string.Empty);
            this.ddlConfigurationFile.Items.AddRange(StoredProcs
                .ConfigurationFiles_GetConfigurationFiles(this.ApplicationId, 0 == DeployableId ? null : (int?)DeployableId, "N")
                    .Execute()
                    .Select(c => new ListItem { Text = c.FilePath_Text, Value = c.ConfigurationFile_Id.ToString() })
                    .ToArray()
            );
            this.ddlConfigurationFile.Items.Add(new ListItem { Text = "Type name...", Value = "X", Enabled = false });
            this.ddlConfigurationFile.SelectedIndexChanged += this.ddlConfigurationFile_SelectedIndexChanged;

            //ddlInstance
            ddlInstance = new DropDownList { ID = "ddlInstance", Width = 300 };
            this.PreRender += (_s, _e) => { CUtil.GetJQuery(Page).IncludeInedoDefaulter = true; };
            this.txtConfigurationFileName = new ValidatingTextBox { Width = 300, Required = true };
            this.txtInstanceName = new ValidatingTextBox { Width = 300, Required = true };

            this.ctl_txtInstanceName = new StandardFormField("Instance Name:", this.txtInstanceName) { Visible = false };
            this.ctl_ddlInstance = new StandardFormField("Instance Name:", this.ddlInstance);
            this.ctl_txtConfigurationFileName = new StandardFormField("Configuration File Name:", this.txtConfigurationFileName) { Visible = false };

            this.Controls.Add(new FormFieldGroup("Configuration File",
                "Configuration file location, select only one. The order of evaluation is configuration file text, disk location, then configuration name.",
                false,
                new StandardFormField("Configuration Text:", this.txtConfigText),
                new StandardFormField("Configuration File Location:", this.ffpConfigFilePath),
                new StandardFormField("Configuration File:", this.ddlConfigurationFile),
                ctl_txtConfigurationFileName,
                ctl_ddlInstance 
                )
            );
        }

        private void ddlConfigurationFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ddlInstance.Items.Clear();
            if (this.ddlConfigurationFile.SelectedIndex <= 0) return;

            if (this.ddlConfigurationFile.SelectedValue == "X")
            {
                this.ctl_txtConfigurationFileName.Visible = true;
                this.ctl_ddlInstance.Visible = false;
                this.ctl_txtInstanceName.Visible = true;
            }
            else
            {
                this.ctl_txtConfigurationFileName.Visible = false;
                this.ctl_ddlInstance.Visible = true;
                this.ctl_txtInstanceName.Visible = false;

                this.ddlInstance.Items.Add(string.Empty);
                this.ddlInstance.Items.AddRange(StoredProcs
                    .ConfigurationFiles_GetConfigurationFile(int.Parse(ddlConfigurationFile.SelectedValue), null)
                    .Execute()
                    .ConfigurationFileInstances_Extended
                    .Select(cfg => new ListItem { Text = cfg.Instance_Name, Value = cfg.Instance_Name })
                    .ToArray()
                );
            }
        }

        public override void InitializeDefaultValues()
        {
            this.EnsureChildControls();

            this.PreRender += (_s, _e) =>
            {
                if (ddlConfigurationFile.Items.Count == 3)
                    ddlConfigurationFile.SelectedIndex = 1;
                ddlConfigurationFile_SelectedIndexChanged(ddlConfigurationFile, EventArgs.Empty);

                if (!IsPostBack)
                {
                    DataRow env = StoredProcs.Environments_GetEnvironment(this.EnvironmentId).ExecuteDataRow();
                    (ddlInstance.Items.FindByValue((string)env[TableDefs.Environments.Environment_Name]) ?? new ListItem())
                    .Selected = true;
                }
            };
        }
 
    }
}
