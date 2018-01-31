namespace Bank.Infrastructure.Domain
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventTypeAttribute : Attribute
    {
        public string Name { get; }

        public int Version { get; }

        public EventTypeAttribute(string name) : this(name, 1)
        {
        }

        public EventTypeAttribute(string name, int version)
        {
            Name = name;
            Version = version;
        }
    }
}