using System;
using System.IO;
using System.Linq;
using System.Text;
using Inedo.BuildMaster;
using Inedo.BuildMaster.ConfigurationFiles;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Variables;

namespace Inedo.BuildMasterExtensions.Azure
{
    public abstract class AzureActionWithConfigBase : AzureComputeActionBase
    {
        [Persistent]
        public string ConfigurationFilePath { get; set; }

        [Persistent]
        public string ConfigurationFileName { get; set; }

        [Persistent]
        public string ConfigurationFileContents { get; set; }

        [Persistent]
        public int ConfigurationFileId { get; set; }

        [Persistent]
        public string InstanceName { get; set; }

        protected string GetConfigurationFileContents()
        {
            if (!string.IsNullOrEmpty(this.ConfigurationFileContents))
                return this.ConfigurationFileContents;

            if (!string.IsNullOrEmpty(this.ConfigurationFilePath))
            {
                var configFile = this.ResolveDirectory(this.ConfigurationFilePath);
                var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();

                if (!fileOps.FileExists(configFile))
                {
                    this.LogError("Configuration file {0} does not exist.", configFile);
                    return null;
                }

                if (this.TestConfigurer != null)
                    return fileOps.ReadAllText(configFile);

                var tree = VariableExpressionTree.Parse(configFile, Domains.VariableSupportCodes.All);
                var variableContext = (IVariableEvaluationContext)Activator.CreateInstance(Type.GetType("Inedo.BuildMaster.Variables.StandardVariableEvaluationContext,BuildMaster"), (IGenericBuildMasterContext)this.Context, this.Context.Variables);
                return tree.Evaluate(variableContext);
            }

            return this.GetConfigText(this.ConfigurationFileId, this.InstanceName);
        }

        protected virtual byte[] GetConfigurationFileContents(int configurationFileId, string instanceName, int? versionNumber)
        {
            var deployer = new ConfigurationFileDeployer(
                new ConfigurationFileDeploymentOptions
                {
                    ConfigurationFileId = configurationFileId,
                    InstanceName = instanceName,
                    VersionNumber = versionNumber
                }
            );

            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream, new UTF8Encoding(false));
                deployer.LogReceived += (s, e) => this.Log(e.LogLevel, e.Message);
                if (!deployer.Write(new SimpleBuildMasterContext(this.Context), writer))
                    return null;

                writer.Flush();
                return memoryStream.ToArray();
            }
        }

        private string GetConfigText(int configurationFileId, string instanceName)
        {
            this.LogDebug("Loading configuration file instance {0}...", instanceName);

            var version = StoredProcs.Releases_GetRelease(this.Context.ApplicationId, this.Context.ReleaseNumber)
                .Execute()
                .ReleaseConfigurationFiles
                .FirstOrDefault(r => r.ConfigurationFile_Id == configurationFileId);
            
            var file = this.GetConfigurationFileContents(configurationFileId, instanceName, version != null ? (int?)version.Version_Number : null);
            if (file == null)
                return null;

            this.LogDebug("Configuration file found.");
            return Encoding.UTF8.GetString(file);
        }
    }
}
