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

        public string ActionPreflop { get; set; }
        public string ActionFlop { get; set; }
        public string ActionTurn { get; set; }
        public string ActionRiver { get; set; }

        public float MyMoneyAddedInPot { get; set; }
        public float MyMoneyCollected { get; set; }

        public float PaidPreflop { get; set; }
        public float PaidFlop { get; set; }
        public float PaidTurn { get; set; }
        public float PaidRiver { get; set; }

        public double SeatNumber { get; set; }
    }
}
