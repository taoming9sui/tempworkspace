using GamePlatformServer.Exceptions;
using GamePlatformServer.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GamePlatformServer.GameServer.ServerObjects
{
    public class GamePlayerDBAgent
    {

        private string m_sqliteConnStr;

        public GamePlayerDBAgent(string sqliteConnStr)
        {
            m_sqliteConnStr = sqliteConnStr;
        }


        public void Start()
        {
            SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr);
            conn.Open();
            conn.Close();
        }
        public void Stop()
        {
        }

        public void PlayerRegister(string playerId, string password)
        {
            using (SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr))
            {
                conn.Open();
                //检查输入格式
                Regex playerId_reg = new Regex(@"^[a-zA-Z0-9]{8,20}$");  //8-20数字大小写字母
                Regex password_reg = new Regex(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).{8,20}$"); //至少有一个数字 至少有一个小写字母 至少有一个大写字母 8-20位密码
                if (!playerId_reg.IsMatch(playerId))
                    throw new InfoException("用户名格式错误");
                if (!password_reg.IsMatch(password))
                    throw new InfoException("密码格式错误");
                //检查用户是否重复
                using (SQLiteCommand cmd1 = new SQLiteCommand(conn))
                {
                    cmd1.CommandText = "select player_id from player_register where player_id=@p0";
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                    cmd1.Parameters[0].Value = playerId;
                    using (SQLiteDataReader reader = cmd1.ExecuteReader())
                    {
                        bool hasOne = reader.HasRows;
                        if (hasOne)
                            throw new InfoException("该用户已被注册");
                    }
                }
                //填写注册表单
                string player_id = playerId;
                string password_salt = SecurityHelper.CreateRandomString(16);
                string password_md5 = SecurityHelper.CreateMD5(password + password_salt);
                long register_date = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
                //执行入库操作
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // player_register入库
                    using (SQLiteCommand cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "insert into player_register (player_id,password_md5,password_salt,register_date) values (@p0,@p1,@p2,@p3)";
                        cmd2.CommandType = System.Data.CommandType.Text;
                        cmd2.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                        cmd2.Parameters[0].Value = playerId;
                        cmd2.Parameters.Add(new SQLiteParameter("@p1", System.Data.DbType.String));
                        cmd2.Parameters[1].Value = password_md5;
                        cmd2.Parameters.Add(new SQLiteParameter("@p2", System.Data.DbType.String));
                        cmd2.Parameters[2].Value = password_salt;
                        cmd2.Parameters.Add(new SQLiteParameter("@p3", System.Data.DbType.Int64));
                        cmd2.Parameters[3].Value = register_date;
                        cmd2.ExecuteNonQuery();
                    }
                    // player_info入库
                    using (SQLiteCommand cmd3 = new SQLiteCommand(conn))
                    {
                        cmd3.CommandText = "insert into player_info (player_id, player_name) values (@p0,@p1)";
                        cmd3.CommandType = System.Data.CommandType.Text;
                        cmd3.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                        cmd3.Parameters[0].Value = playerId;
                        cmd3.Parameters.Add(new SQLiteParameter("@p1", System.Data.DbType.String));
                        cmd3.Parameters[1].Value = playerId;
                        cmd3.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
        public void PlayerLogin(string playerId, string password)
        {
            using (SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr))
            {
                conn.Open();
                using (SQLiteCommand cmd1 = new SQLiteCommand(conn))
                {
                    //检查用户是否存在
                    cmd1.CommandText = "select * from player_register where player_id=@p0";
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                    cmd1.Parameters[0].Value = playerId;
                    using (SQLiteDataReader reader = cmd1.ExecuteReader())
                    {
                        bool hasOne = reader.Read();
                        if (!hasOne)
                            throw new InfoException("用户名密码错误");

                        string password_salt = reader["password_salt"].ToString();
                        string password_md5 = reader["password_md5"].ToString();
                        string input_md5 = SecurityHelper.CreateMD5(password + password_salt);
                        //验证是否成功
                        if (!input_md5.Equals(password_md5))
                            throw new InfoException("用户名密码错误");
                    }
                }
            }
        }

    }
}
