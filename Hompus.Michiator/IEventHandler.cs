namespace Hompus.Michiator; 

public interface IEventHandler<TEvent> 
    where TEvent : IEvent
{
    Task Handle(TEvent @event);
}
