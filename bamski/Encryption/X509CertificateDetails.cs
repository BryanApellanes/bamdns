using Bam.Encryption.Data;
using Bam.Net.Data.Repositories;
using Bam.Net.Encryption;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bam.Encryption
{
    public class X509CertificateDetails
    {
        public X509CertificateDetails()
        {
            ValidForYears = 2;
            SignatureAlgorithm = "SHA512WITHRSA";
        }

        public X509CertificateDetails(Org.BouncyCastle.X509.X509Certificate certificate, string subjectName, int validForYears = 2, string signatureAlgorithm = "SHA512WITHRSA")
        {
            Certificate = certificate;            
            this.SubjectName = subjectName;
            this.ValidForYears = validForYears;
            this.SignatureAlgorithm = signatureAlgorithm;
        }

        public X509CertificateDetails(string signingKeyPairPem, string certificatePem)
        {
            SigningKeyPair = Pem.FromPem(signingKeyPairPem);
            Certificate2 = X509Certificate2.CreateFromPem(certificatePem, signingKeyPairPem);
        }

        /// <summary>
        /// Gets the signing key pair.
        /// </summary>
        public AsymmetricCipherKeyPair SigningKeyPair { get; internal set; }

        /// <summary>
        /// Gets the private key of the signing key pair.
        /// </summary>
        public AsymmetricKeyParameter SigningKey { get => SigningKeyPair.Private; }

        /// <summary>
        /// Gets the Bouncy Castle compatible certifcate.
        /// </summary>
        public Org.BouncyCastle.X509.X509Certificate Certificate { get; private set; }

        X509Certificate2 _certificate2;
        /// <summary>
        /// Gets the System.Security.Cryptography compatible certificate.
        /// </summary>
        public X509Certificate2 Certificate2 
        {
            get
            {
                if(_certificate2 == null)
                {
                    _certificate2 = new X509Certificate2(Certificate.GetEncoded());
                }
                return _certificate2;
            }
            private set
            {
                _certificate2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the subject name.
        /// </summary>
        public string SubjectName{ get; set; }

        /// <summary>
        /// Gets or sets the number of years the certificate is valid for.
        /// </summary>
        public int ValidForYears { get; set; }

        /// <summary>
        /// Gets or sets the signature algorithm.
        /// </summary>
        public string SignatureAlgorithm { get; set; }

        public X509CertificateDescriptor Save(IRepository repository)
        {
            throw new NotImplementedException();
            X509CertificateDescriptor certificateDescriptor = new X509CertificateDescriptor
            {
                
            };
        }
    }
}
