using System;
using System.Security.Cryptography.X509Certificates;

namespace Inedo.BuildMasterExtensions.Azure
{
    [Serializable]
    public class AzureAuthentication
    {
        public string SubscriptionID { get; set; }

        public string PEMENcoded { get; set; }

        public string CertificateName { get; set; }

        public string ConfigFileName { get; set; }

        public bool HasCertificate
        {
            get
            {
                return !string.IsNullOrEmpty(PEMENcoded) || !string.IsNullOrEmpty(CertificateName) || !string.IsNullOrEmpty(ConfigFileName);
            }
        }

        public X509Certificate2 Certificate
        {
            get
            {
                if (!string.IsNullOrEmpty(this.PEMENcoded))
                {
                    return GetFromString(this.PEMENcoded);
                }
                if (!string.IsNullOrEmpty(this.CertificateName))
                {
                    return GetByNameFromCertStore(this.CertificateName);
                }
                return null;
            }
        }

        internal X509Certificate2 GetByNameFromCertStore(string Name)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, Name, false);
                store.Close();
                if (0 == certs.Count)
                    throw new Exception(string.Format("Cannot find certificate named {0} in machine store", Name));
                return certs[0];
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal X509Certificate2 GetFromString(string Value)
        {
            throw new NotImplementedException();
        }
    }
}
