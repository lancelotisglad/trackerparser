using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Awam.Tracker.Model;

using TrackerModel;

namespace Awam.Tracker.Replay
{
    class Program
    {

        static void Main(string[] args)
        {
            string idHand = "2863240-273-1335391174";

            using (trackDBEntities2 t = new trackDBEntities2())
            {
                var dbHand = t.Hands.Where(h => h.Id == idHand);
                Console.WriteLine("hand : " + dbHand.First().Id);

                var sortedHand = dbHand.ToList().OrderBy(x => x.Position);

                foreach (var handse in sortedHand)
                {
                    Console.WriteLine(handse.User + " (" + handse.Stack + ")");
                }

                var hands = dbHand.ToArray();

                ReplayStreet(hands, "preflop");
                ReplayStreet(hands, "flop");
                ReplayStreet(hands, "turn");
                ReplayStreet(hands, "river");


                Console.Read();
            }
        }

        private static void ReplayStreet(Hands[] hands, string street)
        {
         
            Console.WriteLine("-----------");
            Console.WriteLine("Action "+street);
            Console.WriteLine("-----------");

            int position = GetFirstPlayer(hands, street == "preflop").Position;
            int firstPosition = position;
            
            int run = 0;
            string lastactionFromrun = string.Empty;
            while (true)
            {
                string action = GetActionPositionRun(hands, position, run, street);
                if (action != string.Empty)
                    Console.WriteLine(action);
                position = getNext(hands, position).Position;
                lastactionFromrun += action;
                if (position == firstPosition)
                {
                    if (lastactionFromrun == string.Empty)
                    {
                        break;
                    }

                    lastactionFromrun = string.Empty;
                    run++;
                }
            }
        }

        static string GetActionPositionRun(Hands[] hands, int position, int run,string street)
        {
            var user = hands.Single(x => x.Position == position).User;
            string action = string.Empty;
            switch (street)
            {
                case "preflop" :
                    action = hands.Single(x => x.Position == position).ActionPreflop;
                    break;
                case "flop":
                    action = hands.Single(x => x.Position == position).ActionFlop;
                    break;
                case "turn":
                    action = hands.Single(x => x.Position == position).ActionTurn;
                    break;
                case "river":
                    action = hands.Single(x => x.Position == position).ActionRiver;
                    break;
                default:
                    throw new ArgumentException("this street does not exist", street);
            }

            if (action == string.Empty)
                return string.Empty;

            if (action.Split(',').Count() > run)
                return user + " " + action.Split(',')[run];

            return string.Empty;
        }

        static Hands getNext(Hands[] hands , int last)
        {
            return last
                   == hands.Max(x => x.Position)
                       ? hands.First()
                       : hands.Where(x => x.Position > last).First();
        }

        static Hands GetFirstPlayer(Hands[] hands, bool preflop)
        {
            int positionButton = hands.First().PositionButton;
            if (preflop)
            {
                var sb = getNext(hands, positionButton);
                var bb = getNext(hands, sb.Position);

                return getNext(hands, bb.Position);
            }

            return getNext(hands, positionButton);
        }

    }
}
