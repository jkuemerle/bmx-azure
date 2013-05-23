using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MbUnit.Framework;
using Inedo.BuildMasterExtensions.Azure;

namespace Azure.Tests
{
    [TestFixture]
    public class PackageTests
    {
        PackageAction package = null;

        [SetUp]
        public void Setup()
        {
            package = new PackageAction(); // { AzureSDKPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin" };
            package.TestConfigurer = new AzureConfigurer();
            package.TestConfigurer.AzureSDKPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin";
        }

        [Test]
        [Row(@"c:\temp",@"c:\temp\ServiceDefinition.csdef")]
        [Row(@"c:\temp\ServiceDefinition.csdef", @"c:\temp\ServiceDefinition.csdef")]
        [Row(@"c:\temp\foo.csdef", @"c:\temp\foo.csdef")]
        [Row(null, "")]
        [Row("","ServiceDefinition.csdef")]
        public void ParseServiceDefinitionTest(string input, string expected)
        {
            Assert.AreEqual(expected, package.ParseServiceDefinition(input));
        }

        [Test]
        public void TestBuildCommand()
        {
            Assert.AreEqual(@"C:\Program Files\Microsoft SDKs\Windows Azure\.NET SDK\v2.0\bin\cspack.exe", package.BuildCommand());
        }

        [Test]
        [Row("{\"ServiceDefinition\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld\\\\\",\"OutputFile\":\"c:\\\\temp\\\\azure\\\\\",\"WebRole\":{\"RoleName\":\"HelloWorld_WebRole\",\"RoleBinDirectory\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\",\"RoleAssemblyName\":\"HelloWorld_WebRole.dll\"},\"WebRoleSite\":{\"RoleName\":\"HelloWorld_WebRole\",\"VirtualPath\":\"Web\",\"PhysicalPath\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\"},\"UseCtpPackageFormat\":true,\"CopyOnly\":true,\"AzureSDKPath\":\"C:\\\\Program Files\\\\Microsoft SDKs\\\\Windows Azure\\\\.NET SDK\\\\v2.0\\\\bin\",\"ServerId\":0,\"Timeout\":0}", "C:\\src\\Azure-Hello\\C#\\HelloWorld\\ServiceDefinition.csdef /role:HelloWorld_WebRole;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole;HelloWorld_WebRole.dll /sites:HelloWorld_WebRole;Web;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole /useCtpPackageFormat /copyOnly /out:c:\\temp\\azure\\")]
        [Row("{\"ServiceDefinition\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld\\\\\",\"OutputFile\":\"c:\\\\temp\\\\azure\\\\\",\"WebRole\":{\"RoleName\":\"HelloWorld_WebRole\",\"RoleBinDirectory\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\",\"RoleAssemblyName\":\"HelloWorld_WebRole.dll\"},\"WebRoleSite\":{\"RoleName\":\"HelloWorld_WebRole\",\"VirtualPath\":\"Web\",\"PhysicalPath\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\"},\"UseCtpPackageFormat\":false,\"CopyOnly\":true,\"AzureSDKPath\":\"C:\\\\Program Files\\\\Microsoft SDKs\\\\Windows Azure\\\\.NET SDK\\\\v2.0\\\\bin\",\"ServerId\":0,\"Timeout\":0}", "C:\\src\\Azure-Hello\\C#\\HelloWorld\\ServiceDefinition.csdef /role:HelloWorld_WebRole;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole;HelloWorld_WebRole.dll /sites:HelloWorld_WebRole;Web;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole /copyOnly /out:c:\\temp\\azure\\")]
        [Row("{\"ServiceDefinition\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld\\\\\",\"OutputFile\":\"c:\\\\temp\\\\azure\\\\\",\"WebRole\":{\"RoleName\":\"HelloWorld_WebRole\",\"RoleBinDirectory\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\",\"RoleAssemblyName\":\"HelloWorld_WebRole.dll\"},\"WebRoleSite\":{\"RoleName\":\"HelloWorld_WebRole\",\"VirtualPath\":\"Web\",\"PhysicalPath\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\"},\"UseCtpPackageFormat\":true,\"CopyOnly\":false,\"AzureSDKPath\":\"C:\\\\Program Files\\\\Microsoft SDKs\\\\Windows Azure\\\\.NET SDK\\\\v2.0\\\\bin\",\"ServerId\":0,\"Timeout\":0}", "C:\\src\\Azure-Hello\\C#\\HelloWorld\\ServiceDefinition.csdef /role:HelloWorld_WebRole;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole;HelloWorld_WebRole.dll /sites:HelloWorld_WebRole;Web;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole /useCtpPackageFormat /out:c:\\temp\\azure\\")]
        [Row("{\"ServiceDefinition\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld\\\\\",\"OutputFile\":\"c:\\\\temp\\\\azure\\\\\",\"WebRole\":{\"RoleName\":\"HelloWorld_WebRole\",\"RoleBinDirectory\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\",\"RoleAssemblyName\":\"HelloWorld_WebRole.dll\"},\"WebRoleSite\":{\"RoleName\":\"HelloWorld_WebRole\",\"VirtualPath\":\"Web\",\"PhysicalPath\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\"},\"UseCtpPackageFormat\":false,\"CopyOnly\":false,\"AzureSDKPath\":\"C:\\\\Program Files\\\\Microsoft SDKs\\\\Windows Azure\\\\.NET SDK\\\\v2.0\\\\bin\",\"ServerId\":0,\"Timeout\":0}", "C:\\src\\Azure-Hello\\C#\\HelloWorld\\ServiceDefinition.csdef /role:HelloWorld_WebRole;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole;HelloWorld_WebRole.dll /sites:HelloWorld_WebRole;Web;C:\\src\\Azure-Hello\\C#\\HelloWorld_WebRole /out:c:\\temp\\azure\\")]
        public void TestBuildParameters(string input, string expected)
        {
            //package.ServiceDefinition = @"C:\src\Azure-Hello\C#\HelloWorld\";
            //package.UseCtpPackageFormat = true;
            //package.CopyOnly = true;
            //package.WebRole = new AzureRole() { RoleName = "HelloWorld_WebRole", RoleBinDirectory = @"C:\src\Azure-Hello\C#\HelloWorld_WebRole", RoleAssemblyName = "HelloWorld_WebRole.dll" };
            //package.WebRoleSite = new AzureSite() { RoleName = "HelloWorld_WebRole", VirtualPath = "Web", PhysicalPath = @"C:\src\Azure-Hello\C#\HelloWorld_WebRole" };
            //package.OutputFile = @"c:\temp\azure\";
            //string foo = ServiceStack.Text.JsonSerializer.SerializeToString(package);
            var test = ServiceStack.Text.JsonSerializer.DeserializeFromString<PackageAction>(input);
            Assert.AreEqual(expected, test.BuildParameters());
        }

        //[Test]
        //[Row("{\"ServiceDefinition\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld\\\\\",\"OutputFile\":\"c:\\\\temp\\\\azure\\\\\",\"WebRole\":{\"RoleName\":\"HelloWorld_WebRole\",\"RoleBinDirectory\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\",\"RoleAssemblyName\":\"HelloWorld_WebRole.dll\"},\"WebRoleSite\":{\"RoleName\":\"HelloWorld_WebRole\",\"VirtualPath\":\"Web\",\"PhysicalPath\":\"C:\\\\src\\\\Azure-Hello\\\\C#\\\\HelloWorld_WebRole\"},\"UseCtpPackageFormat\":true,\"CopyOnly\":true,\"AzureSDKPath\":\"C:\\\\Program Files\\\\Microsoft SDKs\\\\Windows Azure\\\\.NET SDK\\\\v2.0\\\\bin\",\"ServerId\":0,\"Timeout\":0}")]
        //public void TestPackage(string input)
        //{
        //    var test = ServiceStack.Text.JsonSerializer.DeserializeFromString<PackageAction>(input);
        //    test.Test();
        //}

        [Test]
        public void TestListLocations()
        {
        }
    }
}
