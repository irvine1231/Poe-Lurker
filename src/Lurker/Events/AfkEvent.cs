namespace Lurker.Events
{
    using Lurker.Patreon.Extensions;
    using System;
    using System.Runtime.CompilerServices;

    public class AfkEvent : PoeEvent
    {
        private static readonly string AfkMarker = "AFK mode is now";

        public AfkEvent(string logLine) : base(logLine)
        {
            if (base.Message.Substring(AfkMarker.Length + 1, (base.Message.Length - AfkMarker.Length) - 2).GetLineBefore(".") == "ON")
            {
                this.AfkEnable = true;
            }
        }

        public static AfkEvent TryParse(string logLine)
        {
            string str = ParseInformations(logLine);
            return ((string.IsNullOrEmpty(str) || !str.StartsWith(PoeEvent.MessageMarker + AfkMarker)) ? null : new AfkEvent(logLine));
        }

        public bool AfkEnable { get; set; }
    }
}

