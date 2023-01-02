using Bam.Net;
using Bam.Net.CommandLine;
using Bam.Encryption;
using Bam.Net.Encryption;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Encryption
{
    [Serializable]
    public class CertificateAuthorityConsoleActions : CommandLineTool
    {
        [ConsoleAction("Generate a self signed certificate usable as a certificate authority root certificate")]
        public void GenerateRootCertificate()
        {
            string subjectName = GetArgument("subjectName");
            int validForYears = GetArgumentOrDefault("validForYears", "2").ToInt(2);
            //string certificateFile = GetArgumentOrDefault("certificateFile", "./selfsignedx509.cer");

            X509CertificateDetails certificateDetails = X509.GenerateRootCertificate(subjectName, validForYears, "SHA512WITHRSA");

            //Org.BouncyCastle.X509.X509Certificate certificate = X509.GenerateRootCertificate(subjectName, validForYears, signatureAlgorithm).Certificate;

            
        }

        [ConsoleAction]
        public void GeneratePrivateKey()
        {

        }

        [ConsoleAction]
        public void ExtractPublicKey()
        {
        }

        [ConsoleAction]
        public void AddCertificateToStore()
        {
            StoreName storeName = GetArgumentOrDefault("storeName", "Root").ToEnum<StoreName>();
            StoreLocation storeLocation = GetArgumentOrDefault("storeLocation", "LocalMachine").ToEnum<StoreLocation>();

        }
    }
}
