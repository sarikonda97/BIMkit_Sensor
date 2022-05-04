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
        public List<CatalogInitializerID> CatalogInitializers;
        public List<string> RuleIDs;
        public TokenData DBMSToken;
        public string RMSUsername;
        public LevelOfDetail LOD;
        public Vector3D StartLocation;
        public GenerationType GenerationType;

        public GenerativeDesignSettings GenSettings;

        public GenerativeRequest(TokenData dbmsToken, string rmsUsername, string modelId, List<CatalogInitializerID> catalogInitializers, List<string> ruleIds, LevelOfDetail lod, GenerativeDesignSettings genSettings, GenerationType generationType)
        {
            ModelID = modelId;
            CatalogInitializers = catalogInitializers;
            RuleIDs = ruleIds;
            DBMSToken = dbmsToken;
            RMSUsername = rmsUsername;
            LOD = lod;
            GenSettings = genSettings;
            GenerationType = generationType;
        }
    }
}