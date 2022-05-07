namespace Lurker.Events
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class MonstersRemainEvent : PoeEvent
    {
        private static readonly string MonstersRemainMarker = "monsters remain.";

        private MonstersRemainEvent(string logLine) : base(logLine)
        {
            int index = base.Message.IndexOf(MonstersRemainMarker);
            char[] separator = new char[] { ' ' };
            string s = base.Message.Substring(0, index - 1).Split(separator).Last<string>();
            this.MonsterCount = int.Parse(s);
        }

        public static MonstersRemainEvent TryParse(string logLine)
        {
            string str = ParseInformations(logLine);
            return ((string.IsNullOrEmpty(str) || (!str.StartsWith(PoeEvent.MessageMarker) || !str.EndsWith(MonstersRemainMarker))) ? null : new MonstersRemainEvent(logLine));
        }

        public int MonsterCount { get; private set; }
    }
}

