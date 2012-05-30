using System;
using System.Data.SqlServerCe;
using System.IO;

namespace Awam.Tracker.Data
{
    public class Imports
    {
        public static void LogFileImport(FileInfo fileInfo, DateTime startDate, DateTime endDate, string status)
        {
            using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "insert  into [LogFiles] (FileName, StartDate, EndDate, Status ) Values ( '{0}', '{1}', '{2}', '{3}' )";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        fileInfo.Name,
                        startDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        endDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        status);

                comm.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Log Import
        /// </summary>
        /// <param name="beginDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="status">Import Status</param>
        public static void LogImport(DateTime beginDate, DateTime endDate, string status, string path)
        {
            using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "insert  into [LogImport] (BeginDate, EndDate, Status, Directory ) Values ( '{0}', '{1}', '{2}', '{3}' )";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        beginDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        endDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        status,
                        path);

                comm.ExecuteNonQuery();
            }
        }

        public static DateTime GetLastImportDate(string dir)
        {
            using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "Select Max(BeginDate) from [LogImport] where Directory = '{0}'";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        dir);

                var d = comm.ExecuteScalar();
                return d == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(d);
            }
        }

        public static DateTime GetLastImportFileDate(string fileName)
        {
            using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "Select Max(StartDate) from [LogFiles] where FileName = '{0}'";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        fileName);

                var d = comm.ExecuteScalar();
                return d == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(d);
            }
        }
    }
}
