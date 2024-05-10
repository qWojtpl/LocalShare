
using LocalShareCommunication.Client;

namespace LocalShareCommunication.Events;

public class EventHandler<T>
{

    private readonly List<Action<EventType, T>> events = new();

    public void SendEvent(EventType type, T process)
    {
        try
        {
            foreach (var action in events)
            {
                action.Invoke(type, process);
            }
        } catch(Exception)
        {
            SendEvent(type, process);
        }
    }

    public void AddEventHandler(Action<EventType, T> action)
    {
        events.Add(action);
    }

}
