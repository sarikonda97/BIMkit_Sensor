using DbmsApi;
using DbmsApi.API;
using RuleAPI.Methods;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RMS.Controllers
{
    public class MethodController : ApiController
    {
        private static DBMSAPIController DBMSAPIController = new DBMSAPIController("https://localhost:44322//api/");

        /// <summary>
        /// Gets the property or relation methods
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Get(string id)
        {
            Dictionary<string, Type> methods = new Dictionary<string, Type>();
            if (id == "property")
            {
                foreach (var kvp in MethodFinder.GetAllPropertyMethods())
                {
                    methods.Add(kvp.Key, kvp.Value);
                }
                return Request.CreateResponseRMS(HttpStatusCode.OK, methods);
            }
            if (id == "relation")
            {
                foreach (var kvp in MethodFinder.GetAllRelationMethods())
                {
                    methods.Add(kvp.Key, kvp.Value);
                }
                return Request.CreateResponseRMS(HttpStatusCode.OK, methods);
            }

            APIResponse<List<ObjectType>> responseType = await DBMSAPIController.GetTypes();
            if (responseType.Code != HttpStatusCode.OK)
            {
                return Request.CreateResponseRMS(responseType.Code, "DBMS:" + responseType.ReasonPhrase);
            }
            ObjectTypeTree.BuildTypeTree(responseType.Data);
            if (id == "vo")
            {
                return Request.CreateResponseRMS(HttpStatusCode.OK, MethodFinder.GetAllVOMethods());
            }
            if (id == "type")
            {
                return Request.CreateResponseRMS(HttpStatusCode.OK, MethodFinder.GetAllTypes());
            }

            return Request.CreateResponseRMS(HttpStatusCode.BadRequest, "Bad request");
        }
    }
}