using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;

namespace ShowPlanParser.EntityFramework6
{
    public class ShowPlanSpy : IDisposable
    {
        private readonly DbContext _context;
        private IDbCommandInterceptor _showPlanInterceptor;

        public Guid Id { get; }
        public List<ShowPlanCommand> Commands { get; set; }  = new List<ShowPlanCommand>();

        public ShowPlanSpy(DbContext context)
        {
            _context = context;
            Id = Guid.NewGuid();
            _showPlanInterceptor = new ShowPlanInterceptor(this);
            DbInterception.Add(_showPlanInterceptor);
            CallContext.SetData("ShowPlanInterceptorId", Id);
        }

        public IEnumerable<ShowPlan> GetShowPlans()
        {
            using (var executor = new ShowPlanExecutor(_context.Database.Connection.ConnectionString))
            {
                foreach (var command in Commands)
                {
                    yield return executor.GetShowPlan(command);
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
