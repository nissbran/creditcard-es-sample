using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Bank.Cards.Processes.ReadProjections.Projections
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
                    Console.WriteLine($"Event count: {Counter}");
                }
            });
        }

        public Task ProcessEvent(ResolvedEvent resolvedEvent)
        {
            Counter++;

            return Task.CompletedTask;
        }

        public void QueueEvent(ResolvedEvent resolvedEvent)
        {
            throw new NotImplementedException();
        }
    }
}
