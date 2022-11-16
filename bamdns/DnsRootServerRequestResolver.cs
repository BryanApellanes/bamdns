using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bam.Net.Data.Repositories;
using Bam.Net.CoreServices.NameResolution.Data;
using CsvHelper;
using DNS.Client.RequestResolver;
using DNS.Client;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Bam.Net.Logging;
using DnsClient = DNS.Client.DnsClient;
using System.Diagnostics;
using Markdig.Extensions.Mathematics;
using Bam.Net.CoreServices.ApplicationRegistration.Data;
using System.Runtime.CompilerServices;
using DNS.Protocol.Utils;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Ubiety.Dns.Core;
using Microsoft.AspNetCore.Mvc.Internal;
using System.Threading.Tasks.Dataflow;
using Org.BouncyCastle.Asn1.Ocsp;
using Markdig.Extensions.TaskLists;
using Bam.Net.CoreServices.ApplicationRegistration.Data.Dao;
using System.Collections;
using MySql.Data.MySqlClient.Memcached;
using Microsoft.AspNetCore.Mvc;
using Ubiety.Dns.Core.Common;
using DnsClient;
using Microsoft.AspNetCore.Mvc.RazorPages.Internal;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Bam.Net.UserAccounts.Data;
using MySqlX.XDevAPI;
using System.Drawing.Imaging;
using DnsClient.Protocol;
/*using Ubiety.Dns.Core;
using Ubiety.Dns.Core.Common;*/

namespace Bam.Net.CoreServices.NameResolution
{
    /// <summary>
    /// A Dns name resolver that resolves A records by asking a list of root servers specified in the root-servers.csv file,
    /// </summary>
    public class DnsRootServerRequestResolver : IRequestResolver
    {
        readonly HashSet<DnsServerDescriptorClient> _clients;
        readonly Dictionary<DNS.Protocol.RecordType, List<Action<IResponse>>> _recordTypeHandlers;
        public DnsRootServerRequestResolver()
        {
            _clients = new HashSet<DnsServerDescriptorClient>();
            _recordTypeHandlers = new Dictionary<DNS.Protocol.RecordType, List<Action<IResponse>>>();
            
            Repository = new DaoInheritanceRepository();
            Repository.AddType<DnsResponse>();
            Repository.AddType<RootDnsServerDescriptor>();
            
            LoadRootDnsServerData();
            AddRootServerARecordResolver();
        }
        
        public DaoInheritanceRepository Repository { get; set; }
        
        public async Task<IResponse> Resolve(IRequest request)
        {
            IResponse response = DNS.Protocol.Response.FromRequest(request);
            List<Task> lookupActions = new List<Task>();

            foreach (DNS.Protocol.Question question in response.Questions)
            {

                if (_recordTypeHandlers.ContainsKey(question.Type))
                {
                    if (_recordTypeHandlers.ContainsKey(question.Type))
                    {
                        foreach(Action<IResponse> action in _recordTypeHandlers[question.Type])
                        {
                            lookupActions.Add(Task.Run(() => action(response)));
                        }
                    }
                    else
                    {
                        Log.Warn("No handlers are registered for question type {0}", question.Type.ToString());

                    }
                }
            }

            Task.WaitAll(lookupActions.ToArray());
            return response;
        }

        public void ClearHandlers(DNS.Protocol.RecordType recordType)
        {
            if (_recordTypeHandlers.ContainsKey(recordType))
            {
                _recordTypeHandlers[recordType].Clear();
            }
        }
        
        public void AddHandler(DNS.Protocol.RecordType recordType, Action<IResponse> responseHandler)
        {
            if (!_recordTypeHandlers.ContainsKey(recordType))
            {
                _recordTypeHandlers.Add(recordType, new List<Action<IResponse>>());
            }
            
            _recordTypeHandlers[recordType].Add(responseHandler);
                
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
                    foreach (RootDnsServerDescriptor serverInfo in csvReader.GetRecords<RootDnsServerDescriptor>())
                    {
                        _clients.Add(new DnsServerDescriptorClient(serverInfo));
                    }
                }
            }
        }

        protected void AddRootServerARecordResolver()
        {
            AddHandler(DNS.Protocol.RecordType.A, response =>
            {
                try
                {

                    foreach (DNS.Protocol.Question question in response.Questions)
                    {
                        foreach (DnsServerDescriptorClient client in _clients)
                        {
                            ClientRequest clientRequest = new ClientRequest(client.DnsServerDescriptor.Ipv4Address);
                            clientRequest.Questions.Add(new DNS.Protocol.Question(Domain.FromString(question.Name.ToString()), DNS.Protocol.RecordType.A));
                            clientRequest.RecursionDesired = true;
                            clientRequest.Resolve();

                            /*IList<IPAddress> addresses = client.Lookup(question.Name.ToString()).Result.ToList();
                            foreach (IPAddress address in addresses)
                            {
                                response.AnswerRecords.Add(new IPAddressResourceRecord(question.Name, address));
                            }*/
                        }
                    }
                }
                catch (Exception ex) { }
                
            });
        }
    }
}