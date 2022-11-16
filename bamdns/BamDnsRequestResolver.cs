using Bam.Net.CommandLine;
using Bam.Net.CoreServices.NameResolution.Data;
using Bam.Net.Logging;
using CsvHelper;
using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using MongoDB.Driver.Core.Servers;
using System.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnsClient;

namespace Bam.Net.CoreServices.NameResolution
{
    public class BamDnsRequestResolver : IRequestResolver
    {
        List<RootDnsServerDescriptor> _serverDescriptors;

        public BamDnsRequestResolver()
        {
            this.LoadRootDnsServerData();
        }

        public async Task<IResponse> Resolve(IRequest request)
        {
            IResponse response = Response.FromRequest(request);
            List<Task> resolutions = new List<Task>();
            foreach(RootDnsServerDescriptor serverDescriptor in _serverDescriptors)
            {
                try
                {
                    LookupClient dnsClient = new LookupClient(IPAddress.Parse(serverDescriptor.Ipv4Address));
                    foreach(Question requestQuestion in request.Questions)
                    {
                        DnsQuestion question = new DnsQuestion(requestQuestion.ToString(), QueryType.A);
                    }

                    IDnsQueryResponse dnsResponse = dnsClient.Query();
                }
                catch (Exception ex)
                {
                    Message.PrintLine("{0}:\r\n{1}", ex, ex.Message, ex.StackTrace);
                }
            }
            return response;
        }

        protected void LoadRootDnsServerData()
        {
            Process process = Process.GetCurrentProcess();
            FileInfo main = new FileInfo(process.MainModule.FileName);
            string rootDnsServerData = Path.Combine(main.Directory.FullName, "root-servers.csv");
            if (!File.Exists(rootDnsServerData))
            {
                Log.Warn("root-servers.csv file not found, BamDns will not resolve public host records: {0}", rootDnsServerData);
                return;
            }

            using (StreamReader reader = new StreamReader(rootDnsServerData))
            {
                using (CsvReader csvReader = new CsvReader(reader, new CsvHelper.Configuration.Configuration { HeaderValidated = null, MissingFieldFound = null }))
                {
                    _serverDescriptors = new List<RootDnsServerDescriptor>(csvReader.GetRecords<RootDnsServerDescriptor>());
                }
            }
        }
    }
}
