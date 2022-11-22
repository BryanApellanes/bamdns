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
            try
            {
                IResponse response = Response.FromRequest(request);
                RootDnsServerDescriptor server = _serverDescriptors.FirstOrDefault();

                ClientRequest clientRequest = new ClientRequest(server.Ipv4Address);
                clientRequest.RecursionDesired = true;

                foreach (Question question in response.Questions)
                {
                    clientRequest.Questions.Add(question);
                }

                IResponse clientResponse = await clientRequest.Resolve();
                foreach(DNS.Protocol.ResourceRecords.IResourceRecord record in clientResponse.AdditionalRecords)
                {
                    response.AdditionalRecords.Add(record);
                }
                foreach(DNS.Protocol.ResourceRecords.IResourceRecord record in clientResponse.AnswerRecords)
                {
                    response.AnswerRecords.Add(record);
                }

                response.AuthenticData = clientResponse.AuthenticData;
                response.AuthorativeServer = clientResponse.AuthorativeServer;
            }
            catch (Exception ex)
            {
                Message.PrintLine("{0}:\r\n{1}", ex, ex.Message, ex.StackTrace);
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
