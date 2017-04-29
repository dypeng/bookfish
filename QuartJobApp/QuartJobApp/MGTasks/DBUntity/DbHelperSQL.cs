using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
namespace DBUntity
{
    public abstract class SqlHelper
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["sqlConnectionString"].ConnectionString;
        private static SqlConnection Sqlconnection;
        public static SqlConnection SqlConnection
        {
            get
            {
                if (SqlHelper.Sqlconnection == null)
                {
                    SqlHelper.Sqlconnection = new SqlConnection(SqlHelper.connectionString);
                    SqlHelper.Sqlconnection.Open();
                }
                else if (SqlHelper.Sqlconnection.State == ConnectionState.Closed)
                {
                    SqlHelper.Sqlconnection.Open();
                }
                else if (SqlHelper.Sqlconnection.State == ConnectionState.Broken)
                {
                    SqlHelper.Sqlconnection.Close();
                    SqlHelper.Sqlconnection.Open();
                }
                return SqlHelper.Sqlconnection;
            }
        }
        public SqlHelper()
        {
        }
        public static bool ColumnExists(string tableName, string columnName)
        {
            string sql = string.Concat(new string[]
            {
                "select count(1) from syscolumns where [id]=object_id('",
                tableName,
                "') and [name]='",
                columnName,
                "'"
            });
            object res = SqlHelper.GetSingle(sql);
            return res != null && Convert.ToInt32(res) > 0;
        }
        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = SqlHelper.GetSingle(strsql);
            int result;
            if (obj == null)
            {
                result = 1;
            }
            else
            {
                result = int.Parse(obj.ToString());
            }
            return result;
        }
        public static bool Exists(string strSql)
        {
            object obj = SqlHelper.GetSingle(strSql);
            int cmdresult;
            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            return cmdresult != 0;
        }
        public static bool TabExists(string TableName)
        {
            string strsql = "select count(*) from sysobjects where id = object_id(N'[" + TableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
            object obj = SqlHelper.GetSingle(strsql);
            int cmdresult;
            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            return cmdresult != 0;
        }
        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            object obj = SqlHelper.GetSingle(strSql, cmdParms);
            int cmdresult;
            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            return cmdresult != 0;
        }
        public static int ExecuteSql(string SQLString)
        {
            int result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        result = rows;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public static int ExecuteSqlByTime(string SQLString, int Times)
        {
            int result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        result = rows;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public static int ExecuteSqlTran(List<string> SQLStringList)
        {
            int result;
            using (SqlConnection conn = new SqlConnection(SqlHelper.connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int i = 0; i < SQLStringList.Count; i++)
                    {
                        string strsql = SQLStringList[i];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    result = count;
                }
                catch(Exception ex)
                {
                    tx.Rollback();
                    result = 0;
                }
                finally
                {
                    conn.Dispose();
                    conn.Close();
                }
            }
            return result;
        }
        public static int ExecuteSql(string SQLString, string content)
        {
            int result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    result = rows;
                }
                catch (SqlException e)
                {
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return result;
        }
        public static object ExecuteSqlGet(string SQLString, string content)
        {
            object result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                    {
                        result = null;
                    }
                    else
                    {
                        result = obj;
                    }
                }
                catch (SqlException e)
                {
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return result;
        }
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            int result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                SqlCommand cmd = new SqlCommand(strSQL, connection);
                SqlParameter myParameter = new SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    result = rows;
                }
                catch (SqlException e)
                {
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return result;
        }
        public static object GetSingle(string SQLString)
        {
            object result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            result = null;
                        }
                        else
                        {
                            result = obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public static object GetSingle(string SQLString, int CommandTimeout)
        {
            object result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = CommandTimeout;
                        object obj = cmd.ExecuteScalar();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            result = null;
                        }
                        else
                        {
                            result = obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public static SqlDataReader ExecuteReader(string strSQL)
        {
            SqlConnection connection = new SqlConnection(SqlHelper.connectionString);
            SqlCommand cmd = new SqlCommand(strSQL, connection);
            SqlDataReader result;
            try
            {
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                result = myReader;
            }
            catch (SqlException e)
            {
                connection.Dispose();
                connection.Close();
                throw e;
            }
            return result;
        }
        public static DataSet Query(string SQLString)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Dispose();
                    connection.Close();
                }
                result = ds;
            }
            return result;
        }
        public static DataSet Query(string SQLString, int Times)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    new SqlDataAdapter(SQLString, connection)
                    {
                        SelectCommand =
                        {
                            CommandTimeout = Times
                        }
                    }.Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Dispose();
                    connection.Close();
                }
                result = ds;
            }
            return result;
        }
        public static DataSet QueryPage(string tableName, string whereStr, SqlParameter[] whereParams, string orderBy, int pageIndex, int pageSize, ref int recordCount)
        {
            if (string.IsNullOrEmpty(whereStr)) whereStr = "1=1";
            if (string.IsNullOrEmpty(orderBy)) orderBy = "Id Desc";

            if (tableName.ToLower().Contains("select")) tableName = "(" + tableName + ")";

            #region 获取记录总数
            recordCount = 0;
            string cntSql = string.Format("SELECT COUNT(1) FROM {0} T WHERE 1=1 AND {1}", tableName, whereStr);
            object cnt = GetSingle(cntSql, whereParams);
            if (cnt != DBNull.Value) recordCount = Convert.ToInt32(cnt);
            #endregion

            int startIdx = 0;
            int endIdx = 0;
            int pageCnt = (int)Math.Ceiling(recordCount * 1.0 / pageSize);

            if (pageIndex < 1) pageIndex = 1;
            if (pageIndex > pageCnt) pageIndex = pageCnt;           
            startIdx = (pageIndex - 1) * pageSize + 1;
            endIdx = pageIndex * pageSize;

            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendLine("SELECT * FROM (");
            sbSql.AppendFormat("SELECT ROW_NUMBER() OVER (order by {0}) AS Row, T.* FROM {1} T WHERE 1=1 AND {2}\r\n", orderBy, tableName, whereStr);
            sbSql.AppendLine(") TT");
            sbSql.AppendFormat("WHERE TT.Row between {0} and {1}", startIdx, endIdx);

            return Query(sbSql.ToString(), whereParams);
        }

        public static int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            int result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        SqlHelper.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        result = rows;
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            SqlHelper.PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Dispose();
                        conn.Close();
                    }
                }
            }
        }
        public static int ExecuteSqlTran(List<CommandInfo> cmdList)
        {
            int result;
            using (SqlConnection conn = new SqlConnection(SqlHelper.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        int count = 0;
                        foreach (CommandInfo myDE in cmdList)
                        {
                            string cmdText = myDE.CommandText;
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Parameters;
                            SqlHelper.PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
                            {
                                if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                                {
                                    trans.Rollback();
                                    result = 0;
                                    return result;
                                }
                                object obj = cmd.ExecuteScalar();
                                if (obj == null && obj == DBNull.Value)
                                {
                                }
                                bool isHave = Convert.ToInt32(obj) > 0;
                                if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
                                {
                                    trans.Rollback();
                                    result = 0;
                                    return result;
                                }
                                if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
                                {
                                    trans.Rollback();
                                    result = 0;
                                    return result;
                                }
                            }
                            else
                            {
                                int val = cmd.ExecuteNonQuery();
                                count += val;
                                if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
                                {
                                    trans.Rollback();
                                    result = 0;
                                    return result;
                                }
                                cmd.Parameters.Clear();
                            }
                        }
                        trans.Commit();
                        result = count;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Dispose();
                        conn.Close();
                    }
                }
            }
            return result;
        }
        public static void ExecuteSqlTranWithIndentity(List<CommandInfo> SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        int indentity = 0;
                        foreach (CommandInfo myDE in SQLStringList)
                        {
                            string cmdText = myDE.CommandText;
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Parameters;
                            SqlParameter[] array = cmdParms;
                            for (int i = 0; i < array.Length; i++)
                            {
                                SqlParameter q = array[i];
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = indentity;
                                }
                            }
                            SqlHelper.PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            array = cmdParms;
                            for (int i = 0; i < array.Length; i++)
                            {
                                SqlParameter q = array[i];
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Dispose();
                        conn.Close();
                    }
                }
            }
        }
        public static void ExecuteSqlTranWithIndentity(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        int indentity = 0;
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            SqlParameter[] array = cmdParms;
                            for (int i = 0; i < array.Length; i++)
                            {
                                SqlParameter q = array[i];
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = indentity;
                                }
                            }
                            SqlHelper.PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            array = cmdParms;
                            for (int i = 0; i < array.Length; i++)
                            {
                                SqlParameter q = array[i];
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Dispose();
                        conn.Close();
                    }
                }
            }
        }
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            object result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        SqlHelper.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            result = null;
                        }
                        else
                        {
                            result = obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        throw e;
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(SqlHelper.connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader result;
            try
            {
                SqlHelper.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                result = myReader;
            }
            catch (SqlException e)
            {
                connection.Dispose();
                connection.Close();
                throw e;
            }
            return result;
        }
        public static DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                SqlHelper.PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        connection.Dispose();
                        connection.Close();
                    }
                    result = ds;
                }
            }
            return result;
        }
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                for (int i = 0; i < cmdParms.Length; i++)
                {
                    SqlParameter parameter = cmdParms[i];
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && parameter.Value == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        public static SqlDataReader RunProcedure(string storedProcName, params IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(SqlHelper.connectionString);
            connection.Open();
            SqlCommand command = SqlHelper.BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                new SqlDataAdapter
                {
                    SelectCommand = SqlHelper.BuildQueryCommand(connection, storedProcName, parameters)
                }.Fill(dataSet, tableName);
                connection.Close();
                result = dataSet;
            }
            return result;
        }
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, int Times)
        {
            DataSet result;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlCommand selectCommand = SqlHelper.BuildQueryCommand(connection, storedProcName, parameters);
                selectCommand.CommandTimeout = Times;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = selectCommand;
                adapter.Fill(dataSet, tableName);
                connection.Close();
                result = dataSet;
            }
            return result;
        }
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    SqlParameter parameter = (SqlParameter)parameters[i];
                    if (parameter != null)
                    {
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && parameter.Value == null)
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }
            return command;
        }
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            int result2;
            using (SqlConnection connection = new SqlConnection(SqlHelper.connectionString))
            {
                connection.Open();
                SqlCommand command = SqlHelper.BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                int result = (int)command.Parameters["ReturnValue"].Value;
                result2 = result;
            }
            return result2;
        }


        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = SqlHelper.BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        public static DataTable SqlGetDataTable(string proc, CommandType type, string[] paramValue, out int OutTotalCount)
        {
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand(proc, SqlHelper.SqlConnection);
            SqlParameter[] myParms = new SqlParameter[11];
            myParms[0] = new SqlParameter("@TableName", SqlDbType.VarChar, 50);
            myParms[0].Value = paramValue[0];
            myParms[1] = new SqlParameter("@FieldList", SqlDbType.VarChar, 50);
            myParms[1].Value = paramValue[1];
            myParms[2] = new SqlParameter("@PrimaryKey", SqlDbType.VarChar, 50);
            myParms[2].Value = paramValue[2];
            myParms[3] = new SqlParameter("@Where", SqlDbType.VarChar, 500);
            myParms[3].Value = paramValue[3];
            myParms[4] = new SqlParameter("@Order", SqlDbType.VarChar, 50);
            myParms[4].Value = paramValue[4];
            myParms[5] = new SqlParameter("@SortType", SqlDbType.Int, 4);
            myParms[5].Value = paramValue[5];
            myParms[6] = new SqlParameter("@RecorderCount", SqlDbType.Int, 4);
            myParms[6].Value = paramValue[6];
            myParms[7] = new SqlParameter("@PageSize", SqlDbType.Int, 4);
            myParms[7].Value = paramValue[7];
            myParms[8] = new SqlParameter("@PageIndex", SqlDbType.Int, 4);
            myParms[8].Value = paramValue[8];
            myParms[9] = new SqlParameter("@TotalCount", SqlDbType.Int, 4);
            myParms[9].Direction = ParameterDirection.Output;
            myParms[10] = new SqlParameter("@TotalPageCount", SqlDbType.Int, 4);
            myParms[10].Direction = ParameterDirection.Output;
            SqlParameter[] array = myParms;
            for (int i = 0; i < array.Length; i++)
            {
                SqlParameter parameter = array[i];
                cmd.Parameters.Add(parameter);
            }
            cmd.CommandType = type;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            OutTotalCount = Convert.ToInt32(myParms[9].Value);
            return ds.Tables[0];
        }

        #region 事务处理
        public static SqlTransaction Begin()
        {
            SqlConnection conn = new SqlConnection(SqlHelper.connectionString);
            conn.Open();
            return conn.BeginTransaction();
        }

        public static void CloseConnection(SqlTransaction trans)
        {
            if (trans == null) throw new ArgumentNullException();

            if (trans.Connection.State == ConnectionState.Open)
            {
                trans.Connection.Close();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static object GetSingle(SqlTransaction trans, string SQLString)
        {
            if (trans == null) return GetSingle(SQLString);

            object result;
            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    SqlHelper.PrepareCommand(cmd, trans.Connection, trans, SQLString, null);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();

                    if (Convert.IsDBNull(obj)) obj = null;

                    result = obj;
                }
                catch (SqlException e)
                {
                    throw e;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="SQLString"></param>
        /// <param name="CommandTimeout"></param>
        /// <returns></returns>
        public static object GetSingle(SqlTransaction trans, string SQLString, int CommandTimeout)
        {
            if (trans == null) return GetSingle(SQLString, CommandTimeout);

            object result;
            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    SqlHelper.PrepareCommand(cmd, trans.Connection, trans, SQLString, null);
                    cmd.CommandTimeout = CommandTimeout;
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();

                    if (Convert.IsDBNull(obj)) obj = null;

                    result = obj;
                }
                catch (SqlException e)
                {
                    throw e;
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SQLString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static object GetSingle(SqlTransaction trans, string SQLString, params SqlParameter[] cmdParms)
        {
            if (trans == null) return GetSingle(SQLString, cmdParms);

            object result;
            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    SqlHelper.PrepareCommand(cmd, trans.Connection, trans, SQLString, cmdParms);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();

                    if (Convert.IsDBNull(obj)) obj = null;

                    result = obj;
                }
                catch (SqlException e)
                {
                    throw e;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static int ExecuteSql(SqlTransaction trans, string SQLString)
        {
            if (trans == null) return ExecuteSql(SQLString);

            int result;
            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    SqlHelper.PrepareCommand(cmd, trans.Connection, trans, SQLString, null);
                    result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                catch (SqlException e)
                {
                    throw e;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static int ExecuteSql(SqlTransaction trans, string SQLString, params SqlParameter[] cmdParms)
        {
            if (trans == null) return ExecuteSql(SQLString, cmdParms);

            int result;
            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    SqlHelper.PrepareCommand(cmd, trans.Connection, trans, SQLString, cmdParms);
                    result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                catch (SqlException e)
                {
                    throw e;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SQLString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(SqlTransaction trans, string SQLString, params SqlParameter[] cmdParms)
        {
            if (trans == null) return ExecuteReader(SQLString, cmdParms);

            SqlCommand cmd = new SqlCommand();
            SqlDataReader result;
            try
            {
                SqlHelper.PrepareCommand(cmd, trans.Connection, trans, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.Default);
                cmd.Parameters.Clear();

                result = myReader;
            }
            catch (SqlException e)
            {
                throw e;
            }
            return result;
        }
        #endregion

        #region 新增扩展方法
        /// <summary>
        /// 执行存储过程(无输出参数)
        /// </summary>
        /// <param name="storedProcedureName">存储过程名</param>
        /// <param name="p">参数(允许0个或者0个以上)</param>
        public static DataTable ExecuteStoredProcedure(string storedProcedureName, params SqlParameter[] p)
        {
            //创建命令
            SqlCommand comm = new SqlCommand()
            {
                CommandText = storedProcedureName,
                CommandType = CommandType.StoredProcedure
            };
            return ExecuteSearch(comm, p);
        }

        
        /// <summary>
        /// 可执行输出参数的存储过程
        /// </summary>
        /// <param name="storedProcedureName"></param>
        /// <param name="obj"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static DataTable ExecuteStoredProcedure(string storedProcedureName, out object obj, params SqlParameter[] p) {
            obj = null;
            //创建命令
            SqlCommand comm = new SqlCommand()
            {
                CommandText = storedProcedureName,
                CommandType = CommandType.StoredProcedure
            };
            DataTable dt = ExecuteSearch(comm, p);
            foreach (SqlParameter _p in p) {
                if (_p.Direction == ParameterDirection.Output) {
                    obj = _p.Value;
                    break;
                }
            }
            return dt;
        }


        /// <summary>
        /// 执行 SQL 语句
        /// </summary>
        /// <param name="comm">执行的命令</param>
        /// <param name="p">参数集</param>
        /// <returns>查询的结果集</returns>
        private static DataTable ExecuteSearch(SqlCommand comm, params SqlParameter[] p)
        {
            //获得返回集实例
            //_result = Utility.Result.GetInstance();
            DataTable dt = new DataTable();
            //创建连接
            using (SqlConnection conn = new SqlConnection(SqlHelper.connectionString))
            {
                //设置参数
                if (p != null && p.Length > 0)
                {
                    foreach (SqlParameter item in p)
                        comm.Parameters.Add(item);
                }
                comm.Connection = conn;
                SqlDataAdapter da = new SqlDataAdapter(comm);
                //try
                //{
                //填充数据
                da.Fill(dt);
                //设置查询参数
                // _result.Parameters = comm.Parameters;
                //设置Error标志
                // _result.HasError = false;
                //}
                //catch (Exception e)
                //{
                //    ////设置Error标志
                //    //_result.HasError = true;
                //    ////设置Error信息
                //    //_result.ErrorMessage = e.Message;
                //    ////设置查询信息为空DataTable
                //    //_result.DataTable = new DataTable();
                //    throw e;
                //}
                //finally
                //{
                //    //连接打开时，关闭连接
                //    if (conn != null)
                //    {
                //        conn.Close();
                //    }
                //}
                conn.Close();

            }
            return dt;
        }
        #endregion
    }

    public abstract class SqlDataHelper
    {

        #region from SqlDataReader
        public static string GetString(SqlDataReader dr, string fieldName, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return dr[fieldName].ToString();
            }

            return defaultValue;
        }
        public static int GetInt(SqlDataReader dr, string fieldName, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToInt32(dr[fieldName]);
            }

            return defaultValue;
        }

        public static double GetDouble(SqlDataReader dr, string fieldName, double defaultValue = 0.0)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToDouble(dr[fieldName]);
            }

            return defaultValue;
        }

        public static decimal GetDecimal(SqlDataReader dr, string fieldName, decimal defaultValue = 0.0M)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToDecimal(dr[fieldName]);
            }

            return defaultValue;
        }

        public static bool GetBoolean(SqlDataReader dr, string fieldName, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToBoolean(dr[fieldName]);
            }

            return defaultValue;
        }

        public static byte GetByte(SqlDataReader dr, string fieldName, byte defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToByte(dr[fieldName]);
            }

            return defaultValue;
        }

        public static DateTime GetDateTime(SqlDataReader dr, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToDateTime(dr[fieldName]);
            }

            return DateTime.MinValue;
        }

        public static DateTime? GetDateTime2(SqlDataReader dr, string fieldName, DateTime? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                return Convert.ToDateTime(dr[fieldName]);
            }

            return defaultValue;
        }

        public static Guid GetGuid(SqlDataReader dr, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            if (!Convert.IsDBNull(dr[fieldName]) && dr[fieldName] != null)
            {
                Guid result;
                if(Guid.TryParse(dr[fieldName].ToString(), out result))
                {
                    return result;
                }
            }

            return Guid.Empty;
        }
        #endregion

        #region from DataRow
        public static string GetString(DataRow dr, string fieldName, string defaultValue = null)
        {
            return string.Empty;
        }
        #endregion
    }
}
