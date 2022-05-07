namespace Lurker.Events
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class PlayerEvent : PoeEvent
    {
        protected static readonly string EndOfGuildNameMarker = ">";

        protected PlayerEvent(string logLine) : base(logLine)
        {
        }

        protected void SetPlayerName(int index)
        {
            this.PlayerName = base.Message.Substring(0, index - 1);
        }

        public string PlayerName { get; protected set; }

        public string GuildName { get; protected set; }
    }
}

