using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Parser;
using System.IO;

namespace myTracker
{
    public partial class Form1 : Form
    {
        string _winamaxPath = @"C:\Users\maud\Documents\Winamax Poker\accounts\saadliig\history";

        public Form1()
        {
            InitializeComponent();


            DirectoryInfo di = new DirectoryInfo(_winamaxPath);
            FileInfo[] files = di.GetFiles();

            FileInfo file = files.First();
            FileParser fileParser = new FileParser(file.FullName);

            fileParser.Parse();

        }
    }
}
