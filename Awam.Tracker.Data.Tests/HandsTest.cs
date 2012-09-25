using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Awam.Tracker.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerModel;

namespace Awam.Tracker.Data.Tests
{
    [TestClass]
    public class HandsTest
    {
        [TestMethod]
        [DeploymentItem("trackDB.sdf", @"")]
        public void SaveHandsSqlCommandTest()
        {
            var hands = new List<Hand>();

            Hand hand =
                new Hand
                    {
                        BigBlind = 1,
                        ButtonPosition = 2,
                        FlopCard1 = "As",
                        FlopCard2 = "Js",
                        FlopCard3 = "Th",
                        HandId = "4545454",
                        RiverCard = "2s",
                        Time = new DateTime(2000, 1, 1),
                        TurnCard = "7s",
                        Players = new List<HandPlayer>()
                    };

            HandPlayer player =
                new HandPlayer
                    {
                        ActionFlop = "action Flop",
                        ActionPreflop = "action Preflop",
                        ActionRiver = "action River",
                        ActionTurn = "action Turn",
                        Card1Str = "2a",
                        Card2Str = "3c",
                        MyMoneyAddedInPot = 2,
                        MyMoneyCollected = 1,
                        PaidFlop = 0.5f,
                        PaidPreflop = 0.6f,
                        PaidRiver = 0.7f,
                        PaidTurn = 0.8f,
                        SeatNumber = 4,
                        Player = "player1",
                        Stack = 6.52d
                    };

            hand.Players.Add(player);
            hands.Add(hand);

            Hands.SaveHandsSqlCommand(hands);

            using (trackDBEntities2 t = new trackDBEntities2())
            {
                var dbHands = t.Hands;
                Assert.AreEqual(t.Hands.Count(), 1);
                Assert.AreEqual(t.Hands.First().BB, 1);
                Assert.AreEqual(t.Hands.First().PositionButton, 2);
                Assert.AreEqual(t.Hands.First().Id, "4545454");
                Assert.AreEqual(t.Hands.First().Time, new DateTime(2000, 1, 1));
                Assert.AreEqual(t.Hands.First().Position, 4);
                Assert.AreEqual(t.Hands.First().Stack, 6.52d);
            }

        }

        public static bool nearlyEqual(float a, float b)
        {
            float epsilon = 0.000001f;
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a*b == 0)
            {
                return diff < (epsilon*epsilon);
            }
            else
            {
                return diff/(absA + absB) < epsilon;
            }
        }


    }
}


