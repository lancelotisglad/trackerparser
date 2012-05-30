using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using Awam.Tracker.FileProcessor;
using Awam.Tracker.Parser;

namespace tester
{
    class Program
    {
        static readonly string _winamaxDefaultPath = @"C:\Data\awam\TrackerParser\histo\FewFiles";
        static string _path = string.Empty;

        static void Main(string[] args)
        {
            _path = _winamaxDefaultPath;
            DateTime from  = DateTime.MinValue;

            if (args.Count() > 0 && args[0] != null)
                _path = args[0];

            if (args.Count() > 1 && args[1] != null)
                from = DateTime.Parse(args[1]);

            FileProcessor fileProcessor  = new FileProcessor(_path);
            fileProcessor.ProcessImportOnModifiedFilesSinceLastImport(_path, true);

            Console.Read();
        }

        //private static void ProcessImportOnModifiedFilesSinceLastImport(string directoryPath, bool clearData = false)
        //{
        //    DateTime lastDate = GetLastImportDate(directoryPath);
        //    ProcessImport(directoryPath, lastDate, clearData);
        //}

        //private static void ProcessImport(string directoryPath, DateTime from, bool clearData=false)
        //{
        //    DateTime startDate = DateTime.UtcNow;
        //    string status = "success";
        //    try
        //    {
        //        if (clearData)
        //        {
        //            ClearAll();
        //        }

        //        var files = GetFilesModifiedOrCreatedSinceADate(directoryPath, from);

        //        Stopwatch sw = new Stopwatch();
        //        sw.Start();

        //        foreach (var fileInfo in files)
        //        {   
        //            ProcessFile(fileInfo);
        //        }

        //        sw.Stop();
        //        Console.WriteLine("Terminé : " + files.Count() + " en " + sw.ElapsedMilliseconds);
        //        Console.WriteLine("Terminé");
        //    } catch(Exception e)
        //    {
        //        status = "fail : " + e.Message + " " + e.StackTrace;
        //    }
        //    finally
        //    {
        //        DateTime endDate = DateTime.UtcNow;
        //        LogImport(startDate, endDate, status);

        //    }
        //}

        //private static void ProcessFile(FileInfo fileInfo)
        //{
        //    DateTime fileStartDate = DateTime.UtcNow;
        //    string fileStatus = "succes";
        //    FileParser fileParser = new FileParser(fileInfo.FullName);
        //    try
        //    {
        //        fileParser.Parse();
        //    } catch (Exception e)
        //    {
        //        fileStatus = "fail : " + e.Message + " " + e.StackTrace;
        //    }
        //    finally
        //    {
        //        DateTime endFileDate = DateTime.UtcNow;
        //        LogFileImport(fileInfo, fileStartDate, endFileDate, fileStatus);
        //    }
        //}

        //private static void LogFileImport(FileInfo fileInfo, DateTime startDate, DateTime endDate, string status)
        //{
        //    using (SqlCeConnection conn =new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
        //    using (SqlCeCommand comm = new SqlCeCommand())
        //    {
        //        conn.Open();
        //        comm.Connection = conn;
        //        comm.CommandType = System.Data.CommandType.Text;

        //        const string SqlCommandString =
        //            "insert  into [LogFiles] (FileName, StartDate, EndDate, Status ) Values ( '{0}', '{1}', '{2}', '{3}' )";

        //        comm.CommandText =
        //            string.Format(
        //                SqlCommandString,
        //                fileInfo.Name,
        //                startDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
        //                endDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
        //                status);

        //        comm.ExecuteNonQuery();
        //    }
        //}

        //private static IEnumerable<FileInfo> GetFilesModifiedOrCreatedSinceADate(string path, DateTime d)
        //{
        //    List<FileInfo> files = new List<FileInfo>();
        //    foreach (var fileInfo in new DirectoryInfo(path).GetFiles())
        //    {
        //        if (fileInfo.LastWriteTimeUtc > d || fileInfo.CreationTimeUtc > d)
        //        {
        //            files.Add(fileInfo);
        //        }
        //    }

        //    return files;
        //}

        ///// <summary>
        ///// Delete all Hands from database
        ///// </summary>
        //public static void ClearAll()
        //{
        //    using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
        //    using (SqlCeCommand command = new SqlCeCommand())
        //    {
        //        conn.Open();

        //        command.CommandType = System.Data.CommandType.Text;
        //        command.Connection = conn;
        //        command.CommandText = "delete from [Hands]";
        //        command.ExecuteNonQuery();

        //        command.CommandText = "delete from [LogFiles]";
        //        command.ExecuteNonQuery();

        //        command.CommandText = "delete from [LogImport]";
        //        command.ExecuteNonQuery();
        //    }
        //}

        ///// <summary>
        ///// Log Import
        ///// </summary>
        ///// <param name="beginDate">Start Date</param>
        ///// <param name="endDate">End Date</param>
        ///// <param name="status">Import Status</param>
        //public static void LogImport(DateTime beginDate, DateTime endDate, string status)
        //{ 
        //    using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
        //    using (SqlCeCommand comm = new SqlCeCommand())
        //    {
        //        conn.Open();
        //        comm.Connection = conn;
        //        comm.CommandType = System.Data.CommandType.Text;

        //        const string SqlCommandString =
        //            "insert  into [LogImport] (BeginDate, EndDate, Status, Directory ) Values ( '{0}', '{1}', '{2}', '{3}' )";

        //        comm.CommandText =
        //            string.Format(
        //                SqlCommandString,
        //                beginDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
        //                endDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
        //                status,
        //                _path);

        //        comm.ExecuteNonQuery();
        //    }
        //}

        //private static DateTime GetLastImportDate(string dir)
        //{
        //    using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
        //    using (SqlCeCommand comm = new SqlCeCommand())
        //    {
        //        conn.Open();
        //        comm.Connection = conn;
        //        comm.CommandType = System.Data.CommandType.Text;

        //        const string SqlCommandString =
        //            "Select Max(BeginDate) from [LogImport] where Directory = '{0}'";

        //        comm.CommandText =
        //            string.Format(
        //                SqlCommandString,
        //                dir);

        //        var d = comm.ExecuteScalar();
        //        if (d == null)
        //        {
        //            return DateTime.MinValue;
        //        }

        //        return Convert.ToDateTime(d);
        //    }
        //}
    }
}
