namespace Hompus.Michiator; 

public interface ISagaStartedBy<TEvent> where TEvent : IEvent
{
    Task HandleStart(TEvent @event);
}
