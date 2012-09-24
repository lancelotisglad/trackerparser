﻿using System;
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
            FileParser_Accessor parser = new FileParser_Accessor("Data\\BaseFile.txt", new DateTime());
            var hands = parser.GetHandsFromFile();
            Assert.AreEqual(71, hands.Count);
        }

        [TestMethod]
        public void FileParser_ButtonPositionTest()
        {
            FileParser_Accessor parser = new FileParser_Accessor("Data\\BaseFile.txt", new DateTime());
            var hands = parser.GetHandsFromFile();

            Assert.AreEqual(2, hands.First().ButtonPosition);
            Assert.AreEqual(8, hands.Last().ButtonPosition);
        }

        [TestMethod]
        public void FileParser_PlayerPositionTest()
        {
            FileParser_Accessor parser = new FileParser_Accessor("Data\\BaseFile.txt", new DateTime());
            var hands = parser.GetHandsFromFile();

            Assert.AreEqual(8, hands.First()["saadliig"].SeatNumber);
            Assert.AreEqual(2, hands.Last()["saadliig"].SeatNumber);
        }

    }
}