using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleAPI.Models
{
    public static class DefaultRuleCreator
    {
        public static Rule CreatOverlapRuleFurnishingAndReal()
        {
            Dictionary<string, ExistentialClause> ecDict = new Dictionary<string, ExistentialClause>()
            {
                { "obj1", new ExistentialClause(OccurrenceRule.ALL, new Characteristic("FurnishingElement", new List<PropertyCheck>())) },
                { "obj2", new ExistentialClause(OccurrenceRule.ALL, new Characteristic("Real", new List<PropertyCheck>())) } // BuildingElement
            };

            List<RelationCheck> relChecks = new List<RelationCheck>
            {
                new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckBool("MeshOverlap", OperatorBool.EQUAL, false))
            };

            LogicalExpression newLE = new LogicalExpression(new List<ObjectCheck>(), relChecks, new List<LogicalExpression>(), LogicalOperator.AND);

            Rule newRule = new Rule("Default Overlap Rule", "", ErrorLevel.Error, ecDict, newLE);
            newRule.Id = Guid.NewGuid().ToString().Replace("-", "");
            return newRule;
        }
    }
}