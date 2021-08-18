using DbmsApi;
using DbmsApi.API;
using Microsoft.CSharp;
using RuleAPI.Methods;
using RuleAPI.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModelCheckPackage
{
    public class ModelCheckerFast
    {
        private static Dictionary<string, MethodInfo> PropMethods;
        private static Dictionary<string, MethodInfo> RelMethods;

        public RuleCheckModel Model { get; private set; }
        public List<Rule> Rules { get; private set; }
        public List<Tuple<Rule, Type, MethodInfo>> CompiledRules { get; private set; }
        public List<RuleResult> RuleResults { get; private set; }

        public RuleCheckObject NewObject { get; private set; }

        private static Dictionary<string,Properties> NewProperties;
        private static Dictionary<string, Dictionary<string, RuleCheckRelation>> NewRelations;

        #region Setup Methods:

        public ModelCheckerFast(Model model, List<Rule> rules)
        {
            // Add all the Functions:
            GetAllMethods();
            SetNewRules(rules);
            SetNewModel(model);
        }

        public void SetNewModel(Model model)
        {
            Model = new RuleCheckModel(model);

            RuleResults = new List<RuleResult>();
        }

        public void SetNewModel(RuleCheckModel model)
        {
            Model = model;

            RuleResults = new List<RuleResult>();
        }

        public void SetNewRules(List<Rule> rules)
        {
            Rules = rules;
            CompiledRules = GetCompiledRules();

            RuleResults = new List<RuleResult>();
        }

        public void SetNewRules(List<Tuple<Rule, Type, MethodInfo>> compiledRules)
        {
            Rules = compiledRules.Select(cr=>cr.Item1).ToList();
            CompiledRules = compiledRules;
        }

        public void SetNewObjet(ModelCatalogObject newObject)
        {
            NewObject = new RuleCheckObject(newObject, newObject.CatalogId);
        }

        public void GetAllMethods()
        {
            PropMethods = MethodFinder.GetAllPropertyInfos();
            RelMethods = MethodFinder.GetAllRelationInfos();
        }

        private List<Tuple<Rule, Type, MethodInfo>> GetCompiledRules()
        {
            if (Rules == null)
            {
                throw new Exception("Need to set rules");
            }

            List<Tuple<Rule, Type, MethodInfo>> returnRuleVal = new List<Tuple<Rule, Type, MethodInfo>>();
            foreach (Rule rule in Rules)
            {
                // quick rule sanity checks such as making sure the RelationCheck indecies are not the same in a single check:
                if (!ValidateRule(rule))
                {
                    continue;
                }

                // Generate the code:
                string ruleString = GenerateRuleCode(rule);

                // Compile the rule:
                Tuple<Type, MethodInfo> compileResult = CompileRule(rule, ruleString);

                returnRuleVal.Add(new Tuple<Rule, Type, MethodInfo>(rule, compileResult.Item1, compileResult.Item2));
            }

            return returnRuleVal;
        }

        #endregion

        #region Check Methods:

        public List<RuleResult> CheckModel(double defaultForNoOutput, ObjectTypes? objectTypes = null)
        {
            if (Model == null || Rules == null)
            {
                throw new Exception("Need to set rules and/or model");
            }

            if (CompiledRules == null)
            {
                CompiledRules = GetCompiledRules();
            }

            RuleResults = new List<RuleResult>();
            foreach (Tuple<Rule, Type, MethodInfo> compiledRule in CompiledRules)
            {
                Rule rule = compiledRule.Item1;
                RuleResult result = new RuleResult(rule, 0, new List<RuleInstance>());
                if (RuleRelevance(NewObject.Type, rule))
                {
                    Tuple<Type, MethodInfo> compileResult = new Tuple<Type, MethodInfo>(compiledRule.Item2, compiledRule.Item3);

                    // Execute the rule:
                    result = ExecuteRule(compileResult, rule, this, defaultForNoOutput);
                }

                RuleResults.Add(result);
            }

            return RuleResults;
        }

        public static bool ValidateRule(Rule rule)
        {
            // TODO: More Validation could be added
            if (rule.ExistentialClauses.Any(ec => ec.Value.OccurrenceRule != OccurrenceRule.NONE) && rule.ExistentialClauses.Any(ec => ec.Value.OccurrenceRule == OccurrenceRule.NONE))
            {
                // This would always fail...
                return false;
            }

            if (string.IsNullOrWhiteSpace(rule.Name))
            {
                return false;
            }

            return true;
        }

        public static bool RuleRelevance(ObjectTypes type, Rule rule)
        {
            return rule.ExistentialClauses.Select(ec => ec.Value.Characteristic.Type).Contains(type);
        }

        public static string GenerateRuleCode(Rule rule)
        {
            string returnVal = "";
            returnVal += "using System.Collections.Generic;";
            returnVal += "using System.Linq;";
            returnVal += "using RuleAPI.Models;";
            returnVal += "namespace ModelCheckPackage";
            returnVal += "{";
            returnVal += "    public class Rule" + rule.Id;
            returnVal += "    {";
            returnVal += "        public static List<RuleInstance> Execute(Rule rule, ModelCheckerFast modelCheck)";
            returnVal += "        {";
            returnVal += "            var ruleInstances = new List<RuleInstance>();";
            List<string> objs = new List<string>();
            foreach (var ecKvp in rule.ExistentialClauses)
            {
                returnVal += "            var " + ecKvp.Key + "s = new List<KeyValuePair<string, RuleCheckObject>>();";
            }
            returnVal += "            foreach (RuleCheckObject obj in modelCheck.Model.Objects)";
            returnVal += "            {";
            foreach (var ecKvp in rule.ExistentialClauses)
            {
                returnVal += "                if (ModelCheckerFast.CheckIfObjectHasCharacteristics(obj, rule.ExistentialClauses[\"" + ecKvp.Key + "\"].Characteristic))";
                returnVal += "                {";
                returnVal += "                    " + ecKvp.Key + "s.Add(new KeyValuePair<string, RuleCheckObject>(\"" + ecKvp.Key + "\", obj));";
                returnVal += "                }";
            }
            returnVal += "            }";
            // ======================================================================================================================================================
            foreach (var ecKvp in rule.ExistentialClauses)
            {
                returnVal += "            if (ModelCheckerFast.CheckIfObjectHasCharacteristics(modelCheck.NewObject, rule.ExistentialClauses[\"" + ecKvp.Key + "\"].Characteristic))";
                returnVal += "            {";
                returnVal += "                " + ecKvp.Key + "s.Add(new KeyValuePair<string, RuleCheckObject>(\"" + ecKvp.Key + "\", modelCheck.NewObject));";
                returnVal += "            }";
            }
            // ======================================================================================================================================================
            int ecCount = 0;
            foreach (var ecKvp in rule.ExistentialClauses)
            {
                returnVal += "            foreach (var obj" + ecCount + " in " + ecKvp.Key + "s)";
                returnVal += "            {";

                objs.Add("obj" + ecCount);
                ecCount++;
            }
            if (ecCount > 1)
            {
                List<string> objEqString = new List<string>();
                for (int i = 0; i < ecCount; i++)
                {
                    for (int j = i + 1; j < ecCount; j++)
                    {
                        objEqString.Add("(obj" + i + ".Value == obj" + j + ".Value)");
                    }
                }
                returnVal += "            if (" + string.Join(" || ", objEqString) + ")";
                returnVal += "            {";
                returnVal += "                continue;";
                returnVal += "            }";
            }
            returnVal += "                var objList = new List<KeyValuePair<string, RuleCheckObject>>() { " + string.Join(", ", objs) + " };";
            returnVal += "                List<RuleCheckRelation> relations = new List<RuleCheckRelation>();";
            returnVal += "                double passed = ModelCheckerFast.GetRuleInstanceResult(rule, objList.ToDictionary(x => x.Key, x => x.Value), ref relations);";
            returnVal += "                ruleInstances.Add(new RuleInstance(objList.Select(x=>x.Value).ToList(), passed, rule, relations));";
            foreach (var ecKvp in rule.ExistentialClauses)
            {
                returnVal += "        }";
            }
            returnVal += "            return ruleInstances;";
            returnVal += "        }";
            returnVal += "    }";
            returnVal += "}";

            returnVal.Replace("\\", "");
            return returnVal;
        }

        public static Tuple<Type, MethodInfo> CompileRule(Rule rule, string ruleString)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters
            {
                // True - memory generation, false - external file generation
                GenerateInMemory = false,
                // True - exe file generation, false - dll file generation
                GenerateExecutable = false
                //OutputAssembly = "CompiledRules.dll"
            };
            parameters.ReferencedAssemblies.Add(Assembly.GetCallingAssembly().Location);
            parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(ModelCheckerFast)).Location);
            parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(Rule)).Location);
            parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(Model)).Location);
            parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(Enumerable)).Location);

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, ruleString);
            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(string.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }
                throw new InvalidOperationException(sb.ToString());
            }
            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("ModelCheckPackage.Rule" + rule.Id);
            MethodInfo execute = program.GetMethod("Execute");

            return new Tuple<Type, MethodInfo>(program, execute);
        }

        public static RuleResult ExecuteRule(Tuple<Type, MethodInfo> compileResult, Rule rule, ModelCheckerFast modelCheck, double defaultForNoOutput)
        {
            Type program = compileResult.Item1;
            MethodInfo execute = compileResult.Item2;
            object classInstance = Activator.CreateInstance(program, null);
            object[] parametersArray = new object[] { rule, modelCheck };
            List<RuleInstance> output = execute.Invoke(classInstance, parametersArray) as List<RuleInstance>;

            //input for RFunction keeps track of Clause position in output
            List<KeyValuePair<int, ExistentialClause>> positions = new List<KeyValuePair<int, ExistentialClause>>();

            //orders existential clauses so that ALL clauses are first
            int i = 0;
            foreach (var ecKvp in rule.ExistentialClauses)
            {
                KeyValuePair<int, ExistentialClause> clause = new KeyValuePair<int, ExistentialClause>(i, ecKvp.Value);
                if (ecKvp.Value.OccurrenceRule == OccurrenceRule.ALL)
                {
                    //sorts all cases to the start of the list
                    positions.Insert(0, clause);
                }
                else
                {
                    positions.Add(clause);
                }

                i++;
            }
            //calls recursive function
            double rulePassVal = RFunction(output, positions, defaultForNoOutput);
            RuleResult result = new RuleResult(rule, rulePassVal, output);

            return result;
        }

        public static double RFunction(List<RuleInstance> output, List<KeyValuePair<int, ExistentialClause>> positions, double defaultForNoOutput)
        {
            if (output.Count == 0)
            {
                return defaultForNoOutput;
            }

            double passes = 1.0;
            if (positions[0].Value.OccurrenceRule == OccurrenceRule.ANY)
            {
                passes = 0.0;
            }
            //recursion ends on last clause
            if (positions.Count == 1)
            {
                //checks if final layer passes based on Occurrence rule
                foreach (RuleInstance rule in output)
                {
                    if (positions[0].Value.OccurrenceRule == OccurrenceRule.ALL)
                    {
                        passes = passes * rule.PassVal;
                    }
                    if (positions[0].Value.OccurrenceRule == OccurrenceRule.ANY)
                    {
                        passes = Math.Max(passes, rule.PassVal);
                    }
                    if (positions[0].Value.OccurrenceRule == OccurrenceRule.NONE)
                    {
                        passes = passes * (1 - rule.PassVal);
                    }
                }
            }
            else
            {
                //splits output list by objects in current existential clause
                Dictionary<RuleCheckObject, List<RuleInstance>> objPassedCounter = new Dictionary<RuleCheckObject, List<RuleInstance>>();
                foreach (RuleInstance instance in output)
                {
                    RuleCheckObject obj = instance.Objs[positions[0].Key];
                    if (objPassedCounter.ContainsKey(obj))
                    {
                        objPassedCounter[obj].Add(instance);
                    }
                    else
                    {
                        objPassedCounter[obj] = new List<RuleInstance>() { instance };
                    }
                }

                //loops through each split list
                foreach (KeyValuePair<RuleCheckObject, List<RuleInstance>> objectList in objPassedCounter)
                {
                    KeyValuePair<int, ExistentialClause> T = positions[0];
                    //Removes Existential Clause so that the recursive function uses the next Existential Clause
                    positions.RemoveAt(0);
                    //calls function again for each split list
                    double returnPass = RFunction(objectList.Value, positions, defaultForNoOutput);
                    //adds Existential Clause back so that the foreach loop can continue
                    positions.Insert(0, T);

                    //checks if current level passes based on OccurrenceRule
                    if (positions[0].Value.OccurrenceRule == OccurrenceRule.ALL)
                    {
                        passes = passes * returnPass;
                    }
                    if (positions[0].Value.OccurrenceRule == OccurrenceRule.ANY)
                    {
                        passes = Math.Max(passes, returnPass);
                    }
                    if (positions[0].Value.OccurrenceRule == OccurrenceRule.NONE)
                    {
                        passes = passes * returnPass;
                    }
                }
            }
            return passes;
        }

        #endregion

        #region Static functions that are used in the compiled model check rules:

        public static bool CheckIfObjectHasCharacteristics(RuleCheckObject obj, Characteristic charic)
        {
            // check that the object matches the characteristic type:
            if (!RecusiveTypeCheck(charic.Type, ObjectTypeTree.GetNode(obj.Type)))
            {
                return false;
            }

            // The object has to pass all Characteristic Property Checks:
            bool allCharacterChecksPass = true;
            foreach (PropertyCheck pCheck in charic.PropertyChecks)
            {
                Property props = GetOrAddPropertyToObject(obj, pCheck.Name);
                double checkPassed = OnePropertyPassesPropertyCheck(props, pCheck);

                if (checkPassed != 1.0)
                {
                    // A check has failed thus the object does not match all characterisitics:
                    allCharacterChecksPass = false;
                    break;
                }
            }

            return allCharacterChecksPass;
        }

        public static bool RecusiveTypeCheck(ObjectTypes checkType, ObjectType objType)
        {
            if (objType == null)
            {
                return false;
            }
            if (objType.ID == checkType)
            {
                return true;
            }
            return RecusiveTypeCheck(checkType, objType.Parent);
        }

        public static double OnePropertyPassesPropertyCheck(Property property, PropertyCheck pCheck)
        {
            double checkPassed = 0.0;
            if (pCheck.GetType() == typeof(PropertyCheckBool) && property.Type == PropertyType.BOOL)
            {
                PropertyCheckBool propertyCheckBool = pCheck as PropertyCheckBool;
                checkPassed = propertyCheckBool.CheckProperty(property as PropertyBool);
            }
            if (pCheck.GetType() == typeof(PropertyCheckNum) && property.Type == PropertyType.NUM)
            {
                PropertyCheckNum propertyCheckBool = pCheck as PropertyCheckNum;
                checkPassed = propertyCheckBool.CheckProperty(property as PropertyNum);
            }
            if (pCheck.GetType() == typeof(PropertyCheckString) && property.Type == PropertyType.STRING)
            {
                PropertyCheckString propertyCheckBool = pCheck as PropertyCheckString;
                checkPassed = propertyCheckBool.CheckProperty(property as PropertyString);
            }

            return checkPassed;
        }

        public static Property GetOrAddPropertyToObject(RuleCheckObject obj, string propertyName)
        {
            // Check if the object has the property already:
            if (NewProperties[obj.ID].ContainsKey(propertyName))
            {
                return NewProperties[obj.ID][propertyName];
            }

            // Find the function that matches the property:
            Property newProp = null;
            if (PropMethods.ContainsKey(propertyName))
            {
                object returnVal = PropMethods[propertyName].Invoke(null, new object[] { obj });
                if (returnVal.GetType() == typeof(bool))
                {
                    newProp = new PropertyBool(propertyName, (bool)returnVal);
                }
                if (returnVal.GetType() == typeof(double))
                {
                    newProp = new PropertyNum(propertyName, (double)returnVal);
                }
                if (returnVal.GetType() == typeof(string))
                {
                    newProp = new PropertyString(propertyName, (string)returnVal);
                }
            }
            newProp = newProp ?? new PropertyString(propertyName, "null");
            NewProperties[obj.ID].Add(newProp);

            return newProp;
        }

        public static Property GetOrAddPropertyToRelation(RuleCheckRelation relation, string propertyName)
        {
            // Check if the relation has the property already:
            if (NewRelations[relation.FirstObj.ID][relation.SecondObj.ID].Properties.ContainsKey(propertyName))
            {
                return NewRelations[relation.FirstObj.ID][relation.SecondObj.ID].Properties[propertyName];
            }

            Property newProp = null;
            if (RelMethods.ContainsKey(propertyName))
            {
                object returnVal = RelMethods[propertyName].Invoke(null, new object[] { relation });
                if (returnVal.GetType() == typeof(bool))
                {
                    newProp = new PropertyBool(propertyName, (bool)returnVal);
                }
                if (returnVal.GetType() == typeof(double))
                {
                    newProp = new PropertyNum(propertyName, (double)returnVal);
                }
                if (returnVal.GetType() == typeof(string))
                {
                    newProp = new PropertyString(propertyName, (string)returnVal);
                }
            }
            newProp = newProp ?? new PropertyString(propertyName, "null");
            NewRelations[relation.FirstObj.ID][relation.SecondObj.ID].Properties.Add(newProp);

            return newProp;
        }

        public static RuleCheckRelation FindOrCreateObjectRelation(RuleCheckObject obj1, RuleCheckObject obj2)
        {
            if (NewRelations.ContainsKey(obj1.ID))
            {
                if (NewRelations[obj1.ID].ContainsKey(obj2.ID))
                {
                    return NewRelations[obj1.ID][obj2.ID];
                }
            }
            RuleCheckRelation newObjRel = new RuleCheckRelation(obj1, obj2);
            NewRelations[obj1.ID][obj2.ID] = newObjRel;

            return newObjRel;
        }

        public static double GetRuleInstanceResult(Rule rule, Dictionary<string, RuleCheckObject> objects, ref List<RuleCheckRelation> relations)
        {
            return GetLogicalExpressionResult(rule.LogicalExpression, objects, ref relations);
        }

        public static double GetLogicalExpressionResult(LogicalExpression logicExp, Dictionary<string, RuleCheckObject> objects, ref List<RuleCheckRelation> relations)
        {
            double result = 1.0;
            bool firstResult = true;
            foreach (ObjectCheck oc in logicExp.ObjectChecks)
            {
                Property props = GetOrAddPropertyToObject(objects[oc.ObjName], oc.PropertyCheck.Name);
                double ocResult = OnePropertyPassesPropertyCheck(props, oc.PropertyCheck);
                ocResult = oc.Negation == Negation.MUST_HAVE ? ocResult : 1.0 - ocResult;
                if (firstResult)
                {
                    result = ocResult;
                    firstResult = false;
                }
                else
                {
                    switch (logicExp.LogicalOperator)
                    {
                        case (LogicalOperator.AND):
                            result = result * ocResult;
                            break;
                        case (LogicalOperator.OR):
                            result = Math.Max(result, ocResult);
                            break;
                        case (LogicalOperator.XOR):
                            result = Math.Abs(result - ocResult);
                            break;
                    }
                }
            }
            foreach (RelationCheck rc in logicExp.RelationChecks)
            {
                RuleCheckRelation rel = FindOrCreateObjectRelation(objects[rc.Obj1Name], objects[rc.Obj2Name]);
                relations.Add(rel);
                relations = relations.Distinct().ToList();
                Property props = GetOrAddPropertyToRelation(rel, rc.PropertyCheck.Name);
                double rcResult = OnePropertyPassesPropertyCheck(props, rc.PropertyCheck);
                rcResult = rc.Negation == Negation.MUST_HAVE ? rcResult : 1.0 - rcResult;
                if (firstResult)
                {
                    result = rcResult;
                    firstResult = false;
                }
                else
                {
                    switch (logicExp.LogicalOperator)
                    {
                        case (LogicalOperator.AND):
                            result = result * rcResult;
                            break;
                        case (LogicalOperator.OR):
                            result = Math.Max(result, rcResult);
                            break;
                        case (LogicalOperator.XOR):
                            result = Math.Abs(result - rcResult);
                            break;
                    }
                }
            }
            foreach (LogicalExpression le in logicExp.LogicalExpressions)
            {
                double leResult = GetLogicalExpressionResult(le, objects, ref relations);
                if (firstResult)
                {
                    result = leResult;
                    firstResult = false;
                }
                else
                {
                    switch (logicExp.LogicalOperator)
                    {
                        case (LogicalOperator.AND):
                            result = result * leResult;
                            break;
                        case (LogicalOperator.OR):
                            result = Math.Max(result, leResult);
                            break;
                        case (LogicalOperator.XOR):
                            result = Math.Abs(result - leResult);
                            break;
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
