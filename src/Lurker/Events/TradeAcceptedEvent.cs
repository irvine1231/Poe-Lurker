namespace Lurker.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TradeAcceptedEvent : PoeEvent
    {
        private static readonly List<string> TradeAcceptedMarkerw;

        static TradeAcceptedEvent()
        {
            List<string> list1 = new List<string>();
            list1.Add("Trade accepted");
            list1.Add("Handel angenommen");
            list1.Add("\x00c9change accept\x00e9");
            list1.Add("Negocia\x00e7\x00e3o aceita");
            list1.Add("Сделка совершена");
            list1.Add("ยอมรับการแลกเปลี่ยนแล้ว");
            list1.Add("Intercambio aceptado");
            list1.Add("거래를 수락했습니다");
            list1.Add("完成交易");
            TradeAcceptedMarkerw = list1;
        }

        private TradeAcceptedEvent(string logLine) : base(logLine)
        {
        }

        public static TradeAcceptedEvent TryParse(string logLine)
        {
            string informations = ParseInformations(logLine);
            if (string.IsNullOrEmpty(informations))
            {
                return null;
            }
            bool flag = TradeAcceptedMarkerw.Any<string>(m => informations.StartsWith(PoeEvent.MessageMarker + m));
            return ((string.IsNullOrEmpty(informations) || !flag) ? null : new TradeAcceptedEvent(logLine));
        }
    }
}

