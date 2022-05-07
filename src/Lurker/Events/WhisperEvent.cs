namespace Lurker.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WhisperEvent : PlayerEvent
    {
        private static readonly List<string> ToMarkers;
        private static readonly List<string> FromMarkers;

        static WhisperEvent()
        {
            List<string> list1 = new List<string>();
            list1.Add("@To");
            list1.Add("@An");
            list1.Add("@\x00c0");
            list1.Add("@Para");
            list1.Add("@От");
            list1.Add("@ถึง");
            list1.Add("@Para");
            list1.Add("@발신");
            list1.Add("@向");
            ToMarkers = list1;
            List<string> list2 = new List<string>();
            list2.Add("@From");
            list2.Add("@Von");
            list2.Add("@De");
            list2.Add("@De");
            list2.Add("@Кому");
            list2.Add("@จาก");
            list2.Add("@De");
            list2.Add("@수신");
            list2.Add("@來自");
            FromMarkers = list2;
        }

        protected WhisperEvent(string logLine) : base(logLine)
        {
            int index = base.Informations.IndexOf(PoeEvent.MessageMarker);
            string str = base.Informations.Substring(0, index);
            char[] separator = new char[] { ' ' };
            base.PlayerName = string.Join(" ", str.Split(separator).Skip<string>(1));
            int num2 = base.PlayerName.IndexOf(PlayerEvent.EndOfGuildNameMarker);
            if (num2 != -1)
            {
                base.GuildName = base.PlayerName.Substring(1, num2 - 1);
                base.PlayerName = base.PlayerName.Substring(num2 + 2);
            }
        }

        public static bool IsIncoming(string logLine) =>
            StartWithMarker(FromMarkers, logLine);

        public static bool IsOutgoing(string logLine) =>
            StartWithMarker(ToMarkers, logLine);

        private static bool StartWithMarker(IEnumerable<string> makers, string logLine)
        {
            string informations = ParseInformations(logLine);
            return (!string.IsNullOrEmpty(informations) && makers.Any<string>(m => informations.StartsWith(m)));
        }

        public static WhisperEvent TryParse(string logLine) =>
            IsIncoming(logLine) ? new WhisperEvent(logLine) : null;

        public string WhisperMessage =>
            base.Message;
    }
}

