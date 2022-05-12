using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesignPackage
{
    public static class RecommenderUtil
    {
        public static List<string> GetRecommendedTypeOrderFromRules(List<Rule> rules)
        {
            List<TypeConnect> typeConnections = new List<TypeConnect>();
            foreach (Rule rule in rules)
            {
                List<string> keys = new List<string>(rule.ExistentialClauses.Keys);
                //for (int i = 0; i < keys.Count; i++)
                //{
                int i = 0; // For now you can kinda assume that the first type is the one that the rules is about (makes sense too)
                string key1 = keys[i];
                for (int j = i + 1; j < keys.Count; j++)
                {
                    string key2 = keys[j];

                    string firstType = rule.ExistentialClauses[key1].Characteristic.Type;
                    string secondType = rule.ExistentialClauses[key2].Characteristic.Type;
                    if (firstType == secondType)
                    {
                        continue;
                    }
                    // No duplicates
                    if (!typeConnections.Any(tc => tc.Type1 == firstType && tc.Type2 == secondType))
                    {
                        typeConnections.Add(new TypeConnect(firstType, secondType));
                    }
                }
                //}
            }

            // First add walls and floors to the list of placed items
            List<string> placedTypes = new List<string>() { "Floor", "Wall" };
            typeConnections.RemoveAll(tc => placedTypes.Contains(tc.Type1));
            while (typeConnections.Count > 0)
            {
                // Find everything that depends on the placed items
                List<string> potentialNextTypes = new List<string>();
                foreach (TypeConnect tc in typeConnections)
                {
                    if (!placedTypes.Contains(tc.Type1) && placedTypes.Contains(tc.Type2))
                    {
                        potentialNextTypes.Add(tc.Type1);
                    }
                }
                potentialNextTypes = potentialNextTypes.Distinct().ToList();

                string topTypeConnection = null;
                if (potentialNextTypes.Count == 0)
                {
                    // Find the remaining item with the most dependent connections (second type)
                    List<string> remainingTypes = typeConnections.Select(tc => tc.Type1).Distinct().ToList();
                    topTypeConnection = remainingTypes.OrderBy(ty => typeConnections.Count(tc => tc.Type2 == ty)).Last();
                }

                if (potentialNextTypes.Count == 1)
                {
                    topTypeConnection = potentialNextTypes.First();
                }

                if (potentialNextTypes.Count > 1)
                {
                    // If there is more than one, find the one that has the fewest other not-yet-placed dependencies (ideally zero)
                    Dictionary<string, int> typeDependedntCount = new Dictionary<string, int>();
                    foreach (string nextTypeOption in potentialNextTypes)
                    {
                        typeDependedntCount[nextTypeOption] = 0;
                        foreach (TypeConnect tc in typeConnections)
                        {
                            // Check if this type depends on something that is not placed (which is a bad thing)
                            if (tc.Type1 == nextTypeOption && !placedTypes.Contains(tc.Type2))
                            {
                                typeDependedntCount[nextTypeOption] += 1;
                            }
                        }
                    }
                    topTypeConnection = typeDependedntCount.Keys.First();
                    foreach (var key in typeDependedntCount.Keys)
                    {
                        if (typeDependedntCount[topTypeConnection] == typeDependedntCount[key])
                        {
                            // Might be better to place things that have other dependents first if there is a tie breaker
                            if (typeConnections.Count(tc => tc.Type2 == topTypeConnection) < typeConnections.Count(tc => tc.Type2 == key))
                            {
                                topTypeConnection = key;
                            }
                        }

                        // Lower number is better
                        if (typeDependedntCount[topTypeConnection] > typeDependedntCount[key])
                        {
                            topTypeConnection = key;
                        }
                    }
                }

                placedTypes.Add(topTypeConnection);
                typeConnections.RemoveAll(tc => placedTypes.Contains(tc.Type1));
            }

            return placedTypes;
        }

        private class TypeConnect
        {
            public string Type1;
            public string Type2;

            public TypeConnect(string firstType, string secondType)
            {
                this.Type1 = firstType;
                this.Type2 = secondType;
            }
        }
    }
}
