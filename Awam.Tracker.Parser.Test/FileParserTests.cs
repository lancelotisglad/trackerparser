using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Awam.Tracker.Parser.Test
{
    [TestClass]
    [DeploymentItem(@"Data", "Data")]
    public class FileParserTests
    {
        [TestMethod]
        public void FileParserTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());
            Assert.AreEqual(71, hands.Count);
        }

        [TestMethod]
        public void FileParser_ButtonPositionTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());

            Assert.AreEqual(2, hands.First().ButtonPosition);
            Assert.AreEqual(8, hands.Last().ButtonPosition);
        }

        [TestMethod]
        public void FileParser_PlayerPositionTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());

            Assert.AreEqual(8, hands.First()["saadliig"].SeatNumber);
            Assert.AreEqual(2, hands.Last()["saadliig"].SeatNumber);
        }

        [TestMethod]
        public void FileParser_PlayerStackTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());

            Assert.AreEqual(6.17f, hands.First()["azzarro123"].Stack);
            Assert.AreEqual(10.65f, hands.Last()["saadliig"].Stack);
        }

        [TestMethod]
        public void FileParser_PlayerActionPreflopTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());

            Assert.AreEqual("raises (0.10)",
                hands.First()
                ["azzarro123"].ActionPreflop);

            Assert.AreEqual("raises (0.10),raises (0.64)",
                hands.
                Single(h => h.HandId == "2863240-284-1335391929")
                ["azzarro123"].ActionPreflop);
        }

        [TestMethod]
        public void FileParser_PlayerActionBlindTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());

            Assert.AreEqual("posts small blind 0.02€",
                hands.First()
                ["Nzashryl"].ActionBlind);

            Assert.AreEqual("posts big blind 0.05€",
                hands.First()
                ["vince1351"].ActionBlind);
        }

        [TestMethod]
        public void FileParser_PlayerNetTest()
        {
            WinamaxParser parser =
                new WinamaxParser();
            var hands = parser.Parse("Data\\BaseFile.txt", new DateTime());

            //Assert.AreEqual(1.14,hands.First()["doulali"].MyMoneyCollected);
        }
    }
}
