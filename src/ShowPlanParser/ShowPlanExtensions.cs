using System.Linq;

namespace ShowPlanParser
{
    public static class ShowPlanExtensions
    {
        public static double SubTreeCost(this ShowPlan showPlan)
        {
            return showPlan.BatchSequence.First().First().Items.First().StatementSubTreeCost;
        }

        public static QueryPlanType QueryPlan(this ShowPlan showPlan)
        {
            return ((StmtSimpleType) showPlan.BatchSequence.First().First().Items.First()).QueryPlan;
        }
    }
}