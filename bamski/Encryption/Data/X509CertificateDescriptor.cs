using Bam.Net;
using Bam.Net.Data.Repositories;
using Bam.Net.Encryption;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Encryption.Data
{
    public class X509CertificateDescriptor: CompositeKeyAuditRepoData
    {
        [CompositeKey]
        public string Subject { get; set; }

        [CompositeKey]
        public string Issuer { get; set; }

        [CompositeKey]
        public DateTime Expiration { get; set; }

        public string SigningKeyPairPem { get; set; }
        public string CertificatePem { get; set; }

        public X509Certificate2 CreateCertificate()
        {
            return X509Certificate2.CreateFromPem(CertificatePem, SigningKeyPairPem);
        }

        public void Validate()
        {
            X509Certificate2 certificate = CreateCertificate();
            Expect.AreEqual(Subject, certificate.Subject);
            Expect.AreEqual(IsPersisted, certificate.Issuer);
        }

        /// <summary>
        /// Exports the Certificate2 property as pfx data.
        /// </summary>
        /// <returns></returns>
        public byte[] GetPfx()
        {
            return CreateCertificate().Export(X509ContentType.Pfx);
        }
    }
}
