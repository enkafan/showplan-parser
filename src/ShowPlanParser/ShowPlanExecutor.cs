using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ShowPlanParser
{
    public class ShowPlanExecutor : IDisposable
    {
        private readonly SqlConnection _sqlConnection;
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(ShowPlan));
        private bool _disposed = false;

        public ShowPlanExecutor(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }

        public ShowPlan GetShowPlan(SqlCommand command)
        {
            const string queryPlanQuery = @"SELECT  [QP].[query_plan], text
                FROM sys.dm_exec_cached_plans CP
                CROSS APPLY sys.dm_exec_sql_text(CP.plan_handle) ST
                CROSS APPLY sys.dm_exec_query_plan(CP.plan_handle) QP where [text] = @statement";

            if (_sqlConnection.State == ConnectionState.Closed)
            {
                _sqlConnection.Open();
            }

            var commandText = command.CommandText;
            if (command.Parameters.Count > 0)
            {
                commandText = string.Join(",", GetParamTextForPlan(command.Parameters)) + commandText;
            }

            var sqlCommand = new SqlCommand(queryPlanQuery, _sqlConnection);
            sqlCommand.Parameters.AddWithValue("statement", commandText);
            var plan = (string) sqlCommand.ExecuteScalar();

            using (TextReader reader = new StringReader(plan))
            {
                return (ShowPlan) _serializer.Deserialize(reader);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _sqlConnection.Dispose();
            _disposed = true;
        }

        ~ShowPlanExecutor()
        {
            Dispose(false);
        }

        private static string GetParamTextForPlan(SqlParameterCollection parameterCollection)
        {
            List<string> paramList = new List<string>();

            foreach (var param in parameterCollection.OfType<SqlParameter>())
            {
                if (param.Size > 0)
                {
                    paramList.Add($"@{param.ParameterName} {param.SqlDbType}({param.Size})");
                }
                else
                {
                    paramList.Add($"@{param.ParameterName} {param.SqlDbType}");
                }
            }

            return $"({string.Join(",", paramList)})";
        }
    }
}
