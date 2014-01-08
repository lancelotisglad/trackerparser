using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AmCharts.Windows;
using AmCharts.Windows.Core;
using Awam.Tracker.FileProcessor;
using Awam.Tracker.Parser;
using TrackerModel;
using Awam.Tracker.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IFileParser _parser = new WinamaxParser();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Init();
        }


        private void LoadData()
        {
            if (comboBox1.SelectedValue == null)
                LoadData("saadliig");
            else
                LoadData(comboBox1.SelectedValue.ToString());
        }


        private void LoadData(string user)
        {
            Data = new ObservableCollection<Point>();

            using (trackDBEntities2 t = new trackDBEntities2())
            {
                var hands = t.Hands.Where(x => x.User == user
                    && x.Time >= (fromDatePicker.SelectedDate ?? SqlDateTime.MinValue.Value)
                    && x.Time <= (toDatePicker.SelectedDate ?? SqlDateTime.MaxValue.Value)
                    ).ToList();

                if (hands.Count == 0)
                    return;

                decimal? coll = 0;
                int index = 0;
                int count = hands.Count();
                int modulo = (int)Math.Round((float)count / 1000, 0);
                modulo = modulo == 0 ? 1 : modulo;
                int nbPoints = 0;

                decimal? totalBBWon = 0;
                foreach (var h in hands.OrderBy(x => x.Time))
                {
                    decimal? bbWon = h.Net / h.BB;
                    totalBBWon += bbWon;
                    index++;
                    if (comboBox2.SelectedValue == "BB")
                        coll += bbWon;
                    else
                        coll += h.Net;

                    if (index % modulo == 0)
                    {
                        nbPoints++;
                        Data.Add(new Point(h.Time ?? DateTime.MinValue, coll, index));
                    }
                }

                label1.Content = count + " mains, " + nbPoints + " points";

                int countRaisePF = hands.Where(
                    x => x.User == user && (x.ActionPreflop.Contains("raises"))).
                    Count();

                int countCallPF = hands.Where(
                    x => x.User == user && (x.ActionPreflop.Contains("calls"))).
                    Count();

                int countCheckRaise = hands.Where(
                    x => x.User == user && (x.ActionPreflop.Contains("checks,raises"))).
                    Count();

                int countPaidPF = countRaisePF + countCallPF;


                textBox1.Text = "BB won /100 : " + (totalBBWon / count) * 100;
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % Vol. put money preflop : " + countPaidPF * 100 / count + "%";
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % call pf : " + countCallPF * 100 / count + "%";
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % raise pf : " + countRaisePF * 100 / count + "%";
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % check raise pf : " + countCheckRaise * 100 / count + "%";

                int countBetsFlop = hands.Where(
                x => x.User == user && (x.ActionFlop.Contains("bets"))).
                Count();

                int countRaisesFlop = hands.Where(
                x => x.User == user && (x.ActionFlop.Contains("raises"))).
                Count();

                int countCallFlop = hands.Where(
                    x => x.User == user && (x.ActionFlop.Contains("calls"))).
                    Count();

                int countPaidFlop = countBetsFlop + countBetsFlop + countRaisesFlop;

                textBox1.Text += Environment.NewLine;
                textBox1.Text += "--- FLOP ---";
                textBox1.Text += Environment.NewLine;

                textBox1.Text += " % Vol. put money flop : " + countPaidFlop * 100 / count + "%";
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % call f : " + countCallFlop * 100 / count + "%";
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % raise f : " + countRaisesFlop * 100 / count + "%";
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % bets f : " + countBetsFlop * 100 / count + "%";

                LineChart1.InvalidateMeasure();
                LineChart1.SeriesSource = Data;
            }
        }

        private void LoadUsers()
        {
            using (trackDBEntities2 t = new trackDBEntities2())
            {

                var users =
                  from p in t.Hands
                  group p by p.User into g
                  //where g.Count() >= 1000
                  select new
                  {
                      g.Key
                  };

                _users = new ObservableCollection<string>();
                foreach (var player in users)
                {
                    //if (t.Hands.Where(x => x.User == player).Count() > 1000)
                    _users.Add(player.Key);
                }
            }
        }

        private void Init()
        {
            label4.Content = _path;
            LoadUsers();
            LoadData("saadliig");
            comboBox2.Items.Add("Money");
            comboBox2.Items.Add("BB");

            comboBox2.SelectedValue = "Money";
        }

        ObservableCollection<string> _tables = new ObservableCollection<string>();
        public ObservableCollection<string> Tables
        {
            get { return _tables; }
        }

        ObservableCollection<string> _users = new ObservableCollection<string>();
        public ObservableCollection<string> Users
        {
            get { return _users; }
        }

        ObservableCollection<Point> _data = new ObservableCollection<Point>();
        public ObservableCollection<Point> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public class Point : DependencyObject
        {

            public Point(DateTime date, decimal? value, int index)
            {
                Date = date;
                Value = value;
                Index = index;
            }
            public static readonly DependencyProperty _date = DependencyProperty.Register("Date", typeof(DateTime), typeof(Point));
            public DateTime Date
            {
                get { return (DateTime)GetValue(_date); }
                set { SetValue(_date, value); }
            }

            public static readonly DependencyProperty _index = DependencyProperty.Register("Index", typeof(int), typeof(Point));
            public int Index
            {
                get { return (int)GetValue(_index); }
                set { SetValue(_index, value); }
            }

            public static readonly DependencyProperty _value = DependencyProperty.Register("Value", typeof(decimal), typeof(Point));
            public decimal? Value
            {
                get { return (decimal)GetValue(_value); }
                set { SetValue(_value, value); }
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            LoadData();

        }

        private void fromDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void toDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private string _path = @"C:\_DATA\_awam\__TrackerParser\historyLight";

        private void btnPickFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _path = dialog.SelectedPath;
                label4.Content = _path;
            }
        }

        System.ComponentModel.BackgroundWorker mWorker;

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            mWorker = new System.ComponentModel.BackgroundWorker();
            mWorker.DoWork += worker_DoWork;
            mWorker.ProgressChanged += mWorker_ProgressChanged;

            mWorker.RunWorkerCompleted += mWorker_RunWorkerCompleted;
            mWorker.WorkerReportsProgress = true;
            mWorker.WorkerSupportsCancellation = true;

            mWorker.RunWorkerAsync();
        }

        void mWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

        }

        void mWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            updateLabel("Import finished");
            Init();
        }

        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Refresh(true);
        }

        private void Refresh(bool clearAll)
        {
            FileProcessor fileProcessor = new FileProcessor(_parser, new DataRepository(), _path);
            fileProcessor.NewFileProcessed += fileProcessor_NewFileProcessed;
            fileProcessor.ProcessImportOnModifiedFilesSinceLastImport(clearAll);
        }

        private void Refresh(FileInfo file)
        {
            FileProcessor fileProcessor = new FileProcessor(_parser, new DataRepository(), _path);
            fileProcessor.NewFileProcessed += fileProcessor_NewFileProcessed;
            fileProcessor.ProcessFile(file);
        }

        void fileProcessor_NewFileProcessed(NewFileProcessedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                       DispatcherPriority.SystemIdle,
                       new MyDelegate(updateLabel),
                       e.NewFile);
        }

        public delegate void MyDelegate(string text);
        public delegate void MyDelegateR();


        private void updateLabel(string text)
        {
            label6.Content = text;
        }

        private FileSystemWatcher _watcher;

        private void checkBox2_Checked(object sender, RoutedEventArgs ev)
        {
            if (checkBox2.IsChecked == true)
            {
                _watcher = new FileSystemWatcher(_path);
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
                _watcher.Created += _watcher_Created;
                _watcher.Changed += _watcher_Changed;
                _watcher.EnableRaisingEvents = true;
            }
            else
            {
                _watcher.EnableRaisingEvents = false;
            }
        }


        private void UpdateListOfTables()
        {
            var lst = Process.GetProcessesByName("Winamax Poker.exe");
            if (lst.Length == 0)
                return;
            Process p = lst[0];
          

            List<IntPtr> hWnds = GetChildWindows(p.Handle);
            foreach (var h in hWnds)
            {
                StringBuilder sb = new StringBuilder();           
                GetWindowText(h, sb, 0);

                _tables.Add(sb.ToString());

                MessageBox.Show(sb.ToString());
            }
        }



        ConcurrentDictionary<string, DateTime> syncTable = new ConcurrentDictionary<string, DateTime>();

        void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!syncTable.ContainsKey(e.FullPath) || syncTable[e.FullPath] < DateTime.Now.AddSeconds(-10))
            {
                syncTable[e.FullPath] = DateTime.Now;
                UpdateAsynchronously(new FileInfo(e.FullPath));
            }
        }

        void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!syncTable.ContainsKey(e.FullPath) || syncTable[e.FullPath] < DateTime.Now.AddSeconds(-10))
            {
                syncTable[e.FullPath] = DateTime.Now;
                UpdateAsynchronously(new FileInfo(e.FullPath));
            }
        }

        void UpdateAsynchronously(FileInfo f)
        {
            timerWorker = new System.ComponentModel.BackgroundWorker();
            timerWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(timerWorker_DoWork);

            timerWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(timerWorker_RunWorkerCompleted);
            timerWorker.WorkerReportsProgress = true;
            timerWorker.WorkerSupportsCancellation = true;

            timerWorker.RunWorkerAsync(f);
        }

        void timerWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                       DispatcherPriority.SystemIdle,
                       new MyDelegate(updateLabel),
                       "ok");

        }

        void timerWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Refresh((FileInfo)e.Argument);
        }


        System.ComponentModel.BackgroundWorker timerWorker;

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Init();
            UpdateListOfTables();            
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow(); //Retrieves a handle to the foreground window

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowText(IntPtr hWnd, StringBuilder text, int count); //Copies the text of the specified window's title bar (if it has one) into a buffer

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)] //This function changes the size, position, and z-order of a child, pop-up, or top-level window.
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImportAttribute("User32.dll")]
        private static extern IntPtr FindWindow(String ClassName, String WindowName);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); //Places the window above all non-topmost windows
        static readonly IntPtr HWND_TOP = new IntPtr(0); // Places the window at the top of the Z order.

        const UInt32 SWP_NOSIZE = 0x0001; //Retains current size
        const UInt32 SWP_NOMOVE = 0x0002; // Retains the current position
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;


        [DllImport("kernel32.dll")]
        static extern uint GetModuleFileName(IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);


        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowsProc childProc = new EnumWindowsProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            StringBuilder sb = new StringBuilder(255);
            GetModuleFileName(handle, sb, sb.Capacity);
                MessageBox.Show(sb.ToString());

            list.Add(handle);
            // You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }


    }
}
