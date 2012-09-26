using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using Awam.Tracker.Model;

namespace Awam.Tracker.Data
{
    public static class Hands
    {
        public static void SaveHandsSqlCommand(IList<Hand> hands)
        {
            using (SqlCeConnection conn = new SqlCeConnection(Helper.ConnectionString))
            using (SqlCeCommand comm = new SqlCeCommand())
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;

                const string SqlCommandString = "insert  into [Hands] (Id, [User], Net, Time, ActionPreflop, ActionFlop, ActionTurn, ActionRiver, Card1, Card2, BB, Position, PositionButton, Stack, ActionBlind) Values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}','{12}', '{13}', '{14}')";

                foreach (var hand in hands)
                {
                    foreach (var player in hand.Players)
                    {
                        if (string.IsNullOrEmpty(player.ActionPreflop))
                            continue;
                        float f = player.PaidPreflop + player.PaidFlop + player.PaidTurn + player.PaidRiver;

                        comm.CommandText =
                            string.Format(
                                SqlCommandString,
                                hand.HandId,
                                player.Player,
                                player.MyMoneyCollected - f,
                                hand.Time.ToString("yyyy/MM/dd HH:mm:ss"),
                                player.ActionPreflop,
                                player.ActionFlop,
                                player.ActionTurn,
                                player.ActionRiver,
                                player.Card1Str,
                                player.Card2Str,
                                hand.BigBlind,
                                player.SeatNumber,
                                hand.ButtonPosition,
                                player.Stack,
                                player.ActionBlind);
                        try
                        {
                            comm.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            
                            throw;
                        }
                        
                    }
                }
            }
        }

    }
}
