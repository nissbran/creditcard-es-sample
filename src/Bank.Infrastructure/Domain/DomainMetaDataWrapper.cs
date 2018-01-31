namespace Bank.Infrastructure.Domain
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DomainMetaDataWrapper : IDomainMetaData
    {
        private readonly JObject _json;

        public Guid CorrelationId
        {
            get => Guid.Parse(_json["correlationId"].Value<string>());
            set => _json["correlationId"] = value.ToString();
        }

        public string StreamId
        {
            get => _json["streamId"].Value<string>();
            set => _json["streamId"] = value;
        }

        public string Schema
        {
            get => _json["schema"].Value<string>();
            set => _json["schema"] = value;
        }

        public DateTimeOffset Created
        {
            get => _json["created"].Value<DateTimeOffset>();
            set => _json["created"] = value;
        }

        public DomainMetaDataWrapper()
        {
            _json = new JObject();
        }

        public DomainMetaDataWrapper(string json)
        {
            _json = JObject.Parse(json);
        }

        public override string ToString()
        {
            return _json.ToString(Formatting.None);
        }
    }
}