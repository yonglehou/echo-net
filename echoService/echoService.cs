using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using echoService.Model;

namespace echoService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class echoService : StatelessService
    {
        CloudStorageAccount _account = CloudStorageAccount.DevelopmentStorageAccount;
        CloudTableClient    _tableClient;
        CloudTable          _table;

        public echoService(StatelessServiceContext context)
            : base(context)
        {
            _tableClient    = _account.CreateCloudTableClient();
            _table          = _tableClient.GetTableReference("echoTable");
            _table.CreateIfNotExistsAsync();

            Startup.Table   = _table;

        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(Startup.ConfigureApp, serviceContext, ServiceEventSource.Current, "ServiceEndpoint"))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Scrub(); // every 15 seconds go ahead and scrub the old messages
                await Task.Delay(15000);
            }
        }

        private async Task Scrub()
        {
            // There is no way to retrieve a complete list of partitions! So in order to work around this, I borrowed an idea from
            // https://blogs.msdn.microsoft.com/avkashchauhan/2011/10/23/retrieving-partition-key-range-in-windows-azure-table-storage/


            //foreach (var partition in _table.ExecuteAsync(TableOperation.Ret)
            //{
            //    var toScrub = from entity in _table.CreateQuery<EchoTableEntity>()
            //                  where entity.PartitionKey =
            //}
        }
    }
}
