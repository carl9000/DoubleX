﻿using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace UTH.Infrastructure.Utility
{
    /// <summary>
    /// SQLite 辅助类
    /// </summary>
    public static class SQLiteHelper
    {
        #region 执行SQL语句，返回被操作的行数(ExecuteNonQuery)

        /// <summary>
        /// 执行SQL语句，返回被操作的行数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandText">执行语句或存储过程名</param>
        /// <param name="commandType">执行类型</param>
        /// <param name="cmdParms">SQL参数对象</param>
        /// <returns>所受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, string commandText, CommandType commandType = CommandType.Text, params SqliteParameter[] commandParameters)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                SqliteCommand cmd = new SqliteCommand();
                PrepareCommand(connection, cmd, commandText, commandType, null, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        #endregion

        #region 执行SQL 返回第一行第一列(ExecuteScalar)

        /// <summary>
        /// 执行SQL 返回第一行第一列
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandText">执行语句或存储过程名</param>s
        /// <param name="commandType">执行类型</param>
        /// <param name="commandParameters">SQL参数对象</param>
        /// <returns>object对象</returns>
        public static object ExecuteScalar(string connectionString, string commandText, CommandType commandType = CommandType.Text, params SqliteParameter[] commandParameters)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                SqliteCommand cmd = new SqliteCommand();
                PrepareCommand(connection, cmd, commandText, commandType, null, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// 执行SQL 返回第一行第一列
        /// </summary>
        /// <typeparam name="TEntity">返回对象</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandText">执行语句或存储过程名</param>
        /// <param name="commandType">执行类型</param>
        /// <param name="commandParameters">SQL参数对象</param>
        /// <returns>TEntity</returns>
        public static TEntity ExecuteScalar<TEntity>(string connectionString, string commandText, CommandType commandType = CommandType.Text, params SqliteParameter[] commandParameters)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                SqliteCommand cmd = new SqliteCommand();
                PrepareCommand(connection, cmd, commandText, commandType, null, commandParameters);
                TEntity val = default(TEntity);
                val = (TEntity)cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        #endregion

        #region 执行SQL 返回SqliteDataReader

        // SqliteDataReader返回一个连接，所以不能进行conn释放，在外界代码中使用完DataReader后，注意需要释放reader对象
        // 当返回连接对象报错时，这里进行数据库连接的关闭，保证数据库连接使用完成后保持关闭

        /// <summary>
        /// 执行SQL，返回SqliteDataReader
        /// </summary>
        /// <param name="connectionString">连接字符串(返回关闭Reader)</param>
        /// <param name="commandText">执行语句或存储过程名</param>
        /// <param name="commandType">执行类型</param>
        /// <param name="commandParameters">SQL参数对象</param>
        /// <returns></returns>
        public static SqliteDataReader ExecuteReader(ref SqliteConnection connection, string connectionString, string commandText, CommandType commandType = CommandType.Text, params SqliteParameter[] commandParameters)
        {
            connection = new SqliteConnection(connectionString);
            try
            {
                SqliteCommand cmd = new SqliteCommand();
                PrepareCommand(connection, cmd, commandText, commandType, null, commandParameters);
                SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return reader;
            }
            catch (Exception e)
            {
                connection.Close();
                throw e;
            }
        }

        #endregion

        #region 执行SQL 返回IEnumerable<TEntity>

        /// <summary>
        /// 执行SQL，返回IEnumerable<TEntity>
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="commandType">执行类型</param>
        /// <param name="func">转换方法(将IDataRead 转为 TEntity)</param>
        /// <param name="commandParameters">SQL参数对象</param>
        /// <returns></returns>
        public static IEnumerable<TEntity> ExecuteQuery<TEntity>(string connectionString, string commandText, CommandType commandType = CommandType.Text, Func<IDataReader, TEntity> func = null, params SqliteParameter[] commandParameters)
        {
            SqliteConnection connection = null;
            SqliteCommand cmd = null;
            IDataReader reader = null;
            try
            {
                using (connection = new SqliteConnection(connectionString))
                {
                    cmd = new SqliteCommand();
                    PrepareCommand(connection, cmd, commandText, commandType, null, commandParameters);
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    cmd.Parameters.Clear();

                    var type = typeof(TEntity);
                    var convertToType = Nullable.GetUnderlyingType(type) ?? type;
                    while (reader.Read())
                    {
                        TEntity val = func(reader);
                        if (val == null || val is TEntity)
                        {
                            yield return (TEntity)val;
                        }
                        else
                        {
                            yield return (TEntity)Convert.ChangeType(val, convertToType, CultureInfo.InvariantCulture);
                        }
                    }
                    while (reader.NextResult()) { /* ignore subsequent result sets */ }
                    // happy path; close the reader cleanly - no
                    // need for "Cancel" etc
                    reader.Dispose();
                    reader = null;
                }
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try { cmd.Cancel(); }
                        catch { /* don't spoil the existing exception */ }
                    }
                    reader.Dispose();
                }
                if (connection != null && connection.State != ConnectionState.Closed) connection.Close();
                cmd?.Dispose();
            }
        }

        #endregion

        #region 以事务执行sql语句列表

        /// <summary>
        /// 以事务执行sql语句列表，返回事务执行是否成功
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="commandTextes">sql语句列表</param>
        /// <param name="commandParameterses">sql语句列表对应的参数列表，参数列表必须与sql语句列表匹配</param>
        /// <param name="commandType">默认执行类型</param>
        /// <returns>事务执行是否成功</returns>
        public static bool ExecuteTransaction(string connectionString, List<string> commandTextes, List<SqliteParameter[]> commandParameterses, CommandType commandType = CommandType.Text)
        {
            bool flag = false;
            if (commandTextes.Count == commandParameterses.Count)
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    flag = ExecuteTransaction(connection, commandTextes, commandParameterses, commandType: commandType);
                }
            }
            return flag;
        }

        /// <summary>
        /// 以事务执行sql语句列表，返回事务执行是否成功
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="commandTextes">sql语句列表</param>
        /// <param name="commandParameterses">sql语句列表对应的参数列表，参数列表必须与sql语句列表匹配</param>
        /// <param name="commandType">默认执行类型</param>
        /// <returns>事务执行是否成功</returns>
        public static bool ExecuteTransaction(SqliteConnection connection, List<string> commandTextes, List<SqliteParameter[]> commandParameterses, CommandType commandType = CommandType.Text)
        {
            bool flag = false;
            if (commandTextes.Count == commandParameterses.Count)
            {
                List<SqliteCommand> commands = new List<SqliteCommand>();
                commandTextes.ForEach(x =>
                {
                    commands.Add(new SqliteCommand(x) { CommandType = commandType });
                });
                flag = ExecuteTransaction(connection, commands, commandParameterses);
            }
            return flag;
        }


        /// <summary>
        /// 以事务执行sql语句列表，返回事务执行是否成功
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="commandTextes">sql语句列表</param>
        /// <param name="commandParameterses">sql语句列表对应的参数列表，参数列表必须与sql语句列表匹配</param>
        /// <returns></returns>
        public static bool ExecuteTransaction(string connectionString, List<SqliteCommand> commands, List<SqliteParameter[]> commandParameterses)
        {
            bool flag = false;
            if (commands.Count == commandParameterses.Count)
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    flag = ExecuteTransaction(connection, commands, commandParameterses);
                }
            }
            return flag;
        }

        /// <summary>
        /// 以事务执行sql语句列表，返回事务执行是否成功
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="commandTextes">sql语句列表</param>
        /// <param name="commandParameterses">sql语句列表对应的参数列表，参数列表必须与sql语句列表匹配</param>
        /// <returns>事务执行是否成功</returns>
        public static bool ExecuteTransaction(SqliteConnection connection, List<SqliteCommand> commands, List<SqliteParameter[]> commandParameterses)
        {
            bool flag = false;
            if (commands.Count == commandParameterses.Count)
            {
                using (connection)
                {
                    SqliteTransaction sqlTran = null;
                    try
                    {
                        connection.Open();
                        sqlTran = connection.BeginTransaction();

                        for (int i = 0; i < commands.Count; i++)
                        {
                            PrepareCommand(connection, commands[i], commands[i].CommandText, commands[i].CommandType, sqlTran, commandParameterses[i]);
                            commands[i].ExecuteNonQuery();
                        }

                        sqlTran.Commit();
                        flag = true;
                    }
                    catch (Exception e)
                    {
                        if (sqlTran != null)
                        {
                            sqlTran.Rollback();
                        }
                    }
                }

            }
            return flag;
        }

        #endregion

        #region (Private)辅作操作方法

        /// <summary>
        /// 构造SQLCommand
        /// </summary>
        /// <param name="conn">Connection对象</param>
        /// <param name="cmd">Command对象</param>
        /// <param name="commandText">SQL Text</param>
        /// <param name="commandType">SQL字符串执行类型</param>
        /// <param name="trans">Transcation对象</param>
        /// <param name="cmdParms">SqliteParameters to use in the command</param>
        private static void PrepareCommand(SqliteConnection conn, SqliteCommand cmd, string commandText, CommandType commandType, SqliteTransaction trans, params SqliteParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = commandText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = commandType;

            if (cmdParms != null)
            {
                foreach (SqliteParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
            
        }

        #endregion
        
    }
}
