
namespace IntegrationBus
{
    public interface IIntegrationBus
    {
        /// <summary>
        /// Publish a event to service queue with de routing key the same as @event name.
        /// </summary>
        /// <param name="event"></param>
        bool Publish(IntegrationEvent @event);

        /// <summary>
        /// Subscribe a event with event handler.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        ISubscription Subscribe<TEvent, TEventHandler>(int consumersAmount = -1)
            where TEvent : IntegrationEvent
            where TEventHandler : IIntegrationEventHandler<TEvent>;

        void EnableConsume();
    }
}
