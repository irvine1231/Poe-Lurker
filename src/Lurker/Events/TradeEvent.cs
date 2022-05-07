namespace Lurker.Events
{
    using Lurker.Patreon.Extensions;
    using Lurker.Patreon.Models;
    using Lurker.Patreon.Parsers;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    public class TradeEvent : Patreon.Events.TradeEvent
    {
        protected static readonly Lurker.Patreon.Parsers.ItemClassParser ItemClassParser = new Lurker.Patreon.Parsers.ItemClassParser();
        protected static readonly string[] GreetingMarkers = new string[] { "Hi, I would like to buy your", "Hi, I'd like to buy your", "wtb" };
        private static readonly string[] PriceMarkers = new string[] { "listed for", "for my", " for" };
        private static readonly string LocationMarker = "(";
        private static readonly string LocationMarkerEnd = ")";
        private static readonly string PositionMarker = "position: ";
        private static readonly string LeagueMarker = " in ";
        private static readonly Lurker.Patreon.Parsers.CurrencyTypeParser CurrencyTypeParser = new Lurker.Patreon.Parsers.CurrencyTypeParser();

        protected TradeEvent(string logLine) : base(logLine)
        {
            string str = PriceMarkers.FirstOrDefault<string>(m => base.Message.Contains(m));
            int num = (str == null) ? -1 : base.Message.IndexOf(str);
            int index = base.Message.IndexOf(LeagueMarker);
            if (index != -1)
            {
                this.LeagueName = base.Message.GetLineAfter(LeagueMarker).Split(" (").First<string>();
                char[] trimChars = new char[] { '.' };
                this.LeagueName = this.LeagueName.Trim(trimChars);
            }
            int length = (num == -1) ? (index + 1) : num;
            string str2 = base.Message.Substring(0, length);
            string str3 = GreetingMarkers.FirstOrDefault<string>(m => base.Message.Contains(m));
            this.ItemName = base.Message.Substring(str3.Length + 1, (str2.Length - str3.Length) - 2);
            this.ItemClass = ItemClassParser.Parse(this.ItemName);
            string locationValue = base.Message.Substring(length);
            this.Location = this.ParseLocation(locationValue);
            if (num == -1)
            {
                this.Price = new Lurker.Patreon.Models.Price();
            }
            else
            {
                string text1 = base.Message.Substring((num + str.Length) + 1);
                int num4 = text1.IndexOf(LeagueMarker);
                string priceValue = text1;
                if (num4 != -1)
                {
                    priceValue = priceValue.Substring(0, num4);
                }
                this.Price = this.ParsePrice(priceValue);
            }
        }

        public string BuildSearchItemName()
        {
            string itemName = this.ItemName;
            int index = this.ItemName.IndexOf(" (");
            if (index != -1)
            {
                itemName = itemName.Substring(0, index);
                int length = itemName.IndexOf(" Map");
                if (length != -1)
                {
                    itemName = itemName.Substring(0, length);
                }
            }
            string str2 = "Superior";
            if ((this.ItemClass == Lurker.Patreon.Models.ItemClass.Map) && itemName.StartsWith(str2))
            {
                itemName = itemName.Substring(str2.Length + 1);
            }
            if (this.ItemName.IndexOf("level ") == -1)
            {
                return Regex.Replace(itemName, @"[\d]", string.Empty).Trim();
            }
            char[] separator = new char[] { ' ' };
            string[] source = itemName.Split(separator);
            string str3 = source[2];
            string str4 = string.Join(" ", source.Skip<string>(3));
            return ((str3 != "0%") ? (str4 + " " + str3) : str4);
        }

        public SimpleTradeModel CreateSimpleModel(double exaltedRatio)
        {
            SimpleTradeModel model1 = new SimpleTradeModel();
            model1.Id = base.Id;
            model1.ItemName = this.ItemName;
            model1.LeagueName = this.LeagueName;
            model1.Location = this.Location;
            model1.PlayerName = base.PlayerName;
            model1.Price = this.Price;
            model1.WhisperMessage = base.WhisperMessage;
            model1.ItemClass = this.ItemClass;
            model1.IsOutgoing = this is OutgoingTradeEvent;
            model1.Date = base.Date;
            model1.ExaltedRatio = exaltedRatio;
            return model1;
        }

        public override bool Equals(object obj)
        {
            TradeEvent event2 = obj as TradeEvent;
            return ((event2 != null) ? ((event2.PlayerName == base.PlayerName) ? ((event2.ItemName == this.ItemName) ? (event2.Price.Equals(this.Price) ? (event2.Location.ToString() == this.Location.ToString()) : false) : false) : false) : false);
        }

        public static bool IsTradeMessage(string message) =>
            message.StartsWith("@") ? (!string.IsNullOrEmpty(GreetingMarkers.FirstOrDefault<string>(m => message.Contains(m))) ? !string.IsNullOrEmpty(PriceMarkers.FirstOrDefault<string>(m => message.Contains(m))) : false) : false;

        public Lurker.Patreon.Models.Location ParseLocation(string locationValue)
        {
            Lurker.Patreon.Models.Location location;
            try
            {
                int index = locationValue.IndexOf(LocationMarkerEnd);
                if ((locationValue.IndexOf(LocationMarker) == -1) || (index == -1))
                {
                    location = new Lurker.Patreon.Models.Location();
                }
                else
                {
                    string str = string.Empty;
                    string lineBefore = locationValue.GetLineBefore("\";");
                    if (!string.IsNullOrEmpty(lineBefore))
                    {
                        str = lineBefore.Substring(lineBefore.IndexOf("\"") + 1);
                    }
                    string lineAfter = locationValue.GetLineAfter(";");
                    if (lineAfter.IndexOf(PositionMarker) != -1)
                    {
                        lineAfter = lineAfter.GetLineAfter("position: ");
                    }
                    string[] textArray1 = lineAfter.Split(", ");
                    string str4 = textArray1[0].GetLineAfter("left ");
                    string str5 = textArray1[1].GetLineAfter("top ");
                    if (string.IsNullOrEmpty(str4) || string.IsNullOrEmpty(str5))
                    {
                        Lurker.Patreon.Models.Location location1 = new Lurker.Patreon.Models.Location();
                        location1.StashTabName = str;
                        location = location1;
                    }
                    else
                    {
                        str5 = str5.Substring(0, str5.IndexOf(")"));
                        Lurker.Patreon.Models.Location location2 = new Lurker.Patreon.Models.Location();
                        location2.StashTabName = str;
                        location2.Left = Convert.ToInt32(str4);
                        location2.Top = Convert.ToInt32(str5);
                        location = location2;
                    }
                }
            }
            catch
            {
                location = new Lurker.Patreon.Models.Location();
            }
            return location;
        }

        public Lurker.Patreon.Models.Price ParsePrice(string priceValue)
        {
            Lurker.Patreon.Models.Price price;
            try
            {
                double num;
                char[] separator = new char[] { ' ' };
                string[] source = priceValue.Split(separator);
                if (!double.TryParse(source[0], out num))
                {
                    price = new Lurker.Patreon.Models.Price();
                }
                else
                {
                    string str = string.Join(" ", source.Skip<string>(1));
                    Lurker.Patreon.Models.Price price1 = new Lurker.Patreon.Models.Price();
                    price1.NumberOfCurrencies = double.Parse(source[0].Replace(',', '.'), CultureInfo.InvariantCulture);
                    price1.CurrencyType = CurrencyTypeParser.Parse(str);
                    price = price1;
                }
            }
            catch
            {
                price = new Lurker.Patreon.Models.Price();
            }
            return price;
        }

        public static TradeEvent TryParse(string logLine)
        {
            if (WhisperEvent.IsIncoming(logLine))
            {
                string str = ParseMessage(logLine);
                foreach (string str2 in GreetingMarkers)
                {
                    if (str.StartsWith(str2))
                    {
                        return new TradeEvent(logLine);
                    }
                }
            }
            return null;
        }

        public string ItemName { get; set; }

        public string LeagueName { get; set; }

        public Lurker.Patreon.Models.ItemClass ItemClass { get; set; }

        public Lurker.Patreon.Models.Price Price { get; private set; }

        public Lurker.Patreon.Models.Location Location { get; private set; }
    }
}

