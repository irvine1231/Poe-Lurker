namespace Lurker.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PlayerJoinedEvent : PlayerEvent
    {
        //private static readonly string JoinTheAreaMarker = "has joined the area";
        private static readonly List<string> JoinTheAreaMarkerw;

        static PlayerJoinedEvent()
        {
            List<string> list1 = new List<string>();
            list1.Add("has joined the area");
            list1.Add("進入了此區域");
            JoinTheAreaMarkerw = list1;
        }


        private PlayerJoinedEvent(string logLine) : base(logLine)
        {
            //int index = base.Message.IndexOf(JoinTheAreaMarker);
            //base.SetPlayerName(index);
        }

        public static PlayerJoinedEvent TryParse(string logLine)
        {
            //string str = ParseInformations(logLine);
            //return ((string.IsNullOrEmpty(str) || (!str.StartsWith(PoeEvent.MessageMarker) || !str.Contains(JoinTheAreaMarker))) ? null : new PlayerJoinedEvent(logLine));

            string informations = ParseInformations(logLine);
            if (string.IsNullOrEmpty(informations))
            {
                return null;
            }
            bool flag = JoinTheAreaMarkerw.Any<string>(m => informations.StartsWith(PoeEvent.MessageMarker + m));
            return ((string.IsNullOrEmpty(informations) || !flag) ? null : new PlayerJoinedEvent(logLine));
        }
    }
}

