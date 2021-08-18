using DbmsApi.API;
using GenerativeDesignPackage;
using MathPackage;
using RuleAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesignAPI
{
    public class GenerativeRequest
    {
        public string ModelID;
        public string CatalogID;
        public List<string> RuleIDs;
        public TokenData DBMSToken;
        public string RMSUsername;
        public LevelOfDetail LOD;
        public Vector3D StartLocation;

        public GenerativeDesignSettings GenSettings;

        public GenerativeRequest(TokenData dbmsToken, string rmsUsername, string modelId, string catalogID, List<string> ruleIds, LevelOfDetail lod, Vector3D startLocation, GenerativeDesignSettings genSettings)
        {
            ModelID = modelId;
            CatalogID = catalogID;
            RuleIDs = ruleIds;
            DBMSToken = dbmsToken;
            RMSUsername = rmsUsername;
            LOD = lod;
            StartLocation = startLocation;
            GenSettings = genSettings;
        }
    }
}