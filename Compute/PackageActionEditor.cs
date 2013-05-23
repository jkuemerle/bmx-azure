using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class PackageActionEditor : ActionEditorBase 
    {
        private SourceControlFileFolderPicker ffpServiceDefinition;
        private ValidatingTextBox txtWebRoleName;
        private SourceControlFileFolderPicker ffpWebRoleBinDir;
        private ValidatingTextBox txtWebRoleAssemblyName;
        private ValidatingTextBox txtWebRoleSiteRoleName;
        private ValidatingTextBox txtWebRoleSiteVirtualPath;
        private ValidatingTextBox txtWebRoleSitePhysicaPath;
        private ValidatingTextBox txtWorkerRoleName;
        private SourceControlFileFolderPicker ffpWorkerRoleBinDir;
        private ValidatingTextBox txtWorkerRoleAssemblyName;
        private ValidatingTextBox txtRolePropertiesFileRoleName;
        private SourceControlFileFolderPicker ffpPropertiesFile;
        private CheckBox chkUseCTPPackageFormat;
        private CheckBox chkCopyOnly;
        private SourceControlFileFolderPicker ffpOutput;

        public PackageActionEditor() { }

        public override bool DisplayTargetDirectory
        {
            get
            {
                return false;
            }
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (PackageAction)extension;
            this.ffpServiceDefinition.ServerId = action.ServerId;
            this.ffpServiceDefinition.Text = action.ServiceDefinition;
            this.txtWebRoleName.Text = action.WebRole.RoleName;
            this.ffpWebRoleBinDir.ServerId = action.ServerId;
            this.ffpWebRoleBinDir.Text = action.WebRole.RoleBinDirectory;
            this.txtWebRoleAssemblyName.Text = action.WebRole.RoleAssemblyName;
            this.txtWebRoleSiteRoleName.Text = action.WebRoleSite.RoleName;
            this.txtWebRoleSiteVirtualPath.Text = action.WebRoleSite.VirtualPath;
            this.txtWebRoleSitePhysicaPath.Text = action.WebRoleSite.PhysicalPath;
            this.txtWorkerRoleName.Text = action.WorkerRole.RoleName;
            this.ffpWorkerRoleBinDir.Text = action.WorkerRole.RoleBinDirectory;
            this.txtWorkerRoleAssemblyName.Text = action.WorkerRole.RoleAssemblyName;
            this.txtRolePropertiesFileRoleName.Text = action.RolePropertiesFileRoleName;
            this.ffpPropertiesFile.Text = action.RolePropertiesFile;
            this.chkUseCTPPackageFormat.Checked = action.UseCtpPackageFormat;
            this.chkCopyOnly.Checked = action.CopyOnly;
            this.ffpOutput.Text = action.OutputFile; 
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new PackageAction
            {
                ServiceDefinition = this.ffpServiceDefinition.Text,
                WebRole = new AzureRole() { RoleName = this.txtWebRoleName.Text, RoleBinDirectory = this.ffpWebRoleBinDir.Text, RoleAssemblyName = this.txtWebRoleAssemblyName.Text },
                WebRoleSite = new AzureSite() { RoleName = this.txtWebRoleSiteRoleName.Text, VirtualPath = this.txtWebRoleSiteVirtualPath.Text, PhysicalPath = this.txtWebRoleSitePhysicaPath.Text },
                WorkerRole = new AzureRole() { RoleName = this.txtWorkerRoleName.Text, RoleBinDirectory = this.ffpWorkerRoleBinDir.Text, RoleAssemblyName = this.txtWorkerRoleAssemblyName.Text },
                RolePropertiesFileRoleName = this.txtRolePropertiesFileRoleName.Text, 
                RolePropertiesFile = this.ffpPropertiesFile.Text,
                UseCtpPackageFormat = this.chkUseCTPPackageFormat.Checked,
                CopyOnly = this.chkCopyOnly.Checked,
                OutputFile = this.ffpOutput.Text 
            };
        }

        protected override void CreateChildControls()
        {
            this.ffpServiceDefinition = new SourceControlFileFolderPicker() { ID = "serviceDefinition", DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles, ServerId = 1 };
            this.txtWebRoleName = new ValidatingTextBox() { Width = 300 };
            this.ffpWebRoleBinDir = new SourceControlFileFolderPicker() { ID = "ffpWebRoleBinDir", DisplayMode = SourceControlBrowser.DisplayModes.Folders, ServerId = 1 };
            this.txtWebRoleAssemblyName = new ValidatingTextBox() { Width = 300 };
            this.txtWebRoleSiteRoleName = new ValidatingTextBox() { Width = 300 };
            this.txtWebRoleSiteVirtualPath = new ValidatingTextBox() { Width = 300 };
            this.txtWebRoleSitePhysicaPath = new ValidatingTextBox() { Width = 300 };
            this.txtWorkerRoleName = new ValidatingTextBox() { Width = 300 };
            this.ffpWorkerRoleBinDir = new SourceControlFileFolderPicker() { ID = "ffpWorkerRoleBinDir", DisplayMode = SourceControlBrowser.DisplayModes.Folders, ServerId = 1 };
            this.txtWorkerRoleAssemblyName = new ValidatingTextBox() { Width = 300 };
            this.txtRolePropertiesFileRoleName = new ValidatingTextBox() { Width = 300 };
            this.ffpPropertiesFile = new SourceControlFileFolderPicker() { ID = "ffpPropertiesFile", DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles, ServerId = 1 };
            this.chkUseCTPPackageFormat = new CheckBox() { Width = 300 };
            this.ffpOutput = new SourceControlFileFolderPicker() { ID = "ffpOutput", DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles, ServerId = 1 };
            this.chkCopyOnly = new CheckBox() { Width = 300 };
            this.Controls.Add(
                new FormFieldGroup("Service Definition",
                    "Provide the path to the default service definition file (ServiceDefinition.csdef) or the explicit file name.",
                    false,
                    new StandardFormField("Path:", this.ffpServiceDefinition)
                ),
                new FormFieldGroup("Web Role",
                    "Web role information.",
                    false,
                    new StandardFormField("Role Name:", this.txtWebRoleName),
                    new StandardFormField("Bin Directory:", this.ffpWebRoleBinDir),
                    new StandardFormField("Assembly Name:",this.txtWebRoleAssemblyName)
                ),
                new FormFieldGroup("Site",
                    "Site information.",
                    false,
                    new StandardFormField("Role Name:", this.txtWebRoleSiteRoleName),
                    new StandardFormField("Virtual Path:", this.txtWebRoleSiteVirtualPath),
                    new StandardFormField("Physical Path:",this.txtWebRoleSitePhysicaPath)
                ),
                new FormFieldGroup("Worker Role",
                    "Worker role Information.",
                    false,
                    new StandardFormField("Role Name:", this.txtWorkerRoleName),
                    new StandardFormField("Bin Directory:",this.ffpWorkerRoleBinDir),
                    new StandardFormField("Assembly Name:",this.txtWorkerRoleAssemblyName)
                ),
                new FormFieldGroup("Role Properties",
                    "Role properties file information.",
                    false,
                    new StandardFormField("Role Name:",this.txtRolePropertiesFileRoleName),
                    new StandardFormField("Path:",ffpPropertiesFile)
                ),
                new FormFieldGroup("Options",
                    "Packaging options.",
                    false,
                    new StandardFormField("Use CTP Package Format",chkUseCTPPackageFormat),
                    new StandardFormField("Copy Only:",this.chkCopyOnly)
                ),
                new FormFieldGroup("Output",
                    "Packaging output.",
                    true,
                    new StandardFormField("Path:",this.ffpOutput)
                )
            );
        }

    }
}
