namespace Lurker.Events
{
    using System;

    public class OutgoingTradeEvent : TradeEvent
    {
        public OutgoingTradeEvent(string logLine) : base(logLine)
        {
        }

        public static OutgoingTradeEvent TryParse(string logLine)
        {
            if (WhisperEvent.IsOutgoing(logLine))
            {
                string str = ParseMessage(logLine);
                foreach (string str2 in TradeEvent.GreetingMarkers)
                {
                    if (str.StartsWith(str2))
                    {
                        return new OutgoingTradeEvent(logLine);
                    }
                }
            }
            return null;
        }
    }
}

