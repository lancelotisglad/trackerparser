using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awam.Tracker.Parser;

namespace TrackerTest
{
    [TestClass]
    public class UnitTest1
    {
        static string _winamaxPath = @"C:\Data\awam\TrackerParser\histo\0105";

        [TestMethod]
        public void TestMethod1()
        {
            //DirectoryInfo di = new DirectoryInfo(_winamaxPath);
            //FileInfo[] files = di.GetFiles();


            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            ////foreach (var fileInfo in files.Where(x => x.Name == "20110716_Franconville_real_holdem_no-limit.txt"))
            //foreach (var fileInfo in files)
            //{
            //    Console.WriteLine(fileInfo.Name);

            //    FileParser fileParser = new FileParser(fileInfo.FullName);
            //    try
            //    {
            //        fileParser.Parse();
            //    }
            //    catch (Exception e)
            //    {

            //        Console.WriteLine(e + " fichier pas cashgame");
            //    }

            //}

            //sw.Stop();
            //Console.WriteLine("Terminé : " + files.Count() + " en " + sw.ElapsedMilliseconds / 1000);
            //Console.WriteLine("Terminé");
            

        }
    }
}
