using MathPackage;
using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleGeneratorPackage
{
    public class RuleGenerator
    {
        public static Rule LearnRuleBoolean(OccurrenceRule OccurrenceRule1, string type1, OccurrenceRule OccurrenceRule2, string type2, List<Example> examples)
        {
            // Filter relation instances by type
            List<FilteredExample> filteredExamples = new List<FilteredExample>();
            foreach (Example example in examples)
            {
                FilteredExample newFilteredExample = new FilteredExample();
                foreach (RelationInstance ri in example.relationInstances)
                {
                    if (ri.type1 == type1 && ri.type2 == type2)
                    {
                        newFilteredExample.relationInstances.Add(ri);
                    }
                }
                filteredExamples.Add(newFilteredExample);
            }

            if (filteredExamples.Count == 0)
            {
                return null;
            }

            // For each example group values based on OccurrenceRule, resulting in all possibles options for that example
            foreach (FilteredExample fe in filteredExamples)
            {
                if (OccurrenceRule1 == OccurrenceRule.ALL)
                {
                    if (OccurrenceRule2 == OccurrenceRule.ALL)
                    {
                        List<bool?> firstList = fe.relationInstances[0].BooleanResults;
                        for (int i = 1; i < fe.relationInstances.Count; i++)
                        {
                            firstList = CombineLists(firstList, fe.relationInstances[i].BooleanResults);
                        }
                        fe.boolOptions.Add(firstList);
                    }
                    if (OccurrenceRule2 == OccurrenceRule.ANY)
                    {
                        // Make a list for each ID, then add every combination of one item from one list to the items of the other list(s)
                        List<string> idList = fe.relationInstances.Select(ri => ri.id1).Distinct().ToList();
                        RecursiveCombiner(new List<bool?>(), 0, fe, ref idList);
                    }
                }
                if (OccurrenceRule1 == OccurrenceRule.ANY)
                {
                    if (OccurrenceRule2 == OccurrenceRule.ALL)
                    {
                        // Do it for each unique ID1
                        foreach (string id in fe.relationInstances.Select(ri => ri.id1).Distinct())
                        {
                            List<RelationInstance> tempRelationList = fe.relationInstances.Where(relIst => relIst.id1 == id).ToList();
                            List<bool?> firstList = tempRelationList[0].BooleanResults;
                            for (int i = 1; i < tempRelationList.Count; i++)
                            {
                                firstList = CombineLists(firstList, tempRelationList[i].BooleanResults);
                            }
                            fe.boolOptions.Add(firstList);
                        }
                    }
                    if (OccurrenceRule2 == OccurrenceRule.ANY)
                    {
                        for (int i = 0; i < fe.relationInstances.Count; i++)
                        {
                            fe.boolOptions.Add(fe.relationInstances[i].BooleanResults);
                        }
                    }
                }

                // Get rid of duplicate option results
                for (int i = 0; i < fe.boolOptions.Count; i++)
                {
                    for (int j = i + 1; j < fe.boolOptions.Count; j++)
                    {
                        if (ListsAreEqual(fe.boolOptions[i], fe.boolOptions[j]))
                        {
                            fe.boolOptions.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }

            // Search through example results and find the instance that holds true throughout
            List<bool?> bestResult = new List<bool?>();
            OptionSearch(new List<bool?>(), 0, filteredExamples, ref bestResult);

            // Probably shouldnt happen but might...
            if (bestResult.Count == 0)
            {
                return null;
            }

            // Convert that back to a Rule for the output
            Dictionary<string, ExistentialClause> ecDict = new Dictionary<string, ExistentialClause>()
            {
                { "obj1", new ExistentialClause(OccurrenceRule1, new Characteristic(type1, new List<PropertyCheck>())) },
                { "obj2", new ExistentialClause(OccurrenceRule2, new Characteristic(type2, new List<PropertyCheck>())) }
            };

            // Only add these depending on which were true in the rule checks
            List<RelationCheck> relChecks = new List<RelationCheck>();
            GetRelationChecks(bestResult, relChecks);

            LogicalExpression newLE = new LogicalExpression(new List<ObjectCheck>(), relChecks, new List<LogicalExpression>(), LogicalOperator.AND);
            return new Rule("Generated Rule", "", ErrorLevel.Warning, ecDict, newLE);
        }

        private static void GetRelationChecks(List<bool?> bestResult, List<RelationCheck> relChecks)
        {
            if (bestResult[0] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("Distance", OperatorNum.LESS_THAN_OR_EQUAL, 1.0, Unit.M)));
            if (bestResult[1] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("Distance", OperatorNum.EQUAL, 1.5, Unit.M)));
            if (bestResult[2] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("Distance", OperatorNum.GREATER_THAN_OR_EQUAL, 2.0, Unit.M)));

            if (bestResult[3] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("FacingAngleTo", OperatorNum.EQUAL, 0.0, Unit.DEG)));
            if (bestResult[4] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("FacingAngleTo", OperatorNum.EQUAL, 90.0, Unit.DEG)));
            if (bestResult[5] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("FacingAngleTo", OperatorNum.EQUAL, 180.0, Unit.DEG)));

            if (bestResult[6] == true)
                relChecks.Add(new RelationCheck("obj2", "obj1", Negation.MUST_HAVE, new PropertyCheckNum("FacingAngleTo", OperatorNum.EQUAL, 0.0, Unit.DEG)));
            if (bestResult[7] == true)
                relChecks.Add(new RelationCheck("obj2", "obj1", Negation.MUST_HAVE, new PropertyCheckNum("FacingAngleTo", OperatorNum.EQUAL, 90.0, Unit.DEG)));
            if (bestResult[8] == true)
                relChecks.Add(new RelationCheck("obj2", "obj1", Negation.MUST_HAVE, new PropertyCheckNum("FacingAngleTo", OperatorNum.EQUAL, 180.0, Unit.DEG)));

            if (bestResult[9] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("AlignmentAngle", OperatorNum.EQUAL, 0.0, Unit.DEG)));
            if (bestResult[10] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("AlignmentAngle", OperatorNum.EQUAL, 90.0, Unit.DEG)));
            if (bestResult[11] == true)
                relChecks.Add(new RelationCheck("obj1", "obj2", Negation.MUST_HAVE, new PropertyCheckNum("AlignmentAngle", OperatorNum.EQUAL, 180.0, Unit.DEG)));
        }

        private static List<bool?> CombineLists(List<bool?> l1, List<bool?> l2)
        {
            if (l1.Count == 0)
            {
                return CopyList(l2);
            }
            if (l2.Count == 0)
            {
                return CopyList(l1);
            }

            List<bool?> returnList = new List<bool?>();
            for (int i = 0; i < l1.Count; i++)
            {
                bool? compareVal = null;
                if (l1[i] == l2[i])
                {
                    compareVal = l1[i];
                }
                returnList.Add(compareVal);
            }
            return returnList;
        }

        private static List<bool?> CopyList(List<bool?> list)
        {
            List<bool?> returnList = new List<bool?>();
            for (int i = 0; i < list.Count; i++)
            {
                returnList.Add(list[i]);
            }
            return returnList;
        }

        private static bool ListsAreEqual(List<bool?> l1, List<bool?> l2)
        {
            foreach (bool? b1 in l1)
            {
                foreach (bool? b2 in l2)
                {
                    if (b1 != b2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void RecursiveCombiner(List<bool?> combinedSoFar, int IDIndex, FilteredExample fe, ref List<string> idList)
        {
            if (IDIndex == idList.Count)
            {
                fe.boolOptions.Add(combinedSoFar);
                return;
            }

            string thisID = idList[IDIndex];
            List<RelationInstance> tempRelationList = fe.relationInstances.Where(relIst => relIst.id1 == thisID).ToList();
            foreach (RelationInstance ri in tempRelationList)
            {
                List<bool?> tempCombinedList = CombineLists(ri.BooleanResults, combinedSoFar);
                RecursiveCombiner(tempCombinedList, IDIndex + 1, fe, ref idList);
            }
        }

        private static void OptionSearch(List<bool?> combinedSoFar, int exampleIndex, List<FilteredExample> examples, ref List<bool?> bestResult)
        {
            if (exampleIndex == examples.Count)
            {
                // Check against best result:
                if (bestResult.Count == 0 || TrueValCounter(bestResult) < TrueValCounter(combinedSoFar))
                {
                    bestResult = combinedSoFar;
                }
                return;
            }

            FilteredExample currenctExample = examples[exampleIndex];
            foreach (List<bool?> option in currenctExample.boolOptions)
            {
                List<bool?> tempCombined = CombineLists(option, combinedSoFar);
                OptionSearch(tempCombined, exampleIndex + 1, examples, ref bestResult);
            }
        }

        private static int TrueValCounter(List<bool?> list)
        {
            return list.Count(b => b == true);
        }
    }

    public class Example
    {
        public List<RelationInstance> relationInstances = new List<RelationInstance>();
    }

    public class FilteredExample
    {
        public List<RelationInstance> relationInstances = new List<RelationInstance>();
        public List<List<bool?>> boolOptions = new List<List<bool?>>();
        public List<List<Tuple<double, double>>> rangeOptions = new List<List<Tuple<double, double>>>();
    }

    public class RelationInstance
    {
        public string id1;
        public string id2;
        public string type1;
        public string type2;
        public double distance;
        public double facing12;
        public double facing21;
        public double alignment;

        public List<bool?> BooleanResults;

        public RelationInstance(string id1, string id2, string type1, string type2, double distance, double facing12, double facing21, double alignment)
        {
            this.id1 = id1;
            this.id2 = id2;
            this.type1 = type1;
            this.type2 = type2;
            this.distance = distance;
            this.facing12 = facing12;
            this.facing21 = facing21;
            this.alignment = alignment;
            GetBooleansFromValues();
        }

        private void GetBooleansFromValues()
        {
            BooleanResults = new List<bool?>()
            {
                distance <= 1.0,
                distance > 1.0 && distance <= 2.0,
                distance > 2.0,

                facing12 <= 45.0*Math.PI/180.0,
                facing12 > 45.0*Math.PI/180.0 && facing12 <= 135.0*Math.PI/180.0,
                facing12 > 135.0*Math.PI/180.0,

                facing21 <= 45.0*Math.PI/180.0,
                facing21 > 45.0*Math.PI/180.0 && facing21 <= 135.0*Math.PI/180.0,
                facing21 > 135.0*Math.PI/180.0,

                alignment <= 45.0*Math.PI/180.0,
                alignment > 45.0*Math.PI/180.0 && alignment <= 135.0*Math.PI/180.0,
                alignment > 135.0*Math.PI/180.0,
                };
        }
    }
}