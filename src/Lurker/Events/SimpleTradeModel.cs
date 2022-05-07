namespace Lurker.Events
{
    using Lurker.Patreon.Models;
    using System;
    using System.Runtime.CompilerServices;

    public class SimpleTradeModel
    {
        public double GetChaosValue() =>
            (this.ExaltedRatio != 0.0) ? this.Price.CalculateValue(this.ExaltedRatio) : this.Price.CalculateValue();

        public Guid Id { get; set; }

        public string ItemName { get; set; }

        public string LeagueName { get; set; }

        public string WhisperMessage { get; set; }

        public string PlayerName { get; set; }

        public Lurker.Patreon.Models.Price Price { get; set; }

        public Lurker.Patreon.Models.Location Location { get; set; }

        public Lurker.Patreon.Models.ItemClass ItemClass { get; set; }

        public bool IsOutgoing { get; set; }

        public DateTime Date { get; set; }

        public double ExaltedRatio { get; set; }
    }
}

