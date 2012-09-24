using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Awam.Tracker.Data
{
    internal class Helper
    {
        public static string ConnectionString = GetConnectionString();

        public static string GetConnectionString()
        {
            System.Data.Common.DbConnectionStringBuilder sb= new DbConnectionStringBuilder();
            sb.ConnectionString = ConfigurationManager.ConnectionStrings["trackDBEntities2"].ConnectionString;

            return sb["provider connection string"].ToString();
        }
    }
}
