using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure.Storage
{
    internal sealed class UploadFilesToBlobStorageActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtAccountName;
        private ValidatingTextBox txtAccessKey;
        private ValidatingTextBox txtContainerName;
        private ValidatingTextBox txtFileMasks;
        private CheckBox chkRecursive;
        private ValidatingTextBox txtTargetPath;

        public UploadFilesToBlobStorageActionEditor()
        {
        }

        public override bool DisplaySourceDirectory
        {
            get { return true; }
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (UploadFilesToBlobStorageAction)extension;
            this.txtAccountName.Text = action.AccountName;
            this.txtAccessKey.Text = action.AccessKey;
            this.txtContainerName.Text = action.Container;
            this.txtFileMasks.Text = string.Join(Environment.NewLine, action.FileMasks ?? new string[0]);
            this.chkRecursive.Checked = action.Recursive;
            this.txtTargetPath.Text = action.TargetFolder;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new UploadFilesToBlobStorageAction
            {
                AccountName = this.txtAccountName.Text,
                AccessKey = this.txtAccessKey.Text,
                Container = this.txtContainerName.Text,
                FileMasks = this.txtFileMasks.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                Recursive = this.chkRecursive.Checked,
                TargetFolder = this.txtTargetPath.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtAccountName = new ValidatingTextBox
            {
                ID = "txtAccountName",
                Required = true,
                Width = 300
            };

            this.txtAccessKey = new ValidatingTextBox
            {
                ID = "txtAccessKey",
                Required = true,
                Width = 300
            };

            this.txtContainerName = new ValidatingTextBox
            {
                ID = "txtContainerName",
                Required = true,
                Width = 300
            };

            this.txtFileMasks = new ValidatingTextBox
            {
                Required = true,
                Width = 300,
                Rows = 3,
                TextMode = TextBoxMode.MultiLine
            };

            this.chkRecursive = new CheckBox
            {
                Text = "Recursively upload files from subdirectories"
            };

            this.txtTargetPath = new ValidatingTextBox
            {
                Required = false,
                Width = 300,
                DefaultText = "(container root)"
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Azure Storage Account",
                    "Provide the name of your Azure Storage Account and the Access Key used to access it.",
                    false,
                    new StandardFormField("Account:", this.txtAccountName),
                    new StandardFormField("Access Key:", this.txtAccessKey)
                ),
                new FormFieldGroup(
                    "Files to Upload",
                    "Files in the source directory that match a mask entered here (one per line) will be uploaded.",
                    false,
                    new StandardFormField("File Masks:", this.txtFileMasks),
                    new StandardFormField(string.Empty, this.chkRecursive)
                ),
                new FormFieldGroup(
                    "Container",
                    "Specify the target blob storage container and target path inside the container to upload files to. If the container does not exist, it will be created.",
                    true,
                    new StandardFormField("Container:", this.txtContainerName),
                    new StandardFormField("Target Path:", this.txtTargetPath)
                )
            );
        }
    }
}
