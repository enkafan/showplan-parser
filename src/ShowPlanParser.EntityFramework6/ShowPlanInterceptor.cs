using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace ShowPlanParser.EntityFramework6
{
    class ShowPlanInterceptor : IDbCommandInterceptor
    {
        private readonly ShowPlanSpy _spy;

        public ShowPlanInterceptor(ShowPlanSpy spy)
        {
            _spy = spy;
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            AddCommand(command);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            AddCommand(command);

        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            AddCommand(command);
        }

        private void AddCommand(DbCommand command)
        {
            // we can have multiple interceptors configured depending on 
            // how many tests are running at once. I don't know of a good way
            // to tell which one goes where so forcing them to only work per thread

            if (command.CommandText.Trim().ToLowerInvariant() == "select cast(serverproperty('engineedition') as int)")
                return;

            var id = (Guid)CallContext.GetData("ShowPlanInterceptorId");
            if (id != _spy.Id)
                return;

            var sqlCommand = new SqlCommand(command.CommandText);
            foreach (var parameter in command.Parameters.OfType<SqlParameter>())
            {
                sqlCommand.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
            }

            _spy.Commands.Add(sqlCommand);
        }
    }
}