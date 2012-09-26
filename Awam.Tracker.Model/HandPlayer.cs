using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awam.Tracker.Model
{
    public class HandPlayer
    {
        public string Player { get; set; }

        public string Card1Str { get; set; }
        public string Card2Str { get; set; }

        public string ActionBlind { get; set; }
        public string ActionPreflop { get; set; }
        public string ActionFlop { get; set; }
        public string ActionTurn { get; set; }
        public string ActionRiver { get; set; }

        public decimal MyMoneyAddedInPot { get; set; }
        public decimal MyMoneyCollected { get; set; }

        public decimal PaidPreflop { get; set; }
        public decimal PaidFlop { get; set; }
        public decimal PaidTurn { get; set; }
        public decimal PaidRiver { get; set; }

        public double SeatNumber { get; set; }
        public decimal Stack { get; set; }
    }
}
