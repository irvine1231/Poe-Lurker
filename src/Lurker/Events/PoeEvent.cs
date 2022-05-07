namespace Lurker.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public abstract class PoeEvent : EventArgs
    {
        protected static readonly string MessageMarker = ": ";

        protected PoeEvent(string logLine)
        {
            this.Id = Guid.NewGuid();
            char[] separator = new char[] { ' ' };
            IEnumerable<string> values = logLine.Split(separator).Take<string>(2);
            this.Date = DateTime.Parse(string.Join(" ", values), CultureInfo.InvariantCulture);
            this.Message = ParseMessage(logLine);
            this.Informations = ParseInformations(logLine);
        }

        protected static string ParseInformations(string logLine)
        {
            int index = logLine.IndexOf("]");
            return ((index != -1) ? logLine.Substring(index + 2) : null);
        }

        protected static string ParseMessage(string logLine)
        {
            int index = logLine.IndexOf(MessageMarker);
            return ((index != -1) ? logLine.Substring(index + MessageMarker.Length) : null);
        }

        public Guid Id { get; private set; }

        public DateTime Date { get; private set; }

        protected string Informations { get; private set; }

        protected string Message { get; private set; }
    }
}

