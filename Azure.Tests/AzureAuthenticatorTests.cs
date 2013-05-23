using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using MbUnit.Framework;
using Inedo.BuildMasterExtensions.Azure;

namespace Azure.Tests
{
    [TestFixture]
    public class AzureAuthenticatorTests
    {

        [Test]
        public void FromCertificateStore()
        {
            var test = new AzureAuthentication() { CertificateName = "AzureDemo" };
            Assert.IsNotNull(test.Certificate);
        }

        [Test]
        public void FromPEM()
        {
            string content = File.ReadAllText(@"c:\temp\AzureDemo.pem");
            var test = new AzureAuthentication() { PEMENcoded = content };
            Assert.IsNotNull(test.Certificate);
        }

        [Test]
        public void TestIt()
        {
            X509Certificate2 foo = new X509Certificate2(@"c:\temp\AzureDemo.pem");

        }

    }
}
