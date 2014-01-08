using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Awam.Tracker.Data;
using Awam.Tracker.Model;
using Awam.Tracker.Parser;

namespace Awam.Tracker.FileProcessor
{
    public class FileProcessor
    {
        public delegate void NewFileProcessedHandler( NewFileProcessedEventArgs e);
        public event NewFileProcessedHandler NewFileProcessed;

        private readonly string _path;
        private IFileParser _parser;
        private IDataRepository _dataRepository;
        private FileSystemWatcher _fsw;

        public FileProcessor(IFileParser parser, IDataRepository dataRepository, string directory)
        {
            _path = directory;
            _parser = parser;
            _dataRepository = dataRepository;
        }

        public void ProcessImportOnModifiedFilesSinceLastImport( bool clearData = false)
        {
            if (clearData)
            {
                _dataRepository.GetManagementRepository().ClearAll();
            }
            DateTime lastDate = _dataRepository.GetImportsRepository().GetLastImportDate(_path);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IList<Hand> hands = ProcessImport( lastDate);
            //_dataRepository.GetHandsRepository().SaveHandsSqlCommand(hands);
            sw.Stop();
            Console.WriteLine("Terminé : " + hands.Count() + " en " + sw.ElapsedMilliseconds);
        }

        private IList<Hand> ProcessImport( DateTime from)
        {
            DateTime startDate = DateTime.UtcNow;
            string status = "success";
            List<Hand> hands = new List<Hand>();
            try
            {
                var files = GetFilesModifiedOrCreatedSinceADate( from);
                foreach (var fileInfo in files)
                {
                    hands.AddRange(ProcessFile(fileInfo));
                }
            }
            catch (Exception e)
            {
                status = "fail : " + e.Message + " " + e.StackTrace;
            }
            finally
            {
                DateTime endDate = DateTime.UtcNow;
                _dataRepository.GetImportsRepository().LogImport(startDate, endDate, status, _path);
            }

            return hands;
        }

        public IList<Hand> ProcessFile(FileInfo fileInfo)
        {
            DateTime fileStartDate = DateTime.UtcNow;
            string fileStatus = "succes";
            var lastImportDate = _dataRepository.GetImportsRepository().GetLastImportFileDate(fileInfo.Name);
            IList<Hand> hands = new List<Hand>();
            try
            {
                hands = _parser.Parse(fileInfo.FullName, lastImportDate);
                _dataRepository.GetHandsRepository().SaveHandsSqlCommand(hands);
                OnNewFileProcessed(new NewFileProcessedEventArgs(fileInfo.Name, hands.Last()));                
            }
            catch (Exception e)
            {
                fileStatus = "fail : " + e.Message + " " + e.StackTrace;                
            }
            finally
            {
                DateTime endFileDate = DateTime.UtcNow;
                if (hands.Count > 0)
                {
                    var lastHand = hands.Last();
                    _dataRepository.GetImportsRepository().LogFileImport(fileInfo, fileStartDate, endFileDate, fileStatus, lastHand);
                }
            }
            return hands;
        }

        private IEnumerable<FileInfo> GetFilesModifiedOrCreatedSinceADate( DateTime d)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (var fileInfo in new DirectoryInfo(_path).GetFiles())
            {
                if (fileInfo.LastWriteTimeUtc > d || fileInfo.CreationTimeUtc > d)
                {
                    files.Add(fileInfo);
                }
            }

            return files;
        }
        
        protected virtual void OnNewFileProcessed(NewFileProcessedEventArgs e) { if (NewFileProcessed != null) { NewFileProcessed(e); } } 

        public void WatchAndProcessLive()
        {
            _fsw = new FileSystemWatcher();
            _fsw.Path = _path;

            _fsw.NotifyFilter = NotifyFilters.LastWrite;
            _fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            
            _fsw.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            _fsw.EnableRaisingEvents = false;

            _fsw.Changed -= new FileSystemEventHandler(fsw_Changed);
            _fsw.Dispose();
        }


        private Dictionary<string, DateTime> filesProcessedRecently = new Dictionary<string, DateTime>();
        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            FileInfo fileInfo = new FileInfo(e.FullPath);
            try
            {
                if (!filesProcessedRecently.ContainsKey(fileInfo.FullName) || DateTime.Now > filesProcessedRecently[fileInfo.FullName].AddSeconds(5))
                {
                    filesProcessedRecently[fileInfo.FullName] = DateTime.Now;

                    /// log info
                    Console.Write("processing " + e.FullPath);
                    var hands = ProcessFile(fileInfo);
                    _dataRepository.GetHandsRepository().SaveHandsSqlCommand(hands);
                }
            }
            catch (Exception ex)
            {
                /// log erreur
            }
            finally
            {
                foreach (var item in filesProcessedRecently.Where(kvp => kvp.Value > DateTime.Now.AddSeconds(5)).ToList())
                {
                    filesProcessedRecently.Remove(item.Key);
                }
            }
        }
    }

    public class NewFileProcessedEventArgs : EventArgs
    {
        public NewFileProcessedEventArgs(string newFile, Hand lastHand)
        {
            _newFile = newFile;
            _lastHand = lastHand;
        }

        protected string _newFile;
        public string NewFile
        {
            get { return _newFile; }
            set { _newFile = value; }
        }
        
        protected Hand _lastHand;
        public Hand LastHand
        {
            get { return _lastHand; }
            set { _lastHand = value; }
        }

        
    }
}
