namespace Lurker.Events
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class PlayerLevelUpEvent : PlayerEvent
    {
        private static readonly string LevelUpMarker = " is now level";

        public PlayerLevelUpEvent(string logLine) : base(logLine)
        {
            int index = base.Message.IndexOf("(");
            int num2 = base.Message.IndexOf(")");
            base.SetPlayerName(index);
            this.Class = base.Message.Substring(index + 1, (num2 - index) - 1);
            char[] separator = new char[] { ' ' };
            this.Level = Convert.ToInt32(base.Message.Split(separator).Last<string>());
        }

        public static PlayerLevelUpEvent TryParse(string logLine)
        {
            string str = ParseInformations(logLine);
            if (!string.IsNullOrEmpty(str))
            {
                string messageMarker = PoeEvent.MessageMarker;
                if (PoeEvent.MessageMarker == null)
                {
                    string local1 = PoeEvent.MessageMarker;
                    messageMarker = "";
                }
                if (str.StartsWith(messageMarker) && (str.Contains(LevelUpMarker) && (str.Contains("(") && str.Contains(")"))))
                {
                    return new PlayerLevelUpEvent(logLine);
                }
            }
            return null;
        }

        public string Class { get; set; }

        public int Level { get; set; }
    }
}

