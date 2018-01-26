using System.Collections.Generic;
using System.Threading.Tasks;
using Bank.Infrastructure.EventStore;
using SqlStreamStore;

namespace Bank.Persistence.Sql.Configuration
{
    public static class StreamStoreFactory
    {
        public static async Task<IStreamStore> InitializeMsSqlStreamStore(string connectionString)
        {
            var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(connectionString));

            await streamStore.CreateSchema();

            return streamStore;
        }

        public static async Task<IStreamStore> InitializeMsSqlStreamStoreWithSchemas(string connectionString, List<IEventSchema> eventSchemas)
        {
            var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(connectionString));

            //foreach (var eventSchema in eventSchemas)
            //{
            //    await streamStore.CreateSchema();
            //}

            return streamStore;
        }
    }
}