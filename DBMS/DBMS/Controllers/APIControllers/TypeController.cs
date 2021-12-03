using DBMS.Controllers.DBControllers;
using DBMS.Filters;
using DbmsApi;
using DbmsApi.API;
using DbmsApi.Mongo;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DBMS.Controllers.APIControllers
{
    public class TypeController : ApiController
    {
        protected MongoDbController db;
        public TypeController() { db = MongoDbController.Instance; }
        public TypeController(MongoDbController db) { this.db = db; }

        public HttpResponseMessage Get()
        {
            List<ObjectType> listOfTypes = db.GetAllAvailableTypes();

            return Request.CreateResponseDBMS(HttpStatusCode.OK, listOfTypes);
        }

        public HttpResponseMessage Get(string id)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            ObjectType type = db.RetrieveType(id);
            if (type == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "No Type with that ID exists");
            }

            return Request.CreateResponseDBMS(HttpStatusCode.OK, type);
        }

        public HttpResponseMessage Post([FromBody] ObjectType type)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            if (!user.IsAdmin)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Must be an Admin");
            }

            if (type == null || string.IsNullOrWhiteSpace(type.Name) || string.IsNullOrWhiteSpace(type.ParentName))
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "Missing Type Info");
            }

            if (type.Name == "Root")
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Cannot edit Root");
            }

            string typeId = db.CreateType(type);
            return Request.CreateResponseDBMS(HttpStatusCode.OK, typeId);
        }

        public HttpResponseMessage Put([FromBody] ObjectType type)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            if (!user.IsAdmin)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Must be an Admin");
            }

            if (type == null || string.IsNullOrWhiteSpace(type.Name) || string.IsNullOrWhiteSpace(type.ParentName) )
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "Missing Type Info");
            }

            if (type.Name == "Root")
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Cannot edit Root");
            }

            if (!db.UpdateType(type, true))
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Creates Type Loop");
            }

            return Request.CreateResponseDBMS(HttpStatusCode.OK, "Update Successful");
        }

        public HttpResponseMessage Delete(string id)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            if (!user.IsAdmin)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Must be an Admin");
            }

            if (id == "Root")
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Cannot edit Root");
            }

            db.DeleteType(id);
            return Request.CreateResponseDBMS(HttpStatusCode.OK, "Delete Successful");
        }
    }
}