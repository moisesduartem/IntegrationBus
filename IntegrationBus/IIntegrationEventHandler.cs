using System.Threading.Tasks;

namespace IntegrationBus
{
    public interface IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IntegrationEvent
    {
        Task<bool> Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler2<TIntegrationEvent> 
        : IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IntegrationEvent
    {
        Task<bool> HandleCompensatory(TIntegrationEvent @event);
    }
}