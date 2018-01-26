using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bank.Infrastructure.Domain;
using Bank.Infrastructure.EventStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Bank.Persistence.Sql
{
    public class SqlEventStore : IEventStore
    {
        private const int ReadBatchSize = 5000;

        private readonly IStreamStore _streamStore;
        private readonly Dictionary<string, IEventSchema> _eventSchemas = new Dictionary<string, IEventSchema>();
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public SqlEventStore(IStreamStore streamStore, IEnumerable<IEventSchema> eventSchemas)
        {
            _streamStore = streamStore;

            foreach (var schema in eventSchemas)
            {
                _eventSchemas.Add(schema.Name, schema);
            }
            
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task<IList<IDomainEvent>> GetEventsByStreamId(EventStreamId eventStreamId)
        {
            var streamMessages = new List<StreamMessage>();
            
            ReadStreamPage currentStreamPage;
            var nextSliceStart = 0;
            do
            {
                currentStreamPage = await _streamStore.ReadStreamForwards(
                    streamId: new StreamId(eventStreamId.StreamName),
                    fromVersionInclusive: nextSliceStart,
                    maxCount: ReadBatchSize);

                nextSliceStart = currentStreamPage.NextStreamVersion;
                streamMessages.AddRange(currentStreamPage.Messages);

            } while (!currentStreamPage.IsEnd);

            return streamMessages.Select(ConvertStreamMessageToDomainEvent).ToList();
        }

        public async Task<StreamWriteResult> SaveEvents(EventStreamId eventStreamId, long streamVersion, List<IDomainEvent> events)
        {
            if (events.Any() == false)
                return new StreamWriteResult(-1);

            var commitId = Guid.NewGuid();

            var expectedVersion = (int)streamVersion == 0 ? ExpectedVersion.NoStream : (int)streamVersion - 1;
            var streamMessagesToAppend = new List<NewStreamMessage>();

            foreach (var domainEvent in events)
            {
                streamMessagesToAppend.Add(ToNewStreamMessage(commitId, domainEvent));
            }

            try
            {
                var result = await _streamStore.AppendToStream(
                    new StreamId(eventStreamId.StreamName),
                    expectedVersion,
                    streamMessagesToAppend.ToArray());

                return new StreamWriteResult(result.CurrentPosition);
            }
            catch (WrongExpectedVersionException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private IDomainEvent ConvertStreamMessageToDomainEvent(StreamMessage streamMessage)
        {
            var metadata = JsonConvert.DeserializeObject<DomainMetadata>(streamMessage.JsonMetadata, _jsonSerializerSettings);

            _eventSchemas.TryGetValue(metadata.Schema, out var schema);

            var eventType = schema.GetDomainEventType(streamMessage.Type);

            return (IDomainEvent)JsonConvert.DeserializeObject(streamMessage.GetJsonData().Result, eventType, _jsonSerializerSettings);
        }

        private NewStreamMessage ToNewStreamMessage(Guid commitId, IDomainEvent domainEvent)
        {
            _eventSchemas.TryGetValue(domainEvent.Schema, out var schema);

            var definition = schema.GetEventDefinition(domainEvent);

            var dataJson = JsonConvert.SerializeObject(domainEvent, _jsonSerializerSettings);
            var metadataJson = JsonConvert.SerializeObject(new DomainMetadata
            {
                CorrelationId = commitId,
                Schema = domainEvent.Schema,
                Created = DateTimeOffset.UtcNow
            }, _jsonSerializerSettings);

            return new NewStreamMessage(Guid.NewGuid(), definition, dataJson, metadataJson);
        }
    }
}
