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

            using (var spy = new ShowPlanSpy())
            {
                var queryResult = context.SalesOrderDetails.Where(i => i.OrderQty == 15).ToList();
                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }

        [Fact]
        [AutoRollback]
        public void Can_do_plans_for_inserts()
        {
            var context = new AdventureWorksContext();

            using (var spy = new ShowPlanSpy())
            {
                for (var counter = 0; counter < 5; counter++)
                {
                    context.ProductCategories.Add(new ProductCategory() {Name = $"test category {counter}"});
                }
                context.SaveChanges();
                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }
    }
}
