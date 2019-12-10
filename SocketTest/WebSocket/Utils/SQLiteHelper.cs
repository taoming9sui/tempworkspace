using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace WebSocket.Utils
{
    public class SQLiteHelper
    {
        public static SQLiteConnection GetConnection(string connStr)
        {
            SQLiteConnection conn = new SQLiteConnection(connStr);
            return conn;
        }

    }
}
