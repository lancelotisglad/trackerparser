using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awam.Tracker.Model
{
    public class Hand
    {
        public string HandId { get; set; }

        public string FlopCard1 { get; set; }
        public string FlopCard2 { get; set; }
        public string FlopCard3 { get; set; }


        public string TurnCard { get; set; }
        public string RiverCard { get; set; }

        public float BigBlind { get; set; }

        public DateTime Time { get; set; }

        public List<HandPlayer> Players { get; set; }

        public HandPlayer this[string player]
        {
            get { return Players.Single(p => p.Player == player); }
        }
    }
}
