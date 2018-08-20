using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Bank.Cards.Processes.SubscriptionTests.Projections
{
    class EventCounterInMemoryProjection : IProjection
    {
        public long Counter = 0;

        public EventCounterInMemoryProjection()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);

                    Console.WriteLine("-----Status-----");
                    Console.WriteLine($"Event count: {Interlocked.Read(ref Counter)}");
                }
            });
        }

        public Task ProcessEvent(ResolvedEvent resolvedEvent)
        {
            Interlocked.Increment(ref Counter);

            return Task.CompletedTask;
        }
    }
}
