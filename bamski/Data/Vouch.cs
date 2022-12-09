using Bam.Net.Data.Repositories;
using Bam.SocialKeyInfrastructure.Data;

namespace Bam.Net.CoreServices.NameResolution.Data
{
    public class Vouch : CompositeKeyAuditRepoData
    {
        public virtual DnsServerDescriptor DnsServerDescriptor { get; set; }
        public string User { get; set; }
        public int Value { get; set; }
    }
}