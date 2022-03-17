using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCheckPackage
{
    public class MinorCheckScore
    {
        public double ErrorScore;
        public double WarningScore;
        public double RecommendScore;

        public MinorCheckScore(double errorScore, double warningScore, double recommendScore)
        {
            ErrorScore = errorScore;
            WarningScore = warningScore;
            RecommendScore = recommendScore;
        }

        public MinorCheckScore()
        {

        }

        public static bool operator ==(MinorCheckScore cs1, MinorCheckScore cs2)
        {
            return cs1.ErrorScore == cs2.ErrorScore && cs1.WarningScore == cs2.WarningScore && cs1.RecommendScore == cs2.RecommendScore;
        }

        public static bool operator !=(MinorCheckScore cs1, MinorCheckScore cs2)
        {
            return !(cs1 == cs2);
        }

        public static bool operator >=(MinorCheckScore cs1, MinorCheckScore cs2)
        {
            if (cs1.ErrorScore > cs2.ErrorScore)
            {
                return true;
            }
            if (cs1.ErrorScore == cs2.ErrorScore)
            {
                if (cs1.WarningScore > cs2.WarningScore)
                {
                    return true;
                }
                if (cs1.WarningScore == cs2.WarningScore)
                {
                    if (cs1.RecommendScore > cs2.RecommendScore)
                    {
                        return true;
                    }
                    if (cs1.RecommendScore == cs2.RecommendScore)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool operator >(MinorCheckScore cs1, MinorCheckScore cs2)
        {
            if (cs1.ErrorScore > cs2.ErrorScore)
            {
                return true;
            }
            if (cs1.ErrorScore == cs2.ErrorScore)
            {
                if (cs1.WarningScore > cs2.WarningScore)
                {
                    return true;
                }
                if (cs1.WarningScore == cs2.WarningScore)
                {
                    if (cs1.RecommendScore > cs2.RecommendScore)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool operator <=(MinorCheckScore cs1, MinorCheckScore cs2)
        {
            if (cs1.ErrorScore < cs2.ErrorScore)
            {
                return true;
            }
            if (cs1.ErrorScore == cs2.ErrorScore)
            {
                if (cs1.WarningScore < cs2.WarningScore)
                {
                    return true;
                }
                if (cs1.WarningScore == cs2.WarningScore)
                {
                    if (cs1.RecommendScore < cs2.RecommendScore)
                    {
                        return true;
                    }
                    if (cs1.RecommendScore == cs2.RecommendScore)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool operator <(MinorCheckScore cs1, MinorCheckScore cs2)
        {
            if (cs1.ErrorScore < cs2.ErrorScore)
            {
                return true;
            }
            if (cs1.ErrorScore == cs2.ErrorScore)
            {
                if (cs1.WarningScore < cs2.WarningScore)
                {
                    return true;
                }
                if (cs1.WarningScore == cs2.WarningScore)
                {
                    if (cs1.RecommendScore < cs2.RecommendScore)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class CheckScore : MinorCheckScore
    {
        public List<RuleResult> RuleResults;

        public CheckScore(List<RuleResult> ruleResults)
        {
            RuleResults = ruleResults;
            SetResultValues();
        }

        private void SetResultValues()
        {
            this.ErrorScore = RuleResults.Where(r => r.Rule.ErrorLevel == RuleAPI.Models.ErrorLevel.Error).Sum(r => r.PassVal);
            this.WarningScore = RuleResults.Where(r => r.Rule.ErrorLevel == RuleAPI.Models.ErrorLevel.Warning).Sum(r => r.PassVal);
            this.RecommendScore = RuleResults.Where(r => r.Rule.ErrorLevel == RuleAPI.Models.ErrorLevel.Recommended).Sum(r => r.PassVal);
        }

        public double TotalScore()
        {
            return this.ErrorScore + WarningScore + RecommendScore;
        }

        public void UpdateCheckScore(CheckScore checkScore)
        {
            foreach (RuleResult rr in RuleResults)
            {
                foreach (RuleResult csRR in checkScore.RuleResults)
                {
                    if (rr.Rule.Id != csRR.Rule.Id)
                    {
                        continue;
                    }
                    if (!csRR.CheckCompleted)
                    {
                        continue;
                    }

                    rr.UpdateRuleResult(csRR);
                }
            }

            SetResultValues();
        }

        public static MinorCheckScore TempCheckScore(CheckScore cs1, CheckScore cs2)
        {
            // Assumes cs1 and cs2 have the same rules
            MinorCheckScore returningCheckScore = new MinorCheckScore(0, 0, 0);
            foreach (RuleResult rr1 in cs1.RuleResults)
            {
                foreach (RuleResult rr2 in cs2.RuleResults)
                {
                    if (rr1.Rule.Id != rr2.Rule.Id)
                    {
                        continue;
                    }

                    if (rr1.Rule.ErrorLevel == RuleAPI.Models.ErrorLevel.Error)
                    {
                        returningCheckScore.ErrorScore += rr2.CheckCompleted ? rr2.PassVal : rr1.PassVal;
                    }
                    if (rr1.Rule.ErrorLevel == RuleAPI.Models.ErrorLevel.Warning)
                    {
                        returningCheckScore.WarningScore += rr2.CheckCompleted ? rr2.PassVal : rr1.PassVal;
                    }
                    if (rr1.Rule.ErrorLevel == RuleAPI.Models.ErrorLevel.Recommended)
                    {
                        returningCheckScore.RecommendScore += rr2.CheckCompleted ? rr2.PassVal : rr1.PassVal;
                    }
                }
            }

            return returningCheckScore;
        }
    }
}