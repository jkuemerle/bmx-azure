using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MbUnit.Framework;
using Inedo.BuildMasterExtensions.Azure;

namespace Azure.Tests
{
    [TestFixture]
    public class DeployPackageTests
    {
        PackageAction package = null;

        [SetUp]
        public void Setup()
        {
            package = new PackageAction(); // { AzureSDKPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin" };
            package.TestConfigurer = new AzureConfigurer();
            package.TestConfigurer.AzureSDKPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin";
        }

    }
}
