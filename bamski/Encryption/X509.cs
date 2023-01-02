using Bam.Net.Encryption;
using Bam.Net.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Encryption
{
    public static class X509
    {
        
        public static string ToPemString(this X509Certificate2 certificate)
        {
            byte[] certificateBytes = certificate.RawData;
            char[] certificatePem = PemEncoding.Write("CERTIFICATE", certificateBytes);
            return new string(certificatePem);
        }

        public static string GetPrivateKeyPemString(this X509Certificate2 certificate)
        {
            AsymmetricAlgorithm asymmetricAlgorithm = certificate.GetRSAPrivateKey();
            if(asymmetricAlgorithm == null)
            {
                asymmetricAlgorithm = certificate.GetECDsaPrivateKey();
            }
            byte[] privateKeyBytes = asymmetricAlgorithm.ExportPkcs8PrivateKey();
            char[] privateKeyPem = PemEncoding.Write("PRIVATE KEY", privateKeyBytes);
            return new string(privateKeyPem);
        }

        /// <summary>
        /// Get the public key of the certificate as a pem encoded string.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static string GetPublicKeyPemString(this X509Certificate2 certificate)
        {
            AsymmetricAlgorithm asymmetricAlgorithm = certificate.GetRSAPrivateKey();
            if (asymmetricAlgorithm == null)
            {
                asymmetricAlgorithm = certificate.GetECDsaPrivateKey();
            }
            byte[] publicKeyBytes = asymmetricAlgorithm.ExportSubjectPublicKeyInfo();
            char[] publicKeyPem = PemEncoding.Write("PUBLIC KEY", publicKeyBytes);
            return new string(publicKeyPem);
        }


        public static X509Certificate2 GenerateSignedCertificate(string subjectName, string issuerName, AsymmetricKeyParameter issuerPrivateKeyForSigning)
        {
            int validForYears = 2;
            RsaKeyLength rsaKeyLength = RsaKeyLength._2048;
            string signatureAlgorithm = "SHA512WITHRSA";

            SecureRandom random = SecureRandom.GetInstance("SHA256PRNG");
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            X509Name subjectDN = new X509Name(subjectName);
            X509Name issuerDN = new X509Name(issuerName);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(validForYears);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            AsymmetricCipherKeyPair subjectKeyPair = Rsa.GenerateKeyPair(rsaKeyLength);
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerPrivateKeyForSigning, random);
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(signatureFactory);

            // Corresponding private key
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

            X509Certificate2 certificate2 = new X509Certificate2(certificate.GetEncoded());

            Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(info.GetDerEncoded());

            RsaPrivateKeyStructure rsa = RsaPrivateKeyStructure.GetInstance(seq);
            RsaPrivateCrtKeyParameters rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

            certificate2.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
            return certificate2;
        }

        public static X509Certificate2 Convert(this Org.BouncyCastle.X509.X509Certificate certificate)
        {
            return new X509Certificate2(certificate.GetEncoded());
        }

        public static bool AddCertificateToStore(Org.BouncyCastle.X509.X509Certificate certificate, StoreName storeName, StoreLocation storeLocation, Action<Exception> exceptionHandler = null)
        {
            return AddCertificateToStore(certificate.Convert(), storeName, storeLocation, exceptionHandler);
        }

        public static bool AddCertificateToStore(X509Certificate2 certificate, StoreName storeName, StoreLocation storeLocation, Action<Exception> exceptionHandler = null)
        {
            try
            {
                X509Store store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
            }
            catch (Exception ex)
            {
                Action<Exception> handler = exceptionHandler ?? ((Exception e) => { Console.WriteLine(e.Message); });
                handler(ex);
                return false;
            }
            return true;
        }

        public static X509Certificate2 LoadBytes(string filePath)
        {
            return new X509Certificate2(File.ReadAllBytes(filePath));
        }

        public static X509CertificateDetails CreateRootCertificateFile(string filePath, string subjectName, int validForYears = 2, string signatureAlgorithm = "SHA512WITHRSA")
        {
            X509CertificateDetails certificateDetails = GenerateRootCertificate(subjectName, validForYears, signatureAlgorithm);
            Org.BouncyCastle.X509.X509Certificate generatedCertificate = certificateDetails.Certificate;
            File.WriteAllBytes(filePath, generatedCertificate.GetEncoded());
            Log.AddEntry("Wrote self signed root certificate file: {0}", filePath);
            return certificateDetails;
        }

        /// <summary>
        /// Creates a self signed certificate valid for use as a root certificate.  The issuer is set to the value specified for subjectName.
        /// </summary>
        /// <param name="subjectName">The subject name.  This value is also used as the issuer.</param>
        /// <param name="validForYears">The number of years the certificate is valid for.  Default is 2.</param>
        /// <param name="signatureAlgorithm">The signature algorithm.  Default is "SHA512WITHRSA".</param>
        /// <param name="rsaKeyLength">The kye length.  Default is 2048.</param>
        /// <returns></returns>
        public static X509CertificateDetails GenerateRootCertificate(string subjectName, int validForYears = 2, string signatureAlgorithm = "SHA512WITHRSA", RsaKeyLength rsaKeyLength = RsaKeyLength._2048)
        {
            SecureRandom random = SecureRandom.GetInstance("SHA256PRNG");
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            X509Name subjectDN = new X509Name(subjectName);
            X509Name issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(validForYears);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            AsymmetricCipherKeyPair subjectKeyPair = Rsa.GenerateKeyPair(rsaKeyLength);
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            AsymmetricCipherKeyPair issuerKeyPair = subjectKeyPair;
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private, random);
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(signatureFactory);
            return new X509CertificateDetails(certificate, subjectName, validForYears, signatureAlgorithm)
            {
                SigningKeyPair = issuerKeyPair
            };
        }
    }
}
