using EventSourcing.Shared.Events;
using EventStore.ClientAPI;
using System.ComponentModel.Design;
using System.Text;
using System.Text.Json;

namespace EventSourcing.API.EventStores
{
    public abstract class AbstractStream
    {
        protected readonly LinkedList<IEvent> Events= new LinkedList<IEvent>();

        private readonly string _streamName;
        private readonly IEventStoreConnection _connection;

        protected AbstractStream(string streamName, IEventStoreConnection connection)
        {
            _streamName = streamName;
            _connection = connection;
        }

        public async Task SaveAsync()
        {
            var newEvents=Events.Select(x=> new EventData(Guid.NewGuid(),x.GetType().Name
                 ,true
                 ,Encoding.UTF8.GetBytes(JsonSerializer.Serialize(x,x.GetType()))
                 ,Encoding.UTF8.GetBytes(x.GetType().FullName)
                ));

            await _connection.AppendToStreamAsync(_streamName, ExpectedVersion.Any, newEvents);

            Events.Clear();
        }
    }
}
