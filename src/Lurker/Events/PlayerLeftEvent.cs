namespace Lurker.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PlayerLeftEvent : PlayerEvent
    {
        //private static readonly string LeftTheAreaMarker = "has left the area";
        private static readonly List<string> LeftTheAreaMarkerw;

        static PlayerLeftEvent()
        {
            List<string> list1 = new List<string>();
            list1.Add("has left the area");
            list1.Add("離開了此區域");
            LeftTheAreaMarkerw = list1;
        }

        private PlayerLeftEvent(string logLine) : base(logLine)
        {
            //int index = base.Message.IndexOf(LeftTheAreaMarker);
            //base.SetPlayerName(index);
        }

        public static PlayerLeftEvent TryParse(string logLine)
        {
            //string str = ParseInformations(logLine);
            //return ((string.IsNullOrEmpty(str) || (!str.StartsWith(PoeEvent.MessageMarker) || !str.Contains(LeftTheAreaMarker))) ? null : new PlayerLeftEvent(logLine));

            string informations = ParseInformations(logLine);
            if (string.IsNullOrEmpty(informations))
            {
                return null;
            }
            bool flag = LeftTheAreaMarkerw.Any<string>(m => informations.StartsWith(PoeEvent.MessageMarker + m));
            return ((string.IsNullOrEmpty(informations) || !flag) ? null : new PlayerLeftEvent(logLine));
        }
    }
}

