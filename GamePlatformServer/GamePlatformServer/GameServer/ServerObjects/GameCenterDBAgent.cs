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
    public class GameCenterDBAgent
    {

        private string m_sqliteConnStr;

        public GameCenterDBAgent(string sqliteConnStr)
        {
            m_sqliteConnStr = sqliteConnStr;
        }

        public void Start()
        {
            LogHelper.LogInfo(string.Format("测试GameCenterDBAgent连接在{0}", m_sqliteConnStr));
            SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr);
            conn.Open();
            conn.Close();
            LogHelper.LogInfo(string.Format("测试GameCenterDBAgent连接可用！"));
        }
        public void Stop()
        {
        }

        public void PlayerRegister(string playerId, string password)
        {
            using (SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr))
            {
                conn.Open();
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
                            throw new InfoException("gamecenter.register.multiple_register");
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
                            throw new InfoException("gamecenter.login.idpassword_wrong");

                        string password_salt = reader["password_salt"].ToString();
                        string password_md5 = reader["password_md5"].ToString();
                        string input_md5 = SecurityHelper.CreateMD5(password + password_salt);
                        //验证是否成功
                        if (!input_md5.Equals(password_md5))
                            throw new InfoException("gamecenter.login.idpassword_wrong");
                    }
                }
            }
        }
        public PlayerInfo GetPlayerInfo(string playerId)
        {
            //使用这种方法定义单个实体
            IDictionary<string, object> dbResult = new Dictionary<string, object>();
            dbResult["player_id"] = DBNull.Value;
            dbResult["player_name"] = DBNull.Value;
            dbResult["player_point"] = DBNull.Value;
            dbResult["player_icon"] = DBNull.Value;
            //查询实体
            using (SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr))
            {
                conn.Open();
                using (SQLiteCommand cmd1 = new SQLiteCommand(conn))
                {
                    cmd1.CommandText = "select * from player_info where player_id=@p0";
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                    cmd1.Parameters[0].Value = playerId;
                    using (SQLiteDataReader reader = cmd1.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            foreach (string field in dbResult.Keys.ToArray())
                                dbResult[field] = reader[field];
                        }
                    }

                }
            }
            //赋值返回
            PlayerInfo info;
            info.Id = playerId;
            info.Name = dbResult["player_name"] == DBNull.Value ? "" : (string)dbResult["player_name"];
            info.Point = dbResult["player_point"] == DBNull.Value ? 0 : (int)dbResult["player_point"];
            info.IconNo = dbResult["player_icon"] == DBNull.Value ? 0 : (int)dbResult["player_icon"];
            return info;
        }
        public void UpdatePlayerInfo(string playerId, PlayerInfo info)
        {
            using (SQLiteConnection conn = SQLiteHelper.GetConnection(m_sqliteConnStr))
            {
                conn.Open();
                //填写更新表单
                string player_id = playerId;
                string player_name = info.Name;
                int player_point = info.Point;
                int player_icon = info.IconNo;
                //执行更新操作
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // player_info更新
                    using (SQLiteCommand cmd1 = new SQLiteCommand(conn))
                    {
                        cmd1.CommandText = "update player_info set player_name=@p1,player_point=@p2,player_icon=@p3 where player_id=@p0";
                        cmd1.CommandType = System.Data.CommandType.Text;
                        cmd1.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                        cmd1.Parameters[0].Value = playerId;
                        cmd1.Parameters.Add(new SQLiteParameter("@p1", System.Data.DbType.String));
                        cmd1.Parameters[1].Value = player_name;
                        cmd1.Parameters.Add(new SQLiteParameter("@p2", System.Data.DbType.Int32));
                        cmd1.Parameters[2].Value = player_point;
                        cmd1.Parameters.Add(new SQLiteParameter("@p3", System.Data.DbType.Int32));
                        cmd1.Parameters[3].Value = player_icon;
                        cmd1.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }

        }


    }
}
