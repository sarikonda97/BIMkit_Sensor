﻿using Newtonsoft.Json;
using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCheckPackage
{
    public class RuleResult
    {
        public Rule Rule { get; private set; }
        public double PassVal { get; private set; }
        public List<RuleInstance> RuleInstances { get; private set; }
        public TimeSpan Runtime { get; set; }
        public bool CheckCompleted { get; set; }

        [JsonConstructor]
        public RuleResult(Rule rule, double passVal, List<RuleInstance> ruleInstances, TimeSpan runtime, bool checkCompleted) : this(rule, passVal, ruleInstances, checkCompleted)
        {
            Runtime = runtime;
        }

        public RuleResult(Rule rule, double passVal, List<RuleInstance> ruleInstances, bool checkCompleted)
        {
            Rule = rule;
            PassVal = passVal;
            RuleInstances = ruleInstances;
            Runtime = new TimeSpan();
            CheckCompleted = checkCompleted;
        }

        public void UpdateRuleResult(RuleResult ruleResult)
        {
            if (Rule.Id != ruleResult.Rule.Id)
            {
                throw new Exception("Not the same Rule");
            }

            this.PassVal = ruleResult.PassVal;
            this.RuleInstances = ruleResult.RuleInstances;
            this.Runtime = ruleResult.Runtime;
            this.CheckCompleted = ruleResult.CheckCompleted;
        }

        public override string ToString()
        {
            return Rule.Name + ": " + (PassVal == 1 ? "Passed" : "Failed") + " (" + PassVal.ToString("f2") + ") \t(Runtime:" + Runtime.ToString() + ")";
        }
    }
}
