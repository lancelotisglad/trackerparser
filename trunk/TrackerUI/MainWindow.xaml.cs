using System;
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
using TrackerModel;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


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
                    && x.Time >= (fromDatePicker.SelectedDate ?? SqlDateTime.MinValue.Value )
                    && x.Time <= (toDatePicker.SelectedDate ?? SqlDateTime.MaxValue.Value)  
                    ).ToList();

                if (hands.Count == 0)
                    return;

                double? coll = 0;
                int index = 0;
                int count = hands.Count();
                int modulo = (int) Math.Round((float) count/1000, 0);
                modulo = modulo == 0 ? 1 : modulo;
                int nbPoints = 0;

                double? totalBBWon = 0;
                foreach (var h in hands.OrderBy(x => x.Time))
                {
                    double? bbWon = h.Net/h.BB;
                    totalBBWon += bbWon;
                    index++;
                    if (comboBox2.SelectedValue == "BB")
                        coll += bbWon;
                    else
                        coll += h.Net;

                   if (index%modulo == 0)
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
                    x => x.User == user && ( x.ActionPreflop.Contains("calls"))).
                    Count();

                int countCheckRaise = hands.Where(
                    x => x.User == user && (x.ActionPreflop.Contains("checks,raises"))).
                    Count();
                
                int countPaidPF = countRaisePF + countCallPF;


                textBox1.Text = "BB won /100 : " + (totalBBWon / count) * 100;
                textBox1.Text += Environment.NewLine;
                textBox1.Text += " % Vol. put money preflop : " + countPaidPF*100/count + "%";
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
                           select new {
                                    g.Key
                                   };
                    
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
           
            public Point(DateTime date, double? value, int index)
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

            public static readonly DependencyProperty _value = DependencyProperty.Register("Value", typeof(double), typeof(Point));
            public double? Value
            {
                get { return (double)GetValue(_value); }
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

        private string _path = @"C:\Data\awam\TrackerParser\histo\FewFiles";

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
            mWorker. ProgressChanged += mWorker_ProgressChanged;

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
            Refresh(checkBox1.IsChecked == true);
        }

        private  void Refresh(bool clearAll)
        {
            FileProcessor fileProcessor = new FileProcessor(_path);
            fileProcessor.NewFileProcessed += fileProcessor_NewFileProcessed;
            fileProcessor.ProcessImportOnModifiedFilesSinceLastImport(_path, clearAll);
        }

        private void Refresh(FileInfo file)
        {
            FileProcessor fileProcessor = new FileProcessor(_path);
            fileProcessor.NewFileProcessed += fileProcessor_NewFileProcessed;
            fileProcessor.ProcessFile(file);
        }

        void fileProcessor_NewFileProcessed( NewFileProcessedEventArgs e)
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
                 _watcher.NotifyFilter = NotifyFilters.FileName;
                 _watcher.Created += _watcher_Created;
                 _watcher.Changed += _watcher_Changed;
                 _watcher.EnableRaisingEvents = true;
             }
             else
             {
                 _watcher.EnableRaisingEvents = false;
             }
         }

         void _watcher_Changed(object sender, FileSystemEventArgs e)
         {
             UpdateAsynchronously(new FileInfo(e.FullPath));
         }

         void _watcher_Created(object sender, FileSystemEventArgs e)
         {
             UpdateAsynchronously(new FileInfo(e.FullPath));
         }

         void UpdateAsynchronously(FileInfo f)
         {
            timerWorker = new System.ComponentModel.BackgroundWorker();
            timerWorker.DoWork +=new System.ComponentModel.DoWorkEventHandler(timerWorker_DoWork);

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
         }


    }
}
