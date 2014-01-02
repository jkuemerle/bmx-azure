using System.Text;
using System.IO;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Azure
{
    [ActionProperties(
        "Package Application",
        "Packages Web and Worker Role applications for deployment onto Windows Azure.",
        "Windows Azure")]
    [CustomEditor(typeof(PackageActionEditor))]
    public sealed class PackageAction : AzureAction 
    {
        [Persistent]
        public string ServiceDefinition { get; set; }

        [Persistent]
        public string OutputFile { get; set; }

        [Persistent]
        public AzureRole WebRole { get; set; }

        [Persistent]
        public AzureRole WorkerRole { get; set; }

        [Persistent]
        public AzureSite WebRoleSite { get; set; }

        [Persistent]
        public bool UseCtpPackageFormat { get; set; }

        [Persistent]
        public bool CopyOnly { get; set; }

        [Persistent]
        public string RolePropertiesFile { get; set; }

        [Persistent]
        public string RolePropertiesFileRoleName { get; set; }

        public PackageAction()
        {
            this.WebRole = new AzureRole();
            this.WorkerRole = new AzureRole();
            this.WebRoleSite = new AzureSite();
        }

        public override string ToString()
        {
            return string.Format("Packages the {0} definition to {1}.", this.ServiceDefinition, this.OutputFile);
        }

        internal void Test()
        {
            this.ProcessRemoteCommand(null,null);
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            string workingDir = this.Context.SourceDirectory;
            string cmdLine = BuildCommand();
            string p = BuildParameters();
            LogInformation("Ready to run command line {0} with parameters {1}", cmdLine, p);
            int exitcode = ExecuteCommandLine(cmdLine, p, workingDir);
            LogInformation("Result of command line: {0}", exitcode);
            if(0 != exitcode) 
                LogError("Error creating Azure package. Error Code: {0}",exitcode);
            return exitcode.ToString();
        }

        internal string ParseServiceDefinition(string PathToParse)
        {
            PathToParse = this.ResolveDirectory(PathToParse);
            if (null == PathToParse)
                return string.Empty;
            if(Directory.Exists(PathToParse))
                return Path.Combine(PathToParse, "ServiceDefinition.csdef");
            if (string.IsNullOrEmpty(Path.GetFileName(PathToParse))) // if the name of the service definition file is not specified use the default one
                return Path.Combine(PathToParse, "ServiceDefinition.csdef");
            return PathToParse;

        }

        internal string BuildCommand()
        {
            return Path.Combine(this.Configurer.AzureSDKPath, "cspack.exe");
        }

        internal string BuildParameters()
        {
            StringBuilder p = new StringBuilder();
            p.Append(ParseServiceDefinition(this.ServiceDefinition)); // add the service definition path parameter
            if ((null != this.WebRole) && !string.IsNullOrEmpty(this.WebRole.RoleName)) // add WebRole parameters
            {
                p.AppendFormat(" /role:{0};{1}", this.WebRole.RoleName, this.ResolveDirectory(this.WebRole.RoleBinDirectory));
                if (!string.IsNullOrEmpty(this.WebRole.RoleAssemblyName))
                    p.AppendFormat(";{0}", this.WebRole.RoleAssemblyName);
            }
            if ((null != this.WebRoleSite) && !string.IsNullOrEmpty(this.WebRoleSite.RoleName)) // add WebRole site parameters
            {
                p.AppendFormat(" /sites:{0};{1};{2}", this.WebRoleSite.RoleName, this.WebRoleSite.VirtualPath, this.ResolveDirectory(this.WebRoleSite.PhysicalPath));
            }
            if ((null != this.WorkerRole) && !string.IsNullOrEmpty(this.WorkerRole.RoleName)) // add Worker Role parameters
            {
                p.AppendFormat(" /role:{0};{1};{2}", this.WorkerRole.RoleName, this.ResolveDirectory(this.WorkerRole.RoleBinDirectory), this.WorkerRole.RoleAssemblyName);
            }
            if (!string.IsNullOrEmpty(this.RolePropertiesFileRoleName))
                p.AppendFormat(" /rolePropertiesFile:{0};{1}", this.RolePropertiesFileRoleName, this.ResolveDirectory(this.RolePropertiesFile));
            if (this.UseCtpPackageFormat)
                p.Append(" /useCtpPackageFormat");
            if (this.CopyOnly)
                p.Append(" /copyOnly");
            if (!string.IsNullOrEmpty(this.OutputFile))
            {
                string output = this.ResolveDirectory(this.OutputFile);
                string outputDir = Path.GetDirectoryName(output);
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                p.AppendFormat(" /out:{0}", output);
            }
            return p.ToString();
        }
    }
}
