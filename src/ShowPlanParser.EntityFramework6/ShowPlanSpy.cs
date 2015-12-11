using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace ShowPlanParser.EntityFramework6
{
    public class ShowPlanSpy : IDisposable
    {
        private IDbCommandInterceptor _showPlanInterceptor;

        public Guid Id { get; }
        public List<ShowPlanCommand> Commands { get; set; }  = new List<ShowPlanCommand>();

        public ShowPlanSpy()
        {
            Id = Guid.NewGuid();
            _showPlanInterceptor = new ShowPlanInterceptor(this);
            DbInterception.Add(_showPlanInterceptor);
            CallContext.SetData("ShowPlanInterceptorId", Id);
        }

        public IEnumerable<ShowPlan> GetShowPlans()
        {
            var commandsByConnectionString = Commands.GroupBy(i => i.ConnectionString);
            foreach (var command in commandsByConnectionString)
            {
                using (var executor = new ShowPlanExecutor(command.Key))
                {
                    foreach (var showPlanCommand in command.Distinct())
                    {
                        yield return executor.GetShowPlan(showPlanCommand);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_showPlanInterceptor == null)
                return;

            DbInterception.Remove(_showPlanInterceptor);
            _showPlanInterceptor = null;
        }

        ~ShowPlanSpy()
        {
            Dispose(false);
        }
    }
}
