﻿using System;
using System.Data.SqlServerCe;
using System.IO;
using Awam.Tracker.Model;

namespace Awam.Tracker.Data
{
    public class Imports : IImports
    {
        public void LogFileImport(FileInfo fileInfo, DateTime startDate, DateTime endDate, string status, Hand lastImportedHand)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "insert  into [LogFiles] (FileName, StartDate, EndDate, Status, LastHandId, LastHandDate ) Values ( '{0}', '{1}', '{2}', '{3}', '{4}', '{5}' )";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        fileInfo.Name,
                        startDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        endDate.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        status,
                        lastImportedHand.HandId,
                        lastImportedHand.Time.ToString("yyyy/MM/dd HH:mm:ss.fff"));

                comm.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Log Import
        /// </summary>
        /// <param name="beginDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="status">Import Status</param>
        public void LogImport(DateTime beginDate, DateTime endDate, string status, string path)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
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

        public DateTime GetLastImportDate(string dir)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
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

        public DateTime GetLastImportFileDate(string fileName)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "Select Max(LastHandDate) from [LogFiles] where FileName = '{0}'";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        fileName);

                var d = comm.ExecuteScalar();
                return d == DBNull.Value ? DateTime.MinValue : (DateTime)d;
            }
        }
    }

    public interface IImports
    {
        void LogFileImport(FileInfo fileInfo, DateTime startDate, DateTime endDate, string status, Hand lastImportedHand);
        void LogImport(DateTime beginDate, DateTime endDate, string status, string path);
        DateTime GetLastImportDate(string dir);
        DateTime GetLastImportFileDate(string fileName);
    }
}
