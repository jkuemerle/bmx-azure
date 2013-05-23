using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MbUnit.Framework;
using Inedo.BuildMasterExtensions.Azure;

namespace Azure.Tests
{
    public class HostedServiceTests
    {
        CreateHostedServiceAction createAction = null;
        DeleteHostedServiceAction deleteAction = null;
        DeployPackageAction deployAction = null;
        SwapDeploymentAction swapAction = null;
        DeleteDeploymentAction deleteDeployAction = null;
        ChangeDeploymentConfigurationAction changeAction = null;

        [SetUp]
        public void Setup()
        {
            string subid = File.ReadAllText(@"c:\temp\sub.txt");
            AzureConfigurer testConfig = new AzureConfigurer();
            testConfig.AzureSDKPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin";
            testConfig.Credentials = new AzureAuthentication() { SubscriptionID = subid, CertificateName = "AzureDemo" };
            createAction = new CreateHostedServiceAction();
            createAction.TestConfigurer = testConfig;
            deleteAction = new DeleteHostedServiceAction();
            deleteAction.TestConfigurer = testConfig;
            deployAction = new DeployPackageAction();
            deployAction.TestConfigurer = testConfig;
            swapAction = new SwapDeploymentAction();
            swapAction.TestConfigurer = testConfig;
            deleteDeployAction = new DeleteDeploymentAction();
            deleteDeployAction.TestConfigurer = testConfig;
            changeAction = new ChangeDeploymentConfigurationAction();
            changeAction.TestConfigurer = testConfig;
        }

        [Test]
        public void CreateTest()
        {
            string Name = Guid.NewGuid().ToString();
            createAction.ServiceName = Name; 
            createAction.Label = "FooBar";
            createAction.Description = "This is a test service";
            createAction.AffinityGroup = "Test";
            createAction.WaitForCompletion = true;
            Assert.IsNotEmpty(createAction.Test());
            deleteAction.ServiceName = Name;
            deleteAction.Test();
        }

        [Test]
        public void DeployTest()
        {
            string serviceName = Guid.NewGuid().ToString();
            createAction.ServiceName = serviceName;
            createAction.Label = "FooBar";
            createAction.Description = "This is a test service";
            createAction.AffinityGroup = "Test";
            createAction.WaitForCompletion = true;
            Assert.IsNotEmpty(createAction.Test());
            deployAction.ServiceName = serviceName;
            deployAction.StorageAccessKey = File.ReadAllText(@"c:\temp\store.txt");
            deployAction.StorageAccountName = "inedodemo";
            deployAction.PackageFile = @"C:\BuildMaster\_SVCTMP\_A7\Deploy\HelloWorld.cspkg";
            deployAction.ConfigurationFilePath = @"C:\BuildMaster\_SVCTMP\_A7\Azure\ServiceConfiguration.cscfg";
            deployAction.DeploymentName = serviceName;
            deployAction.Label = "This is a label";
            deployAction.StartDeployment = true;
            deployAction.WaitForCompletion = true;
            deployAction.TreatWarningsAsError = false;
            deployAction.DeploymentSlot = DeployPackageAction.DeploymentSlotType.Staging;
            deployAction.DeletePackageFromStorage = true;
            Assert.IsNotEmpty(deployAction.Test());
            deleteAction.ServiceName = serviceName;
            deleteAction.Test();
        }

        [Test]
        public void SwapTest()
        {
            string serviceName = Guid.NewGuid().ToString();
            createAction.ServiceName = serviceName;
            createAction.Label = "FooBar";
            createAction.Description = "This is a test service";
            createAction.AffinityGroup = "Test";
            createAction.WaitForCompletion = true;
            Assert.IsNotEmpty(createAction.Test());
            deployAction.ServiceName = serviceName;
            deployAction.StorageAccessKey = File.ReadAllText(@"c:\temp\store.txt");
            deployAction.StorageAccountName = "inedodemo";
            deployAction.PackageFile = @"C:\BuildMaster\_SVCTMP\_A7\Deploy\HelloWorld.cspkg";
            deployAction.ConfigurationFilePath = @"C:\BuildMaster\_SVCTMP\_A7\Azure\ServiceConfiguration.cscfg";
            deployAction.DeploymentName = serviceName;
            deployAction.Label = "This is a label";
            deployAction.StartDeployment = true;
            deployAction.WaitForCompletion = true;
            deployAction.TreatWarningsAsError = false;
            deployAction.DeploymentSlot = DeployPackageAction.DeploymentSlotType.Staging;
            deployAction.DeletePackageFromStorage = true;
            Assert.IsNotEmpty(deployAction.Test());
            swapAction.ServiceName = serviceName;
            swapAction.SourceDeploymentName = serviceName;
            swapAction.ProductionDeploymentName = "";
            swapAction.WaitForCompletion = true;
            Assert.IsNotEmpty(swapAction.Test());
            deleteAction.ServiceName = serviceName;
            deleteAction.Test();
        }

        [Test]
        public void DeleteDeployTest()
        {
            string serviceName = Guid.NewGuid().ToString();
            createAction.ServiceName = serviceName;
            createAction.Label = "FooBar";
            createAction.Description = "This is a test service";
            createAction.AffinityGroup = "Test";
            createAction.WaitForCompletion = true;
            Assert.IsNotEmpty(createAction.Test());
            deployAction.ServiceName = serviceName;
            deployAction.StorageAccessKey = File.ReadAllText(@"c:\temp\store.txt");
            deployAction.StorageAccountName = "inedodemo";
            deployAction.PackageFile = @"C:\BuildMaster\_SVCTMP\_A7\Deploy\HelloWorld.cspkg";
            deployAction.ConfigurationFilePath = @"C:\BuildMaster\_SVCTMP\_A7\Azure\ServiceConfiguration.cscfg";
            deployAction.DeploymentName = serviceName;
            deployAction.Label = "This is a label";
            deployAction.StartDeployment = true;
            deployAction.WaitForCompletion = true;
            deployAction.TreatWarningsAsError = false;
            deployAction.DeploymentSlot = DeployPackageAction.DeploymentSlotType.Staging;
            deployAction.DeletePackageFromStorage = true;
            Assert.IsNotEmpty(deployAction.Test());
            deleteDeployAction.ServiceName = serviceName;
            deleteDeployAction.WaitForCompletion = true;
            deleteDeployAction.SlotName = "staging";
            Assert.IsNotEmpty(deleteDeployAction.Test());
            deleteAction.ServiceName = serviceName;
            deleteAction.Test();
        }

        [Test]
        public void ChangeConfigTest()
        {
            string serviceName = Guid.NewGuid().ToString();
            createAction.ServiceName = serviceName;
            createAction.Label = "FooBar";
            createAction.Description = "This is a test service";
            createAction.AffinityGroup = "Test";
            createAction.WaitForCompletion = true;
            Assert.IsNotEmpty(createAction.Test());
            deployAction.ServiceName = serviceName;
            deployAction.StorageAccessKey = File.ReadAllText(@"c:\temp\store.txt");
            deployAction.StorageAccountName = "inedodemo";
            deployAction.PackageFile = @"C:\BuildMaster\_SVCTMP\_A7\Deploy\HelloWorld.cspkg";
            deployAction.ConfigurationFilePath = @"C:\BuildMaster\_SVCTMP\_A7\Azure\ServiceConfiguration.cscfg";
            deployAction.DeploymentName = serviceName;
            deployAction.Label = "This is a label";
            deployAction.StartDeployment = true;
            deployAction.WaitForCompletion = true;
            deployAction.TreatWarningsAsError = false;
            deployAction.DeploymentSlot = DeployPackageAction.DeploymentSlotType.Staging;
            deployAction.DeletePackageFromStorage = true;
            Assert.IsNotEmpty(deployAction.Test());
            changeAction.ConfigurationFilePath = @"C:\BuildMaster\_SVCTMP\_A7\Azure\ServiceConfiguration.cscfg";
            changeAction.ServiceName = serviceName;
            changeAction.Mode = ChangeDeploymentConfigurationAction.ChangeModeType.Auto;
            changeAction.SlotName = "staging";
            changeAction.WaitForCompletion = true;
            Assert.IsNotEmpty(changeAction.Test());
            deleteDeployAction.ServiceName = serviceName;
            deleteDeployAction.WaitForCompletion = true;
            deleteDeployAction.SlotName = "staging";
            Assert.IsNotEmpty(deleteDeployAction.Test());
            deleteAction.ServiceName = serviceName;
            deleteAction.Test();
        }

    }
}
