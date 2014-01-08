using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;

namespace Awam.Tracker.Data
{
    public class Statistics : IStatistics
    {
        public int GetTotalHandsPlayedByPlayer(string playerName)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "Select count(1) from [Hands] where [User] = '{0}'";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        playerName);

                var d = comm.ExecuteScalar();
                return Convert.ToInt32(d);
            }
        }

        public int GetPFRByPlayer(string playerName)
        {
            int nball = this.GetTotalHandsPlayedByPlayer(playerName);
            int pfr = this.GetCountPFRByPlayer(playerName);
                       
            return nball == 0 ? 0 : pfr * 100 / nball;
        }

        public int GetVPIPByPlayer(string playerName)
        {
            int nball = this.GetTotalHandsPlayedByPlayer(playerName);
            int nvpip = this.GetCountNotVPIPByPlayer(playerName);

            return nball == 0 ? 0 : 100 - (nvpip * 100 / nball);
        }

        private int GetCountPFRByPlayer(string playerName)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                   "Select count(1) from [Hands] where [User] = '{0}' and ActionPreflop like '%raises%'";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        playerName);

                var d = comm.ExecuteScalar();
                return Convert.ToInt32(d);
            }
        }
      
        private int GetCountNotVPIPByPlayer(string playerName)
        {
           
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString =
                    "Select count(1) from [Hands] where [User] = '{0}' and ActionPreflop not like '%calls%' and ActionPreflop not like '%raises%'";

                comm.CommandText =
                    string.Format(
                        SqlCommandString,
                        playerName);

                var d = comm.ExecuteScalar();
                return Convert.ToInt32(d);
            }
        }
    }

    public interface IStatistics
    {
        int GetTotalHandsPlayedByPlayer(string playerName);
        int GetVPIPByPlayer(string playerName);
        int GetPFRByPlayer(string playerName);

    }
}
