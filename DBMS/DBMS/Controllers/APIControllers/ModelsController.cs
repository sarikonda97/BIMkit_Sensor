using DBMS.Controllers.DBControllers;
using DBMS.Filters;
using DbmsApi.API;
using DbmsApi.Mongo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;

namespace DBMS.Controllers.APIControllers
{
    public class ModelsController : ApiController
    {
        protected MongoDbController db;
        public ModelsController() { db = MongoDbController.Instance; }
        public ModelsController(MongoDbController db) { this.db = db; }

        public HttpResponseMessage Get()
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            return Request.CreateResponseDBMS(HttpStatusCode.OK, db.RetrieveAvailableModels(user.Username));
        }

        [System.Web.Http.HttpGet]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        public HttpResponseMessage Get(string id, string lod)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            LevelOfDetail levelOfDetail = LevelOfDetail.LOD100;
            try
            {
                levelOfDetail = (LevelOfDetail)Enum.Parse(typeof(LevelOfDetail), lod);
            }
            catch
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "Missing Level of Detail");
            }

            MongoModel model = db.GetModel(id);
            if (model == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "No model exists with the given Id");
            }

            if (!(user.AccessibleModels.Contains(model.Id) || user.OwnedModels.Contains(model.Id) || user.IsAdmin))
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Cannot access this model");
            }

            Model fullModel = new Model
            {
                Id = model.Id,
                Name = model.Name,
                Tags = model.Tags,
                Properties = model.Properties,
                ModelObjects = model.ModelObjects,
                Relations = model.Relations,
            };

            // Swap out reference to Catalog Objects with the real objects
            foreach (CatalogObjectReference catalogRef in model.CatalogObjects)
            {
                MongoCatalogObject mongoCO = db.GetCatalogObject(catalogRef.CatalogId);
                ModelCatalogObject catalogObject;
                if (mongoCO == null)
                {
                    catalogObject = new ModelCatalogObject()
                    {
                        Id = catalogRef.Id,
                        CatalogId = catalogRef.CatalogId,
                        Location = catalogRef.Location,
                        Orientation = catalogRef.Orientation,
                        Tags = catalogRef.Tags,
                        Components = new List<Component>(),
                        Name = "N/A",
                        Properties = new Properties(),
                        TypeId = "N/A"
                    };
                }
                else
                {
                    catalogObject = new ModelCatalogObject()
                    {
                        Id = catalogRef.Id,
                        CatalogId = catalogRef.CatalogId,
                        Location = catalogRef.Location,
                        Orientation = catalogRef.Orientation,
                        Tags = catalogRef.Tags,
                        Components = (mongoCO.MeshReps.Any(c => c.LevelOfDetail == levelOfDetail) ? mongoCO.MeshReps.First(c => c.LevelOfDetail == levelOfDetail) : mongoCO.MeshReps.FirstOrDefault()).Components,
                        Name = mongoCO.Name,
                        Properties = mongoCO.Properties,
                        TypeId = mongoCO.TypeId
                    };
                }

                fullModel.ModelObjects.Add(catalogObject);
            }

            return Request.CreateResponseDBMS(HttpStatusCode.OK, fullModel);
        }

        [System.Web.Http.HttpPost]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        public HttpResponseMessage Post([FromBody] Model model)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            if (model == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "Model missing");
            }

            MongoModel fullModel = new MongoModel(model, user.PublicName);
            fullModel.Id = null;

            // Need to do a quick check that the catalog Ids are valid. If not then they are deleted:
            List<CatalogObjectMetadata> coMetas = db.RetrieveAvailableCatalogObjects();
            foreach (CatalogObjectReference coRef in fullModel.CatalogObjects)
            {
                if (coMetas.Select(com => com.CatalogObjectId).Contains(coRef.CatalogId))
                {
                    continue;
                }
                if (coMetas.Select(com => com.Name).Contains(coRef.CatalogId))
                {
                    coRef.CatalogId = coMetas.First(i => i.Name == coRef.CatalogId).CatalogObjectId;
                    continue;
                }
                coRef.CatalogId = null;
            }

            fullModel.CatalogObjects = fullModel.CatalogObjects.Where(coref => coref.CatalogId != null).ToList();

            string id = db.CreateModel(fullModel);
            db.AddOwnedModel(user.Username, id);

            // Return the id of the new model
            return Request.CreateResponseDBMS(HttpStatusCode.OK, id);
        }

        [System.Web.Http.HttpPut]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        public HttpResponseMessage Put([FromBody] Model model)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            if (model == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.BadRequest, "Model missing");
            }

            if (!(user.AccessibleModels.Contains(model.Id) || user.OwnedModels.Contains(model.Id) || user.IsAdmin))
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Cannot access this model");
            }

            MongoModel fullModel = new MongoModel(model, user.PublicName);
            db.UpdateModel(fullModel);
            return Request.CreateResponseDBMS(HttpStatusCode.OK, "Update Successful");
        }

        public HttpResponseMessage Delete(string id)
        {
            User user = db.GetUserFromToken(ActionContext.Request.Headers.Authorization.Parameter);
            if (user == null)
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Not Logged in or Session has ended");
            }

            if (!(user.OwnedModels.Contains(id) || user.IsAdmin))
            {
                return Request.CreateResponseDBMS(HttpStatusCode.Unauthorized, "Cannot delete this model");
            }

            db.DeleteModel(id);
            return Request.CreateResponseDBMS(HttpStatusCode.OK, "Delete Successful");
        }
    }
}