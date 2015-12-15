using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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
            //const string queryPlanQuery = @"SELECT  [QP].[query_plan], text
            //    FROM sys.dm_exec_cached_plans CP
            //    CROSS APPLY sys.dm_exec_sql_text(CP.plan_handle) ST
            //    CROSS APPLY sys.dm_exec_query_plan(CP.plan_handle) QP where [text] = @statement";

            if (_sqlConnection.State == ConnectionState.Closed)
            {
                _sqlConnection.Open();
                var showPlanOnCommand = new SqlCommand("SET SHOWPLAN_XML ON", _sqlConnection);
                showPlanOnCommand.ExecuteNonQuery();
            }

            var sb = new StringBuilder();
            foreach (var showPlanParameter in command.Parameters)
            {
                switch (showPlanParameter.SqlType)
                {
                    case SqlDbType.Char:
                    case SqlDbType.NVarChar:
                    case SqlDbType.NChar:
                    case SqlDbType.VarChar:
                        sb.AppendLine($"DECLARE {showPlanParameter.Name} {showPlanParameter.SqlType}({showPlanParameter.Size})");
                        sb.AppendLine($"SET {showPlanParameter.Name} = '{showPlanParameter.Value}'");
                        break;
                    case SqlDbType.Text:
                    case SqlDbType.NText:
                    case SqlDbType.UniqueIdentifier:
                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.DateTimeOffset:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.DateTime2:
                    case SqlDbType.Time:
                        sb.AppendLine($"DECLARE {showPlanParameter.Name} {showPlanParameter.SqlType}");
                        sb.AppendLine($"SET {showPlanParameter.Name} = '{showPlanParameter.Value}'");
                        break;
                    case SqlDbType.BigInt:
                    case SqlDbType.Binary:
                    case SqlDbType.Bit:
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Image:
                    case SqlDbType.Int:
                    case SqlDbType.Money:
                    case SqlDbType.Real:
                    case SqlDbType.SmallInt:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Timestamp:
                    case SqlDbType.TinyInt:
                    case SqlDbType.VarBinary:
                    case SqlDbType.Variant:
                    case SqlDbType.Xml: // pretty sure these two won't work regardless
                    case SqlDbType.Udt:
                    case SqlDbType.Structured:
                        sb.AppendLine($"DECLARE {showPlanParameter.Name} {showPlanParameter.SqlType}");
                        sb.AppendLine($"SET {showPlanParameter.Name} = {showPlanParameter.Value}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            sb.AppendLine(command.CommandText);

            var cmdText = sb.ToString();
            var sqlCommand = new SqlCommand(cmdText, _sqlConnection);

            var plan = (string) sqlCommand.ExecuteScalar();
            if (string.IsNullOrWhiteSpace(plan))
                return null;

            using (TextReader reader = new StringReader(plan))
            {
                var showPlan = (ShowPlan) Serializer.Deserialize(reader);
                foreach (var plans in showPlan.BatchSequence)
                {
                    foreach (var stmtBlockType in plans)
                    {
                        stmtBlockType.Items = stmtBlockType.Items.Where(baseStmtInfoType => Math.Abs(baseStmtInfoType.StatementSubTreeCost) > .0000001).ToArray();
                    }
                }
                return showPlan;
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
                else if (param.Precision > 0)
                {
                    paramList.Add($"{name} {param.SqlType}({param.Precision},{param.Scale})");
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
