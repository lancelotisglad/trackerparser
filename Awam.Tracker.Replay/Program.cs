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
            string id1 = "2863240-273-1335391174";
            string id2 = "2863240-274-1335391222";
            string idHand = id2;

            using (trackDBEntities2 t = new trackDBEntities2())
            {
                var dbHand = t.Hands.Where(h => h.Id == idHand);
                Console.WriteLine("hand : " + dbHand.First().Id);

                var sortedHand = dbHand.ToList().OrderBy(x => x.Position);

                Console.WriteLine("Button on seat " + dbHand.First().PositionButton);
                foreach (var handse in sortedHand)
                {
                    Console.WriteLine("Seat "+ handse.Position +":" +handse.User + " (" + handse.Stack + ")");
                }

                var hands = dbHand.ToArray();

                bool continu = ReplayStreet(hands, Enumeration.Street.Blind);
                if (continu)
                    continu = ReplayStreet(hands, Enumeration.Street.Preflop);
                if (continu)
                    continu = ReplayStreet(hands, Enumeration.Street.Flop);
                if (continu)
                    continu = ReplayStreet(hands, Enumeration.Street.Turn);
                if (continu)
                    ReplayStreet(hands, Enumeration.Street.River);

                Console.WriteLine("-----------");
                Console.WriteLine("Summary");
                Console.WriteLine("-----------");

                foreach (var handse in sortedHand)
                {
                    string wonloosefold =
                        handse.Net > 0 ? " won " : (handse.Net == 0 ? " folds preflop " : " loose ");
                    Console.WriteLine(handse.User + wonloosefold + (handse.Net != 0 ? handse.Net.ToString() : ""));
                }

                Console.WriteLine("Rake : " + sortedHand.Sum(x => x.Net));

                Console.Read();
            }
        }

        private static bool ReplayStreet(Hands[] hands, Enumeration.Street street)
        {
         
            Console.WriteLine("-----------");
            Console.WriteLine("Action "+street);
            Console.WriteLine("-----------");

            int position = GetFirstPlayer(hands, street == Enumeration.Street.Preflop).Position;
            int firstPosition = position;
            
            int run = 0;
            string lastactionFromrun = string.Empty;
            while (true)
            {
                string action = GetActionPositionRun(hands, position, run, street);
                if (action != string.Empty)
                    Console.WriteLine(action);

                if (action.Contains("collected"))
                {
                    return false;
                }
                position = getNext(hands, position).Position;
                lastactionFromrun += action;
                if (position == firstPosition)
                {
                    if (lastactionFromrun == string.Empty)
                        break;

                    lastactionFromrun = string.Empty;
                    run++;
                }
            }
            return true;
        }

        static string GetActionPositionRun(Hands[] hands, int position, int run, Enumeration.Street street)
        {
            var user = hands.Single(x => x.Position == position).User;
            string action = string.Empty;
            switch (street)
            {
                case Enumeration.Street.Blind:
                    action = hands.Single(x => x.Position == position).ActionBlind;
                    break;
                case Enumeration.Street.Preflop:
                    action = hands.Single(x => x.Position == position).ActionPreflop;
                    break;
                case Enumeration.Street.Flop:
                    action = hands.Single(x => x.Position == position).ActionFlop;
                    break;
                case Enumeration.Street.Turn:
                    action = hands.Single(x => x.Position == position).ActionTurn;
                    break;
                case Enumeration.Street.River:
                    action = hands.Single(x => x.Position == position).ActionRiver;
                    break;
                default:
                    throw new ArgumentException("this street does not exist", street.ToString());
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
