using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MbUnit.Framework;
using Inedo.BuildMasterExtensions.Azure;

namespace Azure.Tests
{
    [TestFixture]
    public class AzureActionTests
    {
        DeployPackageAction action = null;

        [SetUp]
        public void Setup()
        {
            action = new DeployPackageAction(); 
            action.TestConfigurer = new AzureConfigurer();
            action.TestConfigurer.AzureSDKPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin";
            string subid = File.ReadAllText(@"c:\temp\sub.txt");
            action.TestConfigurer.Credentials = new AzureAuthentication() { SubscriptionID = subid, CertificateName = "AzureDemo" };
        }

        [Test]
        public void ListLocations()
        {
            Assert.IsNotEmpty(action.ListLocations());
        }

        [Test]
        public void ListAffinityGroups()
        {
            Assert.IsNotEmpty(action.ListAffinityGroups());
        }
    }
}
