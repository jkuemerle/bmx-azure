using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;
using Inedo.BuildMaster.Data;

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
                string configFile = this.ResolveDirectory(this.ConfigurationFilePath);
                if (!File.Exists(configFile))
                {
                    LogError("Configuration file {0} does not exist.", configFile);
                    return null;
                }
                if (null != this.TestConfigurer)
                    return File.ReadAllText(configFile);
                return File.ReadAllText(configFile).Substitute(Context.Variables);
            }
            return GetConfigText(this.ConfigurationFileId,this.InstanceName);
        }

        private string GetConfigText(int configID, string instanceName)
        {
            var file = StoredProcs.ConfigurationFiles_GetConfigurationFileVersions(configID, this.Context.ApplicationId, null, null, instanceName, 1).Execute().FirstOrDefault();
            if (null == file)
                return null;
            if (0 == file.File_Bytes.Length)
            {
                this.LogError("Configuration for file {0} in {1} is empty.", configID, instanceName);
                return null;
            }
            LogInformation("Return config.");
            return Encoding.Default.GetString(file.File_Bytes).Substitute(Context.Variables);
        }

        private string GetConfigText(string configName)
        {
            var config = (from r in Inedo.BuildMaster.Data.StoredProcs.ConfigurationFiles_GetConfigurationFiles(this.Context.ApplicationId, this.Context.DeployableId, "N").Execute() where r.FilePath_Text.Trim().ToLowerInvariant() == configName.Trim().ToLowerInvariant() select r).FirstOrDefault();
            if (null == config)
            {
                LogError("Unable to GetConfigurationFiles {0}, {1}, N for FilePath_Text = \"{2}\"", this.Context.ApplicationId, this.Context.DeployableId, configName);
                return null;
            }
            else
            {
                // get the latest config file version
                var file = (from v in Inedo.BuildMaster.Data.StoredProcs.ConfigurationFiles_GetConfigurationFileVersions(config.ConfigurationFile_Id, this.Context.ApplicationId, null, null, null, 1).Execute() select v).FirstOrDefault();
                if (null == file)
                {
                    this.LogError("Unable to find an active configuration for the application id: {0} and deployable id: {1}", this.Context.ApplicationId, this.Context.DeployableId);
                    return null;
                }
                else
                {
                    if (0 == file.File_Bytes.Length)
                    {
                        this.LogError("Configuration for {0} is empty.", configName);
                        return null;
                    }
                    LogInformation("Return config.");
                    return Encoding.Default.GetString(file.File_Bytes).Substitute(Context.Variables);
                }
            }
        }

    }
}
