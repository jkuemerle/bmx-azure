using System.Web;
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
        private ValidatingTextBox txtAdditionalArguments;

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
            this.ffpServiceDefinition.ServerId = this.ServerId;
            this.ffpServiceDefinition.Text = action.ServiceDefinition;
            this.txtWebRoleName.Text = action.WebRole.RoleName;
            this.ffpWebRoleBinDir.ServerId = this.ServerId;
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
            this.txtAdditionalArguments.Text = action.AdditionalArguments;
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
                OutputFile = this.ffpOutput.Text,
                AdditionalArguments = this.txtAdditionalArguments.Text
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
            this.chkUseCTPPackageFormat = new CheckBox() { Width = 300, Text = "Use CTP Package Format", Checked = true };
            this.ffpOutput = new SourceControlFileFolderPicker() { ID = "ffpOutput", DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles, ServerId = 1 };
            this.chkCopyOnly = new CheckBox() { Width = 300, Text = "Copy Only" };
            this.txtAdditionalArguments = new ValidatingTextBox() { Width = 300 };
            this.Controls.Add(
                new FormFieldGroup("Service Definition",
                    "Provide the path to the default service definition file (ServiceDefinition.csdef) or the explicit file name.",
                    false,
                    new StandardFormField("Path:", this.ffpServiceDefinition)
                ),
                new FormFieldGroup("Web Role",
                    "Specify the name of the web role, the path to the \\bin directory of the web application output, and optionally, the " +
                    "file name of the assembly that contains the web role. If there is no web role for this project, leave these " +
                    "fields blank. If more than 1 web role exists, use the additional arguments section with the format: <br /><br />" +
                    HttpUtility.HtmlEncode("/role:<rolename>;[<role-directory>];[<role-entrypoint-DLL>]"),
                    false,
                    new StandardFormField("Role Name:", this.txtWebRoleName),
                    new StandardFormField("Bin Directory:", this.ffpWebRoleBinDir),
                    new StandardFormField("Assembly Name:",this.txtWebRoleAssemblyName)
                ),
                new FormFieldGroup("Site",
                    "Specify a site name for the web role, and a virtual to physical path mapping. If more than 1 mapping is required, " +
                    "use the additional arguments section with the format: <br /><br />" +
                    HttpUtility.HtmlEncode("/sites:<rolename>;<virtual-path1>;<physical-path1>;..."),
                    false,
                    new StandardFormField("Role Name:", this.txtWebRoleSiteRoleName),
                    new StandardFormField("Virtual Path:", this.txtWebRoleSiteVirtualPath),
                    new StandardFormField("Physical Path:",this.txtWebRoleSitePhysicaPath)
                ),
                new FormFieldGroup("Worker Role",
                    "Specify the name of the worker role, the path to the \\bin directory of the project output, and the assembly " +
                    "that contains the entry point for the worker role. If there is no worker role for this project, leave these " + 
                    "fields blank. If more than 1 worker role exists, use the additional arguments section with the format: <br /><br />" +
                    HttpUtility.HtmlEncode("/role:<rolename>;[<role-directory>];[<role-entrypoint-DLL>]"),
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
                    "Specify whether the new package format should be used, and whether to create a directory layout for the role " +
                    "binaries in order to run the service locally. To create a .cspkg, leave Copy Only unchecked.",
                    false,
                    new StandardFormField("",this.chkUseCTPPackageFormat),
                    new StandardFormField("",this.chkCopyOnly)
                ),
                new FormFieldGroup("Additional Arguments",
                    "Specify any additional arguments to pass to cspack.exe.",
                    false,
                    new StandardFormField("Additional Arguments:", this.txtAdditionalArguments)
                ),
                new FormFieldGroup("Output",
                    "Specify the file name of the package output (if Copy Only is unchecked), or the output directory for the role binaries.",
                    true,
                    new StandardFormField("Path:",this.ffpOutput)
                )
            );
        }
    }
}
