using System.Data.SqlServerCe;

namespace Awam.Tracker.Data
{
    public class Management
    {
        /// <summary>
        /// Delete all Hands from database
        /// </summary>
        public static void ClearAll()
        {
            using (SqlCeConnection conn = new SqlCeConnection(@"Data Source=C:\Data\awam\TrackerParser\TrackerParser\tester\bin\Debug\trackDB.sdf"))
            using (SqlCeCommand command = new SqlCeCommand())
            {
                conn.Open();

                command.CommandType = System.Data.CommandType.Text;
                command.Connection = conn;
                command.CommandText = "delete from [Hands]";
                command.ExecuteNonQuery();

                command.CommandText = "delete from [LogFiles]";
                command.ExecuteNonQuery();

                command.CommandText = "delete from [LogImport]";
                command.ExecuteNonQuery();
            }
        }
    }
}
