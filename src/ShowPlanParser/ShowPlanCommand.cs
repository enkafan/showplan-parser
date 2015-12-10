using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ShowPlanParser
{
    public class ShowPlanCommand
    {
        public ShowPlanCommand(SqlCommand command)
        {
            CommandText = command.CommandText;
            ConnectionString = command.Connection.ConnectionString;
            Parameters = new List<ShowPlanParameter>();

            foreach (var parameter in command.Parameters.OfType<SqlParameter>())
            {
                Parameters.Add(new ShowPlanParameter(parameter.ParameterName, parameter.SqlDbType.ToString(),
                    parameter.Size));
            }
        }

        public ShowPlanCommand(string commandText, List<ShowPlanParameter> parameters, string connectionString)
        {
            CommandText = commandText;
            Parameters = parameters;
            ConnectionString = connectionString;
        }

        public string CommandText { get; }
        public string ConnectionString { get; }
        public List<ShowPlanParameter> Parameters { get; }
    }
}