using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using ShowPlanParser.EntityFramework6.Tests.Models;
using Xunit;

namespace ShowPlanParser.EntityFramework6.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void Can_do_simple_select()
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
        public async Task Can_do_simple_select_asyncronously()
        {
            var context = new AdventureWorksContext();

            using (var spy = new ShowPlanSpy())
            {
                var queryResult = await context.SalesOrderDetails.Where(i => i.OrderQty == 15).ToListAsync();
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
                    context.ProductCategories.Add(new ProductCategory {Name = $"test category {counter}"});
                }
                context.SaveChanges();
                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }

        [Fact]
        [AutoRollback]
        public void Can_do_updates()
        {
            var context = new AdventureWorksContext();

            using (var spy = new ShowPlanSpy())
            {
                context.ProductModels.First().Name += " updated";
                context.SaveChanges();
                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }

        [Fact]
        [AutoRollback]
        public void Can_do_delete()
        {
            var context = new AdventureWorksContext();

            using (var spy = new ShowPlanSpy())
            {
                // let's do this with a new row and a seperate select statement to test how this pattern would work too
                context.ProductCategories.Add(new ProductCategory { Name = $"test category" });
                context.SaveChanges();

                var category = context.ProductCategories.First(model => model.Name.Equals("test category"));
                context.ProductCategories.Remove(category);
                context.SaveChanges();

                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }

        [Fact]
        [AutoRollback]
        public void Can_do_insert_with_sub_records()
        {
            var context = new AdventureWorksContext();

            using (var spy = new ShowPlanSpy())
            {

                var category = new ProductCategory {Name = $"test category"};
                category.ProductCategories.Add(new ProductCategory { Name = "child category 1"});
                category.ProductCategories.Add(new ProductCategory { Name = "child category 2" });
                context.ProductCategories.Add(category);
                context.SaveChanges();

                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);
            }
        }

        [Fact]
        [AutoRollback]
        public void Can_deal_with_different_lengths_and_precision_on_select()
        {
            // this should work easily, no parameters needed on generated query
            var context = new AdventureWorksContext();
            using (var spy = new ShowPlanSpy())
            {
                var item = context.DataLengths.First();

                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);

            }
        }

        [Fact]
        [AutoRollback]
        public void Can_deal_with_different_lengths_and_precision_on_insert()
        {
            // this should work easily, no parameters needed on generated query
            var context = new AdventureWorksContext();
            using (var spy = new ShowPlanSpy())
            {
                var data = new DataLength();
                data.DateTime2LengthOf6 = DateTime.Today;
                data.DateTime2LengthOf7 = DateTime.Now.AddMilliseconds(1);
                data.Nvarchar30 = "hello";
                data.Varchar30 = "world";
                data.VarcharMax = new string('-', 500);
                data.NvarcharMax = new string('༆', 500);
                data.DecimalLengthOf18And2 = 22m / 7m;

                context.DataLengths.Add(data);
                context.SaveChanges();

                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);

            }
        }

        [Fact]
        public void Can_do_lazy_loading()
        {
            // this should work easily, no parameters needed on generated query
            var context = new AdventureWorksContext();
            using (var spy = new ShowPlanSpy())
            {
                var product =  context.Products.First();
                var categoryName = product.ProductCategory.Name;

                product.ShouldNotBeNull();
                categoryName.ShouldNotBeNullOrEmpty();

                var showPlan = spy.GetShowPlans().First();
                showPlan.SubTreeCost().ShouldBeLessThan(2);

            }
        }
    }
}
