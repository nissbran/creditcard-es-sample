namespace Bank.Cards.Processes.SubscriptionTests.Projections
{
    using System.Threading.Tasks;
    using EventStore.ClientAPI;

    public interface IProjection
    {
        Task ProcessEvent(ResolvedEvent resolvedEvent);
    }
}