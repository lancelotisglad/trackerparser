using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Awam.Tracker.Data;
using Awam.Tracker.Parser;

namespace Awam.Tracker.FileProcessor
{
    public class FileProcessor
    {
        public delegate void NewFileProcessedHandler( NewFileProcessedEventArgs e);
        public event NewFileProcessedHandler NewFileProcessed;

        private readonly string _path;

        public FileProcessor(string directory)
        {
            _path = directory;
        }

        public void ProcessImportOnModifiedFilesSinceLastImport(string directoryPath, bool clearData = false)
        {
            if (clearData)
            {
                Management.ClearAll();
            }
            DateTime lastDate = Imports.GetLastImportDate(directoryPath);
            ProcessImport(directoryPath, lastDate, clearData);
        }

        private void ProcessImport(string directoryPath, DateTime from, bool clearData = false)
        {
            DateTime startDate = DateTime.UtcNow;
            string status = "success";
            try
            {
                var files = GetFilesModifiedOrCreatedSinceADate(directoryPath, from);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                foreach (var fileInfo in files)
                {
                    ProcessFile(fileInfo);
                }

                sw.Stop();
                Console.WriteLine("Terminé : " + files.Count() + " en " + sw.ElapsedMilliseconds);
                Console.WriteLine("Terminé");
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
        }

        public void ProcessFile(FileInfo fileInfo)
        {
            OnNewFileProcessed(new NewFileProcessedEventArgs(fileInfo.Name));

            DateTime fileStartDate = DateTime.UtcNow;
            string fileStatus = "succes";
            var lastImportDate = Imports.GetLastImportFileDate(fileInfo.Name);

            FileParser fileParser = new FileParser(fileInfo.FullName, lastImportDate);
            try
            {
                fileParser.Parse();
            }
            catch (Exception e)
            {
                fileStatus = "fail : " + e.Message + " " + e.StackTrace;
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
