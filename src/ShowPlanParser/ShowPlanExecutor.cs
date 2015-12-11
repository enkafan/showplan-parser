using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Serialization;

namespace ShowPlanParser
{
    public class ShowPlanExecutor : IDisposable
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(ShowPlan));
        private readonly SqlConnection _sqlConnection;
        private bool _disposed;

        public ShowPlanExecutor(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }

        public ShowPlan GetShowPlan(ShowPlanCommand command)
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
            if (string.IsNullOrWhiteSpace(plan))
                return null;

            using (TextReader reader = new StringReader(plan))
            {
                return (ShowPlan) Serializer.Deserialize(reader);
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

        private static string GetParamTextForPlan(IEnumerable<ShowPlanParameter> parameterCollection)
        {
            var paramList = new List<string>();

            foreach (var param in parameterCollection)
            {
                var name = param.Name;
                if (name.StartsWith("@") == false)
                    name = $"@{name}";

                if (param.Size > 0)
                {
                    paramList.Add($"{name} {param.SqlType}({param.Size})");
                }
                else
                {
                    paramList.Add($"{name} {param.SqlType}");
                }
            }

            return $"({string.Join(",", paramList)})";
        }
    }
}
