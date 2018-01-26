namespace Bank.Infrastructure.Domain
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventNameAttribute : Attribute
    {
        public string Name { get; }

        public EventNameAttribute(string name)
        {
            Name = name;
        }
    }
}