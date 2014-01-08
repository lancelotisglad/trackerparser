using System.Data.SqlServerCe;

namespace Awam.Tracker.Data
{
    public class Management : IManagement
    {
        /// <summary>
        /// Delete all Hands from database
        /// </summary>
        public void ClearAll()
        {
            
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
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

    public interface IManagement
    {
        void ClearAll();
    }
}
