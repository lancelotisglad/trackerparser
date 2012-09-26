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

        public FileProcessor(IFileParser parser,  string directory)
        {
            _path = directory;
            _parser = parser;
        }

        public void ProcessImportOnModifiedFilesSinceLastImport(string directoryPath, bool clearData = false)
        {
            if (clearData)
            {
                Management.ClearAll();
            }
            DateTime lastDate = Imports.GetLastImportDate(directoryPath);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IList<Hand> hands = ProcessImport(directoryPath, lastDate);
            Hands.SaveHandsSqlCommand(hands);
            sw.Stop();
            Console.WriteLine("Terminé : " + hands.Count() + " en " + sw.ElapsedMilliseconds);
        }

        private IList<Hand> ProcessImport(string directoryPath, DateTime from)
        {
            DateTime startDate = DateTime.UtcNow;
            string status = "success";
            List<Hand> hands = new List<Hand>();
            try
            {
                var files = GetFilesModifiedOrCreatedSinceADate(directoryPath, from);
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
                Imports.LogImport(startDate, endDate, status, _path);
            }

            return hands;
        }

        public IList<Hand> ProcessFile(FileInfo fileInfo)
        {
            OnNewFileProcessed(new NewFileProcessedEventArgs(fileInfo.Name));

            DateTime fileStartDate = DateTime.UtcNow;
            string fileStatus = "succes";
            var lastImportDate = Imports.GetLastImportFileDate(fileInfo.Name);

            try
            {
                return _parser.Parse(fileInfo.FullName, lastImportDate);
            }
            catch (Exception e)
            {
                fileStatus = "fail : " + e.Message + " " + e.StackTrace;
                return new List<Hand>();
            }
            finally
            {
                DateTime endFileDate = DateTime.UtcNow;
                Imports.LogFileImport(fileInfo, fileStartDate, endFileDate, fileStatus);
            }
        }

        private IEnumerable<FileInfo> GetFilesModifiedOrCreatedSinceADate(string path, DateTime d)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (var fileInfo in new DirectoryInfo(path).GetFiles())
            {
                if (fileInfo.LastWriteTimeUtc > d || fileInfo.CreationTimeUtc > d)
                {
                    files.Add(fileInfo);
                }
            }

            return files;
        }


        protected virtual void OnNewFileProcessed(NewFileProcessedEventArgs e) { if (NewFileProcessed != null) { NewFileProcessed(e); } } 
    }

    public class NewFileProcessedEventArgs : EventArgs
    {
        public NewFileProcessedEventArgs(string newFile)
        {
            _newFile = newFile;
        }

        protected string _newFile;
        public string NewFile
        {
            get { return _newFile; }
            set { _newFile = value; }
        }
    }
}
