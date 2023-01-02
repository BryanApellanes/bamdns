using Bam.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Encryption
{
    public class Arguments : CommandLineTool
    {
        public static void Add()
        {
            AddValidArgument("subjectName", false, false, GetDescription(nameof(CertificateAuthorityConsoleActions.GenerateRootCertificate), "the subject name used for the certificate"));
            AddValidArgument("validForYears", false, false, GetDescription(nameof(CertificateAuthorityConsoleActions.GenerateRootCertificate), "the number of years the certificate is valid for"));
            AddValidArgument("signatureAlgorithm", false, false, GetDescription(nameof(CertificateAuthorityConsoleActions.GenerateRootCertificate), "the signature algorithm to use"));

            AddValidArgument("storeName", false, false, GetDescription(nameof(CertificateAuthorityConsoleActions.AddCertificateToStore), $"the store name to add the certificate to: valid values are ({string.Join(", ", Enum.GetNames(typeof(StoreName)))})"));
            AddValidArgument("storeLocation", false, false, GetDescription(nameof(CertificateAuthorityConsoleActions.AddCertificateToStore), $"the store location to add the certificate to: valid values are ({string.Join(", ", Enum.GetNames(typeof(StoreLocation)))})"));
            AddValidArgument("certificateFile", false, false, GetDescription(nameof(CertificateAuthorityConsoleActions.AddCertificateToStore), $"the path to the base64 encoded x509 certificate file"));
        }

        private static string GetDescription(string onMethodName, string extendedDescription)
        {
            return $"On {onMethodName}, {extendedDescription}";
        }
    }
}
