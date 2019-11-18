using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SQLite;

namespace WebSocket.Utils
{
    public class SQLiteHelper
    {
        static private string DBConnStr = "";

        static SQLiteHelper()
        {
            DBConnStr = ConfigurationManager.ConnectionStrings["SQLite"].ConnectionString;
        }

        public static SQLiteConnection GetConnection()
        {
            SQLiteConnection conn = new SQLiteConnection(DBConnStr);
            return conn;
        }


    }
}
