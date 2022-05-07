namespace Lurker.Events
{
    using System;
    using System.Runtime.CompilerServices;

    public class LocationChangedEvent : PoeEvent
    {
        private static readonly string LocationMarker = "You have entered";

        private LocationChangedEvent(string logLine) : base(logLine)
        {
            this.Location = base.Message.Substring(LocationMarker.Length + 1, (base.Message.Length - LocationMarker.Length) - 2);
        }

        public static LocationChangedEvent TryParse(string logLine)
        {
            string str = ParseInformations(logLine);
            return ((string.IsNullOrEmpty(str) || !str.StartsWith(PoeEvent.MessageMarker + LocationMarker)) ? null : new LocationChangedEvent(logLine));
        }

        public string Location { get; private set; }
    }
}

