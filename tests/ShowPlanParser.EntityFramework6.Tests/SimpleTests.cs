using System.Linq;
using Shouldly;
using ShowPlanParser.EntityFramework6.Tests.Models;
using Xunit;

namespace ShowPlanParser.EntityFramework6.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void Query_should_not_be_crappy()
        {
            var context = new AdventureWorksContext();

            using (var spy = new ShowPlanSpy(context))
            {
                var queryResult = context.SalesOrderDetails.Where(i => i.OrderQty == 15).ToList();
                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }
    }
}
